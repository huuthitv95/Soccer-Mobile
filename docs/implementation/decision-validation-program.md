# Chương trình kiểm chứng decision register

> [Chỉ mục](../index.md) · [GDD](../product/gdd-soccer-mobile-pro.md) · [Unity audit và backlog](unity-implementation-audit-and-backlog.md)

## 0. Mục lục

- [1. Mục tiêu và authority](#purpose)
- [2. Lifecycle và approval gate](#lifecycle)
- [3. Ma trận 45 quyết định](#decision-matrix)
- [4. Evidence và playtest protocol](#evidence-protocol)
- [5. Thứ tự triển khai](#delivery-order)
- [6. QA, rollback và bàn giao](#handoff)

<a id="purpose"></a>

## 1. Mục tiêu và authority

Tài liệu này là authority duy nhất về trạng thái kiểm chứng của 45 quyết định sản phẩm. GDD và các domain spec mô tả intent/contract; file này quyết định một lựa chọn đã sẵn sàng thử, đang được thử, có đủ evidence hay được phê duyệt. Không suy diễn `Approved` từ việc code hoặc section đã tồn tại.

Nguyên tắc `game-design-core`: core match phải được chứng minh vui và dễ hiểu trước progression, economy hoặc liveops; quan sát hành vi thắng ý kiến thiết kế; ba người chơi độc lập lặp cùng lỗi tạo design issue; không dùng FOMO hoặc power progression để che core loop yếu.

<a id="lifecycle"></a>

## 2. Lifecycle và approval gate

| Trạng thái | Điều kiện vào | Điều kiện ra |
| --- | --- | --- |
| `Open` | Có owner và câu hỏi cần khóa | Protocol đủ cohort, metric, evidence và rollback |
| `TestReady` | Protocol hoàn chỉnh; dependency có thể vẫn đang xếp hàng | Build/config bắt đầu thu evidence |
| `InValidation` | Có build/config version và phiên thử đang chạy | Evidence được đóng băng, kiểm tra chất lượng |
| `EvidenceReady` | Có raw metric, observation, failure log và kết luận | Owner và reviewer độc lập ký quyết định |
| `Approved` | Gate đạt, có sign-off, phạm vi áp dụng và rollback | Mở lại khi metric drift hoặc contract đổi |
| `Rejected` | Evidence bác bỏ hypothesis | Quay lại intent/protocol mới, không tái dùng config cũ |
| `Blocked` | Thiếu authority pháp lý, privacy, licensing, backend hoặc nguồn lực bắt buộc | Blocker có owner và evidence được giải quyết |

Mọi artifact nằm dưới `docs/implementation/evidence/<decision-id>/` và chỉ được tạo khi có evidence thật. Gói phê duyệt bắt buộc gồm `build/config version`, `cohort + device tier`, research question, raw metric, observation notes, failure/abuse review, accessibility review, owner/reviewer sign-off và rollback rehearsal. Không lưu PII, credential hoặc video người tham gia chưa có consent.

<a id="decision-matrix"></a>

## 3. Ma trận 45 quyết định

| ID | Prototype/playtest | Owner | Cohort/device | Metric chính | Evidence artifact | Approval gate | Rollback | Trạng thái |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| D01 | Chemistry scenario suite | Game Design | Minh/Huy; low–high tier | formation viability, dominant pick | seeded results + notes | ≥3 formation khả dụng, không dominant strategy | disable chemistry config | `TestReady` |
| D02 | Market ledger simulation | Economy + Backend | synthetic economy cohorts | sink/source, inflation, false positive | simulation export + abuse report | stable band và manipulation review đạt | pin tax/limit N-1 | `TestReady` |
| D03 | AI manager scenario suite | AI + Gameplay | personas; low–high tier | shape error, pass diversity, exploit rate | seeded traces + profiler | vượt heuristic baseline trong budget | pin heuristic profile | `TestReady` |
| D04 | Upgrade economy simulation | Economy + Product | synthetic + consented playtest | time-to-goal, regret, power gap | receipt log + cohort report | core gate đạt, không pay-to-win | disable recipe/compensate | `TestReady` |
| D05 | Tournament operations rehearsal | Competitive Ops | internal bracket + degraded network | completion, dispute SLA, verified result | signed result + incident log | reconnect/audit/reward pass | freeze event/recompute | `TestReady` |
| D06 | Notification privacy review | LiveOps + Privacy | age/region matrix | opt-out, complaint, re-enable | consent matrix + copy review | legal/privacy sign-off | default off/revoke schedule | `Blocked` |
| D07 | Offer compliance review | Product + Compliance | age/region/store matrix | fairness, refund, ranked power gap | odds/entitlement/refund dossier | compliance sign-off, no P2W | withdraw offer/reconcile | `Blocked` |
| ALS-D01 | Provider/guest matrix | Product + Legal + Backend | age/region/provider matrix | auth/recovery success, loss cases | privacy + recovery report | legal/backend sign-off | guest-only/offline | `Blocked` |
| ALS-D02 | Account merge simulation | Economy + Support | duplicate/loss fixtures | zero double-grant/loss | Fake conflict/immutability tests; backend ledger fixture pending | all merge invariants pass | disable merge/manual support | `InValidation` |
| ALS-D03 | Voice locale hot-swap | Audio + Client | low–high tier; vi/en | memory, resume, download failure | profiler + delivery matrix | no OOM; resumable/fallback works | pin packaged voice | `TestReady` |
| ALS-D04 | Cloud-sync allowlist | Security + UX | conflict/device fixtures | conflict recovery, unsafe field count | Automated registry/merge tests; backend schema diff + threat review pending | zero device-risk fields synced | local-only snapshot | `InValidation` |
| CLS-D01 | Group role/load simulation | Social + Backend | 1–cap synthetic groups | latency, moderation queue, abuse rate | load run + moderation log | SLO and abuse controls pass | reduce cap/disable role | `TestReady` |
| CLS-D02 | Club Challenge roster policy | Game Design + Licensing | licensed territory matrix | eligibility/fairness exceptions | rights register + scenario report | licensing and fairness sign-off | generic roster/offline only | `Blocked` |
| CLS-D03 | Dispute operations rehearsal | Competitive Ops | internal tournament | resolution time, audit completeness | case timeline + audit export | SLA/reviewer separation pass | extend window/freeze bracket | `TestReady` |
| CLS-D04 | Public profile defaults | Privacy + UX | age/region matrix | exposure, opt-out comprehension | privacy assessment + usability notes | privacy sign-off; minor-safe default | private-by-default | `Blocked` |
| CAT-D01 | Catalog API/cache spike | Backend + Data | N/N-1; online/offline | load, cache hit, integrity failure | Client contract/cache tests; backend load/cost pending | signature/version/audit pass | bundled catalog N-1 | `InValidation` |
| CAT-D02 | Addressables staged rollout | Client + DevOps | low–high tier; bad network | success, bytes, rollback time | Local clean build, content-state simulation và atomic rollback pass; remote CDN rehearsal pending | staged update/revert pass | pin prior manifest | `InValidation` |
| CAT-D03 | Model budget profiling | Tech Art | low–high target devices | memory, frame time, load time | profiler capture + LOD matrix | tier budgets met | generic low-LOD model | `TestReady` |
| CAT-D04 | Face/body launch coverage | Production + Data | territory/season matrix | coverage, fallback rate, throughput | License đã được người dùng xác nhận; còn thiếu coverage/capacity/fallback-quality matrix | capacity và quality sign-off | generic likeness fallback | `TestReady` |
| PCS-D01 | Rank/training economy test | Economy + Design | core-gated cohorts | cap clarity, power gap, regret | [eFootball risk/adoption baseline](../research/efootball-mobile/efootball-mobile-risk-control-matrix.md) + simulation/playtest pending | no dominant paid path | disable recipe/compensate | `TestReady` |
| PCS-D02 | Skill taxonomy scenarios | Gameplay | beginner/expert; seeded squads | pick diversity, exploit rate | [eFootball mechanic baseline](../research/efootball-mobile/efootball-mobile-feature-mechanic-inventory.md) + scenario/balance report pending | situational value, bounded modifier | disable skill config | `TestReady` |
| PCS-D03 | Market model/load simulation | Economy + Backend | synthetic market actors | inflation, liquidity, abuse, latency | ledger/load/abuse report | invariants and SLO pass | close market/pin bands | `TestReady` |
| PCS-D04 | Respec policy review | Product + Compliance | age/region + playtest | regret, fairness, paywall signal | usability + compliance dossier | compliance sign-off | free respec/disable charge | `Blocked` |
| AI-D01 | Utility/BT/hybrid spike | AI + Gameplay | seeded scenarios; low–high tier | debugability, frame cost, outcome | decision traces + profiler | deterministic and within budget | heuristic adapter | `TestReady` |
| AI-D02 | Blind difficulty calibration | Design + Analytics | Minh/Huy; low–high tier | win band, retry, perceived fairness | blind results + observation | no hidden physics cheat; flow band met | pin prior profile | `TestReady` |
| AI-D03 | Adaptive assist review | UX + Fairness | onboarding cohorts | opt-out, disclosure comprehension | consented test + fairness review | opt-in and no ranked crossover | disable adaptation | `Blocked` |
| AI-D04 | ML/RL baseline challenge | Tech Lead | offline dataset; device tiers | quality delta, determinism, cost | model card + benchmark | beats scripted baseline safely | stop R&D/use scripted | `TestReady` |
| MCV-D01 | Two HUD preset playtest | Gameplay UX | left/right-handed; phone/tablet | mis-tap, reachability, latency | Input asset + automated tests; human/device trace pending | context error < gate; accessibility pass | legacy preset | `InValidation` |
| MCV-D02 | Assist fairness matrix | Competitive Design | beginner/expert; ranked/unranked | outcome delta, disclosure comprehension | Typed context contract + automated tests; fairness review pending | ranked allowlist approved | disable assist in ranked | `InValidation` |
| MCV-D03 | VAR comprehension test | Rules + Broadcast UX | match cohorts; reduced motion | duration, skip, correct understanding | incident fixtures + observation | rule outcome understood; duration budget | banner-only presentation | `TestReady` |
| MCV-D04 | Replay retention review | Integrity + Privacy | dispute/storage matrix | retrieval, minimization, deletion | privacy/storage assessment | privacy sign-off | metadata-only retention | `Blocked` |
| CIE-D01 | Evidence threshold audit | Integrity + Legal | false-positive fixtures | precision/recall, appeal overturn | blinded audit report | legal/integrity sign-off | manual review only | `Blocked` |
| CIE-D02 | Sanction proportionality review | Policy + Legal | age/region/offense matrix | consistency, overturn, harm | sanction matrix + legal review | legal/policy sign-off | warning/manual hold | `Blocked` |
| CIE-D03 | Reconnect/forfeit network test | Match + Network | packet-loss/device matrix | resume rate, exploit rate, duration | network traces + fairness report | SLO/fairness pass | extended grace/unranked fallback | `TestReady` |
| CIE-D04 | Case retention assessment | Privacy + Security | purpose/region matrix | minimization, deletion, legal hold | DPIA/threat model | privacy/security sign-off | shorten/default delete | `Blocked` |
| CIE-D05 | Appeal staffing rehearsal | Support Ops | synthetic case volume | SLA, reviewer separation, quality | queue rehearsal + audit sample | staffing/SLA pass | extend SLA/manual triage | `TestReady` |
| UI-D01 | Palette/font device review | Art + Accessibility | vi/en; low–high tier | contrast, glyph, overflow | screenshots + accessibility audit | WCAG/legibility gate met | neutral fallback theme/font | `TestReady` |
| UI-D02 | Portrait support usage test | Product + Engineering | phone/tablet; rotation | task success, readability, defect rate | usability + layout matrix | no match readability regression | landscape lock | `TestReady` |
| UI-D03 | Bottom-nav/side-rail A/B playtest | UX | phone/tablet personas | first-click, reachability, mis-tap | observation + task metrics | chosen layout wins declared metric | canonical bottom nav | `TestReady` |
| UI-D04 | Motion budget profiling | UX + Tech Art | low tier + Reduced Motion | frame pacing, discomfort reports | profiler + accessibility notes | frame/accessibility gate met | reduced/zero motion preset | `TestReady` |
| UI-D05 | Liveops attention-budget test | Product + UX | new/returning personas | core task completion, CTA errors | screen recording + task report | one primary CTA; no task regression | suppress promo slots | `TestReady` |
| LOM-D01 | Offer/odds regional review | Product + Compliance | age/region/store matrix | disclosure, refund, complaint | compliance dossier + store matrix | compliance sign-off | withdraw/disable offer | `Blocked` |
| LOM-D02 | Membership economy simulation | Economy | synthetic cohorts | ranked advantage, value, churn pressure | economy model + fairness report | no ranked paid advantage | disable tier benefits | `TestReady` |
| LOM-D03 | Expiry/catch-up comprehension | LiveOps + UX | new/returning personas | missed value, comprehension, FOMO | usability notes + expiry matrix | catch-up available; FOMO review pass | extend/convert expiry | `TestReady` |
| LOM-D04 | Compensation rehearsal | Ops + Economy | incident/ledger fixtures | reconciliation, duplicate/missed grant | ledger audit + runbook log | idempotency and audit pass | stop grant/manual reconcile | `TestReady` |

Trạng thái sau automated evidence P1-02: **27 `TestReady`, 6 `InValidation`, 12 `Blocked`, 0 `EvidenceReady`, 0 `Approved`**. Sáu mục `InValidation` có automated evidence tại [nhật ký P0](p0-foundation-implementation.md), [nhật ký P1-01](p1-localization-settings-implementation.md) và [nhật ký P1-02](p1-football-catalog-and-model-foundation.md), nhưng vẫn thiếu human/device/fairness, backend/threat review hoặc owner gate tương ứng. `CAT-D04` được mở từ `Blocked` sang `TestReady` do license đã được người dùng xác nhận; coverage/capacity/fallback quality vẫn phải qua protocol. `TestReady` chỉ xác nhận protocol đủ rõ; `Blocked` chỉ mở khi blocker nêu trong hàng đã được giải quyết.

<a id="evidence-protocol"></a>

## 4. Evidence và playtest protocol

1. Chốt một research question và hypothesis có thể bác bỏ; ghi build/config/seed trước phiên thử.
2. Tuyển đúng persona, device tier và accessibility profile; người thiết kế không giải thích trong lúc task chạy.
3. Thu hành vi tối thiểu: completion, time, mis-input/failure, retry/quit point và đường đi; survey chỉ là dữ liệu bổ sung.
4. Redact dữ liệu, kiểm tra missing/invalid sample, đóng băng raw metric và ghi mọi deviation.
5. Reviewer độc lập so metric với gate; nếu ba người lặp cùng lỗi, mở design issue trước khi tăng tutorial text.
6. Rehearse rollback trên cùng build/config; chỉ sau đó owner mới ký `Approved`, `Rejected` hoặc yêu cầu vòng validation mới.

<a id="delivery-order"></a>

## 5. Thứ tự triển khai

1. P0-01 deterministic match core tạo seam, seed và test foundation.
2. P0-02 contextual input dùng typed command từ P0-01; bắt đầu `MCV-D01`/`MCV-D02` khi có hai HUD preset.
3. P0-03 account/versioned data tạo contract cho ALS/CAT nhưng quyết định legal/backend vẫn giữ `Blocked` hoặc tối đa `EvidenceReady`.
4. P1 chỉ mở sau core gate: localization/settings → catalog/model → cards/economy → competition/integrity.
5. P2: VAR presentation → tactical AI → telemetry/live config → liveops/monetization.

<a id="handoff"></a>

## 6. QA, rollback và bàn giao

- Validator phải xác nhận đủ đúng 45 ID, không trùng/thiếu và trạng thái thuộc enum.
- Mỗi đổi trạng thái cập nhật file này và domain authority trong cùng commit; link evidence phải tồn tại.
- Không sửa/xóa raw evidence sau sign-off; correction tạo revision có lý do và reviewer.
- Rollback luôn đưa hệ thống về config/version đã xác nhận, không xóa ledger/audit history.
- Sau mỗi batch, bàn giao gồm kết quả/evidence, decision đổi trạng thái, test, hạn chế, link review, commit/push/working tree và kế hoạch đề xuất cho batch kế tiếp.
