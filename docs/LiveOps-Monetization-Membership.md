# LIVEOPS, NẠP TIỀN & HỆ THỐNG HỘI VIÊN (THAM CHIẾU FC MOBILE VN)

## 1. Mục tiêu hệ thống

- Tận dụng mô hình live service với sự kiện liên tục, login rewards, giftcode và quà cộng đồng.
- Thiết kế hệ thống Hội viên/VIP vừa khuyến khích nạp tiền, vừa minh bạch quyền lợi theo tháng.
- Đảm bảo các luồng nạp tiền, nhận quà, redeem code và quà sự kiện không gây rối hoặc tạo cảm giác pay-to-win tuyệt đối.

## 2. Nạp tiền & Entitlement

### 2.1 Luồng nạp cơ bản

1. Người chơi mở Store/Nạp trong game.
2. Chọn gói nạp (gói Kim cương, gói ưu đãi theo sự kiện, season pass…).
3. Gọi API nền tảng (Google Play, App Store…) để thanh toán.
4. Xác thực receipt phía server, tạo entitlement (currency, vật phẩm, điểm hội viên).
5. Gửi thông báo trong game (popup, thư inbox) và cập nhật tài khoản.

### 2.2 Quy tắc sản phẩm

- Đảm bảo transaction id và receipt được log đầy đủ để chống double-charge và hỗ trợ hoàn tiền.
- Mọi entitlement phải gắn với tài khoản (account-level), không gắn device.
- Có cơ chế retry nếu mạng lỗi giữa chừng, tránh mất tiền mà không nhận vật phẩm.

## 3. Hệ thống Hội viên/VIP (tham chiếu FC Mobile VN)

### 3.1 Lõi tính điểm hội viên

- Mỗi 1.000 VNĐ thanh toán = 1 điểm hội viên (áp dụng cho tất cả giao dịch nạp hợp lệ).
- Hệ thống tính **cấp bậc tháng** dựa trên tổng điểm hội viên của tháng trước.
- Ví dụ: cấp bậc Hội viên tháng 2 = tổng điểm tháng 1; cấp bậc tháng 3 = tổng điểm tháng 2…

### 3.2 Cấp bậc gợi ý

| Hạng | Ngưỡng điểm tháng | Ghi chú |
|---|---|---|
| Đồng | 200 | Hạng hội viên cơ bản, mở quà ngày |
| Bạc | 500 | Mở thêm quà tuần, một số ưu đãi sự kiện |
| Vàng | 1.000 (có thể duy trì từ 900 điểm) | Quà sinh nhật, cửa hàng hội viên riêng, quà đặc biệt |

### 3.3 Quyền lợi hội viên

- **Quà ngày**: gửi vào thư cuối mỗi ngày nếu HLV có đăng nhập.
- **Quà tuần**: gửi vào thư vào một ngày cố định (ví dụ thứ 7).
- **Quà sinh nhật**: gửi trong vòng 2 ngày kể từ ngày sinh đã khai báo.
- **Cửa hàng Hội viên**: shop riêng cho hạng Vàng, bán vật phẩm giới hạn theo tháng (thẻ cầu thủ, token, cosmetics…).

### 3.4 Đăng ký thông tin

- Để nhận quyền lợi Hội viên, người chơi phải khai báo: họ tên, ngày sinh, số điện thoại, địa chỉ (tùy quy định region).
- Dữ liệu này phải được xử lý theo pháp luật bảo vệ dữ liệu cá nhân, có consent rõ ràng và quyền yêu cầu xóa.

## 4. Inbox & Reward Calendar

### 4.1 Thư trong game (Inbox)

- Inbox là trung tâm nhận quà: quà login, quà tuần, quà sự kiện, thông báo hệ thống, bồi thường lỗi.
- Mỗi thư có: tiêu đề, nội dung, phần thưởng, hạn dùng, sender (System/Community/Esports).

### 4.2 Lịch thưởng

- Thiết kế lịch quà ngày/tuần/tháng gắn với season pass và hội viên.
- Cho người chơi xem trước toàn bộ quà có thể nhận trong tháng nếu duy trì đăng nhập/nhiệm vụ.

## 5. Giftcode / Redeem Code

- Hỗ trợ nhập giftcode từ NPH/đối tác/thương hiệu để đổi Coin/Kim cương/vật phẩm.
- Mỗi code có: prefix, nguồn, thời hạn, số lần dùng tối đa theo account, nội dung quà.
- Cần hệ thống chống abuse (giới hạn per account, per IP, per thiết bị).

## 6. Sự kiện liveops

- Sự kiện theo chủ đề (Tết, Lễ Hội Mùa Hè, đồng hành World Cup…) với cơ chế nhiệm vụ, tích điểm và đổi quà.
- Gắn kết sự kiện ingame với giải đấu thực tế (đồng hành Vô địch Thế giới, Creators Cup, FVSL…) để tăng gắn bó.
- Mọi sự kiện cần spec rõ: mục tiêu, thời gian, luồng nhiệm vụ, quà, cách hiển thị trên Home, và cách kết thúc.
