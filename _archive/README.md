# _archive — 참고 전용 보관소

> 여기 있는 것은 **현행 작업의 기준이 아니다.** 필요할 때만 열어 본다. 옵시디언 검색에서 `unity-prototype/`는 제외.
> 만든 날: 2026-09-12 (폴더 재정비 — [production/reorg-2026-09.md](../production/reorg-2026-09.md))

## 목록

| 항목 | 무엇 | 왜 보관 | 언제 다시 볼까 |
|---|---|---|---|
| [unity-prototype/](unity-prototype/README.md) | 2026-07(7/4~7/14) 「마왕성 인사팀」 그레이박스 프로토 (MawangHR, S1~S3a: 서류 심사·3D 데스크·면접 루프·밤 파트). 옛 `prototypes/unity-prototype` | 이력서·JD·판정 데이터 구조와 판정 코드를 한 번 짜 본 기록. Unity 6000.5.1f1 + URP | Origin 3단계(서류 루프)에서 데이터 구조 참고. 3단계를 넘기면 삭제 여부 재결정 |
| [mawang-hr-proto-brief.md](mawang-hr-proto-brief.md) | 위 프로토의 작업 명령서 (Fable 스프린트 7/4~7/7). 옛 `docs/` | 가설·기술 규칙·세션 플랜 기록 | 프로토 코드를 읽을 때 같이 |

`unity-prototype/`의 Library·Temp·Logs·UserSettings·csproj·slnx(약 2GB 캐시)는 삭제했다. Unity Hub에서 "Add project from disk"로 열면 Library가 재생성된다(수 분 소요).

## 여기 없는 옛 것 (git 히스토리에서 본다)

삭제 직전 트리 = 커밋 **86ac21f** (2026-09-12). `git show 86ac21f:<경로>` 또는 `git log --all -- <경로>`.

- 옛 데브로그 `production/devlog/` Day 01~29 (2026-04~07) · `Origin/journal/`(DAY-01, 000-log) — 2026-09-12 삭제. 기록은 출근부(`production/desk/`)로 일원화.
- 죽은 컨셉 GDD 본문(1세대 PvP · 2세대 헥사) — 2026-09-12 삭제. 연혁·사인 요약만 [design/gdd/_archive/README.md](../design/gdd/_archive/README.md)에 남김. 아트 쪽은 [design/art/_archive/README.md](../design/art/_archive/README.md).
- 템플릿 껍데기 `src/`·`.github/`·`docs/registry/`·`tr-registry.yaml` — 2026-09-12 삭제 (코드는 `game/` 안에 두므로 불필요).
- 옛 볼트 `C:\ProjectDDWORLD`·`article/` — 폐기. 공부 자료는 읽기 볼트 `C:\Reading`.
