# Ma trận áp dụng cơ chế eFootball Mobile

> [Chỉ mục](../../index.md) · [Nghiên cứu](efootball-mobile-research.md) · [Inventory](efootball-mobile-feature-mechanic-inventory.md) · [Rủi ro](efootball-mobile-risk-control-matrix.md) · [P1-03 plan](../../implementation/p1-player-items-skills-and-progression-plan.md)

## 0. Mục lục

- [1. Quy tắc quyết định](#decision-rules)
- [2. Product và account](#product-decisions)
- [3. Player item và progression](#progression-decisions)
- [4. Tactics, gameplay và modes](#gameplay-decisions)
- [5. Economy, liveops và integrity](#economy-decisions)
- [6. P1-03 slice](#p1-slice)
- [7. Decision register](#open-decisions)

<a id="decision-rules"></a>

## 1. Quy tắc quyết định

- `Adopt`: intent và control phù hợp, có thể đưa vào contract foundation.
- `Adapt`: giữ intent nhưng đổi mechanic/control để phù hợp fairness, UX hoặc architecture.
- `Experiment`: chỉ prototype/simulation; chưa là policy production.
- `Reject`: không phù hợp product principle hoặc risk vượt lợi ích.

Các trạng thái này không thay lifecycle `TestReady/InValidation/...`. `Adopt` vẫn cần evidence trước `Approved`. Không sao chép giá, odds, cap, reward cadence, rating formula hoặc expiry từ eFootball.

<a id="product-decisions"></a>

## 2. Product và account

| Mechanic | Intent/lợi ích | Tác dụng phụ | Hiện trạng Soccer | Quyết định + control |
| --- | --- | --- | --- | --- |
| Link Data/recovery | Bảo toàn tiến trình đa thiết bị | Account loss/merge fraud | P0 account contract/fake adapter | `Adapt`: explicit provider link, no auto merge, ledger recovery |
| Reward khi link account | Tăng tỷ lệ bảo vệ account | Consent dark pattern/double grant | Objective/ledger chưa production | `Reject` mặc định; chỉ `Experiment` với reward không ép consent |
| Tutorial + Practice | Học qua thao tác | Text overload | UX/GDD đã định hướng action-first | `Adopt`: task-based, skip/retry, no economy pressure |
| Home nhiều hub/event | Discover content | Attention/FOMO | UI catalogue/spec có budget | `Adapt`: one primary CTA, badge/slot budget |
| Controller + touch song song | Accessibility/input choice | Context conflict/reconnect | P0 typed input foundation | `Adopt`: capability/focus/reconnect tests |

<a id="progression-decisions"></a>

## 3. Player item và progression

| Mechanic | Intent/lợi ích | Tác dụng phụ | Contract/gap | Quyết định + control |
| --- | --- | --- | --- | --- |
| Standard signing chọn player | Agency/F2P acquisition | Economy inflation | Catalog có stable ID; inventory chưa có | `Adopt`: versioned offer + authoritative grant |
| Random ticket/pack | Surprise/collection | Odds, regret, duplicate, P2W | Compliance/ledger chưa sẵn sàng | `Reject` ở P1-03; chỉ mở sau LOM-D01/D07 |
| Owned player lock | Chống thao tác phá hủy | UX friction | `OwnedPlayerItem.lockState` đã spec | `Adopt`: invariant xuyên release/fusion/exchange |
| Level Training | Tiến triển dễ hiểu | Grind/material sink | Chưa runtime | `Adapt`: XP/value rule versioned, không consume âm thầm |
| Progression Points manual | Build expression | Dominant min-max/respec regret | Allocated skill points mới ở mức contract | `Adapt`: deterministic preview, bounded allocation, free test respec |
| Auto allocation | Giảm cognitive load | Build không mong muốn | Chưa runtime | `Experiment`: explainable preset, preview trước apply |
| Additional Skill tối đa | Tạo player identity | Skill lottery/dominant combo | PCS-D02 TestReady | `Adapt`: deterministic choice trong foundation; random assignment bị loại |
| Position Training ngẫu nhiên | Squad flexibility/sink | Regret/random waste | Chưa contract riêng | `Reject` randomness; `Adapt` thành eligible choice path |
| Player Fusion | Tái sử dụng duplicate | Destructive loss/race | Transaction seam chưa runtime | `Adapt`: atomic transfer preview/receipt; source lock check |
| Booster vượt 99 | Chase item/meta variety | Power creep/P2W/readability | Modifier cap chưa validation | `Reject` vượt hard cap; `Experiment` cosmetic/side-grade modifier |
| Booster crafting ngẫu nhiên | Sink/replayability | Odds/regret | Compliance chưa mở | `Reject` ở foundation |
| Seasonal carryover | Content freshness | Loss/confusion | P1-02 N/N-1 catalog sẵn | `Adopt`: per-asset migration map + compensation ledger |

<a id="gameplay-decisions"></a>

## 4. Tactics, gameplay và modes

| Mechanic | Intent/lợi ích | Risk | Hiện trạng | Quyết định + control |
| --- | --- | --- | --- | --- |
| Game Plan presets | Thử tactic nhanh | Stale roster/config | Squad domain chưa runtime | `Adopt` sau P1-03 projection, revisioned save |
| Năm Team Playstyles | Tactical identity | Dominant meta | GDD có 5 style hypothesis | `Experiment`: scenario suite; không khóa modifier |
| Manager affinity | Collection + tactic | Paid manager power | GDD/spec chưa contract đầy đủ | `Adapt`: manager side-grade, no ranked-exclusive advantage |
| Manager Link-up Play | Counterplay/depth | Trigger opacity | Chưa runtime | `Experiment`: explicit condition/trace/cap |
| Match-up defence | Skill expression | Auto-tackle strength | P0 input contract | `Adapt`: typed intent, rule-authoritative outcome |
| Smart Assist | Onboarding/accessibility | Competitive unfairness | MCV-D02 InValidation | `Adapt`: mode allowlist, disclosure, matchmaking signal |
| Auto Control VS AI | Low-friction offline | Confusing handoff/exploit reward | AI chưa runtime | `Experiment`: offline-only, visible state, no online reward |
| My League | Long-form PvE | Content/AI cost | P2 AI chưa runtime | `Experiment` sau AI scenario suite |
| Quick/Friend/Co-op | Social breadth | Reconnect/griefing | Competition/social chưa runtime | `Adapt`: unranked first, room/reconnect/report controls |

<a id="economy-decisions"></a>

## 5. Economy, liveops và integrity

| Mechanic | Intent | Risk | Quyết định + control |
| --- | --- | --- | --- |
| GP + premium currency | Phân lớp earn/spend | Inflation/P2W/refund | `Adapt`: ledger, source/sink telemetry, no ranked-exclusive power |
| Match Pass | Session goals | FOMO/paid pressure | `Experiment` sau LOM-D02/D03, catch-up + expiry clarity |
| Objectives | Direction/reward | Repetition/double grant | `Adopt`: typed progress + idempotent grant |
| Campaign Hub shop | Event clarity | Currency fragmentation | `Adapt`: reuse wallet/ledger; no orphan currency |
| Phase/division reset | Competitive cadence | Rank anxiety/reset dispute | `Experiment`: transparent mapping + snapshot/audit |
| P2P/client-server by mode | Cost/latency trade-off | Host advantage/security | `Reject` inference/copy; choose architecture by Soccer requirements |
| Account-conduct enforcement | Protect ecosystem | False positive/appeal | `Adopt` policy intent; keep sanction decisions blocked for owner review |
| Live Update title return | Operational simplicity | Mid-match/transaction loss | `Adapt`: drain/read-only/resume/receipt recovery |

<a id="p1-slice"></a>

## 6. P1-03 slice

P1-03 được thu hẹp thành foundation không monetization:

1. `OwnedPlayerItem`, lock state, immutable catalog reference và inventory projection.
2. Deterministic acquisition fixture qua direct grant; không pack/RNG/premium purchase.
3. Progression allocation, skill assignment và position proficiency là state riêng, có preview/command/receipt.
4. Fusion atomic với source/target revision và protected-state checks.
5. Fake authoritative append-only ledger, idempotency/reconnect reconciliation, stale version rejection và read-only rollback.

Market, random pack, Booster, paid respec, production currency và manager monetization nằm ngoài batch. Chi tiết tại [kế hoạch P1-03](../../implementation/p1-player-items-skills-and-progression-plan.md).

<a id="open-decisions"></a>

## 7. Decision register

| ID | Research impact | Lifecycle giữ nguyên | Evidence cần tiếp theo |
| --- | --- | --- | --- |
| PCS-D01 | Tách XP/allocation/fusion; lock và receipt là invariant | `TestReady` | Economy simulation + usability/playtest |
| PCS-D02 | Chọn deterministic skill foundation; random skill bị loại | `TestReady` | Scenario suite, pick diversity, exploit rate |
| PCS-D03 | eFootball không cung cấp P2P market tham chiếu phù hợp | `TestReady` | Synthetic market/source-sink/load simulation |
| PCS-D04 | Random/paid respec không được mở từ research | `Blocked` | Product + compliance + regret/fairness sign-off |
| MCV-D02 | Smart Assist cần mode policy/matchmaking dimension | `InValidation` | Human fairness matrix + ranked allowlist approval |
| AI-D02 | Auto Control chỉ là UX offline, không là difficulty model | `TestReady` | Blind difficulty calibration |
| LOM-D01 | Store listing xác nhận random items nhưng không đủ gate | `Blocked` | Odds/age/region/store/refund dossier |

Không decision nào chuyển `EvidenceReady/Approved` chỉ từ competitor research.
