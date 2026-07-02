using System.Collections.Generic;
using UnityEngine;

namespace DDworld.CombatTest
{
    /// <summary>
    /// combat_test 데이터 라이브러리.
    /// - 아군 병종 + 몬스터 UnitData를 런타임 생성 (씬/에셋 세팅 불필요)
    /// - 인카운터 진형 프리셋 E0~E3
    /// - 몬스터 색 레지스트리 (BattleTile이 조회)
    ///
    /// 좌표 규약: GameManager.EnemyPlacements와 동일 — key = (row 0~4, col 0~4) 적 로컬.
    /// col 4 = 전선(그리드 J열), col 0 = 적 후방(그리드 N열).
    ///
    /// 몬스터는 아군의 거울이 아님 (상성표 없음 — pve-pivot Q19/Q25).
    /// 유불리 = 아군 공격 패턴 × 몬스터 진형/특성.
    /// </summary>
    public static class EncounterLibrary
    {
        // ─────────────────────────────────────────────────────
        // 아군 병종 (원본 ScriptableObject 에셋과 동일 스탯 — 코드 생성)
        // rpsType은 상성 배수가 아니라 "패턴 태그"로만 쓰임
        // (창병 Scissors = 팔랑크스 진형·대기병 방어 / 기병 Rock = 쐐기 진형·돌격)
        // ─────────────────────────────────────────────────────
        private static UnitData militia, spearman, archer, cavalry, trap;

        // ─────────────────────────────────────────────────────
        // 몬스터 (튜닝은 아래 EnsureUnits 숫자만)
        // moveEveryTick: 클수록 느림 (속도 = base 2 / tick). 0 = 고정.
        // ─────────────────────────────────────────────────────
        private static UnitData goblin;    // 약한 물량 — 쉬운 초반 적
        private static UnitData wolf;      // 빠른 돌격 물량
        private static UnitData skeleton;  // 고정 원거리 (아군 궁병 1.5보다 긴 사거리)
        private static UnitData ogre;      // 느리고 단단한 벽

        // 몬스터 색 (아군은 카테고리 기본색 사용)
        private static readonly Dictionary<UnitData, Color> customColors = new();

        public struct EncounterInfo
        {
            public string name;
            public string desc;
        }

        public static readonly EncounterInfo[] Encounters =
        {
            new EncounterInfo { name = "E0 — 고블린 무리",  desc = "성 근처의 약한 무리. 가볍게 쓸어보세요." },
            new EncounterInfo { name = "E1 — 껍질 깨기",    desc = "오우거 벽 뒤에 해골 궁수. 뚫을까, 돌아갈까?" },
            new EncounterInfo { name = "E2 — 비대칭 물량",  desc = "위쪽에 몰린 늑대 떼. 정면 대응 vs 우회." },
            new EncounterInfo { name = "E3 — 혼성 진형",    desc = "물량 + 벽 + 원거리. 배운 걸 전부 시험한다." },
        };

        public static int Count => Encounters.Length;

        /// <summary>몬스터 커스텀 색 조회 (BattleTile에서 사용)</summary>
        public static bool TryGetColor(UnitData data, out Color color)
        {
            EnsureUnits();
            return customColors.TryGetValue(data, out color);
        }

        /// <summary>시작 보유 로스터 생성 (병종별 장수)</summary>
        public static List<CardData> BuildStartingRoster(
            int militiaN, int spearmanN, int archerN, int cavalryN, int trapN)
        {
            EnsureUnits();
            var cards = new List<CardData>();
            int id = 0;
            for (int i = 0; i < militiaN; i++)  cards.Add(new CardData(militia, id++));
            for (int i = 0; i < spearmanN; i++) cards.Add(new CardData(spearman, id++));
            for (int i = 0; i < archerN; i++)   cards.Add(new CardData(archer, id++));
            for (int i = 0; i < cavalryN; i++)  cards.Add(new CardData(cavalry, id++));
            for (int i = 0; i < trapN; i++)     cards.Add(new CardData(trap, id++));
            return cards;
        }

        /// <summary>인카운터 진형 생성 (row, col — 적 로컬 좌표)</summary>
        public static Dictionary<Vector2Int, CardData> BuildFormation(int index)
        {
            EnsureUnits();
            var f = new Dictionary<Vector2Int, CardData>();
            int id = 1000; // 플레이어 카드 id와 겹치지 않게

            switch (Mathf.Clamp(index, 0, Count - 1))
            {
                case 0: // E0 고블린 무리 — 산개한 3분대, 대충 배치해도 이김
                    f[new Vector2Int(1, 4)] = new CardData(goblin, id++);
                    f[new Vector2Int(3, 3)] = new CardData(goblin, id++);
                    f[new Vector2Int(2, 2)] = new CardData(goblin, id++);
                    break;

                case 1: // E1 껍질 깨기 — 오우거 전선 벽 + 해골 궁수 후방 + 틈새 고블린
                    f[new Vector2Int(1, 4)] = new CardData(ogre, id++);
                    f[new Vector2Int(3, 4)] = new CardData(ogre, id++);
                    f[new Vector2Int(2, 3)] = new CardData(goblin, id++);
                    f[new Vector2Int(1, 1)] = new CardData(skeleton, id++);
                    f[new Vector2Int(3, 1)] = new CardData(skeleton, id++);
                    break;

                case 2: // E2 비대칭 물량 — 위쪽(1~2행) 늑대 집중 + 아래쪽 오우거 단독
                    f[new Vector2Int(0, 4)] = new CardData(wolf, id++);
                    f[new Vector2Int(1, 4)] = new CardData(wolf, id++);
                    f[new Vector2Int(0, 3)] = new CardData(wolf, id++);
                    f[new Vector2Int(1, 3)] = new CardData(wolf, id++);
                    f[new Vector2Int(4, 4)] = new CardData(ogre, id++);
                    break;

                case 3: // E3 혼성 — 늑대 전면 + 오우거 중앙 + 해골 궁수 코너
                    f[new Vector2Int(0, 4)] = new CardData(wolf, id++);
                    f[new Vector2Int(4, 4)] = new CardData(wolf, id++);
                    f[new Vector2Int(2, 3)] = new CardData(ogre, id++);
                    f[new Vector2Int(0, 1)] = new CardData(skeleton, id++);
                    f[new Vector2Int(4, 1)] = new CardData(skeleton, id++);
                    break;
            }
            return f;
        }

        private static void EnsureUnits()
        {
            if (militia != null) return;

            // ── 아군 (원본 에셋 스탯 그대로) ──
            //                이름      분류                    rps               수  HP  공  move 사거리 shoot 함정
            militia  = Make("민병",   UnitCategory.Melee,   RpsType.None,     20, 10,  4, 2, 0f,   0, 0);
            spearman = Make("창병",   UnitCategory.Melee,   RpsType.Scissors, 10, 22,  8, 3, 0f,   0, 0);
            archer   = Make("궁병",   UnitCategory.Ranged,  RpsType.Paper,    12, 10,  6, 4, 1.5f, 3, 0);
            cavalry  = Make("기병",   UnitCategory.Cavalry, RpsType.Rock,      5, 28, 12, 1, 0f,   0, 0);
            trap     = Make("함정",   UnitCategory.Special, RpsType.None,      1,  1,  0, 0, 0f,   0, 25);

            // ── 몬스터 ──
            goblin   = Make("고블린",     UnitCategory.Melee,  RpsType.None, 12,   5,  2, 2, 0f,   0, 0);
            wolf     = Make("늑대 떼",    UnitCategory.Melee,  RpsType.None,  8,   9,  5, 1, 0f,   0, 0);
            skeleton = Make("해골 궁수",  UnitCategory.Ranged, RpsType.None,  4,   8,  8, 0, 3.5f, 3, 0);
            ogre     = Make("오우거",     UnitCategory.Melee,  RpsType.None,  2, 100, 14, 5, 0f,   0, 0);

            customColors[goblin]   = new Color(0.45f, 0.75f, 0.20f); // 황록
            customColors[wolf]     = new Color(0.55f, 0.55f, 0.62f); // 회색
            customColors[skeleton] = new Color(0.88f, 0.87f, 0.76f); // 뼈색
            customColors[ogre]     = new Color(0.55f, 0.30f, 0.15f); // 갈색
        }

        private static UnitData Make(string name, UnitCategory cat, RpsType rps,
            int count, int hp, int atk, int moveTick, float range, int shootTick, int trapDmg)
        {
            var u = ScriptableObject.CreateInstance<UnitData>();
            u.unitName = name;
            u.category = cat;
            u.rpsType = rps;
            u.soldierCount = count;
            u.soldierHP = hp;
            u.attack = atk;
            u.moveEveryTick = moveTick;
            u.attackRange = range;
            u.shootEveryTick = shootTick;
            u.trapDamage = trapDmg;
            return u;
        }
    }
}
