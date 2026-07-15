# Phân tích video 03 — Hồ sơ, bạn bè và cài đặt

> [Chỉ mục](../../index.md) › [Nghiên cứu video](./ui-pattern-synthesis.md) › Hồ sơ và cài đặt

## 0. Mục lục

- [1. Phạm vi và bằng chứng](#scope)
- [2. Timeline](#timeline)
- [3. Screen inventory](#screens)
- [4. Cấu trúc cài đặt](#settings)
- [5. Hàm ý thiết kế](#implications)

<a id="scope"></a>

## 1. Phạm vi và bằng chứng

| Thuộc tính | Giá trị |
| --- | --- |
| Video | [`03-profile-settings.mp4`](../../../references/fc-mobile-vn/videos/03-profile-settings.mp4) |
| Thời lượng | 01:01.033 |
| Khung hình | 1920×1080, landscape |
| Contact sheet | [Mở ảnh](../../assets/video-analysis/03-profile-settings/contact-sheet.jpg) |

**Nhãn bằng chứng:** Nội dung dưới đây là **Quan sát từ video** ở chế độ visual-first. Whisper và Tesseract không khả dụng; không phiên âm audio và không OCR tự động.

<a id="timeline"></a>

## 2. Timeline

| Thời gian | Màn hình/trạng thái | Quan sát chính |
| --- | --- | --- |
| 00:00–00:02 | Đổi tên người dùng | Modal nhập tên với chi phí/điều kiện đổi, CTA Hủy bỏ và Xác nhận. |
| 00:03–00:07 | Hồ sơ cầu thủ | Ba tab hồ sơ; card OVR, tên, huy hiệu CLB, sơ đồ đội hình và lịch sử. |
| 00:08–00:20 | Trở về Home | Transition nền sân/loading rồi Home shell phục hồi. |
| 00:21–00:29 | Social/FV đã chơi/Bạn bè | Tab social trên cùng; màn Bạn bè có trạng thái danh sách trống và bộ đếm 0/100. |
| 00:30–00:34 | Home | Quay lại shell. |
| 00:35–00:45 | Cài đặt tổng | Modal lớn hai cột: tài khoản, gameplay, âm thanh, ngôn ngữ, đơn vị, hỗ trợ và xóa tài khoản. |
| 00:46–01:01 | Gameplay settings | Danh sách toggle cuộn dọc cho điều khiển, hỗ trợ trực quan, power shot, chuyển cầu thủ và PlayStyles. |

Key frame: [00:00](../../assets/video-analysis/03-profile-settings/key-00m00s.jpg), [00:04](../../assets/video-analysis/03-profile-settings/key-00m04s.jpg), [00:28](../../assets/video-analysis/03-profile-settings/key-00m28s.jpg), [00:36](../../assets/video-analysis/03-profile-settings/key-00m36s.jpg), [00:48](../../assets/video-analysis/03-profile-settings/key-00m48s.jpg), [00:56](../../assets/video-analysis/03-profile-settings/key-00m56s.jpg).

<a id="screens"></a>

## 3. Screen inventory

| Screen | Layout hierarchy | CTA/trạng thái |
| --- | --- | --- |
| Đổi tên | Title + close → input → điều kiện/chi phí → Hủy/Xác nhận | Confirm disabled khi input chưa hợp lệ; cần error và insufficient currency |
| Hồ sơ | Header/back → tab → OVR/card → danh tính/CLB/formation → lịch sử | Tab active, empty profile slot, edit name |
| Social | Top tabs → content list → counter | Empty list rõ ràng nhưng video không cho thấy search/error |
| Settings | Modal overlay → nhóm hàng hai cột → icon edit/navigation → close | Account UID/copy, logout, gameplay, graphics, audio, language, units, support/delete |
| Gameplay | Header/back/close → hàng setting + mô tả + toggle → cuộn | On/off state bằng teal/gray; có video trợ giúp ở một hàng |

Modal settings chiếm phần lớn chiều ngang nhưng vẫn để lộ nền gradient/sân, tạo cảm giác overlay thay vì scene mới. Mỗi hàng dùng title đậm, mô tả nhỏ và action icon ở mép phải. Các setting con mở trang/modal cùng style, giữ back ở trái và close ở phải.

<a id="settings"></a>

## 4. Cấu trúc cài đặt

Các mục đọc được từ frame gồm liên kết tài khoản/UID, đăng xuất, Gameplay, Đồ họa, Âm thanh, Bình luận và âm thanh, Ngôn ngữ, Đơn vị, Cẩm nang hướng dẫn, Dịch vụ CSKH, Phản hồi người chơi, Pháp lý, Danh hiệu và Xóa tài khoản. Trang Gameplay quan sát được các toggle liên quan chế độ điều khiển, chỉ dẫn cử chỉ, đổi sân giữa hiệp, phản hồi trực quan, bản đồ nhỏ, căn cảnh khi Power Shot, mẹo điều khiển, chuyền nâng cao, chạm/chạm hai lần để yêu cầu chuyền bóng, tự động chuyển cầu thủ và biểu tượng PlayStyles.

- **Navigation:** top-level settings là hub; nhóm gameplay là drill-down có back và close.
- **Feedback:** toggle đổi màu tức thì; video không cho thấy save CTA, gợi ý auto-save.
- **Scrollable state:** thanh cuộn không nổi bật; nội dung cắt ở đáy báo còn setting bên dưới.
- **Risk action:** logout và xóa tài khoản nằm cùng hub; video không cung cấp bằng chứng về confirmation/reauthentication.
- **Accessibility:** mô tả chữ nhỏ, nhiều hàng dày đặc; cần text scaling, focus state, hit target tối thiểu và mô tả không chỉ dựa vào màu.

<a id="implications"></a>

## 5. Hàm ý thiết kế

**Suy luận thiết kế:** Settings là projection của nhiều domain khác nhau (account, controls, presentation, audio, locale, legal/support). Auto-save giúp nhanh nhưng phải có feedback và rollback khi persistence thất bại.

**Đề xuất cho Soccer Mobile Pro:** schema setting phải có owner, default theo thiết bị, phạm vi local/account, version migration và telemetry consent. Tách action nguy hiểm khỏi setting thông thường; xóa tài khoản cần re-auth, thời gian chờ và trạng thái khôi phục. Language đổi được từ shell và có preview/restart rule rõ; gameplay toggle phải đồng bộ với HUD/control context và có “khôi phục mặc định”.
