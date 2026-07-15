# Phân tích video 04 — Câu lạc bộ, đội hình và tùy biến

> [Chỉ mục](../../index.md) › [Nghiên cứu video](./ui-pattern-synthesis.md) › Câu lạc bộ và đội hình

## 0. Mục lục

- [1. Phạm vi và bằng chứng](#scope)
- [2. Timeline](#timeline)
- [3. Screen inventory và layout](#screens)
- [4. Model 3D và tùy biến](#customization)
- [5. Hàm ý thiết kế](#implications)

<a id="scope"></a>

## 1. Phạm vi và bằng chứng

| Thuộc tính | Giá trị |
| --- | --- |
| Video | [`04-club-squad-customization.mp4`](../../../references/fc-mobile-vn/videos/04-club-squad-customization.mp4) |
| Thời lượng | 00:52.367 |
| Khung hình | 1920×1080, landscape |
| Contact sheet | [Mở ảnh](../../assets/video-analysis/04-club-squad-customization/contact-sheet.jpg) |

**Nhãn bằng chứng:** Mô tả là **Quan sát từ video**. Video chứng minh cách model được trình bày, không chứng minh pipeline import/model bundle nội bộ. Whisper/Tesseract không khả dụng; audio bị bỏ qua, chữ đọc thủ công.

<a id="timeline"></a>

## 2. Timeline

| Thời gian | Màn hình/trạng thái | Quan sát chính |
| --- | --- | --- |
| 00:00–00:06 | Loading/entry | Nền sân tối và title campaign ngắn trước khi vào Câu lạc bộ. |
| 00:07–00:13 | Club hub | Card đội hình ở giữa; rail chức năng phải; model cầu thủ 3D xuất hiện bên trái sau một nhịp tải. |
| 00:14–00:19 | Kho cầu thủ | Grid card, bộ lọc OVR, card chọn có panel hành động Chi tiết/Huấn luyện/Thăng hạng/Điểm kỹ năng. |
| 00:20–00:25 | Club hub → Phòng thay đồ | Model 3D và đội hình trở lại; chọn tile có notification dot. |
| 00:26–00:33 | Locker room | Không gian 3D, mannequin/cầu thủ; panel chọn đội nhà và đồng phục. |
| 00:34–00:45 | Sân nhà | Preview sân full canvas, tab Chung/Sự kiện, danh sách sân và điều kiện mở khóa; lựa chọn đổi ánh sáng/thời tiết. |
| 00:46–00:52 | Ngoại hình hub | Grid category quanh model: đội nhà, bóng, đồng phục, biểu cảm, số, ngoại hình, sân vận động. |

Key frame: [00:08](../../assets/video-analysis/04-club-squad-customization/key-00m08s.jpg), [00:16](../../assets/video-analysis/04-club-squad-customization/key-00m16s.jpg), [00:28](../../assets/video-analysis/04-club-squad-customization/key-00m28s.jpg), [00:36](../../assets/video-analysis/04-club-squad-customization/key-00m36s.jpg), [00:48](../../assets/video-analysis/04-club-squad-customization/key-00m48s.jpg).

<a id="screens"></a>

## 3. Screen inventory và layout

| Screen | Layout hierarchy | CTA/modal/state |
| --- | --- | --- |
| Club hub | Back/title → model 3D trái → squad card giữa → rail TCG/Cầu thủ/Phòng thay đồ phải | Tile có badge; model deferred loading |
| Player inventory | Filter/sort → grid card → selected preview → action stack | Chọn card, chi tiết, huấn luyện, thăng hạng, điểm kỹ năng; locked/disabled action |
| Locker room | Back/title → 3D environment → profile/team left → category right | Đội nhà, đồng phục; selection highlight và confirm |
| Stadium | Preview full canvas → tab → selector phải → condition text → confirm | Locked/unlocked, selected, day/weather option |
| Appearance hub | Model center-left → category tile quanh phải/trái | Badge new, preview change, confirm/cancel |

Hierarchy ưu tiên “xem kết quả trước, chỉnh ở panel sau”: model hoặc sân chiếm phần lớn canvas; selector nằm biên phải/trái. Các hub dùng nền stadium/locker room nhất quán, giữ back/title và icon tiện ích ở top bar.

<a id="customization"></a>

## 4. Model 3D và tùy biến

- **Model presentation:** model cầu thủ vào sau UI ở khoảng 00:10–00:12, gợi ý tải bất đồng bộ hoặc transition riêng. Idle pose giữ nhân vật sống động nhưng không che nội dung.
- **Preview:** thay category giữ model/stadium tại chỗ, cho phép đánh giá trực quan trước xác nhận.
- **Asset state:** item có thumbnail, tên, selected border/check và trạng thái khóa; sân khóa hiển thị điều kiện mở bằng tiến trình.
- **Transition:** giữa Club/Locker/Stadium dùng nền tối/loading ngắn rồi reveal environment; không đủ bằng chứng để đo easing.
- **Failure/empty:** video không cho thấy missing model, download asset, low-memory fallback, inventory trống hay lỗi tải preview.

<a id="implications"></a>

## 5. Hàm ý thiết kế

**Suy luận thiết kế:** UI có thể hoạt động trước khi model hoàn tất, nên view state và asset state phải tách nhau. Video không cung cấp bằng chứng về cách FC Mobile VN gọi player database hoặc thêm model 3D; không được dùng clip để kết luận endpoint, bundle hay pipeline sản xuất.

**Đề xuất cho Soccer Mobile Pro:** dùng catalog versioned trỏ từ player identity/card instance tới visual profile và Addressable label; tải model async với silhouette/fallback, hủy request khi đổi selection và giới hạn memory. Tùy biến phải preview cục bộ nhưng entitlement/selection lưu server-authoritative, có confirm, rollback khi save thất bại, asset compatibility theo client version và test LOD trên thiết bị mục tiêu.
