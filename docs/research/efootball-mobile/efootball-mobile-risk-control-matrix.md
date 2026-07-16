# Ma trận rủi ro và kiểm soát từ eFootball Mobile

> [Chỉ mục](../../index.md) · [Nghiên cứu](efootball-mobile-research.md) · [Inventory](efootball-mobile-feature-mechanic-inventory.md) · [Adoption](efootball-mobile-adoption-decision-matrix.md) · [Decision program](../../implementation/decision-validation-program.md)

## 0. Mục lục

- [1. Phương pháp](#method)
- [2. High và Critical risks](#high-risks)
- [3. Medium risks](#medium-risks)
- [4. Control invariants](#invariants)
- [5. Theo dõi và escalation](#monitoring)

<a id="method"></a>

## 1. Phương pháp

Score dùng `Likelihood × Impact`, mỗi chiều 1–5. `Critical` từ 20, `High` từ 12, `Medium` từ 6. Detectability thấp làm tăng ưu tiên nhưng không đổi score. Đây là risk register cho Soccer Mobile Pro, không phải đánh giá chất lượng hay cáo buộc eFootball.

Mọi `High/Critical` bắt buộc có owner, leading indicator, guardrail, gate và rollback. Evidence từ eFootball chỉ xác nhận mechanic/risk surface; acceptance phải do prototype, simulation, device/human test của Soccer Mobile Pro tạo ra.

<a id="high-risks"></a>

## 2. High và Critical risks

| ID | Risk / L×I | Signal và indicator | Owner | Guardrail + acceptance gate | Rollback | Decision |
| --- | --- | --- | --- | --- | --- | --- |
| EFR-01 | Duplicate/missed grant `4×5 Critical` | Retry/reconnect; ledger imbalance, repeated idempotency key | Economy Backend | Atomic ledger; property/concurrency tests không double consume/grant | Disable write, reconcile append-only | D02, D04, LOM-D04 |
| EFR-02 | Stale catalog/rules `4×4 High` | Version mismatch/rejected preview | Data + Economy | Pin catalog/rules in preview/receipt; N/N-1 contract pass | Read-only inventory, pin N-1 | CAT-D01, PCS-D01 |
| EFR-03 | Random-item regret/odds `4×5 Critical` | Complaint/refund, repeat spend, duplicate rate | Product + Compliance | Published odds, age/region gate, spend friction, duplicate/pity policy; sign-off | Withdraw offer, refund/reconcile | D07, LOM-D01 |
| EFR-04 | Paid power/power creep `4×5 Critical` | Win-rate/pick-rate by spend/OVR gap | Economy + Competitive Design | No ranked-exclusive paid power; bounded modifier/scenario test | Disable Booster/rule, compensate | D04, PCS-D01, PCS-D02 |
| EFR-05 | Destructive consume/release `4×4 High` | Locked/in-squad source, support restore | Inventory + UX | Lock invariant, preview consequence, undo grace where possible | Restore via ledger, disable bulk action | PCS-D01 |
| EFR-06 | Fusion loss/race `3×5 High` | Source/target revision conflict, timeout unknown | Economy Backend | Atomic source consume/target update; receipt lookup | Freeze fusion, reconcile receipt | PCS-D01 |
| EFR-07 | Economy inflation `4×4 High` | Source/sink ratio, price/upgrade velocity | Economy | Versioned cap/sink; simulation across cohorts | Pin cost/cap, disable source | D02, PCS-D03 |
| EFR-08 | FOMO/expiry harm `4×4 High` | Late conversion, churn/complaint near expiry | LiveOps + UX | Advance warning, grace/catch-up/convert, no surprise deletion | Extend/convert/compensate | LOM-D03 |
| EFR-09 | Assist competitive unfairness `4×5 Critical` | Outcome delta by assist, setting switch exploit | Competitive Design | Server match-policy allowlist; disclose; matched cohorts; fairness gate | Disable assist in ranked | MCV-D02 |
| EFR-10 | Input context/mis-tap `4×3 High` | Wrong command, reachability, latency | Gameplay UX | Typed context maps, remap/HUD scale/left-hand; device playtest | Legacy preset | MCV-D01 |
| EFR-11 | Reconnect/result desync `4×5 Critical` | Unknown result, duplicate result, forfeit disputes | Match Network | Resume token, authoritative snapshot/result, idempotent submit | Unranked fallback/manual hold | CIE-D03 |
| EFR-12 | Matchmaking manipulation `3×4 High` | Rating/assist queue exploit, smurf pattern | Matchmaking + Integrity | Versioned policy, audit, cohort isolation; false-positive review | Widen queue/freeze rating | CIE-D01 |
| EFR-13 | Account loss/merge double grant `3×5 High` | Cross-OS recovery, duplicated entitlement | Identity + Support | Explicit link/recovery; no auto merge; immutable ledger | Disable merge/manual recovery | ALS-D01, ALS-D02 |
| EFR-14 | Credential/account abuse `4×4 High` | Share/sale/fraud signals | Security + Integrity | Session revoke, MFA/provider policy, support correlation, appeal | Revoke sessions/manual review | ALS-D01, CIE-D01 |
| EFR-15 | Live-update interruption `4×4 High` | Title return mid transaction/match | Operations + Match | Maintenance drain, receipt/result recovery, read-only mode | Stop write, extend event, compensate | LOM-D04, CIE-D03 |
| EFR-16 | Content/license migration `3×5 High` | Removed ID, broken foreign key/model | Data + Production | Stable identity, effective interval, mapping/rightsVersion, fallback | Pin manifest, generic asset | CAT-D01, CAT-D04 |
| EFR-17 | Device OOM/content load `3×5 High` | Fallback rate, crash/load time by tier | Client + Tech Art | Tier LOD/budget, staged Addressables, generic fallback | Pin prior bundle/low LOD | CAT-D02, CAT-D03 |
| EFR-18 | AI difficulty cheats/opacity `3×4 High` | Hidden stat/physics delta, exploit loop | AI + Design | Same rules, traceable bounded intent, blind calibration | Pin heuristic profile | AI-D01, AI-D02 |
| EFR-19 | Social griefing/Co-op abuse `3×4 High` | AFK/quit/report rate, role conflict | Social + Integrity | Ready/role cues, reconnect/AFK policy, report/appeal | Disable reward/ranked Co-op | CLS-D03, CIE-D02 |
| EFR-20 | Privacy/retention overreach `3×5 High` | Excess event/identity data, deletion failure | Privacy + Security | Minimization, consent/age matrix, retention/delete test | Disable telemetry/public profile | ALS-D01, CIE-D04 |

<a id="medium-risks"></a>

## 3. Medium risks

| ID | Risk / score | Control | Acceptance |
| --- | --- | --- | --- |
| EFR-21 | Progression complexity `3×3` | Tách level/allocation/skill/position; guided preview | Người mới giải thích đúng consequence, không cần wiki ngoài game |
| EFR-22 | Manager/tactic dominant meta `3×3` | Counterplay, bounded modifier, scenario suite | Nhiều tactic khả dụng, không một manager bắt buộc |
| EFR-23 | UI attention overload `3×3` | Một primary CTA, badge budget, preserved navigation state | Core task không regress |
| EFR-24 | Random position frustration `3×3` | Choice token hoặc deterministic path; transparent pool | Regret/abandon dưới gate đã phê duyệt |
| EFR-25 | Migration communication gap `3×3` | Preview mapping, inbox, help, compensation | Zero unexplained loss trong fixtures |
| EFR-26 | Accessibility gap `3×3` | Text/icon redundancy, focus, reduced motion, remap | Accessibility review + device matrix pass |
| EFR-27 | Cross-platform capability drift `3×3` | Capability negotiation và mode eligibility | Unsupported path bị chặn trước queue |
| EFR-28 | Community claim contamination `2×3` | Source/claim register và confidence label | Không có unsupported current-state claim |

<a id="invariants"></a>

## 4. Control invariants

1. Mọi mutation inventory/economy có `idempotencyKey`, expected revision, catalog/rules version và immutable receipt.
2. Client không grant, roll RNG, quyết định price/tax, ranked result hoặc sanction.
3. Player lock/in-squad/reserved state chặn release, fusion, exchange và market list.
4. Preview hết hạn hoặc stale không được auto-confirm lại sau refresh.
5. Random offer không hoạt động khi thiếu odds/age/region/refund/duplicate policy đã ký.
6. Assist, tactic và skill modifier có cap/exclusion/trace; ranked policy do authority quyết định.
7. Maintenance/catalog rollback giữ history và receipt; compensation là transaction mới.
8. Unknown/timeout chuyển sang receipt polling, không tạo command mới.

<a id="monitoring"></a>

## 5. Theo dõi và escalation

Dashboard tối thiểu: transaction unknown/recovered, ledger imbalance, duplicate key, stale rejection, release/fusion support case, source/sink, paid-vs-free power gap, assist outcome delta, disconnect/resume, asset fallback/OOM và privacy delete failure.

Khi indicator vượt gate: dừng rollout/config, giữ read-only projection, đóng băng raw evidence, chạy rollback rehearsal và mở decision lifecycle. Không chuyển `Approved` chỉ vì cơ chế tương tự tồn tại trong eFootball.
