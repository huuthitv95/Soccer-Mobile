# Live data và operations

> [Chỉ mục](../index.md) · [Nghiên cứu](../research/fc-mobile-vn-research.md) · [Backlog](../implementation/unity-implementation-audit-and-backlog.md)

## 0. Mục lục

- [1. Mục tiêu và data boundary](#goal)
- [2. Versioning và contract](#versioning)
- [3. CMS và approval](#cms-approval)
- [4. Economy và market](#economy-market)
- [5. Asset delivery](#asset-delivery)
- [6. Observability và incident](#observability-incident)
- [7. Privacy, abuse và accessibility](#privacy-accessibility)
- [8. QA và rollback](#quality)
- [9. Publish state machine và failure matrix](#publish-failure)

<a id="goal"></a>

## 1. Mục tiêu và data boundary

Vận hành catalog cầu thủ/CLB/giải, card program, economy, event và asset theo version có thể audit/rollback. Server là authority cho inventory, currency, progression, purchase, market và reward grant; client chỉ cache/projection.

<a id="versioning"></a>

## 2. Versioning và contract

- Tách `player_identity`, `club`, `league`, `card_definition`, `card_instance`, market/economy và event config.
- Manifest có `schema_version`, `content_version`, region/season, effective time, minimum client version, hash và rollback pointer.
- API catalog hỗ trợ bootstrap manifest, paging/lazy detail, prefetch lineup và conditional request bằng version/ETag.
- Schema thay đổi cần migration, backward-compatibility window và validator trước publish.

<a id="cms-approval"></a>

## 3. CMS và approval

- CMS quản lý event, banner, giftcode, pass, shop bundle, odds, localization và reward table.
- Four-eyes approval cho currency/odds/reward; audit log ghi actor, reason, before/after và publish ID.
- Preview sandbox kiểm tra ID/reference, entitlement, thời gian, translation coverage, inventory cap và conflict schedule.

<a id="economy-market"></a>

## 4. Economy và market

- Ledger append-only/idempotent cho mọi currency và vật phẩm; transaction có correlation ID.
- Market có price band, listing limit, tax, anti-bot, anomaly detection, suspend và investigation tooling.
- Dashboard theo cohort cho source/sink, inflation, negative balance, duplicate grant, abnormal trade graph và claim spike.

<a id="asset-delivery"></a>

## 5. Asset delivery

- Player asset manifest chứa body/face/hair/kit/animation refs, LOD tier, size, checksum, dependency và generic fallback.
- Addressable/bundle hot update staged theo cohort; giữ manifest trước để rollback và không xóa bundle còn được client cũ tham chiếu.
- Acceptance budget phải nêu device tier, memory, texture, triangle, load time và fallback visual.

<a id="observability-incident"></a>

## 6. Observability và incident

- Theo dõi API latency/error, catalog mismatch, match completion/desync, payment verify, event claim và crash-free users.
- Runbook: detect → freeze/disable bằng flag → mitigate/rollback → communicate → reconcile/compensate → postmortem.
- Alert có owner, threshold, severity, runbook link và chống alert storm.

<a id="privacy-accessibility"></a>

## 7. Privacy, abuse và accessibility

- Không đưa PII vào telemetry/catalog; access CMS theo least privilege và log truy cập.
- Rate limit, signature/checksum, replay protection và publish approval ngăn abuse.
- Content text có localization completeness, readable contrast và alt/accessible labels cho asset quan trọng.

<a id="quality"></a>

## 8. QA và rollback

- Contract test cho manifest/schema; migration test từ hai client version gần nhất; chaos test CDN/cache mismatch.
- Rehearse rollback catalog, event, odds và asset bundle trước live launch.
- Acceptance: publish không tạo dangling reference, grant idempotent và rollback hoàn tất trong SLA đã định.

<a id="publish-failure"></a>

## 9. Publish state machine và failure matrix

```text
Draft → Validated → Approved → Scheduled → Publishing → Active
                    ↘ Rejected      ↘ Failed → RolledBack
Active → Superseded|Revoked
```

| Failure | Detection | Mitigation | Recovery/acceptance |
| --- | --- | --- | --- |
| Catalog/schema mismatch | Contract/reference validator | Không activate; giữ last-known-good | N-2 clients bootstrap được |
| CDN partial/corrupt | Checksum/download telemetry | Retry/backoff/alternate edge | Atomic activation, generic fallback |
| Economy config sai | Source/sink/claim anomaly | Freeze offer/recipe/grant | Reconcile ledger + compensation audit |
| Clock/region sai | Effective-time canary | Pause publish theo region | Server time đúng, không early/late claim |
| CMS compromise/mis-publish | Audit/risk alert | Revoke credential + rollback pointer | Four-eyes/key rotation/postmortem |

Analytics có `publish_validated/approved/activated/rolled_back`, catalog mismatch, fallback và reconcile result; actor ID chỉ trong audit access-controlled. Accessibility validator kiểm tra localization key, text length/alt label trước publish. Decision mở: SLA rollback theo severity, backward window, key rotation và device-tier budget do DevOps/Data/Tech Art khóa sau rehearsal; không hard-code số chưa đo.
