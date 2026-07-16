# Kế hoạch P1-03 player items, skills và progression foundation

> [Chỉ mục](../index.md) · [Cards/progression spec](../systems/player-cards-skills-progression-market-and-exchange.md) · [Catalog P1-02](p1-football-catalog-and-model-foundation.md) · [Nghiên cứu eFootball](../research/efootball-mobile/efootball-mobile-research.md) · [Adoption matrix](../research/efootball-mobile/efootball-mobile-adoption-decision-matrix.md)

## 0. Mục lục

- [1. Mục tiêu và phạm vi](#goal)
- [2. Contract và boundary](#contracts)
- [3. Luồng transaction](#transactions)
- [4. Fixture và integration](#fixtures)
- [5. Failure, abuse và rollback](#failure)
- [6. Test và acceptance](#tests)
- [7. Evidence và decision lifecycle](#evidence)
- [8. Thứ tự triển khai](#delivery)

<a id="goal"></a>

## 1. Mục tiêu và phạm vi

Mục tiêu là foundation server-authoritative giả lập cho owned player inventory, deterministic progression, skill/position assignment và fusion trên stable catalog ID từ P1-02. Batch phải chứng minh atomicity, idempotency, revision/version rejection, reconnect reconciliation và read-only rollback trước khi mở economy production.

Trong phạm vi: contract domain thuần C#, fake authoritative repository/ledger, fixture direct grant, inventory projection/delta, preview/command/receipt, lock/protected-state checks, serialization N/N-1 và UI projection tối thiểu phục vụ test.

Ngoài phạm vi: backend thật, premium currency, random pack/RNG, Booster crafting/vượt cap, market giữa người chơi, paid respec, production balance, manager monetization và nhập dataset production.

<a id="contracts"></a>

## 2. Contract và boundary

```text
OwnedPlayerItem {
  itemId, ownerId, itemDefinitionId, catalogVersion, acquiredAt,
  levelXp, progressionAllocation, additionalSkills,
  positionProficiencies, lockState, state, revision, rulesVersion
}
InventorySnapshot { ownerId, revision, catalogVersion, rulesVersion, items }
InventoryDelta { baseRevision, targetRevision, upsertedItems, removedItemIds }
ProgressionPreview { previewHash, itemRevision, rulesVersion, cost, before, after, warnings, expiresAt }
ProgressionCommand { itemId, operation, payload, expectedRevision, previewHash, idempotencyKey }
TransactionReceipt { transactionId, idempotencyKey, status, ledgerEntries, inventoryDelta, failureCode }
FusionCommand { sourceItemId, targetItemId, expectedSourceRevision, expectedTargetRevision, previewHash, idempotencyKey }
```

Interfaces tối thiểu: `IInventoryRepository`, `IProgressionRuleSet`, `IProgressionPreviewService`, `IInventoryTransactionService`, `ITransactionReceiptRepository`, `ILedger`. Domain không tham chiếu Unity API; adapter fixture/file/in-memory nằm ở assembly integration riêng.

Client chỉ query projection, yêu cầu preview và gửi command. Authority giả lập xác minh owner, catalog/rules version, expected revision, balance/material, lock/state và idempotency; chỉ authority tạo delta/receipt. Direct grant chỉ tồn tại trong fixture/test adapter và vẫn đi qua ledger.

<a id="transactions"></a>

## 3. Luồng transaction

1. Load immutable catalog/rules và inventory snapshot.
2. Build preview canonical từ command intent + expected revisions; hash bao gồm catalog/rules version và expiry.
3. Confirm command với idempotency key; stale/expired/mismatch bị reject mà không mutation.
4. Reserve/check toàn aggregate, append balanced ledger entries và inventory delta trong một atomic commit.
5. Lưu receipt trước khi trả response; timeout/unknown chỉ poll receipt bằng cùng idempotency key.
6. Retry cùng key/payload trả cùng receipt; cùng key khác payload trả `IdempotencyConflict`.

Operation foundation: direct fixture grant, allocate/reset progression miễn phí trong test, assign deterministic additional skill, choose eligible position proficiency và fusion XP/skill. Không có random outcome.

<a id="fixtures"></a>

## 4. Fixture và integration

- Dùng player/item definition của fixture P1-02; tạo hai owner hư cấu, roster nhỏ và rules version fixed-width.
- Có item `Available`, `Locked`, `InSquad` và revision cũ để test destructive guard.
- Skill/position fixture có eligibility, cap, exclusion và trace label; không dùng tên/công thức eFootball production.
- Scene/menu legacy không bị chặn. Integration mới nằm sau feature flag, mặc định chỉ test/diagnostic cho tới khi UI collection được duyệt.
- Persistence fixture dùng atomic file hoặc in-memory; không dùng `PlayerPrefs` làm authority.

<a id="failure"></a>

## 5. Failure, abuse và rollback

| Failure/abuse | Kết quả bắt buộc |
| --- | --- |
| Duplicate callback/retry | Cùng receipt, không double grant/consume |
| Same key/different payload | Reject conflict, audit correlation |
| Stale item/catalog/rules | Reject + current projection; không auto-confirm |
| Locked/in-squad/reserved source | Reject với failure code và dependency |
| Source bằng target/owner mismatch | Reject trước reservation |
| Partial persistence/exception | Không inventory delta hoặc ledger nửa vời |
| Timeout/unknown | Receipt lookup; không tạo command mới |
| Corrupt snapshot/N-2 | Quarantine, last-known-good/read-only |
| Rollback rule/config | Pin N-1; giữ receipt và item revision hợp lệ |

Rollback feature flag đưa client về read-only inventory/catalog projection; disable từng operation/rule; compensation là ledger transaction mới, không sửa/xóa history.

<a id="tests"></a>

## 6. Test và acceptance

EditMode: serialization/canonical hash, N/N-1 migration, unknown field round-trip, preview determinism, cap/eligibility, revision/version/expiry, lock state, insufficient resource, idempotent retry/conflict, concurrent commands, balanced ledger, fusion atomicity và corrupt cache.

PlayMode: fixture load/direct grant, collection projection, operation apply/reload, timeout receipt recovery, feature-flag read-only rollback và Quick Match/Cup regression. Android smoke xác nhận compile/startup; console không có project-authored error.

Acceptance bắt buộc:

- Cùng snapshot/rules/intent tạo cùng preview hash và result.
- Mọi committed receipt có ledger cân bằng và đúng một inventory revision transition.
- Retry/reconnect không mất hoặc nhân đôi item/resource.
- Stale/locked/invalid operation không mutation.
- N/N-1 đọc/migrate được; N-2/corrupt chuyển read-only last-known-good.
- Legacy flow hoạt động khi feature flag tắt.

<a id="evidence"></a>

## 7. Evidence và decision lifecycle

Automated evidence có thể đưa `PCS-D01` và `PCS-D02` tối đa sang `InValidation`; không đủ cho `EvidenceReady` vì thiếu economy simulation, balance scenario, accessibility và human playtest. `PCS-D03` giữ `TestReady`; `PCS-D04` giữ `Blocked`. Research eFootball không tự thay đổi lifecycle.

Artifact phải ghi build/config/catalog/rules version, seed/fixture, test result, failure log, rollback rehearsal và giới hạn backend/device/human evidence. Không lưu PII hoặc production economy data.

<a id="delivery"></a>

## 8. Thứ tự triển khai

1. Understand brief cho flow `catalog → inventory → transaction → UI projection → analytics/audit`.
2. Domain assembly và immutable contracts/rules.
3. Repository, ledger, preview và transaction services.
4. Direct grant/progression/skill/position/fusion fixture.
5. Integration flag, persistence và read-only rollback.
6. EditMode/PlayMode/Android regression, docs/evidence và graph refresh.

Không bắt đầu market hoặc random offer cho tới khi foundation pass và các gate product/compliance tương ứng được mở.
