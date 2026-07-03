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
