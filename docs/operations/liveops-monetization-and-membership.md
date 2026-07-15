# LiveOps, monetization và membership

> [Chỉ mục](../index.md) · [Live data](live-data-and-operations.md) · [GDD](../product/gdd-soccer-mobile-pro.md)

## 1. Mục tiêu

Tạo live service minh bạch, có entitlement server-side, tôn trọng thời gian người chơi và không biến progression thành nghĩa vụ. Mọi ngưỡng, giá và quyền lợi bên dưới là **đề xuất cho Soccer Mobile Pro**, không phải xác nhận cơ chế nội bộ FC Mobile VN.

## 2. Purchase và entitlement

1. Client tải offer đã ký/versioned.
2. Người chơi xem giá, nội dung, odds/terms và xác nhận.
3. Platform checkout trả receipt; server verify và tạo transaction idempotent.
4. Ledger grant entitlement; client nhận kết quả hoặc inbox fallback.
5. History hỗ trợ restore/refund/reconciliation.

Không gắn purchase vào device; không grant từ client; retry dùng transaction ID để chống double charge.

## 3. Membership

- Điểm membership chỉ phát sinh từ giao dịch hợp lệ và có ledger riêng.
- Tier tính theo chu kỳ công bố trước; UI hiển thị điểm, thời gian, quyền lợi và chính sách hạ hạng.
- Quyền lợi ưu tiên cosmetic/convenience; không tạo lợi thế ranked không thể đạt bằng chơi.
- Quà ngày/tuần/sinh nhật đi qua inbox với expiry và claim idempotent.

Ngưỡng tiền/điểm cụ thể phải được economy, legal và privacy phê duyệt trước khi publish; tài liệu này không hard-code tỷ lệ thử nghiệm thành policy production.

## 4. Inbox, calendar và giftcode

- Inbox item có sender, localization key, attachment, eligibility, sent/expiry time và claim state.
- Reward calendar cho xem trước nhưng có catch-up hợp lý; không phạt mất chuỗi theo cách gây ép buộc.
- Giftcode có campaign/source, region, expiry, max redemption, per-account/device/IP abuse controls và audit log.

## 5. Event và season pass

- Mỗi event có mục tiêu, eligibility, nhiệm vụ, currency, source/sink, reward table, Home placement, end-state và compensation policy.
- Pass có free/paid tracks, preview đầy đủ và claim-all an toàn.
- Event liên quan giải thật chỉ publish khi catalog, asset và licensing scope đã được xác nhận.

## 6. Analytics, abuse và privacy

Event tối thiểu: `offer_viewed`, `checkout_started`, `receipt_verified`, `entitlement_granted`, `claim_result`, `code_redeemed`, `membership_tier_changed`. Không log receipt hoặc PII thô.

Chống abuse: receipt replay, guest reroll, refund fraud, code farming, duplicate claim và clock manipulation. Dữ liệu hồ sơ membership chỉ thu khi thật sự cần, có consent, retention và delete flow.

## 7. Accessibility và failure states

- Giá và nội dung đọc được bằng screen reader; không dùng màu làm tín hiệu duy nhất.
- Pending purchase có lịch sử/retry support; entitlement chậm dùng inbox reconciliation.
- Event expired/maintenance/offline phải chặn hành động ghi và giải thích rõ.

## 8. QA và rollback

- Sandbox purchase, duplicate callback, delayed receipt, refund, reconnect và cross-device restore.
- Feature flag cho offer/event/tier; rollback không xóa entitlement đã grant.
- Acceptance: không double grant/charge, odds và terms luôn khớp offer version, mọi claim có trạng thái cuối có thể audit.
