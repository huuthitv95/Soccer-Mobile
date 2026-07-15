# Match controls, set pieces và VAR

> [Chỉ mục](../index.md) · [GDD](../product/gdd-soccer-mobile-pro.md) · [Audit Unity](../implementation/unity-implementation-audit-and-backlog.md)

## 1. Mục tiêu và authority

Tạo control model dễ học nhưng có skill ceiling, đồng thời tách quyết định luật khỏi lớp trình bày VAR. Match simulation/server là authority cho ranked; client gửi intent có timestamp, không gửi kết quả.

## 2. Input matrix

| Trạng thái | Cụm trái | Cụm phải | Context chuyển đổi |
| --- | --- | --- | --- |
| Có bóng | Fixed/floating joystick | Pass, Through, Shoot, Sprint & Skill | Cross/Lob ở biên; Clear trong vùng nguy hiểm |
| Không bóng | Fixed/floating joystick | Switch, Press/Sprint, Tackle, Slide, Match-Up | GK Rush khi đối thủ thoát xuống |
| Thủ môn chủ động | Di chuyển + hướng phân phối | Rush, Dive, Catch/Parry, Throw/Kick | Dive chỉ khả dụng trong cửa sổ phản ứng |
| Corner | Aim/target zone | Power, curl, runner trigger | Defending đổi thành mark/clear/GK command |
| Free kick | Aim/curve | Shoot, cross, short routine | Wall/jump/charge ở phía phòng ngự |
| Penalty | Aim | Power/timing | GK direction/commit timing |
| Throw-in/goal kick | Chọn hướng/người nhận | Short/long/quick restart | Timeout chống câu giờ |

## 3. Input contract

- Action dùng state rõ: `started`, `performed`, `canceled`; buffer chỉ tồn tại trong cửa sổ đã định và bị xóa khi possession/match state đổi.
- Context remap giữ vị trí cơ bắp ổn định; label/icon và accessible name phải đổi đồng thời.
- Swipe/hold có threshold theo dp và thời gian, không phụ thuộc FPS; cancel khi ngón rời vùng an toàn hoặc modal/pause mở.
- Auto-switch, pass/shoot assist và target selection là policy riêng, versioned và ghi vào match metadata.
- Preset: Beginner, Assisted, Semi, Manual. Ranked phải công bố preset nào được phép và không ghép hàng đợi mơ hồ.

## 4. Data flow

```text
Touch/gamepad intent → input context → command buffer → match state validation
→ player/ball/AI decision → animation/physics → HUD/audio/haptic
→ authoritative result/replay log → telemetry
```

Client chịu trách nhiệm sampling, HUD và feedback; authoritative simulation chịu trách nhiệm possession, foul, offside, goal, stamina và kết quả. Input sequence ID chống gửi trùng; reconnect không phát lại command đã hết hiệu lực.

## 5. Set pieces

- Corner hỗ trợ target zone, power/curl, near/far runner; phòng ngự chọn man/zonal và người phá bóng.
- Free kick có direct/cross/short routine; wall position và distance do luật quyết định.
- Penalty có state machine setup → ready → strike → resolution; shoot-out lưu lượt, thứ tự và sudden death.
- Throw-in/goal kick có quick option, timeout và tactical positioning; không cho exploit reset stamina hoặc kéo dài vô hạn.

## 6. VAR presentation

- **Đề xuất cho Soccer Mobile Pro:** VAR là lớp giải thích cho offside sát nút, goal line, penalty và red card; không tự quyết định luật.
- Match engine chốt `decision`, `reason_code`, event time, actor IDs và evidence snapshot trước khi presentation chạy.
- Replay 2–4 giây, có overlay/line visualization và skip setting. Skip chỉ bỏ cutscene, không đổi kết quả.
- Ranked áp dụng frequency budget; nếu replay/evidence thiếu thì hiển thị quyết định ngắn, không dựng minh họa sai.

## 7. Analytics và integrity

Event tối thiểu: `input_action`, `input_context_changed`, `command_rejected`, `auto_switch`, `assist_applied`, `set_piece_started`, `var_triggered`, `var_skipped`, `rule_decision`. Sampling input phải giảm tần suất và không lưu dữ liệu cá nhân.

## 8. Accessibility

- Cho đổi kích thước, vị trí, opacity, fixed/floating joystick và left-handed mirror.
- Haptic, visual feedback và audio cue độc lập; reduced motion áp dụng cho camera/VAR replay.
- Training overlay dạy đúng lúc, có thể mở lại; không dựa riêng vào text hoặc màu.

## 9. QA và acceptance

- Đo input-to-action latency p50/p95, accidental tap, dropped/canceled command, auto-switch correctness và goal conceded from input error.
- Test 30/60 FPS, nhiều aspect ratio, multi-touch, pause/resume, packet loss/reconnect và mirrored HUD.
- VAR phải deterministic từ cùng replay log; test timeout, skip, offside line, penalty contact, goal-line và red-card reason.
- Acceptance: không có cùng input tạo hai action, context đổi không kích hoạt nhầm, và rule result không phụ thuộc cutscene.

## 10. Rollback

Control preset/config có version và feature flag. Khi preset mới lỗi, server từ chối version không tương thích và client quay về Classic; VAR presentation có thể tắt độc lập mà không tắt rule engine.

## 11. Contracts và decision register

```text
MatchCommand { matchId, actorId, sequenceId, clientTick, inputContext,
  action, direction, magnitude, modifiers, assistVersion }
RuleIncident { incidentId, matchTick, type, decision, reasonCode,
  actorIds, evidenceStateHash, rulesVersion }
VARPresentationState { incidentId, state, presentationProfile,
  startedAt, timeoutAt, skippable }
```

`MatchCommand` là intent và có thể bị reject; không chứa kết quả bóng. `RuleIncident` bất biến sau authoritative resolution; correction chỉ qua result dispute ngoài match, không qua replay UI. VAR state: `NotEligible → Queued → Reviewing → DecisionShown → Resume|TimedOut`; timeout dùng instant decision fallback.

Trạng thái/evidence chi tiết nằm tại [chương trình kiểm chứng](../implementation/decision-validation-program.md#decision-matrix).

| ID | Quyết định | Owner | Gate | Trạng thái |
| --- | --- | --- | --- | --- |
| MCV-D01 | Layout/preset production | Gameplay UX | Automated preset/binding pass; còn thiếu latency/mis-tap + left-hand playtest | `InValidation` |
| MCV-D02 | Ranked assist allowlist | Competitive Design | Typed context pass; còn thiếu fairness/matchmaking disclosure review | `InValidation` |
| MCV-D03 | VAR eligibility/frequency | Rules + Broadcast UX | Match duration và comprehension test | `TestReady` |
| MCV-D04 | Replay evidence retention | Integrity + Privacy | Dispute/privacy/storage review | `Blocked` |
