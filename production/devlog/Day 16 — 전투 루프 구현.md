---
Day: 16
날짜: 2026-03-23
작업시간: 1.5
상태: 완료
---
[[데브로그/데브로그]]
# Day 16 — 전투 루프 구현

## 한 일
- Health.cs — 플레이어/적 공용 HP 컴포넌트 (OnHPChanged / OnDeath 이벤트)
- EnemyHit.cs — 적 피격 처리 (넉백, HitFlash, 사망 연출)
- EnemyAI.cs — Idle → Chase → Attack 상태 머신 (감지/추적/공격)
- EnemyAnimator.cs — AI 상태를 Animator 파라미터로 변환
- WorldHPBar.cs — 캐릭터 머리 위 따라다니는 월드 스페이스 HP 바
- CameraFollow.cs — 플레이어 부드럽게 추적하는 카메라

**기존 스크립트 개선**
- PlayerCombat.cs — EnemyHit.TakeDamage 연결, 공격 판정 origin을 스프라이트 위치 기준으로 수정
- PlayerHit.cs — Health 컴포넌트 연결, 넉백 중 MovementLocked 처리
- PlayerDodge.cs — 구르기 중 적 레이어 충돌 무시 (통과 가능)
- PlayerMovement.cs — SpriteTransform 프로퍼티 공개
- EnemyAI.cs — 피격 스턴 중 velocity 간섭 제거

## 배운 것
- Physics.IgnoreLayerCollision으로 특정 레이어 간 충돌만 선택적으로 껐다 켤 수 있음
- 월드 스페이스 Canvas는 Scale을 0.01로 줄여야 적절한 크기로 보임
- EnemyAI와 EnemyHit가 같은 Rigidbody를 건드릴 때 우선순위 충돌 주의

## 막힌 것
- 기즈모가 스프라이트 위치와 달라 보이는 문제 → 쿼터뷰 카메라 특성 + 루트/자식 오프셋 차이로 파악, SpriteTransform 기준으로 판정 origin 수정

## 다음 목표
- Phase 3-2: 전투 흐름 완성 (입장 → 전투 → 탈출)
- Phase 3-3: 스킬 1개 구현
