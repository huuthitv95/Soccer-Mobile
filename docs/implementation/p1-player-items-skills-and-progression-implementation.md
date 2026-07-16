# Nhật ký P1-03 player items, skills và progression

> [Chỉ mục](../index.md) · [Kế hoạch P1-03](p1-player-items-skills-and-progression-plan.md) · [Cards/progression spec](../systems/player-cards-skills-progression-market-and-exchange.md) · [Decision program](decision-validation-program.md)

## 0. Mục lục

- [1. Kết quả và phạm vi](#result)
- [2. Understand brief](#understand-brief)
- [3. Contract và transaction authority](#contracts)
- [4. Persistence, integration và rollback](#persistence)
- [5. Bằng chứng kiểm thử](#evidence)
- [6. Decision lifecycle](#decisions)
- [7. Giới hạn còn lại](#limitations)

<a id="result"></a>

## 1. Kết quả và phạm vi

Foundation P1-03 đã được triển khai tại content baseline `dc23b73`. Hai assembly mới tách domain thuần C# khỏi adapter Unity: `SoccerMobilePro.PlayerItems` và `SoccerMobilePro.PlayerItems.Unity`. Phạm vi đã có owned-player inventory, deterministic preview, direct grant, progression allocation/reset, skill assignment, position choice, fusion, receipt/ledger atomic, file persistence N/N-1 và read-only rollback.

Batch không triển khai backend production, currency thật, random pack, market giữa người chơi, paid respec, UI collection production hoặc công thức balance cuối cùng. Legacy scene/match flow không bị thay thế; feature gate mới mặc định tắt và chỉ cho phép ghi khi diagnostic override được bật rõ ràng.

<a id="understand-brief"></a>

## 2. Understand brief

- **Boundary:** `CatalogSnapshot`/stable item definition ID → owned item/rules → preview → command → atomic inventory/receipt/ledger → read-only Unity projection.
- **Owner:** `PlayerItemContracts.cs` giữ entity/interface; `ProgressionRules.cs` giữ rule và canonical preview; `InventoryTransactions.cs` giữ authority in-memory; `InventoryPersistence.cs` giữ codec/file store; `PlayerItemsUnity` giữ feature gate/projection.
- **Invariant:** một idempotency key và cùng payload luôn trả cùng receipt; cùng key khác payload bị từ chối; stale revision/catalog/rules/preview không mutation; fusion chỉ commit khi source/target cùng hợp lệ; committed ledger phải cân bằng.
- **Persistence:** schema hiện hành là N=2; chỉ đọc N và N-1; unknown field được round-trip; active file được ghi staging rồi activate atomic; corrupt active chỉ đọc last-known-good backup.
- **Side effect:** domain không tham chiếu Unity API và không ghi `PlayerPrefs`; file adapter là authority giả lập cục bộ, chưa phải server authority production.
- **Rollback:** tắt feature gate hoặc ép read-only; pin rules/schema N-1; giữ receipt/ledger append-only, không sửa lịch sử giao dịch.

<a id="contracts"></a>

## 3. Contract và transaction authority

Các contract runtime gồm `OwnedPlayerItem`, `InventorySnapshot`, `InventoryDelta`, `ProgressionIntent`, `ProgressionPreview`, `ProgressionCommand`, `GrantCommand`, `TransactionReceipt` và `LedgerEntry`. Boundary được thể hiện qua `IInventoryRepository`, `ITransactionReceiptRepository`, `ILedger`, `IProgressionPreviewService` và `IInventoryTransactionService`.

Luồng mutation:

1. Authority đọc snapshot, catalog/rules version và item revision.
2. Preview service tạo kết quả và SHA-256 canonical từ intent, version, revision và expiry.
3. Transaction service xác minh idempotency, owner, lock/state, revision, catalog/rules, preview hash và thời hạn.
4. Inventory delta, receipt và ledger được commit trong cùng critical section/store transaction.
5. Retry sau timeout/reload đọc receipt đã lưu, không phát sinh grant hoặc revision thứ hai.

Skill modifier được giới hạn bởi fixture rules; assign skill và position chỉ chấp nhận ID đủ eligibility. Fusion xóa source và cập nhật target trong một commit; source `Locked`, `InSquad`, `Reserved`, khác owner hoặc trùng target bị từ chối trước mutation.

<a id="persistence"></a>

## 4. Persistence, integration và rollback

`FileInventoryStore` ghi envelope vào staging, dùng replace atomic khi active đã tồn tại và giữ backup N-1. Receipt cùng ledger được persist chung với inventory. Khi active bị hỏng nhưng backup hợp lệ, repository trả last-known-good ở chế độ read-only; schema N-2 bị từ chối thay vì migration suy đoán.

`PlayerItemsFeatureGate` mặc định `Enabled=false` và `ForceReadOnly=true`. `ConfigureForDiagnostics` là seam thử nghiệm, không phải remote production flag. `InventoryProjectionFactory` chỉ tạo projection hiển thị và không trao authority mutation cho client/scene legacy.

<a id="evidence"></a>

## 5. Bằng chứng kiểm thử

| Gate | Kết quả ngày 16/07/2026 | Phạm vi |
| --- | --- | --- |
| EditMode toàn project | 83/83 pass | 22 test P1-03 cộng regression P0/P1-01/P1-02 |
| PlayMode toàn project | 18/18 pass | 2 test gate/projection P1-03 cộng Quick Match/Cup và regression hiện hữu |
| Unity console | Sạch sau test | Không có project-authored error/warning mới |
| MCP project-local | Pass | Endpoint root `http://localhost:22113`; domain reload giữ cấu hình |
| Android development smoke | Blocked by tooling | Build đã qua preprocessing/Addressables và vào Android post-process; MCP 0.84.1 lặp Hub reconnect trong batchmode, không tạo APK nên không được tính pass |

Hai mươi hai EditMode test P1-03 phủ deterministic preview, lock/eligibility/cap, balanced ledger, idempotent retry/conflict, stale/tampered/expired command, concurrency, fusion atomic, direct grant, read-only, N/N-1, unknown-field round-trip, corrupt-active fallback và reconnect receipt recovery. Hai PlayMode test xác nhận default gate read-only, fixture projection và diagnostic writable override.

Android smoke không phải bằng chứng device acceptance. Log tạm không được commit; không có APK/hash để bàn giao. Cần tách MCP Editor connection khỏi batch build hoặc chạy build trên runner không khởi tạo Hub trước khi đóng gate này.

<a id="decisions"></a>

## 6. Decision lifecycle

- `PCS-D01`: `TestReady → InValidation` vì deterministic progression, atomic receipt/ledger, version/revision rejection và rollback đã có automated evidence. Economy simulation, power-gap metric và human playtest còn thiếu.
- `PCS-D02`: `TestReady → InValidation` vì taxonomy fixture, cap/eligibility và deterministic scenario test đã có. Balance diversity, gameplay telemetry và human/device validation còn thiếu.
- `PCS-D03`: giữ `TestReady`; market/load/liquidity chưa thuộc foundation này.
- `PCS-D04`: giữ `Blocked`; paid/free respec policy vẫn cần Product + Compliance sign-off.

Trạng thái tổng sau batch: 25 `TestReady`, 8 `InValidation`, 12 `Blocked`, 0 `EvidenceReady`, 0 `Approved`.

<a id="limitations"></a>

## 7. Giới hạn còn lại

- Fake/file authority chỉ chứng minh contract và failure semantics, không thay backend có authentication, concurrency phân tán, audit retention và recovery operations.
- Chưa có inventory/upgrade UI production, analytics transport, accessibility/human playtest hoặc device performance matrix.
- Chưa có currency/material balance, offer/odds, market, refund/compensation workflow hoặc production live configuration.
- Android smoke phải chạy lại sau khi xử lý MCP batchmode reconnect; trạng thái hiện tại không được dùng để phát hành build.
