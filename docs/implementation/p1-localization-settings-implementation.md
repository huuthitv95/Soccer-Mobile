# P1-01 localization và settings — nhật ký triển khai và kiểm chứng

> [Chỉ mục](../index.md) · [Account/localization/settings](../systems/account-localization-and-settings.md) · [Decision validation](decision-validation-program.md) · [Unity audit và backlog](unity-implementation-audit-and-backlog.md)

## 0. Mục lục

- [1. Phạm vi và trạng thái](#scope-status)
- [2. Contract localization](#localization-contract)
- [3. Registry, scope và authority](#settings-contract)
- [4. Persistence và migration](#persistence-migration)
- [5. UI và localization assets](#ui-assets)
- [6. Evidence kiểm thử](#test-evidence)
- [7. Giới hạn và decision lifecycle](#limits-decisions)
- [8. Rollback và bước tiếp theo](#rollback-next)

<a id="scope-status"></a>

## 1. Phạm vi và trạng thái

P1-01 thêm foundation kỹ thuật cho locale `vi-VN`/`en`, settings typed, persistence local và panel chọn ngôn ngữ trên `MainMenu`. Batch không localize toàn bộ legacy UI, không triển khai voice-pack delivery, cloud backend, production account sync, brand font hoặc phê duyệt accessibility trên thiết bị thật.

Runtime mới nằm trong assembly `SoccerMobilePro.Platform` và `SoccerMobilePro.SettingsUI`; package Unity Localization cung cấp locale/string table. `LocalePreference` và `SettingsSnapshot` từ P0-03 được giữ nguyên contract công khai. Legacy scene flow và gameplay không nhận authority mới từ settings.

<a id="localization-contract"></a>

## 2. Contract localization

- `ILocaleResolver.Resolve` nhận account preference, local preference, device locale và thời điểm UTC; trả `LocaleResolutionResult` gồm preference đã chuẩn hóa và cờ `RequiresConfirmation`.
- Precedence là account hợp lệ → local hợp lệ → device exact → device language match → fallback `vi-VN`.
- Locale hỗ trợ production trong batch là `vi-VN` và `en`; `qps-ploc` chỉ là pseudo-locale QA, không xuất hiện trong lựa chọn người dùng.
- Device/fallback chưa được người dùng xác nhận luôn đặt `RequiresConfirmation=true`; account/local hợp lệ không buộc hỏi lại.
- Voice locale không hỗ trợ được đưa về text locale. Đây chỉ là fallback contract, không chứng minh audio package hoặc hot-swap đã có.

<a id="settings-contract"></a>

## 3. Registry, scope và authority

`SettingDefinition` khai báo key, `SettingValueType`, `SettingScope`, default, range/enum và cờ cloud-sync. Registry mặc định có các domain locale, audio, graphics, controls và accessibility; `match.assist` minh họa policy read-only.

| Scope | Authority trong batch | Đồng bộ |
| --- | --- | --- |
| `Account` | Client snapshot với revision; backend production chưa có | Chỉ key có `CloudSync=true` được merger xét |
| `Device` | Client local, gồm graphics/control tuning theo thiết bị | Không cloud-sync và luôn giữ bản local khi merge |
| `MatchPolicy` | Projection chỉ đọc; consumer resolve policy trước snapshot | Không ghi qua repository |

`ISettingsRegistry` tra definition và sanitize snapshot; field sai được đưa về default riêng, field hợp lệ và unknown key vẫn được giữ để round-trip. `ISettingsRepository` dùng optimistic revision và trả reason typed cho invalid key/value, read-only hoặc conflict. `SettingsCloudMerger` chỉ thực thi allowlist trong registry, từ chối account khác nhau và schema ngoài cửa sổ N/N-1; đây là deterministic client foundation, không thay security/threat review hay dịch vụ cloud thật.

<a id="persistence-migration"></a>

## 4. Persistence và migration

- `VersionedSettingsMigrator` chấp nhận schema N và N-1, hỗ trợ rename key đã khai báo và giữ unknown key; version cũ hơn bị từ chối.
- `FileSettingsRepository` lưu UTF-8 deterministic tại `Application.persistentDataPath`, ghi file tạm rồi replace/backup; không dùng `PlayerPrefs` làm authority.
- File thiếu, hỏng UTF-8/header/scalar hoặc schema không hỗ trợ quay về safe initial snapshot; primary hỏng có thể khôi phục atomic backup gần nhất và dọn file tạm cũ. Snapshot hợp lệ nhưng có một field sai chỉ sửa field đó.
- Mỗi write kiểm tra expected revision, normalize theo invariant culture và tăng revision. `locale.text`/`locale.confirmed` được validate toàn bộ rồi ghi trong một batch atomic và chỉ tăng một revision; production cloud transaction vẫn chưa tồn tại.
- `InMemorySettingsRepository` vẫn là adapter deterministic cho test/composition không cần I/O.

<a id="ui-assets"></a>

## 5. UI và localization assets

- Unity Localization có locale asset production `vi-VN`, `en` và asset pseudo-locale `qps-ploc` chỉ dành QA; pseudo-locale không được đưa vào Addressables/runtime locale provider. String collection `SettingsUI` chứa copy bootstrap/settings cho hai locale production.
- `SettingsUiBootstrap` chỉ tạo panel ở scene `MainMenu`, không chặn scene navigation. Nếu file repository không khả dụng, UI dùng session-only fallback và cảnh báo.
- `SettingsPanelPresenter` hỗ trợ first launch, Việt/English, Apply, Cancel và Reset về `vi-VN`; lựa chọn chỉ persist khi Apply/Reset thành công.
- `SettingsPanelBehaviour` tạo UGUI/TMP runtime, đợi Localization initialization bất đồng bộ, có text fallback khi table/key chưa sẵn sàng, focus target và safe-area adapter.
- Liberation Sans SDF dynamic fallback là font kỹ thuật tạm thời. Palette, typography, glyph coverage và motion production vẫn thuộc `UI-D01` và review accessibility.

<a id="test-evidence"></a>

## 6. Evidence kiểm thử

Unity 2022.3.62f3 runner đã báo:

- EditMode: **44/44 pass** toàn runner. P1-01 coverage gồm locale precedence/confirmation, chuỗi tiếng Việt, registry/range, sửa field hỏng, unknown-key round-trip, schema N/N-1, revision/read-only, batch atomic, match-policy override, cloud allowlist/device-local, account/schema merge guard, file round-trip, backup recovery và corrupt-file fallback.
- PlayMode: **12/12 pass** toàn runner. P1-01 coverage gồm first launch, apply/hot-switch, cancel, save failure giữ panel mở, reset `vi-VN`, safe area, exact Vietnamese string-table diacritics, pseudo-locale exclusion, focus graph và smoke load Quick Match/Cup.
- Automated tests không thay thế Android device matrix, human usability/accessibility review, voice download/resume profiling hoặc production cloud threat review.
- Android development build smoke: **pass**. Unity ghi `Build Successful` trong **361,602 ms**; APK tại `Temp/P1-01/SoccerMobilePro-smoke.apk` có kích thước **336.160.006 byte**, SHA-256 `1A4294FC20C03CACD001796580D310FAAF85DC83C924AF16AF33A8926BCEA603`. Kiểm tra ZIP xác nhận có `assets/aa/catalog.json`, `settings.json` và bốn bundle locale/string-table `vi-VN`/`en`. Artifact dưới `Temp/` chỉ là evidence local, không commit vào repository.

<a id="limits-decisions"></a>

## 7. Giới hạn và decision lifecycle

`ALS-D04` chuyển `TestReady → InValidation`: registry, merger và automated conflict/allowlist test đã tồn tại; device-only settings không được merge từ remote. Decision chưa đạt `EvidenceReady` vì còn thiếu backend schema diff, threat review, production conflict fixtures và Security/UX sign-off.

`ALS-D03` giữ `TestReady`: text-locale fallback không phải voice-pack download/hot-swap; còn thiếu memory, download/resume và low–high device matrix. `UI-D01` giữ `TestReady`: font fallback và UI tự động chưa chứng minh brand palette/font, contrast, glyph/overflow trên thiết bị hoặc human accessibility review.

Tổng lifecycle sau automated evidence P1-01 là **27 `TestReady`, 5 `InValidation`, 13 `Blocked`, 0 `EvidenceReady`, 0 `Approved`**. Foundation code không tự phê duyệt policy sản phẩm.

<a id="rollback-next"></a>

## 8. Rollback và bước tiếp theo

Rollback theo lớp: bỏ composition `SettingsUiBootstrap` để legacy `MainMenu` hoạt động không panel; giữ session-only fallback nếu file I/O lỗi; pin schema N-1 hoặc safe defaults nếu migration lỗi; bỏ Localization assets/package cùng assembly UI trong revert độc lập. Không xóa toàn bộ preference khi chỉ một field hỏng.

Batch đề xuất tiếp theo là P1-02 catalog/player/model pipeline. Dependency: `CatalogManifest`, Addressables và license metadata. Rủi ro chính: checksum/signature sai, referential integrity, tải dở, OOM và asset fallback. Acceptance: delta sync có rollback manifest, foreign-key validation, clean-cache/missing-asset test và model budget theo device tier.
