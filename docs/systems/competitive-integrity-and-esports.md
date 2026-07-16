# Competitive integrity, báo cáo gian lận và esports

> [Chỉ mục](../index.md) · [Match controls và VAR](match-controls-set-pieces-and-var.md) · [Live data](../operations/live-data-and-operations.md)

## 0. Mục lục

- [1. Mục tiêu](#goal)
- [2. Các hành vi gian lận phổ biến](#abuse-types)
- [3. Luồng báo cáo trong game](#report-flow)
- [4. Case, sanction và appeal state machine](#case-state)
- [5. Reconnect và xử lý mất mạng](#reconnect)
- [6. Luồng giải đấu và esports](#esports-flow)
- [7. Công cụ CS và vận hành](#operations-tooling)
- [8. Analytics, privacy và abuse](#analytics-privacy)
- [9. Accessibility, QA và rollback](#quality)
- [10. Version, migration và decision register](#version-decisions)

<a id="goal"></a>

## 1. Mục tiêu

- Bảo vệ người chơi chân chính khỏi hành vi gian lận, sử dụng phần mềm thứ ba.
- Thiết kế luồng báo cáo gian lận rõ ràng, dễ dùng và có phản hồi.
- Chuẩn hóa chế tài vi phạm, liên kết với hệ thống giải đấu (FVSL-style) và môi trường esports.

**Input:** match log đã ký, account/device risk signals, report và tournament state.
**Output:** case ID, enforcement decision, appeal state và reward eligibility.
**Authority:** server xác nhận kết quả, telemetry, sanction và reward; client chỉ thu input/report và trình bày trạng thái.

<a id="abuse-types"></a>

## 2. Các hành vi gian lận phổ biến

- Sử dụng phần mềm thứ ba/MOD APK/ứng dụng trái phép để thay đổi hành vi game.
- Can thiệp vào file game, vượt qua hệ thống bảo mật, sử dụng phiên bản không chính thức.
- Gian lận thẻ ngân hàng/nạp tiền bằng phương thức có dấu hiệu lừa đảo.
- Thông đồng dàn xếp kết quả trận, win-trade, AFK cố ý trong ranked.

<a id="report-flow"></a>

## 3. Luồng báo cáo trong game

**Thông tin công khai đã xác minh:** Garena mô tả bốn bước: hồ sơ đối thủ → biểu tượng “Cái Loa” → “Nội dung không phù hợp” → lý do “Gian lận” → gửi; thông tin được bảo mật và đội ngũ giám sát kiểm tra trước quyết định. Nguồn [S17](../research/fc-mobile-vn-source-register.md#claim-register), truy cập 15/07/2026. Soccer Mobile Pro giữ pattern dễ tìm này nhưng contract bên dưới là đề xuất riêng.

1. Người chơi mở màn hình kết quả trận đấu.
2. Nhấn nút "Báo cáo" cạnh tên đối thủ.
3. Chọn lý do (gian lận phần mềm, hành vi phá game, chat xúc phạm, nghi dàn xếp…).
4. Gửi kèm mô tả chi tiết và, nếu có, ảnh/video bằng form ngoài game.
5. Hệ thống gắn report với match ID, telemetry, ping, input summary và kết quả authoritative; không cho client sửa evidence.

<a id="case-state"></a>

## 4. Case, sanction và appeal state machine

```text
ReportReceived → Triaged → Investigating → Decided → Notified → Closed
                         ↘ NeedMoreEvidence       ↘ Appealed → Upheld|Modified|Revoked
Sanction: Proposed → Reviewed → Active → Expired|Revoked
```

`IntegrityCase` gồm case ID, subject pseudonymous ID, scoped evidence refs, rules version, investigator/audit và retention class. `SanctionDecision` gồm violation code, evidence standard, severity band, scope, start/end, reviewer và appeal eligibility. Client không tự thu raw device data hoặc quyết định vi phạm.

| Severity band | Loại biện pháp khả dụng | Gate bắt buộc |
| --- | --- | --- |
| Education/warning | Cảnh báo, hướng dẫn, mute hạn chế | Rule rõ, thông báo và audit |
| Temporary restriction | Tạm chặn chat/market/ranked/tournament | Proportionality, duration config, human review theo risk |
| Result/economy correction | Hủy result/reward bất hợp lệ, ledger reconcile | Signed evidence, không balance âm im lặng |
| Account enforcement | Suspend/terminate theo region/policy | Dual review, legal/privacy, appeal |

Không hard-code 30 ngày, khóa vĩnh viễn hay blacklist thiết bị như policy chung. Device signal chỉ là risk input; biện pháp theo thiết bị cần necessity/proportionality, false-positive test và privacy/legal approval. Enforcement matrix versioned theo region và decision register.

<a id="reconnect"></a>

## 5. Reconnect & xử lý mất mạng

- Reconnect dùng `Connected → Interrupted → Reconnecting → Resynced|ForfeitPending → Resolved`; cửa sổ thời gian là config theo mode/rules, chưa khóa trước load/network/fairness test.
- Nếu hết cửa sổ, authoritative server xác nhận forfeit/result theo reason code; không kết luận “cố ý” chỉ từ một lần disconnect.
- Telemetry cần ghi rõ: thời gian disconnect, trạng thái mạng, hành vi input cuối cùng.

<a id="esports-flow"></a>

## 6. Luồng giải đấu & esports

- Hỗ trợ tạo giải đấu ingame: đăng ký, check-in, bracket, lịch thi đấu, luật.
- Kết nối với giải FVSL-style: tổng thưởng lớn, nhiều vòng tuyển chọn, broadcast.
- Có chế độ spectator cho trận đấu quan trọng (cam tự động hoặc manual).

<a id="operations-tooling"></a>

## 7. Công cụ CS & vận hành

- Bảng điều khiển cho đội vận hành: xem log trận, hành vi báo cáo, lịch sử xử phạt.
- Hệ thống ticket liên kết tài khoản, hỗ trợ appeal vi phạm.
- Quy trình rà soát định kỳ các tài khoản có dấu hiệu gian lận hoặc khiếu nại nhiều.

<a id="analytics-privacy"></a>

## 8. Analytics, privacy và abuse

- Event tối thiểu: `report_opened`, `report_submitted`, `case_triaged`, `sanction_applied`, `appeal_submitted`, `reconnect_result`, `tournament_check_in`.
- Không lưu raw input vô thời hạn; retention theo mục đích chống gian lận và privacy policy.
- Chống spam report, brigading, false evidence, account farming và replay/tamper match log.

<a id="quality"></a>

## 9. Accessibility, QA và rollback

- Report form dùng label rõ, keyboard/screen-reader accessible và không buộc nhập mô tả dài.
- Test disconnect/reconnect, duplicate report, late result, bracket conflict, sanction expiry và appeal restore.
- Enforcement rule versioned; rollback rule không tự động gỡ sanction đã review mà tạo re-evaluation queue.
- Acceptance: mọi report có acknowledgement, sanction có reason code/audit trail, và tournament reward chỉ grant sau result verification.

<a id="version-decisions"></a>

## 10. Version, migration và decision register

Rule, evidence schema, sanction matrix và tournament/reconnect policy có version/effective time theo region. Case đang mở pin version lúc incident nhưng có thể áp dụng policy có lợi hơn theo legal decision; migration không xóa evidence/audit. Retention expiry xóa/anonymize theo class và legal hold. Rollback rule tạo re-evaluation queue, không tự động kết tội hoặc gỡ sanction.

Trạng thái/evidence chi tiết nằm tại [chương trình kiểm chứng](../implementation/decision-validation-program.md#decision-matrix).

| ID | Quyết định | Owner | Gate | Trạng thái |
| --- | --- | --- | --- | --- |
| CIE-D01 | Evidence standard và automation threshold | Integrity + Legal | False-positive/appeal audit | `Blocked` |
| CIE-D02 | Sanction matrix theo region | Policy + Legal | Proportionality/age/privacy review | `Blocked` |
| CIE-D03 | Reconnect window/forfeit | Match + Network | Packet-loss/load/fairness test | `TestReady` |
| CIE-D04 | Case/telemetry retention | Privacy + Security | Purpose, minimization, legal hold | `Blocked` |
| CIE-D05 | Appeal SLA và reviewer separation | Support Ops | Staffing rehearsal và audit | `TestReady` |
