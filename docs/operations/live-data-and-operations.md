# Live data và operations

> [Chỉ mục](../index.md) · [Nghiên cứu](../research/fc-mobile-vn-research.md) · [Backlog](../implementation/unity-implementation-audit-and-backlog.md)

## 1. Mục tiêu và data boundary

Vận hành catalog cầu thủ/CLB/giải, card program, economy, event và asset theo version có thể audit/rollback. Server là authority cho inventory, currency, progression, purchase, market và reward grant; client chỉ cache/projection.

## 2. Versioning và contract

- Tách `player_identity`, `club`, `league`, `card_definition`, `card_instance`, market/economy và event config.
- Manifest có `schema_version`, `content_version`, region/season, effective time, minimum client version, hash và rollback pointer.
- API catalog hỗ trợ bootstrap manifest, paging/lazy detail, prefetch lineup và conditional request bằng version/ETag.
- Schema thay đổi cần migration, backward-compatibility window và validator trước publish.

## 3. CMS và approval

- CMS quản lý event, banner, giftcode, pass, shop bundle, odds, localization và reward table.
- Four-eyes approval cho currency/odds/reward; audit log ghi actor, reason, before/after và publish ID.
- Preview sandbox kiểm tra ID/reference, entitlement, thời gian, translation coverage, inventory cap và conflict schedule.

## 4. Economy và market

- Ledger append-only/idempotent cho mọi currency và vật phẩm; transaction có correlation ID.
- Market có price band, listing limit, tax, anti-bot, anomaly detection, suspend và investigation tooling.
- Dashboard theo cohort cho source/sink, inflation, negative balance, duplicate grant, abnormal trade graph và claim spike.

## 5. Asset delivery

- Player asset manifest chứa body/face/hair/kit/animation refs, LOD tier, size, checksum, dependency và generic fallback.
- Addressable/bundle hot update staged theo cohort; giữ manifest trước để rollback và không xóa bundle còn được client cũ tham chiếu.
- Acceptance budget phải nêu device tier, memory, texture, triangle, load time và fallback visual.

## 6. Observability và incident

- Theo dõi API latency/error, catalog mismatch, match completion/desync, payment verify, event claim và crash-free users.
- Runbook: detect → freeze/disable bằng flag → mitigate/rollback → communicate → reconcile/compensate → postmortem.
- Alert có owner, threshold, severity, runbook link và chống alert storm.

## 7. Privacy, abuse và accessibility

- Không đưa PII vào telemetry/catalog; access CMS theo least privilege và log truy cập.
- Rate limit, signature/checksum, replay protection và publish approval ngăn abuse.
- Content text có localization completeness, readable contrast và alt/accessible labels cho asset quan trọng.

## 8. QA và rollback

- Contract test cho manifest/schema; migration test từ hai client version gần nhất; chaos test CDN/cache mismatch.
- Rehearse rollback catalog, event, odds và asset bundle trước live launch.
- Acceptance: publish không tạo dangling reference, grant idempotent và rollback hoàn tất trong SLA đã định.
