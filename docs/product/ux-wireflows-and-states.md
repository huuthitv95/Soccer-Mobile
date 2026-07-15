# UX wireflows và trạng thái UI

> [Chỉ mục](../index.md) · [GDD](gdd-soccer-mobile-pro.md) · [UI design system](ui-design-system-and-screen-catalogue.md) · [Bằng chứng video](../research/video/ui-pattern-synthesis.md)

## 1. Mục tiêu và phạm vi

Đặc tả này định nghĩa điều hướng, trạng thái bất đồng bộ, accessibility và contract UX dùng chung. Hình ảnh FC Mobile VN chỉ là tham chiếu; Soccer Mobile Pro dùng hệ thống hình ảnh và dữ liệu riêng.

**Input:** trạng thái phiên đăng nhập, catalog/version manifest, entitlement, đội hình, sự kiện và trạng thái mạng.
**Output:** route hiện tại, UI state có thể phục hồi, analytics event và lệnh domain được xác nhận.
**Client:** render, navigation, local preference, preview và optimistic state an toàn.
**Server:** account, entitlement, economy, progression, event eligibility và transaction result.

## 2. Global navigation

- Home là hub; thanh điều hướng chính gồm Home, Club/Squad, Play, Exchange/Market và Store/Profile tùy ngữ cảnh sản phẩm.
- Top bar giữ UID/avatar, currency và shortcut inbox/settings; rail phụ chỉ dành cho nhiệm vụ hoặc live event cần ưu tiên.
- Back đóng modal trước, quay về parent route sau; không thoát app từ checkout, nâng cấp hoặc match flow.
- Deep link phải đi qua login gate, asset gate và eligibility gate; event hết hạn đưa về fallback có giải thích.
- Mọi route quan trọng giữ scroll/filter/tab state khi quay lại.

## 3. Core wireflows

1. **First launch:** splash → chọn ngôn ngữ → privacy/age consent → tải core asset → guest/login → đồng bộ → Home.
2. **Account binding:** Settings → Account → provider → web/native authorization → merge check → success/recovery state.
3. **Player upgrade:** Player detail → compare impact → chọn material → safety-lock check → preview → confirm → server result → squad refresh.
4. **Purchase:** offer → odds/terms → platform checkout → server receipt verify → entitlement/inbox → history.
5. **Match:** mode → eligibility/squad check → opponent/loading → gameplay → reconnect/result → reward reconciliation.
6. **Report:** result/profile → reason → match context → submit → case acknowledgement → resolution/appeal.
7. **Tournament:** rules → eligibility → register → check-in → bracket/lobby → verified result → reward.
8. **Settings:** Profile → Settings → category → preview/change → owner validates → auto-save/apply → sync result; reset chỉ tác động category đã chọn.
9. **Club/squad/player:** Club → squad slot → inventory/filter → player detail/compare → assign → squad validation → save revision.
10. **Market/exchange:** entry → browse/recipe → detail + current revision → preview cost/output → protected-item check → confirm → authoritative receipt → inventory projection.
11. **Offline AI:** mode → difficulty/tactics → squad validation → local asset gate → seeded match → pause/resume → result + decision trace → optional replay.
12. **VAR:** immutable rule incident → presentation eligibility → review overlay/replay → decision shown → resume; skip chỉ bỏ presentation.

Mỗi flow đi qua cùng gate theo thứ tự: `session → version/catalog → entitlement/eligibility → asset → domain command`. Gate thất bại trả về state có hành động và route fallback; không chuyển thẳng về Home làm mất context.

## 4. State catalogue

| State | Hiển thị | Hành động hợp lệ | Không được làm |
| --- | --- | --- | --- |
| Loading | Skeleton đúng layout; tiến độ cho asset pack | Cancel nếu an toàn | Spinner vô hạn hoặc thay đổi layout liên tục |
| Empty | Lý do + CTA có ích | Tạo squad, tìm market, nhận nhiệm vụ | Chỉ hiển thị “không có dữ liệu” |
| Error | Thông điệp thân thiện, error/correlation ID | Retry idempotent, support | Retry mù transaction |
| Offline/degraded | Cache read-only và phạm vi bị hạn chế | Xem catalog/squad cache | Ghi economy/progression |
| Maintenance | Countdown, phạm vi, kênh trạng thái | Thoát hoặc đọc thông báo | Cho vào flow không thể hoàn tất |
| Expired | Event/offer đã hết hạn | Về Home hoặc event kế tiếp | Giữ CTA mua/claim hoạt động |
| Conflict | Dữ liệu local cũ hơn server | Reload/merge theo policy | Ghi đè im lặng |

## 5. Layout và interaction contract

- Touch target tối thiểu 44 dp; safe-area áp dụng cho top bar, bottom nav và cụm điều khiển trận.
- Primary CTA không bị banner, toast hoặc keyboard che; modal destructive dùng hierarchy rõ và yêu cầu xác nhận phù hợp rủi ro.
- Motion có mục đích: 150–250 ms cho navigation, skeleton-to-content không flash, reduced-motion bỏ parallax/zoom mạnh.
- Currency change phải chỉ rõ nguồn/sink; premium spend và dùng thẻ khóa làm nguyên liệu cần xác nhận hai lớp.
- Webview đăng nhập/sự kiện có domain indicator, nút đóng/back, timeout và quay về đúng route.

## 6. Localization và accessibility

- UI string dùng localization key; tên riêng cầu thủ/CLB/giải giữ canonical name và có trường display override khi hợp đồng yêu cầu.
- Hỗ trợ font scale, subtitle, high contrast, color-blind palette, reduced motion, vibration toggle và left-handed HUD.
- Voice pack hiển thị dung lượng, trạng thái tải/xóa, checksum và fallback subtitle.
- Không truyền nghĩa chỉ bằng màu; icon có label/accessible name và focus order nhất quán.

## 7. Analytics

Event tối thiểu: `screen_view`, `navigation_action`, `cta_tap`, `modal_result`, `loading_duration`, `ui_error`, `deep_link_result`, `accessibility_setting_changed`. Không ghi token, mật khẩu, tên thật hoặc nội dung form nhạy cảm.

## 8. QA và rollback

- Test portrait/landscape theo thiết bị hỗ trợ, notch/safe-area, bàn phím, back gesture, mất mạng và resume app.
- Snapshot các trạng thái loading/empty/error/offline và kiểm tra toàn bộ link/route.
- Feature flag cho navigation/live-event surface; rollback về Home shell ổn định nếu config hoặc asset lỗi.
- Acceptance: không có dead-end, mọi transaction có trạng thái cuối xác định, và keyboard/controller/touch đều tới được CTA chính.

## 9. Nguồn liên quan

- [Quan sát video UI](../research/video/ui-pattern-synthesis.md)
- [Nghiên cứu FC Mobile VN](../research/fc-mobile-vn-research.md)
- [Audit Unity và backlog](../implementation/unity-implementation-audit-and-backlog.md)
- [UI design system và screen catalogue](ui-design-system-and-screen-catalogue.md)
