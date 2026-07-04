using System;
using System.IO;
using UnityEngine;

namespace MawangHR
{
    [Serializable]
    public class LayoutRect { public float x, y, w, h; }

    /// 이력서 템플릿 레이아웃 명세 — 좌표는 전부 **템플릿 PNG 픽셀 기준** (눈대중 금지 원칙).
    /// 픽셀 분석으로 측정한 값을 resume_template.layout.json에 저장하고, 코드는 배율만 곱해 쓴다.
    /// 템플릿 교체 = 이미지 + 이 JSON만 갱신 (코드 무수정). 파일이 없으면 아래 기본값 사용.
    [Serializable]
    public class ResumeLayout
    {
        public float templateW = 1054f;
        public float templateH = 1492f;
        public LayoutRect title = new LayoutRect { x = 0, y = 150, w = 1054, h = 72 };
        public LayoutRect portrait = new LayoutRect { x = 152, y = 294, w = 270, h = 314 };
        public float[] infoRowYs = { 342, 412, 488, 565 }; // 정보란 점선 y (이름/종족/직무/연봉)
        public float infoLabelX = 528;
        public float infoValueX = 660;
        public float infoRightX = 930;
        public LayoutRect quote = new LayoutRect { x = 475, y = 582, w = 465, h = 66 };
        public LayoutRect banner1 = new LayoutRect { x = 132, y = 680, w = 204, h = 49 };
        public LayoutRect banner2 = new LayoutRect { x = 115, y = 983, w = 266, h = 49 };
        public float hintX = 356;
        public float rowX = 150, rowW = 760, rowH = 56;        // 이력 줄 상자
        public float[] clueLineYs = { 798, 858, 942 };          // 이력 상자의 점선 y (글자가 그 위에 앉음)
        public LayoutRect special = new LayoutRect { x = 150, y = 1000, w = 760, h = 94 }; // 하단 정렬 → 점선(1096) 위에 앉음
        public float stmtStartY = 1040, stmtStep = 108, stmtRowH = 96; // 면접 진술 기록 흐름

        public static ResumeLayout LoadOrDefault()
        {
            try
            {
                string p = Path.Combine(Application.streamingAssetsPath, "MawangHR/resume_template.layout.json");
                if (File.Exists(p))
                {
                    var l = JsonUtility.FromJson<ResumeLayout>(File.ReadAllText(p));
                    if (l != null) return l;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MawangHR] 레이아웃 파일 로드 실패 — 기본값 사용: " + e.Message);
            }
            return new ResumeLayout();
        }
    }

    // 콘텐츠는 전부 StreamingAssets/MawangHR/gamedata.json 에서 로드 (JsonUtility 규격).
    // 지원자·JD·일일지침을 여기서 늘리면 코드 수정 없이 콘텐츠가 는다.

    [Serializable]
    public class GameData
    {
        public string version;
        public Jd[] jds;
        public Applicant[] applicants;    // 서류 심사 케이스 풀 (시트 파이프라인 대상)
        public Applicant[] interviewees;  // 면접 케이스 풀 (단계 독립 세트 — 시트 대상 아님, answers 필수)
        public QuestionCard[] cards;      // 면접 질문 카드
        public DayConfig[] days;
        public SchedulingData scheduling; // Day 1 후반 업무 — 면접 일정 잡기 (단계 독립 지원자 세트)
    }

    /// 면접 질문 카드 — 책상 위 3D 카드로 잡아 지원자에게 던지면 발동.
    [Serializable]
    public class QuestionCard
    {
        public string id;      // 예: basic / terms / motive / trap
        public string label;   // 예: "기본 질문"
        public string prompt;  // 면접관이 실제로 묻는 문장
        public int cost;       // 질문 포인트 소모 (★ 개수)
    }

    /// 면접 답변 — 카드 하나당 하나. 답변도 마킹 가능한 단서다.
    [Serializable]
    public class Answer
    {
        public string cardId;   // 어느 카드에 대한 답인가
        public string text;     // 답변 대사
        public string tell;     // 반응 지시문 (예: "…목소리가 작아진다") — 없으면 ""
        public string evidence; // "" | PASS | FAIL (resumeLines와 같은 규칙)
    }

    [Serializable]
    public class SchedulingData
    {
        public string intro;            // 인턴의 업무 인계 대사
        public SchedSlot[] slots;       // 캘린더 슬롯 (2일 × 오전/오후/밤)
        public SchedCandidate[] candidates; // 통과자 명단 (서류 단계와 독립)
    }

    [Serializable]
    public class SchedSlot
    {
        public string label;   // 예: "내일 오전"
        public string warn;    // 슬롯 경고 표시 (예: "비 예보", "보름달") — 없으면 ""
        public string[] tags;  // 판정용 태그 (day2/day3, am/pm/night, rain/fullmoon …)
    }

    [Serializable]
    public class SchedCandidate
    {
        public string id;
        public string name;
        public string species;
        public string hint;          // 사진 카드에 적힌 힌트 (이력서 특이사항급 — 일부만 누설)
        public string callLine;      // 통화 대사 (제약의 진짜 출처 + 코미디)
        public string requiredTag;   // 이 태그가 있는 슬롯에만 가능 ("" = 없음)
        public string bannedTag;     // 이 태그가 있는 슬롯 금지 ("" = 없음)
        public string violationLine; // 위반 시 다음날 아침 사고 보고서 문구
    }

    [Serializable]
    public class Jd
    {
        public string id;
        public string title;      // 예: "일반 행정 (하급)"
        public string[] required; // 요구 자질
        public string[] banned;   // 결격 사유
        public string note;       // 직무 한 줄 설명
    }

    /// 이력서의 한 줄 = 클릭 가능한 단서.
    /// evidence: "" = 근거 아님 / "PASS" = 합격의 결정적 근거 / "FAIL" = 탈락의 결정적 근거
    [Serializable]
    public class ResumeLine
    {
        public string text;
        public string evidence;
    }

    [Serializable]
    public class Applicant
    {
        public string id;
        public string name;
        public string species;        // 종족
        public string jdId;           // 지원한 JD
        public string salary;         // 희망 연봉 (표기용)
        public string quote;          // 한 줄 어필
        public ResumeLine[] resumeLines; // 이력서 본문 (클릭 가능한 단서) — ⚠️ 레이아웃 한계 최대 3줄 (4줄부터 특이사항 영역과 겹침)
        public string special;        // 특이사항 (인사팀 메모 — 이것도 단서)
        public string specialEvidence;// 특이사항의 근거 여부 ("", "PASS", "FAIL")
        public string correct;        // 정답: "PASS" | "FAIL"
        public string reveal;         // 결산 해설 (정답 근거 + 코미디)
        public Answer[] answers;      // 면접 단계 전용 — 질문 카드별 답변 (서류 단계는 비움)

        public bool CorrectIsPass => correct == "PASS";
    }

    [Serializable]
    public class DayConfig
    {
        public int day;
        public string title;       // 예: "Day 1 — 서류 심사 (수습)"
        public string directive;   // 오늘의 공문 전문
        public string phase;       // "" = 서류 심사 / "interview" = 1차 면접
        public int meritGoal;      // 승진에 필요한 공적 원점수 (정확 ×2 + 근거 적중 ×1) — 게이지 100% 지점
        public int drawCount;      // 풀에서 매 판 뽑을 서류 수 (0 = 뽑기 없이 명단 순서 그대로)
        public string firstId;     // 첫 슬롯 고정 케이스 — 미출현일 때만 고정 (반전 튜토리얼 보장)
        public int questionPoints; // 면접 전용 — 면접당 질문 포인트 (카드 ★ 소모)
        public string[] applicantIds; // 케이스 풀 전체 (drawCount > 0이면 여기서 뽑는다. 면접 day는 interviewees에서 찾음)
    }

    /// 로드 직후 데이터 무결성 검사 + null 정규화.
    /// JsonUtility는 JSON에 없는 배열/문자열을 null로 두므로, 여기서 잡지 않으면
    /// 플레이 도중 NRE로 화면이 멈춘다(소프트락). 치명 문제 = 에러 반환, 경미 = 경고 로그.
    public static class GameDataValidator
    {
        /// 통과하면 null, 치명 문제면 에러 메시지 반환.
        public static string Validate(GameData d)
        {
            if (d == null) return "파싱 결과가 비었습니다";
            if (d.jds == null || d.jds.Length == 0) return "jds가 비었습니다";
            if (d.applicants == null || d.applicants.Length == 0) return "applicants가 비었습니다";
            if (d.days == null || d.days.Length == 0) return "days가 비었습니다";

            foreach (var j in d.jds)
            {
                if (j.required == null) j.required = new string[0];
                if (j.banned == null) j.banned = new string[0];
            }

            // 지원자 공통 검사 + null 정규화 (서류·면접 풀 공용)
            string CheckApplicant(Applicant a, string poolName)
            {
                if (a.resumeLines == null || a.resumeLines.Length == 0)
                    return $"{poolName} '{a.id}': resumeLines가 비었습니다";
                if (a.resumeLines.Length > 3)
                    Debug.LogWarning($"[MawangHR] {poolName} '{a.id}': 이력 {a.resumeLines.Length}줄 — 레이아웃 한계(3줄) 초과, 특이사항과 겹칩니다");
                if (a.correct != "PASS" && a.correct != "FAIL")
                    return $"{poolName} '{a.id}': correct는 PASS/FAIL이어야 합니다 (현재 '{a.correct}')";
                foreach (var l in a.resumeLines)
                    if (l.evidence == null) l.evidence = "";
                if (a.special == null) a.special = "";
                if (a.specialEvidence == null) a.specialEvidence = "";
                if (a.answers == null) a.answers = new Answer[0];
                foreach (var ans in a.answers)
                {
                    if (ans.tell == null) ans.tell = "";
                    if (ans.evidence == null) ans.evidence = "";
                    if (ans.evidence != "" && ans.evidence != a.correct)
                        return $"{poolName} '{a.id}': 답변({ans.cardId}) evidence는 빈칸 또는 정답({a.correct})이어야 합니다";
                }
                return null;
            }

            foreach (var a in d.applicants)
            {
                string err = CheckApplicant(a, "지원자");
                if (err != null) return err;
            }

            // ─ 면접 데이터 (phase == "interview"인 day가 있을 때만 필수) ─
            bool hasInterview = false;
            foreach (var day in d.days)
            {
                if (day.phase == null) day.phase = "";
                if (day.phase == "interview") hasInterview = true;
            }
            if (d.interviewees == null) d.interviewees = new Applicant[0];
            if (d.cards == null) d.cards = new QuestionCard[0];
            if (hasInterview)
            {
                if (d.interviewees.Length == 0) return "면접 day가 있는데 interviewees가 비었습니다";
                if (d.cards.Length == 0) return "면접 day가 있는데 cards(질문 카드)가 비었습니다";
                foreach (var c in d.cards)
                    if (c.cost < 1) return $"질문 카드 '{c.id}': cost는 1 이상이어야 합니다";
                foreach (var a in d.interviewees)
                {
                    string err = CheckApplicant(a, "면접자");
                    if (err != null) return err;
                    foreach (var c in d.cards)
                    {
                        bool found = false;
                        foreach (var ans in a.answers) if (ans.cardId == c.id) { found = true; break; }
                        if (!found) return $"면접자 '{a.id}': 카드 '{c.id}'에 대한 답변이 없습니다";
                    }
                    // 결정적 근거(정답 방향)가 이력·답변·특이사항 어딘가엔 있어야 함
                    bool hasKey = a.specialEvidence == a.correct;
                    foreach (var l in a.resumeLines) if (l.evidence == a.correct) hasKey = true;
                    foreach (var ans in a.answers) if (ans.evidence == a.correct) hasKey = true;
                    if (!hasKey) return $"면접자 '{a.id}': 결정적 근거(정답 방향 표시)가 하나도 없습니다";
                }
            }

            foreach (var day in d.days)
            {
                if (day.applicantIds == null || day.applicantIds.Length == 0)
                    return $"Day {day.day}: applicantIds가 비었습니다";
                if (day.meritGoal < 1)
                    return $"Day {day.day}: meritGoal(승진 공적 기준)이 없거나 0입니다";
                if (day.firstId == null) day.firstId = "";
                if (day.phase == "interview" && day.questionPoints < 1)
                    return $"Day {day.day}: questionPoints(질문 포인트)가 없거나 0입니다";
                if (day.drawCount > day.applicantIds.Length)
                    Debug.LogWarning($"[MawangHR] Day {day.day}: drawCount({day.drawCount})가 풀({day.applicantIds.Length}건)보다 큼 — 전원 등장");
                if (!string.IsNullOrEmpty(day.firstId) && Array.IndexOf(day.applicantIds, day.firstId) < 0)
                    Debug.LogWarning($"[MawangHR] Day {day.day}: firstId '{day.firstId}'가 풀에 없음 — 고정 슬롯 무시됨");
                int lineupSize = day.drawCount > 0 ? Math.Min(day.drawCount, day.applicantIds.Length) : day.applicantIds.Length;
                if (day.meritGoal > lineupSize * 3)
                    return $"Day {day.day}: meritGoal({day.meritGoal})이 하루 최대 공적({lineupSize * 3})보다 큼 — 승진 불가능";

                // 풀 id 실존 확인 (없으면 조용히 제외되므로 경고)
                var source = day.phase == "interview" ? d.interviewees : d.applicants;
                foreach (var id in day.applicantIds)
                {
                    bool found = false;
                    foreach (var a in source) if (a.id == id) { found = true; break; }
                    if (!found) Debug.LogWarning($"[MawangHR] Day {day.day}: '{id}'가 풀에 없음 — 등장에서 제외됩니다");
                }
            }

            // scheduling은 선택 블록 — 있으면 내부 배열만 정규화
            if (d.scheduling != null && d.scheduling.candidates != null && d.scheduling.candidates.Length > 0)
            {
                if (d.scheduling.slots == null || d.scheduling.slots.Length == 0)
                    return "scheduling.slots가 비었습니다 (candidates만 있고 슬롯이 없음)";
                foreach (var s in d.scheduling.slots)
                    if (s.tags == null) s.tags = new string[0];
                foreach (var c in d.scheduling.candidates)
                {
                    if (c.requiredTag == null) c.requiredTag = "";
                    if (c.bannedTag == null) c.bannedTag = "";
                }
            }
            return null;
        }
    }
}
