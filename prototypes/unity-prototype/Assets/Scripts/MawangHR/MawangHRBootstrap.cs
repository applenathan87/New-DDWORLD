using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace MawangHR
{
    /// 마왕성 인사팀 프로토 — 씬 진입점.
    /// 씬에는 이 컴포넌트 하나뿐, 나머지(UI·이벤트시스템)는 전부 코드로 생성한다.
    /// 실행: mawanghr.unity 열고 Play.
    public class MawangHRBootstrap : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset koreanFont; // 씬에서 MonaS12 SDF 연결. 없으면 OS 폰트 폴백.

        private void Start()
        {
            UiKit.Init(koreanFont != null ? koreanFont : CreateOsFallbackFont());
            Sfx.Init();
            CreateEventSystem();

            var canvas = UiKit.MakeCanvas();

            GameData data;
            string path = Path.Combine(Application.streamingAssetsPath, "MawangHR/gamedata.json");
            try
            {
                data = JsonUtility.FromJson<GameData>(File.ReadAllText(path));
            }
            catch (System.Exception e)
            {
                ShowFatal(canvas, "데이터 로드 실패:\n" + path + "\n\n" + e.Message);
                return;
            }
            // 누락된 배열/필드는 여기서 한 번에 잡는다 (진행 중 NRE 소프트락 예방)
            string dataError = GameDataValidator.Validate(data);
            if (dataError != null)
            {
                ShowFatal(canvas, "gamedata.json 오류:\n" + path + "\n\n" + dataError);
                return;
            }

            // 3D 무대 + 카메라 세팅
            var cam = Camera.main;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.045f, 0.032f, 0.028f);
            cam.nearClipPlane = 0.05f;
            var raycaster = cam.gameObject.AddComponent<PhysicsRaycaster>(); // 3D 도장·소품 포인터 이벤트용
            raycaster.eventMask = ~(1 << 2); // 레이어 2(환경 물리 콜라이더)는 클릭 대상 제외 — "허공 클릭 = 내려놓기" 유지

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.14f, 0.11f, 0.10f);
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { l.intensity = 0.3f; l.color = new Color(0.75f, 0.65f, 0.55f); }

            var rig = new GameObject("DeskRig").AddComponent<DeskRig>();
            rig.Build(cam);

            var flow = new GameObject("GameFlow").AddComponent<GameFlow>();
            flow.Run(data, canvas, rig);
        }

        private static void CreateEventSystem()
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            var uiModule = es.AddComponent<InputSystemUIInputModule>(); // 이 프로젝트는 Input System 전용 (legacy Input 금지)
            // 런타임 AddComponent는 Reset()이 호출되지 않아 (특히 빌드에서) 기본 액션이 비어
            // 클릭/드래그가 전혀 안 먹을 수 있다 → 기본 UI 액션을 명시적으로 할당
            uiModule.AssignDefaultActions();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        private static TMP_FontAsset CreateOsFallbackFont()
        {
            foreach (var name in new[] { "Apple SD Gothic Neo", "AppleGothic", "Malgun Gothic", "NanumGothic" })
            {
                var f = Font.CreateDynamicFontFromOSFont(name, 40);
                if (f != null)
                {
                    Debug.LogWarning("[MawangHR] MonaS12 SDF 미연결 — OS 폰트 폴백: " + name);
                    return TMP_FontAsset.CreateFontAsset(f);
                }
            }
            Debug.LogError("[MawangHR] 사용할 한글 폰트를 찾지 못했습니다.");
            return null;
        }

        private static void ShowFatal(Canvas canvas, string message)
        {
            var panel = UiKit.PanelRect(canvas.transform, "Error", new Color(0.2f, 0.02f, 0.02f));
            UiKit.Fill(panel);
            var label = UiKit.Label(panel, message, 34, Color.white, TextAlignmentOptions.Center);
            UiKit.Fill((RectTransform)label.transform);
            Debug.LogError("[MawangHR] " + message);
        }
    }
}
