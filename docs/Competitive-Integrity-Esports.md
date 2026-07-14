# CÔNG BẰNG THI ĐẤU, BÁO CÁO GIAN LẬN & ESPORTS

## 1. Mục tiêu

- Bảo vệ người chơi chân chính khỏi hành vi gian lận, sử dụng phần mềm thứ ba.
- Thiết kế luồng báo cáo gian lận rõ ràng, dễ dùng và có phản hồi.
- Chuẩn hóa chế tài vi phạm, liên kết với hệ thống giải đấu (FVSL-style) và môi trường esports.

## 2. Các hành vi gian lận phổ biến

- Sử dụng phần mềm thứ ba/MOD APK/ứng dụng trái phép để thay đổi hành vi game.
- Can thiệp vào file game, vượt qua hệ thống bảo mật, sử dụng phiên bản không chính thức.
- Gian lận thẻ ngân hàng/nạp tiền bằng phương thức có dấu hiệu lừa đảo.
- Thông đồng dàn xếp kết quả trận, win-trade, AFK cố ý trong ranked.

## 3. Luồng báo cáo trong game

1. Người chơi mở màn hình kết quả trận đấu.
2. Nhấn nút "Báo cáo" cạnh tên đối thủ.
3. Chọn lý do (gian lận phần mềm, hành vi phá game, chat xúc phạm, nghi dàn xếp…).
4. Gửi kèm mô tả chi tiết và, nếu có, ảnh/video bằng form ngoài game.
5. Hệ thống ghi log trận: telemetry, ping, hành vi input, nghiệm thu kết quả.

## 4. Cơ chế xử phạt đề xuất

| Hành vi | Xử phạt |
|---|---|
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
