# P0 foundation — nhật ký triển khai và kiểm chứng

> [Chỉ mục](../index.md) · [Decision validation](decision-validation-program.md) · [Unity audit và backlog](unity-implementation-audit-and-backlog.md)

## 0. Mục lục

- [1. Understand brief P0-01](#p0-01-understand)
- [2. Contract deterministic match core](#match-contract)
- [3. Validation và trạng thái](#validation-status)
- [4. Rollback và bước tiếp theo](#rollback-next)

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
