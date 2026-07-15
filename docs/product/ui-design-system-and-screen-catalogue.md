# UI design system và screen catalogue

> [Chỉ mục](../index.md) · [GDD](gdd-soccer-mobile-pro.md) · [Wireflows](ux-wireflows-and-states.md) · [Bằng chứng video](../research/video/ui-pattern-synthesis.md)

## 0. Mục lục

- [1. Mục tiêu và ranh giới](#goal)
- [2. Layout foundation](#layout)
- [3. Design tokens](#tokens)
- [4. Component contracts](#components)
- [5. Screen catalogue](#screens)
- [6. Responsive và accessibility](#responsive)
- [7. Analytics, QA và rollback](#quality)
- [8. Decision register](#decisions)

<a id="goal"></a>

## 1. Mục tiêu và ranh giới

Tài liệu định nghĩa contract trình bày để design, product và Unity UI cùng dùng. Đây là **Đề xuất cho Soccer Mobile Pro**, không phải bản sao pixel của FC Mobile VN. Video chỉ cung cấp pattern app shell, hub/detail, CTA, modal, loading và preview; giá trị cụ thể dưới đây là token khởi điểm cần kiểm tra trên thiết bị.

**Input:** route, safe area, locale, text scale, device tier, domain/view state, asset state và accessibility preferences. **Output:** visual tree, focus order, action intent và analytics không chứa PII. Client sở hữu layout/render/local preview; server sở hữu eligibility, entitlement, reward, economy và competitive result.

<a id="layout"></a>

## 2. Layout foundation

- Canvas tham chiếu landscape 1920×1080, scale theo chiều ngắn; mọi UI nằm trong safe area thực tế, không giả định notch đối xứng.
- Grid 12 cột, gutter 16 dp, margin safe 24 dp; shell chia `top bar 72 dp`, `content`, `bottom/side navigation 80 dp`. Màn hẹp dùng 8 cột; modal/form có max-width thay vì kéo dãn.
- Hub ưu tiên ba vùng: context/rail `<25%`, canvas/preview `>50%`, action/detail `25–35%`. Đây là tỷ lệ vùng, không phải pixel copy từ video.
- Match HUD tách khỏi shell: joystick trái, contextual actions phải, scoreboard trên; vùng giữa giữ bóng/cầu thủ và không đặt CTA meta.
- Z-order cố định: content `0`, persistent HUD `10`, toast `20`, modal scrim `30`, modal `40`, blocking gate `50`, system permission/web auth ngoài Unity.

<a id="tokens"></a>

## 3. Design tokens

| Nhóm | Token khởi điểm | Quy tắc |
| --- | --- | --- |
| Spacing | 4, 8, 12, 16, 24, 32, 48 dp | Không dùng giá trị lẻ ngoài token nếu chưa có lý do |
| Radius | 4, 8, 12, pill | Card 8; modal 12; pill chỉ chip/status |
| Typography | Display 32/38; H1 24/30; H2 20/26; Body 16/22; Meta 14/18 | Meta không chứa thông tin bắt buộc duy nhất |
| Touch | tối thiểu 44×44 dp; HUD chính 56–72 dp | Khoảng cách hai action nguy hiểm ≥8 dp |
| Motion | fast 120 ms; standard 200 ms; reveal 320 ms | Reduced Motion đổi reveal thành fade ≤120 ms |
| Elevation | base, raised, modal, blocking | Không dùng shadow làm dấu hiệu selected duy nhất |
| Semantic color | surface, text, accent, success, warning, danger, info | Contrast body ≥4.5:1; large text ≥3:1 |

Màu brand cụ thể do art direction khóa sau contrast test. Semantic token không mang tên màu vật lý để theme/high-contrast thay thế được. Currency premium luôn có icon + label; trạng thái lỗi/khóa không chỉ dựa màu.

<a id="components"></a>

## 4. Component contracts

Mọi component hỗ trợ state phù hợp trong tập `default`, `hover` (editor/gamepad), `focused`, `pressed`, `selected`, `disabled`, `locked`, `loading`, `error`, `new`, `claimable`, `claimed`, `expired`. Không component nào tự gọi economy API; nó phát action intent có `sourceRoute`, `entityId`, `revision`.

| Component | Content/behavior | Failure/accessibility |
| --- | --- | --- |
| Primary button | Một động từ, loading giữ width, chống double-submit | Accessible name; disabled có lý do; timeout trả action |
| Player/reward card | Art, identity, rarity, value/progress, lock | Placeholder theo aspect; không dùng rarity color đơn độc |
| Top resource bar | Account, currency, inbox, settings | Pending balance có label; refresh conflict không animate sai |
| Tab/rail | Active marker, badge, locked/new | Focus roving; badge có text thay thế; restore tab hợp lệ |
| Modal/confirmation | Title, consequence, primary/secondary | Focus trap, back policy, destructive re-auth khi cần |
| Toast/banner | Result ngắn, correlation ID khi lỗi | Không che CTA/HUD; screen reader announcement không lặp |
| Skeleton/progress | Giữ cấu trúc; determinate cho asset lớn | Sau timeout đổi sang error/retry; Reduced Motion |
| Webview handoff | Domain/provider indicator, close/back, callback | Allowlist, session expiry, offline, callback idempotent |

<a id="screens"></a>

## 5. Screen catalogue

| ID | Route/screen | Primary CTA | Required states | Authority/dependency |
| --- | --- | --- | --- | --- |
| SC-01 | Bootstrap/locale | Tiếp tục | first-run, cached locale, missing table, legal update | Local preference + remote legal version |
| SC-02 | Account/provider | Đăng nhập / Chơi khách | provider unavailable, callback, merge conflict, recovery | Account server; platform/web auth |
| SC-03 | Core asset gate | Tải/tiếp tục | storage low, network switch, checksum fail, partial/offline | Signed catalog; client downloader |
| SC-04 | Home | Chơi | loading/empty/config error/maintenance/deep-link fallback | Remote slots + local stable shell |
| SC-05 | Profile/friends | Thêm bạn/Quản lý | empty, blocked, pending, rate-limited | Social server/moderation |
| SC-06 | Settings | Áp dụng/auto-save | dirty, migration, unsupported, reset, sync conflict | Setting owner theo field |
| SC-07 | Club/squad | Chỉnh đội hình | invalid squad, asset partial, formation conflict | Catalog + inventory + rule set |
| SC-08 | Player inventory/detail | So sánh/Nâng cấp | empty/filter no result/locked/stale revision | Catalog projection + inventory server |
| SC-09 | Upgrade | Xác nhận | preview changed, insufficient, protected material, duplicate result | Progression server/ledger |
| SC-10 | Market/exchange | Mua/Bán/Trao đổi | price moved, expired, reserved, limit, conflict | Economy server authoritative |
| SC-11 | Event/missions/pass | Đi/Nhận | ineligible, claimable/claimed, expired, maintenance | Server time/config/grant ledger |
| SC-12 | Mode/tournament | Vào trận/Check-in | rules unread, roster invalid, late, bracket update | Match/tournament server |
| SC-13 | Match loading/gameplay | Sẵn sàng/context actions | reconnect, degraded asset, pause offline, forfeit | Match state; server online/local offline |
| SC-14 | VAR presentation | Bỏ qua trình bày | review, decision, timeout, low-tier fallback | Rule result immutable; presentation client |
| SC-15 | Result/report | Tiếp tục/Báo cáo | result pending, reconcile, duplicate report, appeal | Result/report server + audit |

Mỗi screen spec triển khai phải nêu entry/exit route, back behavior, loading owner, empty/error copy key, focus order, analytics và screenshot acceptance. Screen không có state cuối xác định không được qua design review.

<a id="responsive"></a>

## 6. Responsive và accessibility

- Hỗ trợ aspect 16:9 đến 22:9 bằng reflow/letterbox content có chủ đích; không scale chữ xuống dưới token. Web auth portrait hiển thị max-width và zoom 100%, tránh form bị thu nhỏ như quan sát video.
- Font scale 100–200%: row chuyển từ một dòng sang stacked, CTA sticky không che content; truncation có accessible full label.
- Touch, keyboard/gamepad editor test và screen reader có cùng thứ tự logic. Focus trở về trigger sau khi đóng modal.
- HUD có scale/opacity/left-handed/remap; safe zone preview; haptic, camera shake, motion, ball visibility và color-blind palette điều chỉnh độc lập.
- Timer event dùng thời gian server nhưng không tạo FOMO copy; nếu hết hạn khi đang xem, giữ context và giải thích trước khi route fallback.

<a id="quality"></a>

## 7. Analytics, QA và rollback

Event: `screen_view`, `component_action`, `ui_state_entered`, `loading_duration`, `asset_gate_result`, `deep_link_result`, `focus_trap_detected`, `text_overflow_detected`, `accessibility_preference_changed`. Không gửi nội dung text field, token hoặc raw touch path.

QA matrix bắt buộc: locale vi/en; text scale 100/150/200%; aspect 16:9/19.5:9/22:9; notch; low/mid/high tier; touch + keyboard/gamepad; online/offline/network switch; cold/warm resume; mọi state catalog. Acceptance: CTA chính reachable, không overlap safe area, không dead-end, loading có timeout, transaction không double-submit, contrast đạt ngưỡng và Reduced Motion không mất thông tin.

Rollback: token/theme/config có version; giữ `lastKnownGoodUiConfig`; remote slot lỗi bị ẩn thay vì phá shell; navigation mới sau feature flag; schema setting mới có migration ngược hoặc reset có thông báo. Screenshot baseline chỉ phát hiện drift, không thay usability/accessibility test.

<a id="decisions"></a>

## 8. Decision register

Trạng thái/evidence chi tiết nằm tại [chương trình kiểm chứng](../implementation/decision-validation-program.md#decision-matrix).

| ID | Quyết định | Owner | Gate | Trạng thái |
| --- | --- | --- | --- | --- |
| UI-D01 | Brand palette/font production | Art + Accessibility | Contrast và glyph vi/en trên device matrix | `TestReady` |
| UI-D02 | Portrait support ngoài web auth | Product + Engineering | Usage/rotation test; không làm giảm match readability | `TestReady` |
| UI-D03 | Bottom nav so với side rail theo aspect | UX | First-click success, reachability và mis-tap playtest | `TestReady` |
| UI-D04 | Motion duration cuối | UX + Tech Art | Frame pacing low tier và Reduced Motion review | `TestReady` |
| UI-D05 | Store/liveops attention budget | Product + UX | Không quá một primary CTA; task completion không giảm | `TestReady` |
