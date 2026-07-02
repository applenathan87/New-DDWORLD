using TMPro;
using UnityEngine;

namespace DDworld.CombatTest
{
    /// <summary>
    /// combat_test 부트스트랩 — 빈 씬에서 빈 GameObject에 이것 하나 붙이고 Play.
    /// 카메라·조명·전장·UI·게임매니저를 전부 코드로 생성한다 (씬 세팅 불필요).
    ///
    /// 원본 PvP 프로토(SampleScene)는 건드리지 않고, 같은 클래스들의 네임스페이스
    /// 사본(DDworld.CombatTest)으로 PvE를 돌린다.
    /// </summary>
    public class CombatTestBootstrap : MonoBehaviour
    {
        [Header("보유 로스터 구성 (병종별 장수)")]
        public int militiaCount = 3;
        public int spearmanCount = 2;
        public int archerCount = 2;
        public int cavalryCount = 2;
        public int trapCount = 1;

        [Header("한글 폰트 (비우면 에디터에서 자동 로드 시도)")]
        public TMP_FontAsset koreanFont;

        private void Awake()
        {
#if UNITY_EDITOR
            if (koreanFont == null)
                koreanFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    "Assets/Fonts/MonaS12 SDF.asset");
#endif
            EnsureCameraAndLight();

            // 생성 순서: 필드/UI 먼저 (Awake에서 Instance 등록) → GameManager 마지막 (Start에서 게임 시작)
            var bf = new GameObject("BattleField_CT").AddComponent<BattleField>();
            bf.koreanFont = koreanFont;

            var hand = new GameObject("Hand3D_CT").AddComponent<Hand3D>();
            hand.koreanFont = koreanFont;

            var ui = new GameObject("PlacementUI_CT").AddComponent<PlacementUI>();
            ui.koreanFont = koreanFont;

            var gm = new GameObject("GameManager_CT").AddComponent<GameManager>();
            gm.militiaCount = militiaCount;
            gm.spearmanCount = spearmanCount;
            gm.archerCount = archerCount;
            gm.cavalryCount = cavalryCount;
            gm.trapCount = trapCount;
        }

        private void EnsureCameraAndLight()
        {
            if (Camera.main == null)
            {
                var camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            if (FindAnyObjectByType<Light>() == null)
            {
                var lightObj = new GameObject("Directional Light");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }
    }
}
