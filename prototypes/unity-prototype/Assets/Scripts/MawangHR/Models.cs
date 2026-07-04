using System;
using UnityEngine;

namespace MawangHR
{
    // 콘텐츠는 전부 StreamingAssets/MawangHR/gamedata.json 에서 로드 (JsonUtility 규격).
    // 지원자·JD·일일지침을 여기서 늘리면 코드 수정 없이 콘텐츠가 는다.

    [Serializable]
    public class GameData
    {
        public string version;
        public Jd[] jds;
        public Applicant[] applicants;
        public DayConfig[] days;
        public SchedulingData scheduling; // Day 1 후반 업무 — 면접 일정 잡기 (단계 독립 지원자 세트)
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

        public bool CorrectIsPass => correct == "PASS";
    }

    [Serializable]
    public class DayConfig
    {
        public int day;
        public string title;       // 예: "Day 1 — 서류 심사 (수습)"
        public string directive;   // 오늘의 공문 전문
        public int quotaMin;       // 최소 서류통과 인원
        public int promoteMin;     // 승진(면접 루프 해금)에 필요한 최소 정답 수
        public string[] applicantIds;
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

            foreach (var a in d.applicants)
            {
                if (a.resumeLines == null || a.resumeLines.Length == 0)
                    return $"지원자 '{a.id}': resumeLines가 비었습니다";
                if (a.resumeLines.Length > 3)
                    Debug.LogWarning($"[MawangHR] 지원자 '{a.id}': 이력 {a.resumeLines.Length}줄 — 레이아웃 한계(3줄) 초과, 특이사항과 겹칩니다");
                if (a.correct != "PASS" && a.correct != "FAIL")
                    return $"지원자 '{a.id}': correct는 PASS/FAIL이어야 합니다 (현재 '{a.correct}')";
                foreach (var l in a.resumeLines)
                    if (l.evidence == null) l.evidence = "";
                if (a.special == null) a.special = "";
                if (a.specialEvidence == null) a.specialEvidence = "";
            }

            foreach (var day in d.days)
            {
                if (day.applicantIds == null || day.applicantIds.Length == 0)
                    return $"Day {day.day}: applicantIds가 비었습니다";
                if (day.promoteMin < 1)
                    return $"Day {day.day}: promoteMin(승진 필요 정답 수)이 없거나 0입니다";
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
