# Phân tích video 02 — Home và live operations

> [Chỉ mục](../../index.md) › [Nghiên cứu video](./ui-pattern-synthesis.md) › Home và liveops

## 0. Mục lục

- [1. Phạm vi và bằng chứng](#scope)
- [2. Timeline](#timeline)
- [3. Kiến trúc Home](#home-layout)
- [4. Liveops và trạng thái](#liveops)
- [5. Hàm ý thiết kế](#implications)

<a id="scope"></a>

## 1. Phạm vi và bằng chứng

| Thuộc tính | Giá trị |
| --- | --- |
| Video | [`02-home-liveops.mp4`](../../../references/fc-mobile-vn/videos/02-home-liveops.mp4) |
| Thời lượng | 01:00.333 |
| Khung hình | 1920×1080, landscape |
| Contact sheet | [Mở ảnh](../../../references/video-analysis/02-home-liveops/contact-sheet.jpg) |
| Chế độ phân tích | Visual-first, lấy mẫu 4 giây và key frame chọn lọc |

**Nhãn bằng chứng:** Các mô tả là **Quan sát từ video**. Whisper/Tesseract không khả dụng, nên audio bị bỏ qua và không có OCR tự động; nhãn màn hình được đọc thủ công.

<a id="timeline"></a>

## 2. Timeline

| Thời gian | Màn hình/trạng thái | Quan sát chính |
| --- | --- | --- |
| 00:00–00:13 | Home carousel | Hero thay đổi giữa World’s Game, Giải đấu, Signature Archive và Giao hữu; CTA “Chơi ngay” giữ vị trí ổn định. |
| 00:14–00:21 | Nhiệm vụ khởi động/hằng ngày | Danh sách mục tiêu bên trái, preview phần thưởng bên phải; loading spinner xuất hiện khi đổi tab. |
| 00:22–00:27 | Đăng nhập hằng ngày | Lịch phần thưởng dạng grid, progress/tổng cộng phía dưới và CTA nhận màu vàng. |
| 00:28–00:34 | Quay lại Home | Hero carousel tiếp tục tự động đổi nội dung trong cùng khung. |
| 00:35–00:43 | Sổ siêu sao/pass | Track phần thưởng ngang có tier miễn phí/trả phí, marker tiến độ và CTA mua. |
| 00:44–00:47 | Home | Trở lại shell mà không mất top/bottom navigation. |
| 00:48–00:55 | Event webview | Trang “Hành Trình Chinh Phục” tải trong lớp web có rail thumbnail trái, nút đóng và refresh phải. |
| 00:56–01:00 | Home | Hero “Record Holders”; shell phục hồi sau khi đóng event. |

Key frame: [00:00](../../../references/video-analysis/02-home-liveops/key-00m00s.jpg), [00:16](../../../references/video-analysis/02-home-liveops/key-00m16s.jpg), [00:24](../../../references/video-analysis/02-home-liveops/key-00m24s.jpg), [00:36](../../../references/video-analysis/02-home-liveops/key-00m36s.jpg), [00:48](../../../references/video-analysis/02-home-liveops/key-00m48s.jpg), [00:52](../../../references/video-analysis/02-home-liveops/key-00m52s.jpg).

<a id="home-layout"></a>

## 3. Kiến trúc Home

Home dùng ba tầng điều hướng ổn định:

1. **Top bar:** hồ sơ ở trái; nhiều loại tiền tệ/tài nguyên và icon tiện ích/cài đặt ở phải.
2. **Content canvas:** hero lớn bên trái với CTA chính; tile nội dung phụ ở phải gồm chiêu mộ, câu lạc bộ và chơi nhanh.
3. **Bottom nav:** Nhiệm vụ, Huy hiệu đội, Trao đổi, Cửa hàng; active state dùng màu/độ sáng nổi bật.

Rail icon bên trái cung cấp shortcut liveops và badge đỏ báo nội dung mới. Hero là carousel tự động nhưng CTA giữ cố định, giúp thay campaign mà không thay mental model. Nền, key art và accent đổi theo campaign; shell điều hướng không đổi.

<a id="liveops"></a>

## 4. Liveops và trạng thái

| Module | Layout/CTA | Trạng thái quan sát được | Khoảng trống bằng chứng |
| --- | --- | --- | --- |
| Nhiệm vụ | Tab ngày, danh sách task, tiến độ, CTA “Đi/Nhận”, preview reward | Claimable, incomplete, loading | Empty, expired, claim failure |
| Daily login | Calendar/grid reward, milestone tổng cộng, CTA nhận | Current day, claimed/check mark, locked future day | Missed day, make-good, reconnect |
| Pass/sổ siêu sao | Hai reward rail, level marker, CTA mua/thu thập tất cả | Locked/available reward, scroll ngang | Purchase failure, entitlement recovery |
| Event webview | Rail campaign, header tabs, map/node content, close/refresh | Initial blank/loading rồi content | Offline cache, auth expiry, deep-link failure |

Các màn hình dùng spinner giữa canvas khi tải; shell hoặc nền sân đôi khi vẫn tồn tại để giảm cảm giác rời context. Animation quan sát được chủ yếu là carousel slide/fade, progress reveal và chuyển panel; video không đủ frame để xác nhận timing/easing chính xác.

<a id="implications"></a>

## 5. Hàm ý thiết kế

**Suy luận thiết kế:** Shell tách khỏi campaign content cho phép liveops đổi key art, tile và web content mà không cần thay navigation nền. Badge đỏ và nhiều CTA “Nhận/Đi” tạo vòng lặp quay lại nhưng dễ gây quá tải chú ý.

**Đề xuất cho Soccer Mobile Pro:** định nghĩa Home bằng slot cấu hình từ server/CMS nhưng giữ navigation và entitlement ở client có versioning; mọi reward claim phải idempotent và server-authoritative. Module cần đủ `loading/empty/error/expired/maintenance`, deep link có fallback, badge có quy tắc ưu tiên, carousel có pause và giảm chuyển động cho accessibility.
