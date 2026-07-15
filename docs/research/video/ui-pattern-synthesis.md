# Tổng hợp mẫu UI/UX từ bộ video tham chiếu

> [Chỉ mục](../../index.md) › Nghiên cứu video › Tổng hợp UI/UX

## 0. Mục lục

- [1. Phạm vi và phương pháp](#method)
- [2. Bản đồ bằng chứng](#evidence-map)
- [3. Hệ thống layout](#layout-system)
- [4. Điều hướng và trạng thái](#navigation-state)
- [5. Hiệu ứng và chuyển động](#motion)
- [6. Mẫu chức năng](#functional-patterns)
- [7. Đề xuất cho Soccer Mobile Pro](#recommendations)

<a id="method"></a>

## 1. Phạm vi và phương pháp

Tài liệu tổng hợp năm video do người dùng cung cấp, phân tích visual-first bằng `ffprobe`, contact sheet có timestamp và key frame chọn lọc. Mọi nhận định được phân loại như sau:

- **Quan sát từ video:** chi tiết nhìn thấy trực tiếp trong frame.
- **Suy luận thiết kế:** giải thích hợp lý về mục đích hoặc cấu trúc, chưa phải sự thật nội bộ.
- **Đề xuất cho Soccer Mobile Pro:** yêu cầu thiết kế/triển khai cho dự án.

Whisper và Tesseract không khả dụng trên máy tại thời điểm phân tích. Vì vậy audio không được phiên âm, không có OCR tự động và không dùng lời thoại/âm thanh làm bằng chứng. Chữ UI chỉ được đọc thủ công ở những key frame đủ rõ. Bộ video là ảnh chụp một phiên bản/thời điểm cụ thể, không chứng minh trạng thái hiện tại hay kiến trúc backend của FC Mobile VN.

<a id="evidence-map"></a>

## 2. Bản đồ bằng chứng

| Video | Trọng tâm | Contact sheet | Phân tích chi tiết |
| --- | --- | --- | --- |
| 01 | Provider picker, Garena web login, keyboard, loading handoff | [Ảnh](../../../references/video-analysis/01-login-account/contact-sheet.jpg) | [Đăng nhập và tài khoản](./01-login-account.md) |
| 02 | Home shell, carousel, nhiệm vụ, daily login, pass, event webview | [Ảnh](../../../references/video-analysis/02-home-liveops/contact-sheet.jpg) | [Home và liveops](./02-home-liveops.md) |
| 03 | Đổi tên, hồ sơ, social/bạn bè, settings và gameplay toggles | [Ảnh](../../../references/video-analysis/03-profile-settings/contact-sheet.jpg) | [Hồ sơ và cài đặt](./03-profile-settings.md) |
| 04 | Club hub, squad/player inventory, locker room, stadium, appearance | [Ảnh](../../../references/video-analysis/04-club-squad-customization/contact-sheet.jpg) | [Câu lạc bộ và tùy biến](./04-club-squad-customization.md) |
| 05 | Cosmetic item, exchange, badge, missions và events | [Ảnh](../../../references/video-analysis/05-exchange-missions-events/contact-sheet.jpg) | [Trao đổi và sự kiện](./05-exchange-missions-events.md) |

<a id="layout-system"></a>

## 3. Hệ thống layout

### 3.1. App shell

**Quan sát từ video:** Home dùng top bar cho hồ sơ/tài nguyên/tiện ích, rail shortcut ở trái, content canvas ở giữa và bottom navigation cho các domain thường dùng. Shell giữ ổn định khi hero campaign thay đổi và được phục hồi sau khi đóng màn con.

**Suy luận thiết kế:** hệ thống slot cho phép campaign/liveops thay key art và nội dung mà không thay mental model. Điểm neo cố định gồm hồ sơ góc trái, tài nguyên góc phải, CTA chính vùng dưới hero và navigation sát cạnh màn.

### 3.2. Hub và detail

Các hub Club/Appearance đặt preview 3D lớn ở center-left, category hoặc action ở biên phải. Player inventory dùng grid + selected detail/action. Mission dùng rail campaign + list task + reward panel. Settings dùng overlay modal hai cột rồi drill-down thành danh sách cuộn. Các mẫu đều tuân theo “chọn context → xem detail → thực hiện CTA” và giữ back/title trên header.

### 3.3. Visual hierarchy

- CTA chính dùng màu sáng tương phản (vàng/xanh lục/đỏ theo context), kích thước lớn và vị trí ổn định.
- Active tab dùng underline, màu accent, border hoặc nền sáng; item chọn có check/border.
- Card player/reward/event là đơn vị thông tin tái sử dụng, kết hợp rarity/color, thumbnail, count/progress và lock state.
- Background stadium/gradient tối giúp card và typography sáng nổi lên; campaign key art được phép nhiều màu hơn trong content slot.
- Thông tin cấp hai thường dùng chữ nhỏ; đây là rủi ro accessibility trên màn mobile nhỏ hoặc khi stream/scale.

<a id="navigation-state"></a>

## 4. Điều hướng và trạng thái

| Pattern | Bằng chứng quan sát | Yêu cầu trạng thái tối thiểu cho Soccer Mobile Pro |
| --- | --- | --- |
| Back/close | Back ở header màn con; close ở modal/webview | Phân biệt pop navigation với dismiss modal; giữ state khi quay lại |
| Deep link CTA | “Đi/Chơi ngay” từ nhiệm vụ/event | Kiểm tra eligibility, route hợp lệ, fallback và analytics source |
| Loading gate | Spinner giữa canvas, modal spinner, nền sân trống khi tải | Skeleton/spinner, timeout, retry, cancel, stale-cache policy |
| Tab/rail | Event, profile, settings category, exchange | Active/disabled/locked/new, restore selection, keyboard/gamepad focus |
| Progress/reward | Daily login, pass, badge, missions | Server time, claimable/claimed/expired, conflict recovery, idempotency |
| Webview | Garena login và event page | Auth handoff, close/back, offline, refresh, allowlist, session expiry |
| Asset preview | Model, kit, shoe, stadium | Placeholder, download, cancel, failure, low-memory fallback, version mismatch |

**Quan sát từ video:** empty state thấy rõ nhất ở danh sách Bạn bè 0/100. Error, offline, timeout, maintenance và recovery gần như không xuất hiện; không được suy luận rằng sản phẩm không có các trạng thái này.

<a id="motion"></a>

## 5. Hiệu ứng và chuyển động

- **Carousel:** hero tự đổi campaign bằng slide/fade nhưng giữ CTA và slot layout.
- **Loading/reveal:** UI có thể hiện trước model 3D; model hoặc environment được reveal sau một nhịp tải.
- **Modal:** overlay settings/account giữ nền shell bị làm tối, tạo continuity.
- **Content swap:** rail/tab đổi nội dung tại chỗ, tránh scene transition lớn.
- **3D idle/preview:** cầu thủ giữ idle pose; chọn cosmetic/stadium ưu tiên phản hồi trực quan trực tiếp.
- **Web transition:** game → webview → game là hard context switch, có spinner làm cầu nối.

Video lấy mẫu không đủ để xác nhận chính xác duration, easing, haptic hoặc sound cue. **Đề xuất cho Soccer Mobile Pro:** chuẩn hóa motion token (`fast` cho selection, `standard` cho panel, `slow` cho scene reveal), hỗ trợ Reduce Motion, không khóa input lâu hơn animation và luôn có trạng thái kết thúc khi callback thất bại.

<a id="functional-patterns"></a>

## 6. Mẫu chức năng

### 6.1. Account và settings

Provider picker tách khỏi web authentication; settings gom account/gameplay/presentation/locale/support nhưng drill-down theo domain. Cần state machine auth, action nguy hiểm có re-auth, setting có owner/default/scope/version và feedback persistence.

### 6.2. Liveops, missions và reward

Home, daily login, pass, badge và mission đều trình bày tiến độ + phần thưởng + CTA. Campaign dùng template chung với nội dung cấu hình. Client chỉ project trạng thái; grant, exchange, entitlement và deadline phải do server quyết định và có ledger/audit.

### 6.3. Club, squad và player presentation

Squad card, player grid và model 3D liên kết bằng selection context. Video chỉ chứng minh presentation, không chứng minh endpoint player database, Addressables/bundle hoặc pipeline tạo model. Với Soccer Mobile Pro, catalog versioned cần tách player identity, card instance, gameplay stats và visual profile.

### 6.4. Customization

Model/stadium là preview surface, selector là control surface; selected/locked/new là state phổ biến. Preview local phải có confirm/cancel, entitlement server-side và rollback khi lưu thất bại.

<a id="recommendations"></a>

## 7. Đề xuất cho Soccer Mobile Pro

1. Xây app shell responsive với safe area, top resource bar, contextual rail, content canvas và bottom navigation; campaign chỉ điền slot đã định nghĩa.
2. Dùng design token cho màu, typography, spacing, radius, elevation, icon, motion và minimum touch target; kiểm tra contrast, text scaling, left-handed layout và Reduce Motion.
3. Chuẩn hóa component state `default/pressed/focused/selected/disabled/locked/loading/error/new/claimable/claimed/expired` thay vì xử lý riêng từng màn.
4. Tách navigation state, domain state, remote content và asset-download state; quay lại màn phải phục hồi selection/scroll khi còn hợp lệ.
5. Mọi giao dịch reward/exchange/account là server-authoritative, idempotent, có telemetry và recovery; không suy diễn backend FC Mobile VN từ video.
6. Bổ sung UX chưa xuất hiện trong clip: loading skeleton, empty có CTA, offline/retry, timeout, maintenance, partial asset, validation, confirmation, entitlement conflict và hỗ trợ người dùng.
7. Dùng key frame trong bộ này làm reference evidence, không dùng làm pixel-perfect source hoặc asset nguồn. Spec triển khai phải chuyển quan sát thành wireflow và acceptance criteria độc lập.
