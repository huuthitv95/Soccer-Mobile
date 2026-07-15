# Competitive integrity, báo cáo gian lận và esports

> [Chỉ mục](../index.md) · [Match controls và VAR](match-controls-set-pieces-and-var.md) · [Live data](../operations/live-data-and-operations.md)

## 1. Mục tiêu

- Bảo vệ người chơi chân chính khỏi hành vi gian lận, sử dụng phần mềm thứ ba.
- Thiết kế luồng báo cáo gian lận rõ ràng, dễ dùng và có phản hồi.
- Chuẩn hóa chế tài vi phạm, liên kết với hệ thống giải đấu (FVSL-style) và môi trường esports.

**Input:** match log đã ký, account/device risk signals, report và tournament state.
**Output:** case ID, enforcement decision, appeal state và reward eligibility.
**Authority:** server xác nhận kết quả, telemetry, sanction và reward; client chỉ thu input/report và trình bày trạng thái.

## 2. Các hành vi gian lận phổ biến

- Sử dụng phần mềm thứ ba/MOD APK/ứng dụng trái phép để thay đổi hành vi game.
- Can thiệp vào file game, vượt qua hệ thống bảo mật, sử dụng phiên bản không chính thức.
- Gian lận thẻ ngân hàng/nạp tiền bằng phương thức có dấu hiệu lừa đảo.
- Thông đồng dàn xếp kết quả trận, win-trade, AFK cố ý trong ranked.

## 3. Luồng báo cáo trong game

**Thông tin công khai đã xác minh:** Garena mô tả bốn bước: hồ sơ đối thủ → biểu tượng “Cái Loa” → “Nội dung không phù hợp” → lý do “Gian lận” → gửi; thông tin được bảo mật và đội ngũ giám sát kiểm tra trước quyết định. Nguồn [S17](../research/fc-mobile-vn-source-register.md#claim-register), truy cập 15/07/2026. Soccer Mobile Pro giữ pattern dễ tìm này nhưng contract bên dưới là đề xuất riêng.

1. Người chơi mở màn hình kết quả trận đấu.
2. Nhấn nút "Báo cáo" cạnh tên đối thủ.
3. Chọn lý do (gian lận phần mềm, hành vi phá game, chat xúc phạm, nghi dàn xếp…).
4. Gửi kèm mô tả chi tiết và, nếu có, ảnh/video bằng form ngoài game.
5. Hệ thống gắn report với match ID, telemetry, ping, input summary và kết quả authoritative; không cho client sửa evidence.

## 4. Cơ chế xử phạt đề xuất

| Hành vi | Xử phạt |
| --- | --- |
| Sử dụng phần mềm thứ 3 | Khóa tài khoản 30 ngày hoặc vĩnh viễn tùy mức độ; tái phạm khóa vĩnh viễn |
| Can thiệp file/bảo mật | Khóa vĩnh viễn, blacklist thiết bị |
| Gian lận nạp thẻ | Tạm khóa, yêu cầu liên hệ CS để xác minh, rollback giao dịch bất thường |
| Win-trade có hệ thống | Reset thành tích mùa, cấm tham gia ranked/giải đấu trong khoảng thời gian nhất định |

## 5. Reconnect & xử lý mất mạng

- Cho phép reconnect vào trận trong thời gian ngắn nếu bị mất kết nối tạm thời.
- Nếu người chơi rời trận quá lâu hoặc cố ý thoát, coi là thua và áp dụng penalty phù hợp.
- Telemetry cần ghi rõ: thời gian disconnect, trạng thái mạng, hành vi input cuối cùng.

## 6. Luồng giải đấu & esports

- Hỗ trợ tạo giải đấu ingame: đăng ký, check-in, bracket, lịch thi đấu, luật.
- Kết nối với giải FVSL-style: tổng thưởng lớn, nhiều vòng tuyển chọn, broadcast.
- Có chế độ spectator cho trận đấu quan trọng (cam tự động hoặc manual).

## 7. Công cụ CS & vận hành

- Bảng điều khiển cho đội vận hành: xem log trận, hành vi báo cáo, lịch sử xử phạt.
- Hệ thống ticket liên kết tài khoản, hỗ trợ appeal vi phạm.
- Quy trình rà soát định kỳ các tài khoản có dấu hiệu gian lận hoặc khiếu nại nhiều.

## 8. Analytics, privacy và abuse

- Event tối thiểu: `report_opened`, `report_submitted`, `case_triaged`, `sanction_applied`, `appeal_submitted`, `reconnect_result`, `tournament_check_in`.
- Không lưu raw input vô thời hạn; retention theo mục đích chống gian lận và privacy policy.
- Chống spam report, brigading, false evidence, account farming và replay/tamper match log.

## 9. Accessibility, QA và rollback

- Report form dùng label rõ, keyboard/screen-reader accessible và không buộc nhập mô tả dài.
- Test disconnect/reconnect, duplicate report, late result, bracket conflict, sanction expiry và appeal restore.
- Enforcement rule versioned; rollback rule không tự động gỡ sanction đã review mà tạo re-evaluation queue.
- Acceptance: mọi report có acknowledgement, sanction có reason code/audit trail, và tournament reward chỉ grant sau result verification.
