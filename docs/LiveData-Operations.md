# LIVE DATA & OPERATIONS

## Data Versioning
- Tách static player/club/league assets, dynamic card data, market/economy data và event config.
- Manifest version theo region/season; CDN cache với hash, staged rollout, compatibility gate và rollback pointer.
- Mọi schema thay đổi phải versioned, có migration, validation và backward compatibility policy.

## CMS & Approval
- CMS cho event, banner, giftcode, pass, shop bundle, drop odds, localization và reward table.
- Bốn mắt phê duyệt cho currency/odds/reward; audit log: ai đổi gì, khi nào, trước/sau thay đổi.
- Preview sandbox và automated validation trước publish: ID tồn tại, reward hợp lệ, translated strings, start/end time, inventory cap.

## Economy & Market Operations
- Ledger server-side cho tất cả currency; source/sink dashboard theo cohort.
- Market có price floor/ceiling, listing limits, tax, anomaly detection, anti-bot và suspend tooling.
- Alert khi lạm phát, reward duplication, negative balance, abnormal trade graph hoặc claim spike.

## Asset Delivery
- Player model có asset manifest, LOD/tier, size budget, checksum, fallback generic head và hot-update bundle.
- Patch rollback tránh crash: retain manifest trước, feature flag, crash/performance telemetry sau rollout.

## Observability & Incident
- Dashboard: API latency/error, match completion, desync, payment verify, event claims, crash-free users.
- Incident runbook: detect > mitigate/disable via flag > communicate > compensate > postmortem.
