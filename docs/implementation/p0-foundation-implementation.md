# P0 foundation — nhật ký triển khai và kiểm chứng

> [Chỉ mục](../index.md) · [Decision validation](decision-validation-program.md) · [Unity audit và backlog](unity-implementation-audit-and-backlog.md)

## 0. Mục lục

- [1. Understand brief P0-01](#p0-01-understand)
- [2. Contract deterministic match core](#match-contract)
- [3. Validation và trạng thái](#validation-status)
- [4. Rollback và bước tiếp theo](#rollback-next)
- [5. Understand brief P0-02](#p0-02-understand)
- [6. Contract contextual input](#input-contract)
- [7. Evidence P0-02](#input-evidence)
- [8. Rollback và gate còn lại](#input-rollback)

<a id="p0-01-understand"></a>

## 1. Understand brief P0-01

**Ranh giới:** thêm deterministic state seam cho kickoff, in-play, foul, corner, halftime và fulltime; không thay physics, animation, HUD, score persistence, economy hoặc online authority trong batch này.

| Owner hiện tại | Trách nhiệm và phụ thuộc |
| --- | --- |
| `InitGame` + `GameManager` | Clock, half/fulltime, dialog, score và cờ global `IsGameReady`/`IsFirstHalf`. |
| `Player` + role AI | Đọc input/heuristic, tác động trực tiếp transform, animation và bóng. |
| `BallScript` + foul/corner trigger | Ball owner, Rigidbody và incident flags/position dựa trên tag/collider. |
| HUD/dialog/result | Đọc global/static state; ghi cup/result vào `PlayerPrefs`; load scene bằng tên chuỗi. |

```text
SoccerInput/legacy GUI
  → Player hoặc role AI
  → BallScript/Rigidbody + foul/corner trigger
  → GameManager/InitGame clock và half/result
  → animation/HUD/dialog/audio
  → PlayerPrefs + scene result
```

Invariant và hidden dependency: `BallScript.ownerPlayer` là possession local; `IsGameReady=false` vừa pause gameplay vừa mở set-piece/restart; `IsFirstHalf` đổi camera/score; tag, scene name, animation name và `PlayerPrefs` key là contract không type-check. Source chưa có asmdef/test/inputactions. Unity MCP không kết nối ở đầu batch, nên inspection dùng knowledge graph và source tĩnh; test sẽ chạy bằng Unity batchmode 2022.3.62f3.

Rủi ro: shadow state lệch legacy, sequence/tick không đơn điệu, event buffer tăng vô hạn và assembly mới gây compile regression. Rollback: feature flag mặc định tắt, adapter không cấp score/reward và core thuần C# không điều khiển physics/presentation. Tài liệu cần đồng bộ: GDD, controls/VAR, decision program và implementation audit.

<a id="match-contract"></a>

## 2. Contract deterministic match core

- `MatchCommand`: intent có sequence/tick/type/team; không chứa physics outcome.
- `MatchSnapshot`: phase/half/tick/score/seed/hash bất biến từ góc nhìn consumer.
- `MatchEvent`: phase, tick, goal hoặc rejection có reason code.
- `IRuleEngine`: kiểm tra transition; `IMatchSimulation`: áp command và trả transition result.
- Cùng seed và command stream phải tạo cùng snapshot hash/event sequence.
- Event buffer có cap; duplicate/out-of-order/malformed command bị reject mà không thay snapshot.
- `LegacyMatchCoreAdapter` chỉ shadow clock/incident khi flag `smp_match_core_v1=1`; mặc định `0` giữ nguyên runtime.

<a id="validation-status"></a>

## 3. Validation và trạng thái

Unity 2022.3.62f3 batchmode đã compile toàn bộ project và chạy:

- EditMode Match Core: **6/6 pass**; toàn runner **7/7 pass** khi tính thêm một test stub Addressables có sẵn. Bộ Match Core gồm phase flow, deterministic replay, duplicate sequence, invalid transition/team và bounded event buffer.
- PlayMode: **1/1 pass**, lifecycle chạy qua nhiều frame và giữ phase/tick đúng.
- Không có compiler error; Unity MCP service không chạy nên validation dùng Unity CLI chính thức. Batchmode tự đổi `runInBackground`, nhưng side effect này đã được khôi phục; `ProjectSettings/` và `Packages/` không còn diff.

Evidence đủ cho **P0-01 foundation**, nhưng chưa đủ cho bất kỳ decision sản phẩm nào trong 45 mục: chưa có human playtest, device profiling, incident taxonomy/VAR fixture hoặc owner sign-off. Vì vậy decision register giữ 32 `TestReady`, 13 `Blocked`, 0 `Approved`; không nâng trạng thái giả từ unit test kỹ thuật.

<a id="rollback-next"></a>

## 4. Rollback và bước tiếp theo

Rollback runtime: đặt `smp_match_core_v1=0`; adapter không được phép thay score, reward, Rigidbody, HUD hoặc scene routing. Nếu assembly compile lỗi, revert commit P0-01 độc lập.

Sau P0-01, batch đề xuất tiếp theo là P0-02 contextual input: typed action maps dùng `MatchCommand`, hai HUD preset và binding/accessibility tests. Dependency vào core chỉ được coi đạt khi deterministic tests pass và legacy flow không có console regression.

<a id="p0-02-understand"></a>

## 5. Understand brief P0-02

**Ranh giới:** thêm lớp Input System theo context và adapter tạo typed `MatchCommand`; không thay HUD prefab, assist policy, physics, animation, server authority hoặc reward. Feature flag `smp_contextual_input_v1` mặc định tắt nên `SoccerInput`/joystick và nhánh xử lý hiện tại trong `Player` vẫn là đường production của prototype.

| Owner | Trách nhiệm và phụ thuộc |
| --- | --- |
| `SoccerInput`, `Joystick`, `Player` | Đọc Enhanced Touch/legacy GUI, chọn action theo possession và tác động trực tiếp gameplay. |
| `SoccerMobileControls.inputactions` | Khai báo năm context map và control scheme touch/gamepad/keyboard. |
| `ContextualMatchInputAdapter` | Chỉ bật một map, chuẩn hóa direction/magnitude, cấp sequence và tạo `MatchCommand`. |
| `ContextualMatchInputRuntime` | Nạp asset từ `Resources`, nối callback và giữ queue bounded sau feature flag. |
| `HudLayoutProfile` | Hai prototype Standard/LeftHanded cùng preset Legacy rollback; clamp scale/opacity/dead-zone. |

```text
device binding hoặc on-screen control callback
  → active InputActionMap theo possession/set-piece/GK/UI
  → ContextualMatchInputAdapter
  → typed MatchCommand + sequence/context
  → bounded queue cho Match Core adapter tương lai
```

Invariant: chỉ một action map được enable; context không hợp lệ không tạo command; client/domain không nhận raw touch coordinate; binding override không sửa asset mặc định; mất asset hoặc flag tắt phải giữ legacy controller. Hidden dependency còn lại là possession từ `BallScript.ownerPlayer`, lifecycle scene của `Player`, callback Input System và `PlayerPrefs` chỉ dùng cho local feature flag, không dùng để lưu token hay authority.

Rủi ro chính là context đổi trễ một frame, binding conflict, callback sống quá asset, queue spam và HUD prototype chưa được chứng minh trên thiết bị. Rollback độc lập bằng cách giữ `smp_contextual_input_v1=0`; không xóa controller/prefab cũ.

<a id="input-contract"></a>

## 6. Contract contextual input

- Action maps: `Match_OnBall`, `Match_OffBall`, `SetPiece`, `Goalkeeper`, `UI`.
- Control schemes: `Touch`, `Gamepad`, `Keyboard`; touch HUD gọi `SubmitTouchAction` bằng action semantic, không chuyển tọa độ thô vào domain.
- `MatchCommand` mở rộng action typed cho move, pass/shoot/skill, switch/press/tackle, goalkeeper, set piece và UI; direction được clamp, magnitude nằm trong `0..1`, sequence tăng đơn điệu.
- `ContextualMatchInputAdapter.SetContext` disable toàn asset rồi chỉ enable map đích.
- Binding override dùng serialization của Input System; conflict validator kiểm tra binding đơn trùng trong cùng map/control group.
- `HudLayoutProfile` cung cấp Standard và LeftHanded mirror, cùng Legacy fallback; đây là dữ liệu prototype, chưa phải layout production được phê duyệt.

<a id="input-evidence"></a>

## 7. Evidence P0-02

Unity 2022.3.62f3 batchmode đã import asset và chạy toàn bộ suite:

- EditMode: **13/13 pass**, gồm 6 test Match Core, 6 test contextual input và 1 Addressables stub có sẵn. Input tests xác nhận đủ map/scheme, không conflict binding đơn, chỉ một map active, semantic theo context, sequence đơn điệu, HUD mirror/bounds và binding override round-trip.
- PlayMode: **2/2 pass**, gồm lifecycle Match Core và typed `Shoot` command qua nhiều frame.
- Không có compiler error; `ProjectSettings/` và `Packages/` không còn diff sau khi hoàn nguyên side effect batchmode.

`MCV-D01` và `MCV-D02` chuyển `TestReady → InValidation`: build/config và automated evidence đã tồn tại, nhưng chưa có touch HUD production, phép đo latency/mis-tap/reachability, device matrix hoặc fairness playtest. Vì vậy không decision nào được nâng `EvidenceReady`/`Approved`.

<a id="input-rollback"></a>

## 8. Rollback và gate còn lại

Rollback runtime: tắt `smp_contextual_input_v1`; `Player` tiếp tục dùng legacy input. Nếu asset hoặc assembly lỗi, revert riêng batch P0-02; không cần migration save và không ảnh hưởng Match Core shadow flag.

Gate còn lại cho P0-02: nối HUD prefab thật vào callback typed, xử lý focus loss/controller reconnect, chạy Android thấp/trung/cao và playtest người thuận tay trái/phải. Batch kế tiếp theo dependency là P0-03 account/session và versioned-data contract; nó độc lập với HUD measurement và có thể triển khai client contract/fake adapter trong khi MCV-D01/02 tiếp tục `InValidation`.
