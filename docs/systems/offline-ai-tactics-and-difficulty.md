# Offline AI, tactics và difficulty

> [Chỉ mục](../index.md) · [GDD](../product/gdd-soccer-mobile-pro.md) · [Controls](match-controls-set-pieces-and-var.md) · [Nghiên cứu](../research/fc-mobile-vn-research.md)

## 0. Mục lục

- [1. Mục tiêu và ranh giới](#goal)
- [2. Kiến trúc và contracts](#architecture)
- [3. State/decision flow](#flow)
- [4. Difficulty và tuning pipeline](#difficulty)
- [5. Failure, fairness và accessibility](#failure)
- [6. Analytics, QA và rollback](#quality)
- [7. Decision register](#decisions)

<a id="goal"></a>

## 1. Mục tiêu và ranh giới

Mục tiêu: AI offline tạo shape/role/decision bóng đá có thể giải thích, deterministic theo seed để test, và difficulty thay hành vi chứ không buff ẩn. Non-goal: khẳng định FC Mobile VN dùng ML/RL, tự học từ dữ liệu người chơi trên thiết bị, hoặc dùng NavMesh làm toàn bộ football AI. S03 chỉ xác nhận chế độ giả lập; T05 là tham chiếu R&D scenario, không phải production algorithm.

<a id="architecture"></a>

## 2. Kiến trúc và contracts

```text
AIDifficultyProfile { id, reactionDelayRange, perceptionNoise,
  planningBudget, executionError, tacticalRisk, assistPolicy, version }
TacticalPlan { formation, phaseRules, width, lineHeight, tempo,
  pressing, transition, setPiecePlan, version }
AIDecisionTrace { tick, seed, actorId, phase, perceivedStateHash,
  candidates[{action, utility, rejectReason}], chosenAction }
AIScenario { id, initialState, seedSet, successMetrics, maxTicks }
AIModelCard { modelId, datasetVersion, policyVersion, intendedUse,
  excludedUse, metricsByScenario, fairnessLimits, runtimeBudget, rollbackId }
```

Input: authoritative/local match snapshot, perception filtered theo profile, tactic/role, stamina và rule state. Output: `MatchCommand` giống command người chơi (`Move`, `Pass`, `Shoot`, `Tackle`, `SwitchRole`, set-piece intent), không sửa trực tiếp ball/result. Trace bật trong dev/test và sampling support, không ship chi tiết nhạy cảm trong ranked.

<a id="flow"></a>

## 3. State/decision flow

```text
Match phase → tactical shape → role assignment → perception/blackboard
→ candidate generation → utility + constraint → command
→ locomotion/animation/physics → outcome → trace/metrics
```

Phase: `InPossession`, `OutOfPossession`, `PositiveTransition`, `NegativeTransition`, `SetPiece`, `DeadBall`. Role assignment dùng hysteresis/cooldown để tránh hai cầu thủ cùng đổi nhiệm vụ; GK và set-piece có invariant riêng. Planner không biết state ngoài perception profile. Cùng build/config/seed/input log phải tái tạo chosen action trong deterministic test tolerance.

<a id="difficulty"></a>

## 4. Difficulty và tuning pipeline

Difficulty tăng theo reaction delay, perception noise, candidate breadth, tactical risk và execution error trong biên công bố. Không thay speed/OVR/stamina/physics ngoài catalog/rules chung. Adaptive assist, nếu có, chỉ trong onboarding/offline, opt-out được và log profile change; không rubber-band score ẩn.

Pipeline: định nghĩa hypothesis → scenario seed set → batch simulation → metric/trace review → human playtest → config candidate → regression → staged release. ML/RL chỉ là nhánh R&D offline để đề xuất policy/tuning; policy phải export cố định, kiểm tra exploit/fairness/performance và rollback được trước runtime.

Dataset R&D có `datasetId/version`, provenance, rights/consent, collection purpose, retention, anonymization, feature schema và train/validation/test split chống leakage. Không ingest raw player telemetry nếu consent/purpose không cho phép; delete request phải đi qua lineage. Mỗi candidate có model card, seed/build/config, evaluation theo scenario/persona, known limitation và scripted fallback. Dataset/model registry do AI Platform sở hữu; Unity client chỉ nhận policy artifact đã ký/versioned.

Scenario bắt buộc: build-up press, low block, wing overload, counter/recovery, marking/cross, through-ball line, time wasting, foul risk, corner/free kick/penalty, GK distribution và final-minutes risk.

<a id="failure"></a>

## 5. Failure, fairness và accessibility

| Failure | Invariant/fallback |
| --- | --- |
| No valid action/path | Safe hold/clear/reposition; không teleport |
| Role thrash/crowding | Hysteresis, assignment ownership, spacing constraint |
| Planner vượt budget | Dùng cached/simple policy; ghi budget miss |
| Animation/command reject | Replan tick sau; không force ball outcome |
| Save/resume/version mismatch | Pin config/version trong save; migrate hoặc restart có thông báo |
| Exploit lặp | Scenario regression + telemetry; patch config có version |

UI difficulty mô tả hành vi dễ hiểu, không ghi “AI thông minh hơn” chung chung; assist/onboarding có thể tắt; pause/resume, speed option cho Manager presentation và color/ball visibility. Failure phải dạy qua replay/trace tóm tắt, không phạt bằng load dài.

<a id="quality"></a>

## 6. Analytics, QA và rollback

Metric/event: possession/field tilt không dùng đơn độc; shape deviation, open-pass miss, marking error, decision diversity, action reject, planner budget, exploit success, goal source, perceived fairness, difficulty changed, retry/quit. Production không thu raw full trace mặc định.

QA: deterministic replay, 30/60 FPS independence, pause/save/resume, all formations/roles/set pieces, low-end budget, 100+ seeds/scenario, property invariant (11 players, valid possession/rule, no teleport), human blind playtest. Acceptance candidate: zero invariant violation, deterministic replay pass, p95 budget trong frame allocation, không hidden attribute boost, exploit regression dưới gate do AI owner đặt. Rollback pin previous profile/policy by match/save; new config feature flag; save cũ giữ version hoặc migration explicit.

<a id="decisions"></a>

## 7. Decision register

Trạng thái/evidence chi tiết nằm tại [chương trình kiểm chứng](../implementation/decision-validation-program.md#decision-matrix).

| ID | Quyết định | Owner | Gate | Trạng thái |
| --- | --- | --- | --- | --- |
| AI-D01 | Utility/BT/hybrid implementation | AI + Gameplay | Prototype profile + debugability/performance | `TestReady` |
| AI-D02 | Difficulty profile values | Design + Analytics | Blind playtest theo persona/device | `TestReady` |
| AI-D03 | Adaptive onboarding assist | UX + Fairness | Opt-out, disclosure, no ranked crossover | `Blocked` |
| AI-D04 | ML/RL R&D continuation | Tech Lead | Vượt scripted baseline, deterministic export, safety/cost | `TestReady` |
