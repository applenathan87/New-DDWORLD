'use strict';
/*
 * 마왕성 인사팀 · 출근부 — 화면 스크립트 (의존성 0, 브라우저 기본 API만)
 *
 * 흐름: 서버의 /api/state 를 받아(S) 화면 전체를 그린다(render).
 *       버튼을 누르면 서버에 POST → 돌아온 새 상태로 다시 그린다.
 */

// ───────────────────────── 작은 도우미 ─────────────────────────
const $ = (sel, root = document) => root.querySelector(sel);

/** el('div', {class:'x', onclick: fn}, '텍스트', 자식...) — DOM 만들기 도우미 */
function el(tag, attrs = {}, ...children) {
  const node = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (v == null || v === false) continue;
    if (k.startsWith('on')) node.addEventListener(k.slice(2), v);
    else if (k === 'class') node.className = v;
    else if (k === 'checked' || k === 'value' || k === 'disabled') node[k] = v;
    else node.setAttribute(k, v === true ? '' : v);
  }
  for (const c of children.flat()) {
    if (c == null || c === false) continue;
    node.append(c instanceof Node ? c : document.createTextNode(String(c)));
  }
  return node;
}

const esc = (s) => String(s).replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const uniq = (arr) => [...new Set(arr.map((x) => String(x).trim()).filter(Boolean))];

const WEEKDAYS = ['일', '월', '화', '수', '목', '금', '토'];
const pad2 = (n) => String(n).padStart(2, '0');
const parseDate = (s) => { const [y, m, d] = s.split('-').map(Number); return new Date(y, m - 1, d); };
const dateStr = (d) => `${d.getFullYear()}-${pad2(d.getMonth() + 1)}-${pad2(d.getDate())}`;
const addDays = (d, n) => { const x = new Date(d); x.setDate(x.getDate() + n); return x; };
const fmtDate = (s) => { const d = parseDate(s); return `${d.getMonth() + 1}월 ${d.getDate()}일 (${WEEKDAYS[d.getDay()]})`; };
const fmtHours = (h) => `${Math.round(h * 10) / 10}h`;
const fmtDuration = (min) => { const h = Math.floor(min / 60), m = min % 60; return h ? `${h}시간 ${m}분` : `${m}분`; };

async function api(path, body) {
  const res = await fetch(path, body ? { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) } : undefined);
  const data = await res.json().catch(() => ({}));
  if (!res.ok || data.error) throw new Error(data.error || res.statusText);
  return data;
}

// ───────────────────────── 상태 ─────────────────────────
let S = null;            // 서버가 준 상태 (/api/state)
let view = 'idle';       // 근무 카드의 화면: idle | pick | clockout
let pickSel = new Set(); // 출근 시 고른 할 일
let heatIndex = {};      // date → day 요약 (툴팁용)

async function load() {
  S = await api('/api/state');
  render();
}

/** 출근 중이면 열린 세션의 경과 분 */
function elapsedMinutes() {
  if (!S.active) return 0;
  const [h, m] = S.active.openSince.split(':').map(Number);
  const start = parseDate(S.active.date);
  start.setHours(h, m, 0, 0);
  return Math.max(0, Math.floor((Date.now() - start.getTime()) / 60000));
}

/** 날짜별 작업시간 (출근 중이면 경과 시간을 오늘에 더함) */
function hoursByDate() {
  const map = {};
  for (const d of S.days) map[d.date] = d.hours;
  if (S.active) map[S.active.date] = (map[S.active.date] || 0) + elapsedMinutes() / 60;
  return map;
}

// ───────────────────────── 그리기 ─────────────────────────
function render() {
  renderTop();
  renderWork();
  renderTodos();
  renderStats();
  renderHeatmap();
  renderLegend();
  renderHistory();
}

function renderTop() {
  $('#top-date').textContent = `${S.today.slice(0, 4)}년 ${fmtDate(S.today)}`;
  const day = S.active ? S.active.day : S.nextDayNumber;
  $('#top-day').textContent = `Day ${day}${S.active ? ' · 출근 중' : ''}`;
  $('#top-day').classList.toggle('on', !!S.active);
}

// ── 근무 카드 ──
function renderWork() {
  const c = $('#work-card');
  c.innerHTML = '';

  if (S.active) {
    if (view === 'clockout') return renderClockOutForm(c);
    c.append(
      el('div', { class: 'work-head' },
        el('span', { class: 'status-badge open' }, '출근 중'),
        el('span', { class: 'work-time' }, `${S.active.openSince} 출근 · 경과 `, el('strong', { id: 'elapsed' }, fmtDuration(elapsedMinutes()))),
      ),
      el('h2', {}, `Day ${S.active.day} · 오늘 할 일`),
      renderPickedList(),
      renderAddToToday(),
      el('div', { class: 'actions' },
        el('button', { class: 'btn primary big', onclick: () => { view = 'clockout'; renderWork(); } }, '퇴근'),
      ),
    );
    return;
  }

  if (view === 'pick') return renderPickPanel(c);

  if (S.todayDay) {
    c.append(
      el('span', { class: 'status-badge closed' }, '퇴근 완료'),
      el('h2', {}, `Day ${S.todayDay.day} — ${S.todayDay.summary}`),
      el('p', { class: 'muted' }, `오늘 ${fmtHours(S.todayDay.hours)} · ${S.todayDay.sessions.join(', ')}`),
      el('div', { class: 'actions' },
        el('button', { class: 'btn', onclick: () => openDevlog(S.today) }, '오늘 일지 보기'),
        el('button', { class: 'btn primary', onclick: () => { view = 'pick'; renderWork(); } }, '다시 출근'),
      ),
    );
    return;
  }

  c.append(
    el('span', { class: 'status-badge idle' }, '퇴근 상태'),
    el('h2', {}, `Day ${S.nextDayNumber}을 시작할까요?`),
    el('p', { class: 'muted' }, '출근하면 오늘 할 일을 고르고, 오늘 날짜의 데브로그 파일이 생깁니다.'),
    el('div', { class: 'actions' },
      el('button', { class: 'btn primary big', onclick: () => { view = 'pick'; renderWork(); } }, '출근'),
    ),
  );
}

function renderPickPanel(c) {
  c.append(
    el('h2', {}, '오늘 할 일 고르기'),
    el('p', { class: 'muted' }, `${S.config.maxPick}개 이하를 권장합니다. 고르지 않고 출근해도 됩니다.`),
  );
  const ul = el('ul', { class: 'list pick' });
  for (const t of S.todos.open) {
    ul.append(el('li', {},
      el('label', {},
        el('input', { type: 'checkbox', checked: pickSel.has(t), onchange: (e) => (e.target.checked ? pickSel.add(t) : pickSel.delete(t)) }),
        el('span', {}, t),
      ),
    ));
  }
  if (!S.todos.open.length) ul.append(el('li', { class: 'muted' }, '할 일 목록이 비어 있습니다. 오른쪽 카드에서 추가하세요.'));
  c.append(
    ul,
    el('div', { class: 'actions' },
      el('button', { class: 'btn', onclick: () => { view = 'idle'; renderWork(); } }, '취소'),
      el('button', { class: 'btn primary big', onclick: doClockIn }, '출근 도장 찍기'),
    ),
  );
}

async function doClockIn() {
  try {
    const r = await api('/api/clockin', { picked: [...pickSel] });
    pickSel.clear();
    view = 'idle';
    S = r.state;
    render();
    stamp('출근');
  } catch (e) {
    toast(e.message, true);
  }
}

function renderPickedList() {
  const ul = el('ul', { class: 'list picked' });
  const doneSet = new Set(S.active.done);
  for (const t of S.active.picked) {
    const isDone = doneSet.has(t);
    ul.append(el('li', { class: isDone ? 'is-done' : '' },
      el('label', {},
        el('input', { type: 'checkbox', checked: isDone, onchange: () => todo(isDone ? 'undone' : 'done', t) }),
        el('span', {}, t),
      ),
      el('button', { class: 'icon-btn', title: '오늘 목록에서 빼기', onclick: () => todo('unpick', t) }, '−'),
    ));
  }
  if (!S.active.picked.length) ul.append(el('li', { class: 'muted' }, '오늘 고른 할 일이 없습니다.'));
  return ul;
}

function renderAddToToday() {
  const rest = S.todos.open.filter((t) => !S.active.picked.includes(t));
  const det = el('details', { class: 'add-today' }, el('summary', {}, `오늘 할 일에 추가 (${rest.length})`));
  const ul = el('ul', { class: 'list' });
  for (const t of rest) {
    ul.append(el('li', {}, el('span', {}, t), el('button', { class: 'btn small', onclick: () => todo('pick', t) }, '오늘로')));
  }
  if (!rest.length) ul.append(el('li', { class: 'muted' }, '남은 할 일이 없습니다.'));
  det.append(ul);
  return det;
}

const field = (label, input) => el('label', { class: 'field' }, el('span', { class: 'field-label' }, label), input);
const textarea = (name, value) => el('textarea', { name, rows: 3, value, placeholder: '줄마다 하나씩' });

function renderClockOutForm(c) {
  const d = S.active;
  const f = el('form', { class: 'clockout', onsubmit: doClockOut });
  f.append(
    el('h2', {}, `Day ${d.day} 퇴근 보고`),
    field('오늘을 한 줄로 (필수)', el('input', { name: 'title', type: 'text', required: true, value: d.summary || '', placeholder: '예: 출근부 v1 완성, 첫 퇴근' })),
    field('한 일', textarea('did', uniq([...d.did, ...d.done]).join('\n'))),
    field('배운 것', textarea('learned', d.learned.join('\n'))),
    field('막힌 것', textarea('blocked', d.blocked.join('\n'))),
    field('다음에 할 것', textarea('next', d.next.join('\n'))),
    el('p', { class: 'muted small-text' },
      '줄마다 하나씩 적으면 목록으로 저장됩니다.' + (S.config.autoPush ? ' 퇴근하면 데이터 폴더만 커밋·푸시됩니다.' : ''),
    ),
    el('div', { class: 'actions' },
      el('button', { type: 'button', class: 'btn', onclick: () => { view = 'idle'; renderWork(); } }, '취소'),
      el('button', { type: 'submit', class: 'btn primary big' }, '퇴근 도장 찍기'),
    ),
  );
  c.append(f);
  f.querySelector('input[name=title]').focus();
}

async function doClockOut(e) {
  e.preventDefault();
  const fd = new FormData(e.target);
  const body = Object.fromEntries(['title', 'did', 'learned', 'blocked', 'next'].map((k) => [k, fd.get(k) || '']));
  const btn = e.target.querySelector('button[type=submit]');
  btn.disabled = true;
  btn.textContent = '저장 중…';
  try {
    const r = await api('/api/clockout', body);
    view = 'idle';
    S = r.state;
    render();
    stamp('퇴근');
    const g = r.git || {};
    if (g.skipped) toast(`저장 완료 (git 건너뜀: ${g.reason})`);
    else if (!g.ok) toast(`저장은 됐지만 git 커밋 실패: ${g.error}`, true);
    else if (g.nothing) toast('저장 완료 · 커밋할 변경 없음');
    else if (g.pushed) toast('저장 · 커밋 · 푸시 완료');
    else toast(`저장 · 커밋 완료, 푸시 실패: ${g.error}`, true);
  } catch (err) {
    toast(err.message, true);
    btn.disabled = false;
    btn.textContent = '퇴근 도장 찍기';
  }
}

// ── 할 일 카드 ──
function renderTodos() {
  const ul = $('#todo-list');
  ul.innerHTML = '';
  const picked = new Set(S.active ? S.active.picked : []);
  for (const t of S.todos.open) {
    ul.append(el('li', { class: picked.has(t) ? 'is-picked' : '' },
      el('label', {},
        el('input', { type: 'checkbox', onchange: () => todo('done', t) }),
        el('span', {}, t),
      ),
      el('span', { class: 'li-actions' },
        S.active && !picked.has(t) ? el('button', { class: 'icon-btn', title: '오늘 할 일로', onclick: () => todo('pick', t) }, '+') : null,
        el('button', { class: 'icon-btn danger', title: '삭제', onclick: () => confirm(`삭제할까요?\n${t}`) && todo('remove', t) }, '×'),
      ),
    ));
  }
  if (!S.todos.open.length) ul.append(el('li', { class: 'muted' }, '할 일이 없습니다.'));

  const dl = $('#done-list');
  dl.innerHTML = '';
  for (const t of S.todos.done.slice(0, 15)) {
    dl.append(el('li', {}, el('label', {}, el('input', { type: 'checkbox', checked: true, onchange: () => todo('undone', t) }), el('span', {}, t))));
  }
  $('#done-count').textContent = S.todos.done.length ? `(${S.todos.done.length})` : '';
}

async function todo(action, text) {
  try {
    const r = await api('/api/todos', { action, text });
    S = r.state;
    render();
  } catch (e) {
    toast(e.message, true);
  }
}

$('#todo-form').addEventListener('submit', async (e) => {
  e.preventDefault();
  const input = $('#todo-input');
  const text = input.value.trim();
  if (!text) return;
  input.value = '';
  await todo('add', text);
  input.focus();
});

// ── 통계 ──
function computeStats() {
  const hours = hoursByDate();
  const today = parseDate(S.today);
  const weekStart = dateStr(addDays(today, -((today.getDay() + 6) % 7))); // 월요일
  const monthPrefix = S.today.slice(0, 7);
  let week = 0, month = 0, total = 0, days = 0;
  for (const [date, h] of Object.entries(hours)) {
    if (date >= weekStart && date <= S.today) week += h;
    if (date.startsWith(monthPrefix)) month += h;
    if (date >= S.config.seasonStart) { total += h; days += 1; }
  }
  // 연속 출근일: 오늘 기록이 아직 없으면 어제부터 거슬러 센다
  const has = new Set(S.days.map((d) => d.date));
  let streak = 0;
  let cur = has.has(S.today) ? today : addDays(today, -1);
  while (has.has(dateStr(cur))) { streak++; cur = addDays(cur, -1); }
  return { todayH: hours[S.today] || 0, week, month, total, days, streak };
}

function renderStats() {
  const s = computeStats();
  const tile = (label, value, sub) => el('div', { class: 'tile' }, el('div', { class: 'tile-value' }, value), el('div', { class: 'tile-label' }, label), sub ? el('div', { class: 'tile-sub muted' }, sub) : null);
  const row = $('#stats-row');
  row.innerHTML = '';
  row.append(
    tile('오늘', fmtHours(s.todayH)),
    tile('이번 주', fmtHours(s.week)),
    tile('이번 달', fmtHours(s.month)),
    tile('연속 출근', `${s.streak}일`),
    tile('시즌 누적', fmtHours(s.total), `${s.days}일 출근`),
  );
}

// ── 히트맵 ──
/** 색 단계: 기록이 없으면 0, 출근한 날은 1 + 넘은 경계 개수 (경계 4개면 5단계). 시간이 0이어도 최소 1 */
function level(h, hasRecord) {
  if (!hasRecord) return 0;
  return 1 + S.config.heatLevels.filter((t) => h >= t).length;
}

/**
 * 히트맵: 설정의 heatStart(시작 달)부터 앞으로 최소 53주. 왼쪽 = 시작, 오른쪽 = 미래.
 * 오늘이 그 범위를 넘어가면 오늘 주까지 열이 늘어난다(가로 스크롤). 아직 안 온 날은 흐린 빈 칸.
 */
function renderHeatmap() {
  const hours = hoursByDate();
  heatIndex = Object.fromEntries(S.days.map((d) => [d.date, d]));
  const CELL = 12, GAP = 3, STEP = CELL + GAP, LEFT = 26, TOP = 18;
  const monday = (d) => addDays(d, -((d.getDay() + 6) % 7));
  const today = parseDate(S.today);
  const heatStart = parseDate(S.config.heatStart);
  const start = monday(heatStart);                                   // 시작 달 첫 주의 월요일
  let windowEnd = new Date(heatStart.getFullYear() + 1, heatStart.getMonth(), heatStart.getDate() - 1); // 딱 1년 뒤 전날
  let end = monday(windowEnd);
  if (monday(today) > end) { end = monday(today); windowEnd = addDays(end, 6); } // 1년을 넘기면 오늘 주까지 늘림
  const WEEKS = Math.round((end - start) / (7 * 86400000)) + 1;
  const width = LEFT + WEEKS * STEP, height = TOP + 7 * STEP;
  const ym = (d) => `${d.getFullYear()}.${pad2(d.getMonth() + 1)}`;
  $('#season-label').textContent = `${ym(heatStart)} ~ ${ym(windowEnd)} · 시즌 시작 ${S.config.seasonStart}`;

  let svg = `<svg width="${width}" height="${height}" viewBox="0 0 ${width} ${height}" role="img" aria-label="출근 히트맵">`;
  for (const [name, r] of [['월', 0], ['수', 2], ['금', 4]]) {
    svg += `<text class="hm-label" x="0" y="${TOP + r * STEP + CELL - 2}">${name}</text>`;
  }
  for (let w = 0; w < WEEKS; w++) {
    const weekStart = addDays(start, w * 7);
    // 그 주에 1일이 들어 있으면 달 이름표. 첫 열은 시작일의 달로.
    let label = null;
    for (let r = 0; r < 7; r++) {
      const d = addDays(weekStart, r);
      if (d.getDate() === 1 && d >= heatStart && d <= windowEnd) { label = d.getMonth() + 1; break; }
    }
    if (w === 0 && label === null) label = heatStart.getMonth() + 1;
    if (label !== null) svg += `<text class="hm-label" x="${LEFT + w * STEP}" y="11">${label}월</text>`;

    for (let r = 0; r < 7; r++) {
      const d = addDays(weekStart, r);
      if (d < heatStart || d > windowEnd) continue; // 창 밖(시작 전·1년 뒤)은 빈자리로
      const ds = dateStr(d);
      const x = LEFT + w * STEP, y = TOP + r * STEP;
      if (ds > S.today) { // 미래: 흐린 빈 칸, 마우스 반응 없음
        svg += `<rect class="hm-cell lvl-0 future" x="${x}" y="${y}" width="${CELL}" height="${CELL}" rx="2"></rect>`;
        continue;
      }
      const lvl = level(hours[ds] || 0, !!heatIndex[ds]);
      const todayCls = ds === S.today ? ' today' : ''; // 오늘 칸은 테두리로 표시
      svg += `<rect class="hm-cell lvl-${lvl}${todayCls}" x="${x}" y="${y}" width="${CELL}" height="${CELL}" rx="2" data-date="${ds}" tabindex="0" aria-label="${ds}"></rect>`;
    }
  }
  svg += '</svg>';
  $('#heatmap').innerHTML = svg;
}

/** 범례: 적게 [한 날의 단계 색들] 많이 — 글자 없이 색만. 경계 시간은 마우스를 올렸을 때 title 로만 */
function renderLegend() {
  const th = S.config.heatLevels;
  const lg = $('#legend');
  lg.innerHTML = '';
  lg.append(el('span', { class: 'muted' }, '적게'));
  for (let i = 1; i <= th.length + 1; i++) {
    const lo = th[i - 2], hi = th[i - 1];
    const name = i === 1 ? `${hi}h 미만` : hi === undefined ? `${lo}h 이상` : `${lo}~${hi}h`;
    lg.append(el('span', { class: `swatch lvl-${i}`, title: name }));
  }
  lg.append(el('span', { class: 'muted' }, '많이'));
}

function tooltipHtml(ds) {
  const d = heatIndex[ds];
  const h = hoursByDate()[ds] || 0;
  const year = ds.slice(0, 4) !== S.today.slice(0, 4) ? `${ds.slice(0, 4)}년 ` : ''; // 작년 칸은 연도도 표시
  const head = `<div class="tt-date">${year}${fmtDate(ds)}${d ? ` · Day ${d.day}` : ''}</div>`;
  if (!d) return head + '<div class="muted">출근 기록 없음</div>';
  const items = (d.did.length ? d.did : d.done).slice(0, 6);
  return (
    head +
    `<div class="tt-title">${esc(d.summary || '(퇴근 전)')}</div>` +
    `<div class="tt-hours">${fmtHours(h)}${d.status === 'open' ? ' · 출근 중' : ''}</div>` +
    (items.length ? `<ul>${items.map((i) => `<li>${esc(i)}</li>`).join('')}</ul>` : '')
  );
}

function showTooltip(target) {
  const ds = target.dataset.date;
  const tt = $('#tooltip');
  tt.innerHTML = tooltipHtml(ds);
  tt.hidden = false;
  const r = target.getBoundingClientRect();
  const pad = 8;
  let x = r.left + r.width / 2 - tt.offsetWidth / 2;
  let y = r.top - tt.offsetHeight - pad;
  x = Math.max(pad, Math.min(x, window.innerWidth - tt.offsetWidth - pad));
  if (y < pad) y = r.bottom + pad;
  tt.style.left = `${x}px`;
  tt.style.top = `${y}px`;
}
const hideTooltip = () => { $('#tooltip').hidden = true; };

const heat = $('#heatmap');
heat.addEventListener('pointerover', (e) => { const c = e.target.closest('.hm-cell'); if (c && c.dataset.date) showTooltip(c); });
heat.addEventListener('pointerout', (e) => { if (e.target.closest('.hm-cell')) hideTooltip(); });
heat.addEventListener('focusin', (e) => { const c = e.target.closest('.hm-cell'); if (c && c.dataset.date) showTooltip(c); });
heat.addEventListener('focusout', hideTooltip);
heat.addEventListener('click', (e) => { const c = e.target.closest('.hm-cell'); if (c && heatIndex[c.dataset.date]) openDevlog(c.dataset.date); });
heat.addEventListener('keydown', (e) => { const c = e.target.closest('.hm-cell'); if (c && e.key === 'Enter' && heatIndex[c.dataset.date]) openDevlog(c.dataset.date); });
$('.heatmap-wrap').addEventListener('scroll', hideTooltip);

// ── 지난 데브로그 ──
function renderHistory() {
  const ul = $('#history-list');
  ul.innerHTML = '';
  const days = [...S.days].sort((a, b) => (a.date < b.date ? 1 : -1)).slice(0, 40);
  for (const d of days) {
    ul.append(el('li', {},
      el('button', { class: 'history-item', onclick: () => openDevlog(d.date) },
        el('span', { class: 'h-date' }, `${d.date.slice(0, 4) === S.today.slice(0, 4) ? d.date.slice(5) : d.date} (${WEEKDAYS[parseDate(d.date).getDay()]})`),
        el('span', { class: 'pill small' }, `Day ${d.day}`),
        el('span', { class: 'h-hours' }, d.status === 'open' ? '출근 중' : fmtHours(d.hours)),
        el('span', { class: 'h-title' }, d.summary || '(퇴근 전)'),
      ),
    ));
  }
  if (!days.length) ul.append(el('li', { class: 'muted' }, '아직 데브로그가 없습니다. 첫 출근을 해보세요.'));
}

// ── 데브로그 보기 (모달) ──
async function openDevlog(date) {
  try {
    const { text } = await api(`/api/devlog/${date}`);
    const d = heatIndex[date] || S.days.find((x) => x.date === date);
    const body = $('#modal-body');
    body.innerHTML = '';
    body.append(
      el('div', { class: 'modal-meta muted' }, `${date.slice(0, 4)}년 ${fmtDate(date)}${d ? ` · Day ${d.day} · ${fmtHours(d.hours)} · ${d.sessions.join(', ')}` : ''}`),
      el('h2', { class: 'modal-title' }, d ? d.title : date),
    );
    const content = el('div', { class: 'md' });
    content.innerHTML = renderMarkdown(text);
    body.append(content, el('p', { class: 'muted small-text' }, `파일: production/desk/devlog/${date}.md`));
    $('#modal').hidden = false;
  } catch (e) {
    toast(e.message, true);
  }
}

$('#modal').addEventListener('click', (e) => { if (e.target.closest('[data-close]')) $('#modal').hidden = true; });
document.addEventListener('keydown', (e) => { if (e.key === 'Escape') $('#modal').hidden = true; });

/** 아주 작은 마크다운 변환기 (제목·목록·문단·굵게·코드·링크만) */
function renderMarkdown(text) {
  let body = text;
  if (text.startsWith('---')) {
    const end = text.indexOf('\n---', 3);
    if (end > 0) body = text.slice(end + 4);
  }
  const inline = (s) => esc(s)
    .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
    .replace(/`([^`]+)`/g, '<code>$1</code>')
    .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>');
  let html = '', inList = false, para = [];
  const flushPara = () => { if (para.length) { html += `<p>${inline(para.join(' '))}</p>`; para = []; } };
  const closeList = () => { if (inList) { html += '</ul>'; inList = false; } };
  for (const raw of body.split(/\r?\n/)) {
    const line = raw.trimEnd();
    let m;
    if ((m = line.match(/^(#{1,3})\s+(.*)$/))) { flushPara(); closeList(); const lv = m[1].length + 1; html += `<h${lv}>${inline(m[2])}</h${lv}>`; continue; }
    if ((m = line.match(/^\s*[-*]\s+(.*)$/))) { flushPara(); if (!inList) { html += '<ul>'; inList = true; } html += `<li>${inline(m[1])}</li>`; continue; }
    if (!line.trim()) { flushPara(); closeList(); continue; }
    para.push(line);
  }
  flushPara();
  closeList();
  return html;
}

// ── 도장 · 토스트 · 타이머 ──
function stamp(text) {
  const s = $('#stamp');
  $('#stamp-text').textContent = text;
  s.hidden = false;
  s.classList.remove('play');
  void s.offsetWidth; // 애니메이션 재시작용
  s.classList.add('play');
  setTimeout(() => { s.hidden = true; }, 1500);
}

let toastTimer = null;
function toast(msg, isError = false) {
  const t = $('#toast');
  t.textContent = msg;
  t.classList.toggle('error', isError);
  t.hidden = false;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { t.hidden = true; }, isError ? 7000 : 3500);
}

let tick = 0;
setInterval(() => {
  if (!S || !S.active) return;
  const e = $('#elapsed');
  if (e) e.textContent = fmtDuration(elapsedMinutes());
  if (++tick % 60 === 0) { renderStats(); renderHeatmap(); }
}, 1000);

load().catch((e) => toast('서버에 연결할 수 없습니다: ' + e.message, true));
