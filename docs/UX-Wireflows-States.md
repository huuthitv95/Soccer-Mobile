# UX WIREFLOWS & UI STATES

## Global Navigation
- Bottom navigation: Home, Squad, Play, Market/Store, Profile; Events được ưu tiên bằng banner + deep link.
- Back behavior: đóng modal trước, trở về parent screen sau, không thoát app vô tình từ transaction hoặc match flow.
- Deep links phải xử lý app chưa đăng nhập, event hết hạn, asset chưa tải và entitlement không hợp lệ.

## Core Wireflows
1. First launch: splash > chọn language > tải core asset > guest/login > privacy consent > home.
2. Player upgrade: player detail > compare impact > chọn material > safety lock check > preview cost/result > confirm > server result > updated squad.
3. Purchase: bundle > odds/terms > platform checkout > server receipt verify > inbox/entitlement confirmation > history.
4. Report: opponent profile > reason > match context > submit > case acknowledgement > resolution/appeal.
5. Tournament: rules > eligibility > register > check-in > bracket/lobby > verified result > reward.

## State Catalogue
- Loading: skeleton đúng cấu trúc trang, progress cho asset pack; không dùng spinner vô hạn.
- Empty: message hành động được, CTA rõ cho squad/market/inbox/event.
- Error: mã lỗi thân thiện, retry, support link, correlation id; transaction không được retry mù.
- Offline/degraded: cache-readonly, chặn hành động có ghi dữ liệu, giải thích rõ.
- Maintenance: countdown, phạm vi ảnh hưởng, bù đắp nếu có.

## Accessibility
- Font scale, subtitle, high contrast/color-blind palettes, reduced motion, vibration toggle, left-handed HUD, touch target tối thiểu 44dp.
- UI strings dùng localization key; audio pack có size, download state, fallback subtitle và delete flow.
