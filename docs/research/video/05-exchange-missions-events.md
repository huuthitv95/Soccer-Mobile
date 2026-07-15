# Phân tích video 05 — Ngoại hình, trao đổi, nhiệm vụ và sự kiện

> [Chỉ mục](../../index.md) › [Nghiên cứu video](./ui-pattern-synthesis.md) › Trao đổi và sự kiện

## 0. Mục lục

- [1. Phạm vi và bằng chứng](#scope)
- [2. Timeline](#timeline)
- [3. Screen inventory](#screens)
- [4. Hành vi xuyên module](#behavior)
- [5. Hàm ý thiết kế](#implications)

<a id="scope"></a>

## 1. Phạm vi và bằng chứng

| Thuộc tính | Giá trị |
| --- | --- |
| Video | [`05-exchange-missions-events.mp4`](../../../references/fc-mobile-vn/videos/05-exchange-missions-events.mp4) |
| Thời lượng | 00:53.035 |
| Khung hình | 1920×1080, landscape |
| Contact sheet | [Mở ảnh](../../../references/video-analysis/05-exchange-missions-events/contact-sheet.jpg) |

**Nhãn bằng chứng:** Các chi tiết là **Quan sát từ video**. Whisper/Tesseract không có trên máy; phân tích bỏ qua audio và không dùng OCR tự động.

<a id="timeline"></a>

## 2. Timeline

| Thời gian | Màn hình/trạng thái | Quan sát chính |
| --- | --- | --- |
| 00:00–00:10 | Ngoại hình cầu thủ | Rail category dọc cho tay áo/áo trong/quần/tất/giày; thumbnail item và preview model 3D trực tiếp. |
| 00:11–00:17 | Club → Home | Quay về Club rồi Home, shell và hero campaign phục hồi. |
| 00:18–00:25 | Trao đổi | Hai tab Trao đổi vật phẩm/cầu thủ; category ngang, card công thức và số lượng sở hữu/yêu cầu. |
| 00:26–00:31 | Huy hiệu đội | Card huy hiệu theo event, thanh cấp/tiến độ, tab Lịch sử/Đã trang bị/Phần thưởng. |
| 00:32–00:37 | Nhiệm vụ Vietnam All-Star | Rail event trái, task list ở giữa, phần thưởng phải; CTA “Đi” trên từng nhiệm vụ. |
| 00:38–00:41 | Thử thách đấu xếp hạng | Cùng shell nhiệm vụ nhưng đổi campaign, banner và progress. |
| 00:42–00:47 | Giải Vô Địch Thế Giới | Task event khác trong cùng template, có thời gian còn lại và reward panel. |
| 00:48–00:53 | Home | Trở lại hero carousel; shell không đổi. |

Key frame: [00:00](../../../references/video-analysis/05-exchange-missions-events/key-00m00s.jpg), [00:08](../../../references/video-analysis/05-exchange-missions-events/key-00m08s.jpg), [00:20](../../../references/video-analysis/05-exchange-missions-events/key-00m20s.jpg), [00:28](../../../references/video-analysis/05-exchange-missions-events/key-00m28s.jpg), [00:36](../../../references/video-analysis/05-exchange-missions-events/key-00m36s.jpg), [00:40](../../../references/video-analysis/05-exchange-missions-events/key-00m40s.jpg), [00:44](../../../references/video-analysis/05-exchange-missions-events/key-00m44s.jpg).

<a id="screens"></a>

## 3. Screen inventory

| Screen | Layout hierarchy | CTA/trạng thái |
| --- | --- | --- |
| Ngoại hình item | Category rail trái → model preview → item grid phải → confirm | Selected/check, owned/locked, no-change disabled confirm |
| Trao đổi | Header/back → event tab ngang → recipe/card → sở hữu/yêu cầu → exchange action | Eligible/ineligible, quantity, expiry; video chưa cho thấy confirmation |
| Huy hiệu | Header/event → badge cards → progress → tabs history/equipped/rewards | Equipped, level progress, claimable reward |
| Nhiệm vụ event | Rail campaign trái → banner/task list giữa → reward preview phải | CTA “Đi”, progress current/target, completed/claimable |

<a id="behavior"></a>

## 4. Hành vi xuyên module

- **Template reuse:** ba campaign nhiệm vụ dùng cùng khung, chỉ đổi banner, màu, task/reward và icon rail; hỗ trợ vận hành nội dung theo cấu hình.
- **Contextual CTA:** “Đi” nằm cạnh từng task và có thể deep-link tới mode liên quan; “Phần thưởng” là tab ở badge screen; “Xác nhận” ở customization chỉ active khi có thay đổi hợp lệ.
- **Progressive disclosure:** danh sách category/event ở rail, chi tiết tại center canvas, phần thưởng hoặc action ở panel phải.
- **State feedback:** selected border/check, progress bar, current/target counter, tab underline và notification dot. Video không cho thấy loading/error/empty/expired trong các module này.
- **Transition:** quay về Home phục hồi vị trí shell; chuyển event trong nhiệm vụ là thay content tại chỗ thay vì scene toàn màn.
- **Animation/effect:** model 3D idle và preview item là phản hồi chính; campaign screen dùng reveal/fade ngắn. Không có đủ bằng chứng để đặc tả duration/easing.

<a id="implications"></a>

## 5. Hàm ý thiết kế

**Suy luận thiết kế:** Exchange, badge và mission đều phụ thuộc catalog/event version, inventory/progress và reward entitlement; nếu mỗi module tự giữ state sẽ dễ lệch hạn dùng hoặc double claim.

**Đề xuất cho Soccer Mobile Pro:** dùng event definition versioned và grant ledger server-authoritative; exchange phải preview input/output, khóa item, xác nhận giao dịch và idempotency key. Mission deep link cần kiểm tra eligibility và fallback. Mọi module phải có loading, empty, offline, expired, maintenance, claim conflict và recovery; timer dùng server time. Customization preview không được tiêu entitlement trước khi xác nhận thành công.
