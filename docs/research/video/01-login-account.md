# Phân tích video 01 — Đăng nhập và liên kết tài khoản

> [Chỉ mục](../../index.md) › [Nghiên cứu video](./ui-pattern-synthesis.md) › Đăng nhập và tài khoản

## 0. Mục lục

- [1. Phạm vi và bằng chứng](#scope)
- [2. Timeline](#timeline)
- [3. Screen inventory và layout](#screens)
- [4. Hành vi UI/UX](#behavior)
- [5. Hàm ý thiết kế](#implications)

<a id="scope"></a>

## 1. Phạm vi và bằng chứng

| Thuộc tính | Giá trị |
| --- | --- |
| Video | [`01-login-account.mp4`](../../../references/fc-mobile-vn/videos/01-login-account.mp4) |
| Thời lượng | 00:31.672 |
| Khung hình | 1920×864, landscape; nội dung web đăng nhập hiển thị portrait ở giữa |
| Contact sheet | [Mở ảnh](../../../references/video-analysis/01-login-account/contact-sheet.jpg) |
| Chế độ phân tích | Visual-first, lấy mẫu 2 giây và key frame chọn lọc |

**Nhãn bằng chứng:** Toàn bộ mô tả trực quan dưới đây là **Quan sát từ video**, không phải xác nhận kiến trúc nội bộ của FC Mobile VN. Whisper và Tesseract không có trên máy tại thời điểm phân tích, vì vậy không có phiên âm âm thanh hoặc OCR tự động; chữ được đọc thủ công từ frame rõ nét.

<a id="timeline"></a>

## 2. Timeline

| Thời gian | Màn hình/trạng thái | Quan sát chính |
| --- | --- | --- |
| 00:00–00:03 | Chọn tài khoản | Modal landscape trên nền sân tối; bốn lựa chọn Garena User, Google, Facebook và Guest. |
| 00:04–00:06 | Chuyển sang webview | Màn hình trung gian xám rồi trang đăng nhập portrait xuất hiện ở giữa, hai dải đen hai bên. |
| 00:07–00:24 | Nhập thông tin | Hai trường nhập, CTA đỏ toàn chiều rộng form, bàn phím hệ điều hành bật/tắt theo focus. |
| 00:25–00:27 | Quay lại game | Webview đóng; modal chọn tài khoản trở lại với spinner khóa tương tác. |
| 00:28–00:31 | Loading toàn màn | Nền đen và glyph/spinner trắng ở tâm cho biết tác vụ xác thực tiếp tục. |

Key frame: [00:00](../../../references/video-analysis/01-login-account/key-00m00s.jpg), [00:05](../../../references/video-analysis/01-login-account/key-00m05s.jpg), [00:08](../../../references/video-analysis/01-login-account/key-00m08s.jpg), [00:26](../../../references/video-analysis/01-login-account/key-00m26s.jpg), [00:28](../../../references/video-analysis/01-login-account/key-00m28s.jpg).

<a id="screens"></a>

## 3. Screen inventory và layout

| Screen | Thứ bậc layout | CTA/chức năng | Trạng thái cần có |
| --- | --- | --- | --- |
| Account picker | Tiêu đề → mô tả liên kết → provider chính → phân cách “hoặc” → provider phụ | Garena User, Google, Facebook, Guest | Idle, provider pressed, provider unavailable, loading |
| Garena web login | Header web → username → password → CTA đỏ → liên kết hỗ trợ | Đăng nhập, quên mật khẩu/đăng ký nếu có | Focus, keyboard open, validation error, network error, success |
| Authentication loading | Spinner giữa màn, không có CTA cạnh tranh | Chờ phản hồi | Timeout, retry, cancel/back an toàn |

Modal chọn tài khoản rộng khoảng một phần ba khung landscape, đặt lệch trái-trung tâm để giữ nền sân làm bối cảnh. Các provider được xếp dọc; màu thương hiệu tạo nhận diện, trong khi kích thước và khoảng cách thống nhất giữ cùng mức affordance. Webview không thích ứng landscape mà bảo toàn layout portrait, tạo khoảng trống đen lớn hai bên.

<a id="behavior"></a>

## 4. Hành vi UI/UX

- **Điều hướng:** chọn Garena mở luồng ngoài lớp game; sau khi trả về, game phục hồi modal trước khi chuyển sang loading toàn màn.
- **Phản hồi:** spinner xuất hiện ngay tại modal ở 00:26, sau đó thành loading riêng ở 00:28; đây là hai tầng phản hồi cho handoff và xác thực.
- **Bàn phím:** bàn phím hệ điều hành chiếm nửa dưới màn portrait; form vẫn giữ CTA nhìn thấy phía trên bàn phím.
- **Transition:** modal → nền xám/webview là cắt cảnh; webview → game có một nhịp phục hồi modal; không quan sát thấy animation trang phức tạp.
- **Modal và back:** video cho thấy webview là lớp tách biệt, nhưng không cung cấp bằng chứng về cancel, lỗi sai mật khẩu, mất mạng hoặc back gesture.
- **Accessibility:** chữ mô tả trên modal nhỏ và tương phản vừa phải; webview portrait bị thu nhỏ trên landscape có nguy cơ khó đọc. Video không chứng minh hỗ trợ screen reader, focus order hay phóng đại chữ.

<a id="implications"></a>

## 5. Hàm ý thiết kế

**Suy luận thiết kế:** Luồng có ít nhất ba context UI — game shell, web authentication và loading gate — nên cần state machine rõ ràng để tránh double-submit, mất callback hoặc kẹt spinner.

**Đề xuất cho Soccer Mobile Pro:** dùng một account gateway duy nhất với trạng thái `Idle → LaunchingProvider → AwaitingCallback → Linking → Authenticated/Failed`; giữ provider picker responsive ở landscape, thêm timeout/retry, thông báo lỗi có thể hành động, nút hủy an toàn và liên kết trợ giúp. Guest phải mô tả rõ rủi ro mất dữ liệu và có đường nâng cấp sang tài khoản liên kết.
