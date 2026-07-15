# Player cards, skills, progression, market và exchange

> [Chỉ mục](../index.md) · [GDD](../product/gdd-soccer-mobile-pro.md) · [Catalog](football-catalog-player-database-and-model-assets.md) · [LiveOps](../operations/liveops-monetization-and-membership.md)

## 0. Mục lục

- [1. Mục tiêu và non-goal](#goal)
- [2. Entities và contracts](#contracts)
- [3. State machines và authority](#states)
- [4. Version, migration và economy](#version)
- [5. Failure, abuse và accessibility](#failure)
- [6. Analytics, QA và rollback](#quality)
- [7. Decision register](#decisions)

<a id="goal"></a>

## 1. Mục tiêu và non-goal

Mục tiêu: card instance, skill taxonomy, upgrade và market/exchange có preview/receipt, server authority và meaningful trade-off. Non-goal: hard-code vật phẩm/công thức FC Mobile theo một version, dùng progression thay core fun, hoặc cho client tính/grant kết quả. S08/S11 có drift nên mọi rule phải versioned.

<a id="contracts"></a>

## 2. Entities và contracts

```text
OwnedPlayerItem { itemId, ownerId, itemDefinitionId, rank, trainingLevel,
  allocatedSkillPoints, lockState, acquisition, revision }
SkillMoveDefinition { id, inputId, requirements, animationProfile,
  staminaCost, riskTags, rulesVersion }
TraitDefinition { id, triggers, conditions, modifiers, caps,
  exclusions, traceLabelKey, rulesVersion }
PlayStyleDefinition { id, triggers, modifiers, caps, exclusions, rulesVersion }
UpgradePreview { commandHash, itemRevision, ruleVersion, cost,
  consumedItems, before, after, warnings, expiresAt }
UpgradeCommand { itemId, previewHash, itemRevision, idempotencyKey }
UpgradeReceipt { transactionId, status, ledgerEntries, newItemRevision }
```

Market listing có `listingId`, seller, item snapshot hash, price, currency, created/expiry, status/revision. Exchange recipe có version, eligibility, inputs, outputs, repeat limit và effective interval. Client nhận projection/preview; server giữ inventory, currency, lock/reservation, order matching, tax và grant ledger.

<a id="states"></a>

## 3. State machines và authority

- Upgrade: `Draft → Previewed → Confirming → Committed|Rejected|Unknown`; `Unknown` poll receipt bằng idempotency key, không gửi lệnh mới.
- Item: `Available → Locked|Reserved|InSquad → Consumed|Transferred`; state transition atomic với ledger.
- Listing: `Draft → Active → Reserved → Sold|Cancelled|Expired`; price/revision đổi làm preview cũ invalid.
- Exchange: `Eligible → Previewed → Reserved → Granted|Rejected`; input consume và output grant cùng transaction.

Skill/Trait/PlayStyle modifier có trigger/context/cap/exclusion và trace; Trait là đặc tính thụ động, SkillMove là input/animation, PlayStyle là identity chiến thuật. Không tạo option mạnh hơn mọi mặt. Rank/Training/Skill là trục riêng; respec preview consequence và không mất điểm do timeout.

<a id="version"></a>

## 4. Version, migration và economy

Rule definition immutable trong transaction receipt; balance patch tạo version mới/effective time. Existing item migration phải công bố mapping và compensation khi làm giảm entitlement. Economy theo dõi source/sink, price distribution, velocity và power gap; số GDD là hypothesis đến playtest. Random offer chỉ mở sau odds/age/privacy/store review; market tax/limit qua config four-eyes approval.

<a id="failure"></a>

## 5. Failure, abuse và accessibility

| Failure/abuse | Xử lý |
| --- | --- |
| Double tap/callback timeout | Idempotency + receipt lookup; CTA loading có timeout |
| Protected/in-squad material | Chặn và chỉ rõ nơi đang dùng; explicit unlock/remove |
| Inventory/currency stale | Reject revision, refresh preview, không auto-confirm lại |
| Price move/reservation race | Hiện giá mới; release reservation theo TTL |
| Bot/farming/wash trade | Rate/velocity/device risk, price band, graph investigation, appeal |
| Recipe/event hết hạn giữa flow | Server time; invalidate preview và giữ input |
| Refund/rollback config | Reconcile ledger; không tạo balance âm im lặng |

Preview đọc được bởi screen reader, nêu input bị tiêu hao, OVR/stat/skill delta, cap, expiry, tax và premium spend bằng text + icon. Filter/sort, compare và confirmation dùng focus hợp lý; rarity không chỉ bằng màu.

<a id="quality"></a>

## 6. Analytics, QA và rollback

Event: `player_detail_viewed`, `upgrade_previewed/result`, `skill_allocated/reset`, `listing_created/result`, `market_purchase_result`, `exchange_previewed/result`, `transaction_unknown_recovered`. Không log full inventory/receipt.

QA property/contract: duplicate command, revision race, insufficient balance, protected material, max cap, respec, price race, expiry boundary, partial service failure, cross-device retry, migration N-1. Acceptance: ledger cân bằng, không double consume/grant, receipt truy xuất được, dominant strategy bị scenario/balance review chặn, UI luôn hiện consequence. Rollback disable rule/recipe/market action bằng flag, giữ đọc/history; config rollback không đảo transaction hợp lệ, compensation qua ledger riêng.

<a id="decisions"></a>

## 7. Decision register

| ID | Quyết định | Owner | Gate |
| --- | --- | --- | --- |
| PCS-D01 | Rank/Training caps và cost | Economy + Design | Core gate + cohort simulation/playtest |
| PCS-D02 | Skill taxonomy/caps | Gameplay | Scenario suite, không dominant strategy |
| PCS-D03 | Market model/tax/band | Economy + Backend | Inflation/abuse/load simulation |
| PCS-D04 | Respec price/policy | Product + Compliance | Regret/fairness test; không paywall experimentation |
