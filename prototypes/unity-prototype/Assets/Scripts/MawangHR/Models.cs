using System;

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
        public ResumeLine[] resumeLines; // 이력서 본문 (클릭 가능한 단서)
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
        public string[] applicantIds;
    }
}
