# Unity Engine — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | Unity **6000.5.1f1** (Unity 6.5, 2026-07-02 실측 — 프로토타입·Origin 동일) |
| **Project Pinned** | 2026-02-13 (6.3 LTS) → 2026-07-02 갱신 (6000.5.1f1) |
| **LLM Knowledge Cutoff** | May 2025 (~Unity 2022 LTS 2022.3) |

> ⚠️ 이 폴더의 나머지 레퍼런스 문서들(modules/, plugins/, breaking-changes 등)은 **6.3 LTS 기준으로 작성**된 스냅샷이다.
> 6.5에서 대부분 유효하나, API 세부가 다를 수 있으니 의심스러우면 공식 문서(6000.5) 교차 확인.

## Knowledge Gap Warning

LLM 학습 데이터는 Unity 2022 LTS 수준까지만 포함합니다. **Unity 6 시리즈는 모릅니다**:

- Entities/DOTS 1.0+ 전면 재설계 (production-ready)
- Input System 기본화 (Legacy Input Manager deprecated)
- URP/HDRP 대규모 개선, GPU Resident Drawer
- UI Toolkit 런타임 production-ready (UGUI 대체 권장)
- WebGPU 지원

Unity API 제안 시 반드시 공식 문서 교차 참조.

## Reference Links

- [Official Manual](https://docs.unity3d.com/6000.0/Documentation/Manual/index.html)
- [Migration Guide](https://docs.unity3d.com/6000.0/Documentation/Manual/upgrade-guides.html)
- [C# API Reference](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/index.html)
- [Unity 6.3 LTS Release](https://unity.com/blog/unity-6-3-lts-is-now-available)
