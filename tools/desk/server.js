#!/usr/bin/env node
'use strict';
/*
 * 마왕성 인사팀 · 출근부 — 로컬 서버
 *
 * 의존성 0: Node 내장 모듈(http, fs, path, child_process)만 사용한다. npm install 불필요.
 * 역할: ① public/ 의 화면을 브라우저에 보내고 ② production/desk/ 의 md 파일을 읽고 쓴다.
 *
 * 실행: node tools/desk/server.js [--no-open] [--no-git]
 *   --no-open : 브라우저를 자동으로 열지 않음
 *   --no-git  : 퇴근 시 git 커밋/푸시를 건너뜀 (테스트용)
 * 환경변수 DESK_DATA_DIR : 데이터 폴더를 바꿈 (테스트용)
 */
const http = require('http');
const fs = require('fs');
const path = require('path');
const { execFile } = require('child_process');

// ───────────────────────── 설정 ─────────────────────────
const ROOT = __dirname; // tools/desk
const CONFIG_PATH = path.join(ROOT, 'desk.config.json');
const DEFAULTS = {
  port: 4123,
  dataDir: '../../production/desk',
  seasonStart: '2026-09-09',
  heatStart: null, // 히트맵 시작 달 (YYYY-MM-DD). 비우면 seasonStart 가 속한 달의 1일
  autoPush: true,
  heatLevels: [1, 2, 4, 6], // 색 단계 경계(시간). 경계 개수 + 1 = 단계 수
  maxPick: 3,
  pomodoro: { focus: 50, break: 10 }, // 뽀모도로 집중/휴식 (분)
};

function loadConfig() {
  try {
    return Object.assign({}, DEFAULTS, JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8')));
  } catch (e) {
    console.warn('[desk] desk.config.json 을 읽지 못해 기본값을 씁니다:', e.message);
    return Object.assign({}, DEFAULTS);
  }
}

const config = loadConfig();
const DATA_DIR = process.env.DESK_DATA_DIR
  ? path.resolve(process.env.DESK_DATA_DIR)
  : path.resolve(ROOT, config.dataDir);
const DEVLOG_DIR = path.join(DATA_DIR, 'devlog');
const TODO_PATH = path.join(DATA_DIR, 'todo.md');
const PUBLIC_DIR = path.join(ROOT, 'public');
const NO_OPEN = process.argv.includes('--no-open');
const NO_GIT = process.argv.includes('--no-git') || process.env.DESK_NO_GIT === '1';

// ───────────────────────── 날짜/시간 ─────────────────────────
const pad2 = (n) => String(n).padStart(2, '0');
const todayStr = (d = new Date()) => `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
const timeStr = (d = new Date()) => `${pad2(d.getHours())}:${pad2(d.getMinutes())}`;

function minutesOf(hhmm) {
  const [h, m] = hhmm.split(':').map(Number);
  return h * 60 + m;
}

/** "HH:MM-HH:MM" 세션 문자열의 길이(분). 열린 세션("HH:MM-")은 0. 자정을 넘기면 24시간을 더한다. */
function sessionMinutes(s) {
  const [a, b] = s.split('-');
  if (!b) return 0;
  let d = minutesOf(b) - minutesOf(a);
  if (d < 0) d += 24 * 60;
  return d;
}

const computeHours = (sessions) =>
  Math.round((sessions.reduce((acc, s) => acc + sessionMinutes(s), 0) / 60) * 10) / 10;

const uniq = (arr) => [...new Set(arr.map((x) => String(x).trim()).filter(Boolean))];

// ───────────────────────── frontmatter (YAML 부분집합) ─────────────────────────
// 지원 형식: `key: 값`, `key: []`, 그리고
// key:
//   - 항목
// 문자열은 JSON 따옴표로 저장/해석해서 특수문자 걱정을 없앤다.

function serializeValue(v) {
  if (typeof v === 'number' || typeof v === 'boolean') return String(v);
  const s = String(v);
  if (/^\d{4}-\d{2}-\d{2}$/.test(s) || /^[a-z]+$/.test(s)) return s; // 날짜·status 같은 단순값은 따옴표 없이
  return JSON.stringify(s);
}

function serializeFrontmatter(fm) {
  const lines = ['---'];
  for (const [k, v] of Object.entries(fm)) {
    if (Array.isArray(v)) {
      if (v.length === 0) lines.push(`${k}: []`);
      else {
        lines.push(`${k}:`);
        v.forEach((item) => lines.push(`  - ${serializeValue(item)}`));
      }
    } else lines.push(`${k}: ${serializeValue(v)}`);
  }
  lines.push('---');
  return lines.join('\n');
}

function parseValue(raw) {
  const s = raw.trim();
  if (s === '') return '';
  if (s === '[]') return [];
  if (s.startsWith('"')) {
    try { return JSON.parse(s); } catch { return s; }
  }
  if (s.startsWith('[')) {
    try { return JSON.parse(s); } catch { return s.slice(1, -1).split(',').map((x) => x.trim()).filter(Boolean); }
  }
  if (/^-?\d+(\.\d+)?$/.test(s)) return Number(s);
  if (s === 'true') return true;
  if (s === 'false') return false;
  return s;
}

/** md 텍스트 → { fm: frontmatter 객체, body: 본문 } */
function parseDoc(text) {
  if (!text.startsWith('---')) return { fm: {}, body: text };
  const end = text.indexOf('\n---', 3);
  if (end < 0) return { fm: {}, body: text };
  const fmText = text.slice(3, end).replace(/^\r?\n/, '');
  const body = text.slice(end + 4).replace(/^\r?\n/, '');
  const fm = {};
  let listKey = null;
  for (const rawLine of fmText.split(/\r?\n/)) {
    const line = rawLine.replace(/\s+$/, '');
    if (!line.trim()) continue;
    const li = line.match(/^\s+-\s*(.*)$/);
    if (li && listKey) { fm[listKey].push(parseValue(li[1])); continue; }
    const kv = line.match(/^([A-Za-z_][\w-]*):\s*(.*)$/);
    if (!kv) continue;
    const [, key, val] = kv;
    if (val.trim() === '') { fm[key] = []; listKey = key; }
    else { fm[key] = parseValue(val); listKey = null; }
  }
  return { fm, body };
}

// ───────────────────────── 본문 섹션 ─────────────────────────
const SECTIONS = ['한 일', '배운 것', '막힌 것', '다음에 할 것'];

/** 본문 → [{ name, lines }] (name=null 은 첫 헤더 앞 텍스트) */
function parseSections(body) {
  const out = [];
  let cur = { name: null, lines: [] };
  for (const line of body.split(/\r?\n/)) {
    const h = line.match(/^##\s+(.+?)\s*$/);
    if (h) { out.push(cur); cur = { name: h[1], lines: [] }; continue; }
    cur.lines.push(line);
  }
  out.push(cur);
  return out;
}

const bulletsOf = (section) =>
  section
    ? section.lines.map((l) => l.match(/^\s*[-*]\s+(.*)$/)).filter(Boolean).map((m) => m[1].trim()).filter(Boolean)
    : [];

/** 폼 입력(줄 배열) → 4단 본문. 모르는 섹션은 뒤에 그대로 보존한다. */
function buildBody(fields, existingBody) {
  const map = { '한 일': fields.did, '배운 것': fields.learned, '막힌 것': fields.blocked, '다음에 할 것': fields.next };
  const existing = existingBody ? parseSections(existingBody) : [];
  const parts = [];
  for (const name of SECTIONS) {
    const lines = (map[name] || [])
      .map((l) => String(l).trim())
      .filter(Boolean)
      .map((l) => (/^[-*]\s/.test(l) ? l : `- ${l}`));
    parts.push(`## ${name}\n${lines.join('\n')}`.trimEnd());
  }
  for (const s of existing) {
    if (s.name && !SECTIONS.includes(s.name)) parts.push(`## ${s.name}\n${s.lines.join('\n').trim()}`);
  }
  return parts.join('\n\n') + '\n';
}

// ───────────────────────── 데브로그 파일 (하루 한 파일) ─────────────────────────
const dayPath = (date) => path.join(DEVLOG_DIR, `${date}.md`);

function readDay(date) {
  const p = dayPath(date);
  if (!fs.existsSync(p)) return null;
  const { fm, body } = parseDoc(fs.readFileSync(p, 'utf8'));
  return { date, fm, body };
}

function writeDay(day) {
  fs.mkdirSync(DEVLOG_DIR, { recursive: true });
  fs.writeFileSync(dayPath(day.date), serializeFrontmatter(day.fm) + '\n\n' + day.body, 'utf8');
}

function listDays() {
  if (!fs.existsSync(DEVLOG_DIR)) return [];
  return fs
    .readdirSync(DEVLOG_DIR)
    .filter((f) => /^\d{4}-\d{2}-\d{2}\.md$/.test(f))
    .sort()
    .map((f) => readDay(f.slice(0, -3)))
    .filter(Boolean);
}

const isOpenSession = (s) => typeof s === 'string' && s.endsWith('-');
const openSessionOf = (day) => (day.fm.sessions || []).find(isOpenSession) || null;
/** 근무 중(열린 세션)이거나 부재 중이면 "활성" — 아직 퇴근하지 않은 날 */
const isActiveDay = (day) => !!openSessionOf(day) || day.fm.status === 'away';
/** 닫힌 세션들의 합(분). 부재로 나뉜 세션 사이의 빈 시간은 자연히 빠진다 */
const closedMinutes = (sessions) => (sessions || []).reduce((acc, s) => acc + sessionMinutes(s), 0);

/** 활성인 날(자정을 넘긴 경우 어제일 수 있음) */
function findActiveDay() {
  const days = listDays();
  for (let i = days.length - 1; i >= 0; i--) if (isActiveDay(days[i])) return days[i];
  return null;
}

/** Day 번호 = 시즌 시작일 이후 작업한 날 수 + 1 */
function nextDayNumber(date) {
  const n = listDays().filter((d) => d.date >= config.seasonStart && d.date < date).length;
  return n + 1;
}

/** "Day 3 — 요약" → "요약". 아직 요약이 없는 "Day 3" 만 있으면 빈 문자열. */
const stripDayPrefix = (title) => String(title || '').replace(/^Day\s*\d+\s*([—–-]\s*)?/, '');

/** API 로 내보낼 요약 (히트맵 툴팁·목록·통계용) */
function summarizeDay(d) {
  const secs = parseSections(d.body);
  const find = (n) => secs.find((s) => s.name === n);
  const sessions = d.fm.sessions || [];
  const open = openSessionOf(d);
  const status = open ? 'open' : d.fm.status === 'away' ? 'away' : 'closed';
  const last = sessions[sessions.length - 1] || '';
  return {
    date: d.date,
    day: d.fm.day || 0,
    title: d.fm.title || '',
    summary: stripDayPrefix(d.fm.title),
    status,
    hours: Number(d.fm.hours) || 0,
    sessions,
    openSince: open ? open.slice(0, -1) : null,
    awaySince: status === 'away' ? last.split('-')[1] || null : null, // 부재 버튼을 누른 시각
    workedMinutes: closedMinutes(sessions),                            // 닫힌 세션 합(분) — 화면의 "오늘 누적"용
    pomodoros: Number(d.fm.pomodoros) || 0,
    picked: d.fm.picked || [],
    done: d.fm.done || [],
    did: bulletsOf(find('한 일')),
    learned: bulletsOf(find('배운 것')),
    blocked: bulletsOf(find('막힌 것')),
    next: bulletsOf(find('다음에 할 것')),
  };
}

// ───────────────────────── 할 일 (todo.md) ─────────────────────────
function readTodos() {
  if (!fs.existsSync(TODO_PATH)) return { open: [], done: [] };
  const open = [];
  const done = [];
  for (const line of fs.readFileSync(TODO_PATH, 'utf8').split(/\r?\n/)) {
    const m = line.match(/^\s*[-*]\s+\[( |x|X)\]\s+(.*)$/);
    if (!m) continue;
    (m[1] === ' ' ? open : done).push(m[2].trim());
  }
  return { open, done };
}

function writeTodos(t) {
  const txt = [
    '# 할 일',
    '',
    '> 출근부 앱(tools/desk)이 읽고 쓰는 파일. 손으로 고쳐도 됩니다 (형식: `- [ ] 할 일`).',
    '',
    '## 할 일',
    ...t.open.map((x) => `- [ ] ${x}`),
    '',
    '## 완료',
    ...t.done.map((x) => `- [x] ${x}`),
    '',
  ].join('\n');
  fs.mkdirSync(DATA_DIR, { recursive: true });
  fs.writeFileSync(TODO_PATH, txt, 'utf8');
}

const stripDoneDate = (s) => s.replace(/\s*\(\d{4}-\d{2}-\d{2}\)\s*$/, '');

/** 오늘 파일(열린 세션 우선, 없으면 오늘 날짜 파일)에 done 항목을 반영 */
function markDoneInDay(text, add) {
  const day = findActiveDay() || readDay(todayStr());
  if (!day) return;
  const done = uniq(day.fm.done || []);
  day.fm.done = add ? uniq([...done, text]) : done.filter((x) => x !== text);
  writeDay(day);
}

function todoAction(action, textRaw) {
  const text = String(textRaw || '').trim();
  if (!text) throw new Error('내용이 비어 있습니다');
  const t = readTodos();
  const today = todayStr();
  switch (action) {
    case 'add':
      if (!t.open.includes(text)) t.open.push(text);
      writeTodos(t);
      break;
    case 'remove':
      t.open = t.open.filter((x) => x !== text);
      t.done = t.done.filter((x) => x !== text && stripDoneDate(x) !== text);
      writeTodos(t);
      break;
    case 'done':
      t.open = t.open.filter((x) => x !== text);
      t.done = [`${text} (${today})`, ...t.done.filter((x) => stripDoneDate(x) !== text)];
      writeTodos(t);
      markDoneInDay(text, true);
      break;
    case 'undone': {
      const plain = stripDoneDate(text);
      t.done = t.done.filter((x) => x !== text && stripDoneDate(x) !== plain);
      if (!t.open.includes(plain)) t.open.unshift(plain);
      writeTodos(t);
      markDoneInDay(plain, false);
      break;
    }
    case 'pick':
    case 'unpick': {
      const day = findActiveDay();
      if (!day) throw new Error('출근 중일 때만 오늘 할 일을 바꿀 수 있습니다');
      const picked = uniq(day.fm.picked || []);
      day.fm.picked = action === 'pick' ? uniq([...picked, text]) : picked.filter((x) => x !== text);
      writeDay(day);
      break;
    }
    default:
      throw new Error('알 수 없는 동작: ' + action);
  }
}

// ───────────────────────── 출근 / 퇴근 ─────────────────────────
function clockIn(pickedRaw) {
  if (findActiveDay()) throw new Error('이미 출근 중입니다');
  const picked = uniq(Array.isArray(pickedRaw) ? pickedRaw : []);
  const date = todayStr();
  const now = timeStr();
  let d = readDay(date);
  if (d) {
    // 오늘 두 번째 출근: 세션만 추가
    d.fm.sessions = [...(d.fm.sessions || []), `${now}-`];
    d.fm.status = 'open';
    d.fm.picked = uniq([...(d.fm.picked || []), ...picked]);
  } else {
    const day = nextDayNumber(date);
    d = {
      date,
      fm: { title: `Day ${day}`, date, day, status: 'open', sessions: [`${now}-`], hours: 0, picked, done: [], pomodoros: 0, tags: [] },
      body: buildBody({}, ''),
    };
  }
  writeDay(d);
  return d;
}

function clockOut(fields) {
  const d = findActiveDay();
  if (!d) throw new Error('출근 상태가 아닙니다');
  const summary = String(fields.title || '').trim();
  if (!summary) throw new Error('오늘을 한 줄로 적어야 퇴근할 수 있습니다');
  const sessions = [...(d.fm.sessions || [])];
  const i = sessions.findIndex(isOpenSession);
  if (i >= 0) sessions[i] = sessions[i] + timeStr(); // 부재 중에 퇴근하면 닫을 세션이 없다
  d.fm.sessions = sessions;
  d.fm.status = 'closed';
  d.fm.hours = computeHours(sessions);
  d.fm.title = `Day ${d.fm.day} — ${summary}`;
  const asLines = (v) => (Array.isArray(v) ? v : String(v || '').split(/\r?\n/));
  d.body = buildBody(
    { did: asLines(fields.did), learned: asLines(fields.learned), blocked: asLines(fields.blocked), next: asLines(fields.next) },
    d.body,
  );
  writeDay(d);
  return d;
}

// ───────────────────────── 부재 / 복귀 / 뽀모도로 ─────────────────────────
/** 부재: 현재 세션을 닫고 status=away. 복귀 전까지의 시간은 근무에서 빠진다 */
function goAway() {
  const d = findActiveDay();
  if (!d) throw new Error('출근 상태가 아닙니다');
  if (d.fm.status === 'away') throw new Error('이미 부재 중입니다');
  const sessions = [...(d.fm.sessions || [])];
  const i = sessions.findIndex(isOpenSession);
  sessions[i] = sessions[i] + timeStr();
  d.fm.sessions = sessions;
  d.fm.status = 'away';
  d.fm.hours = computeHours(sessions);
  writeDay(d);
  return d;
}

/** 복귀: 새 세션을 열고 status=open */
function comeBack() {
  const d = findActiveDay();
  if (!d || d.fm.status !== 'away') throw new Error('부재 중이 아닙니다');
  d.fm.sessions = [...(d.fm.sessions || []), `${timeStr()}-`];
  d.fm.status = 'open';
  writeDay(d);
  return d;
}

/** 집중 블록 하나 완료 → 그날 파일의 pomodoros 를 1 올린다 (통계·툴팁용) */
function addPomodoro() {
  const d = findActiveDay() || readDay(todayStr());
  if (!d) throw new Error('오늘 출근 기록이 없습니다');
  d.fm.pomodoros = (Number(d.fm.pomodoros) || 0) + 1;
  writeDay(d);
  return d;
}

// ───────────────────────── git (데이터 폴더만 커밋 → 푸시) ─────────────────────────
function git(args, cwd) {
  return new Promise((resolve) => {
    execFile('git', args, { cwd, windowsHide: true }, (err, stdout, stderr) => {
      resolve({ ok: !err, out: String(stdout || '').trim(), err: String(stderr || '').trim() || (err ? err.message : '') });
    });
  });
}

async function pushData(message) {
  if (NO_GIT) return { skipped: true, reason: '--no-git' };
  if (!config.autoPush) return { skipped: true, reason: 'autoPush=false' };
  const top = await git(['rev-parse', '--show-toplevel'], DATA_DIR);
  if (!top.ok) return { ok: false, error: '데이터 폴더가 git 저장소 안에 없습니다: ' + top.err };
  const repo = top.out;
  const rel = path.relative(repo, DATA_DIR).split(path.sep).join('/') || '.';
  const add = await git(['add', '-A', '--', rel], repo);
  if (!add.ok) return { ok: false, error: add.err };
  const status = await git(['status', '--porcelain', '--', rel], repo);
  if (!status.out) return { ok: true, nothing: true };
  const commit = await git(['commit', '-m', message, '--', rel], repo);
  if (!commit.ok) return { ok: false, error: commit.err };
  const push = await git(['push'], repo);
  return push.ok ? { ok: true, pushed: true } : { ok: true, pushed: false, error: push.err };
}

// ───────────────────────── 상태 (화면이 한 번에 받아가는 것) ─────────────────────────
function stateJson() {
  const today = todayStr();
  const days = listDays().map(summarizeDay);
  const active = days.find((d) => d.status === 'open' || d.status === 'away') || null;
  const todayDay = days.find((d) => d.date === today) || null;
  return {
    now: new Date().toISOString(),
    today,
    active,
    todayDay,
    nextDayNumber: todayDay ? todayDay.day : nextDayNumber(today),
    todos: readTodos(),
    days,
    config: {
      seasonStart: config.seasonStart,
      heatStart: /^\d{4}-\d{2}-\d{2}$/.test(config.heatStart || '') ? config.heatStart : `${config.seasonStart.slice(0, 7)}-01`,
      heatLevels: config.heatLevels,
      maxPick: config.maxPick,
      pomodoro: Object.assign({}, DEFAULTS.pomodoro, config.pomodoro || {}),
      autoPush: config.autoPush && !NO_GIT,
    },
    dataDir: DATA_DIR,
  };
}

// ───────────────────────── HTTP ─────────────────────────
const MIME = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.ico': 'image/x-icon',
};

function send(res, code, body, type = 'application/json; charset=utf-8') {
  res.writeHead(code, { 'Content-Type': type, 'Cache-Control': 'no-store' });
  res.end(body);
}
const json = (res, obj, code = 200) => send(res, code, JSON.stringify(obj));
const fail = (res, e, code = 400) => json(res, { error: e && e.message ? e.message : String(e) }, code);

function readBody(req) {
  return new Promise((resolve, reject) => {
    let b = '';
    req.on('data', (c) => { b += c; if (b.length > 1e6) req.destroy(); });
    req.on('end', () => { try { resolve(b ? JSON.parse(b) : {}); } catch (e) { reject(e); } });
    req.on('error', reject);
  });
}

function serveStatic(res, urlPath) {
  const rel = urlPath === '/' ? '/index.html' : urlPath;
  const file = path.normalize(path.join(PUBLIC_DIR, rel));
  if (!file.startsWith(PUBLIC_DIR) || !fs.existsSync(file) || fs.statSync(file).isDirectory()) {
    return send(res, 404, 'not found', 'text/plain; charset=utf-8');
  }
  send(res, 200, fs.readFileSync(file), MIME[path.extname(file)] || 'application/octet-stream');
}

async function handle(req, res) {
  const url = new URL(req.url, 'http://localhost');
  const p = url.pathname;
  try {
    if (req.method === 'GET' && p === '/api/state') return json(res, stateJson());

    if (req.method === 'GET' && p.startsWith('/api/devlog/')) {
      const date = p.slice('/api/devlog/'.length);
      if (!/^\d{4}-\d{2}-\d{2}$/.test(date) || !fs.existsSync(dayPath(date))) return fail(res, new Error('없는 날짜'), 404);
      return json(res, { date, text: fs.readFileSync(dayPath(date), 'utf8') });
    }

    if (req.method === 'POST' && p === '/api/clockin') {
      const body = await readBody(req);
      const d = clockIn(body.picked);
      return json(res, { ok: true, day: summarizeDay(d), state: stateJson() });
    }

    if (req.method === 'POST' && p === '/api/clockout') {
      const body = await readBody(req);
      const d = clockOut(body);
      const msg = `desk: ${d.date} 퇴근 (${d.fm.hours}h) — ${stripDayPrefix(d.fm.title)}`;
      const gitResult = await pushData(msg);
      return json(res, { ok: true, day: summarizeDay(d), git: gitResult, state: stateJson() });
    }

    if (req.method === 'POST' && p === '/api/away') {
      const d = goAway();
      return json(res, { ok: true, day: summarizeDay(d), state: stateJson() });
    }

    if (req.method === 'POST' && p === '/api/back') {
      const d = comeBack();
      return json(res, { ok: true, day: summarizeDay(d), state: stateJson() });
    }

    if (req.method === 'POST' && p === '/api/pomodoro') {
      const body = await readBody(req);
      if (body.action !== 'done') throw new Error('알 수 없는 동작: ' + body.action);
      const d = addPomodoro();
      return json(res, { ok: true, day: summarizeDay(d), state: stateJson() });
    }

    if (req.method === 'POST' && p === '/api/todos') {
      const body = await readBody(req);
      todoAction(body.action, body.text);
      return json(res, { ok: true, state: stateJson() });
    }

    if (req.method === 'GET' && !p.startsWith('/api/')) return serveStatic(res, p);
    return fail(res, new Error('not found'), 404);
  } catch (e) {
    return fail(res, e);
  }
}

// ───────────────────────── 시작 ─────────────────────────
function openBrowser(url) {
  if (NO_OPEN) return;
  const [cmd, args] =
    process.platform === 'win32' ? ['cmd', ['/c', 'start', '', url]]
    : process.platform === 'darwin' ? ['open', [url]]
    : ['xdg-open', [url]];
  execFile(cmd, args, { windowsHide: true }, () => {});
}

fs.mkdirSync(DEVLOG_DIR, { recursive: true });
if (!fs.existsSync(TODO_PATH)) writeTodos({ open: [], done: [] });

const PORT = Number(process.env.DESK_PORT) || config.port; // DESK_PORT 는 테스트용 포트 바꾸기
const server = http.createServer(handle);
const url = `http://localhost:${PORT}`;

server.on('error', (e) => {
  if (e.code === 'EADDRINUSE') {
    console.log(`[desk] 이미 켜져 있는 출근부가 있습니다 → 브라우저만 엽니다 (${url})`);
    openBrowser(url);
    setTimeout(() => process.exit(0), 300);
  } else {
    console.error('[desk] 서버 오류:', e);
    process.exit(1);
  }
});

server.listen(PORT, '127.0.0.1', () => {
  console.log('┌──────────────────────────────────────────┐');
  console.log('│  마왕성 인사팀 · 출근부                    │');
  console.log('└──────────────────────────────────────────┘');
  console.log(`  화면    : ${url}`);
  console.log(`  데이터  : ${DATA_DIR}`);
  console.log(`  시즌    : ${config.seasonStart} 부터  ·  자동 푸시: ${config.autoPush && !NO_GIT ? 'ON' : 'OFF'}`);
  console.log('  끄기    : 이 창을 닫거나 Ctrl+C');
  openBrowser(url);
});
