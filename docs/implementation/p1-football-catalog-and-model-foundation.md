# Nhật ký P1-02 football catalog và model foundation

> [Chỉ mục](../index.md) · [Catalog/model spec](../systems/football-catalog-player-database-and-model-assets.md) · [Unity audit](unity-implementation-audit-and-backlog.md) · [Decision program](decision-validation-program.md)

## 0. Mục lục

- [1. Phạm vi và Understand brief](#scope)
- [2. Contract và ranh giới assembly](#contracts)
- [3. Fixture và luồng cài đặt](#fixture)
- [4. Addressables và fallback model](#addressables)
- [5. Evidence kiểm thử](#evidence)
- [6. Giới hạn, rollback và quyết định](#limits)
- [7. Bước tiếp theo](#next-step)

<a id="scope"></a>

## 1. Phạm vi và Understand brief

Batch ngày 16/07/2026 triển khai foundation P1-02 bằng fixture đại diện, không nhập dữ liệu hoặc model production. Quyền sử dụng toàn bộ giải đấu, CLB và cầu thủ đã được người dùng xác nhận; vì vậy thiếu license không còn là risk của batch. `rightsVersion` và `provenanceId` vẫn bắt buộc để quản lý mapping, mùa giải và nguồn kỹ thuật.

Ranh giới sở hữu:

- `SoccerMobilePro.Platform` tiếp tục sở hữu API công khai `CatalogManifest`, `CatalogReadResult` và `ICatalogRepository`; batch không phá contract hiện có.
- `SoccerMobilePro.Catalog` là assembly C# thuần, sở hữu entity, codec, validation, delta, file store, installer và fixture.
- `SoccerMobilePro.Catalog.Unity` là adapter Unity/Addressables, không đẩy `GameObject` hoặc address vào domain thuần.
- Scene/prefab gameplay legacy không bị sửa và chưa đọc catalog fixture; generic fallback chỉ tái sử dụng prefab cầu thủ hiện có qua Addressables.

Luồng dữ liệu là `manifest/payload → signature/hash → parse → schema/reference validation → staging → atomic activate → model resolution → generic fallback`. Invariant chính: không sửa snapshot active tại chỗ; delta phải khớp base version; lỗi không được chặn scene flow.

<a id="contracts"></a>

## 2. Contract và ranh giới assembly

Foundation bổ sung `LeagueDefinition`, `CompetitionDefinition`, `ClubDefinition`, `PlayerIdentity`, `PlayerSeasonRating`, `PlayerItemDefinition`, `PlayerClubRegistration`, `ModelAssetManifest`, `CatalogSnapshot` và `CatalogDelta`.

`CatalogVersion` là chuỗi đúng 12 chữ số fixed-width và so sánh ordinal. Delta có `baseVersion`, `targetVersion`, tập upsert/removal và hash canonical của payload. `DefaultCatalogValidator` từ chối ID rỗng/trùng, foreign key treo, rating/item/model không có player, club không có league, fallback cycle, base mismatch và hash delta sai.

JSON dùng dependency trực tiếp `com.unity.nuget.newtonsoft-json` 3.0.2. Codec bỏ qua field chưa biết để client N có thể round-trip payload additive từ N+1; installer chỉ chấp nhận schema hiện tại và N-1.

<a id="fixture"></a>

## 3. Fixture và luồng cài đặt

Fixture hư cấu gồm 2 giải, 4 CLB, 44 cầu thủ, registration/rating/item đại diện và 2 model profile. Không có tên, logo hoặc model production. Một model profile trỏ tới generic player hợp lệ; profile còn lại cố ý dùng address thiếu để kiểm thử fallback.

`CatalogInstaller` thực hiện:

1. Kiểm tra manifest, expiry, client schema, signature và SHA-256.
2. Parse JSON và kiểm tra schema/reference.
3. Ghi snapshot staging ra file mới.
4. Atomic activate con trỏ version; snapshot cũ vẫn là last-known-good.
5. Khi delta lỗi hoặc rollback target không tồn tại, giữ nguyên active snapshot.

Rollback rehearsal đã cài snapshot `202607160001`, áp delta `202607160002`, rồi quay lại version ban đầu; active pointer và dữ liệu fixture trở về đúng snapshot N-1.

<a id="addressables"></a>

## 4. Addressables và fallback model

Hai group local được thêm: `FootballCatalog-Local` và `FootballModels-Local`. Address chính là `football/catalog/fixture` và `football/model/generic-player`. Resolver hỗ trợ `NotRequested`, `Downloading`, `Ready`, `Failed`, `Evicted`.

Checksum/manifest bị từ chối, address thiếu hoặc load thất bại sẽ resolve generic model nếu fallback hợp lệ. Instance được release qua Addressables và state có thể chuyển sang `Evicted`; gameplay không chờ model production để tiếp tục scene flow.

Đây là delivery fixture local, không phải CDN production. `CAT-D03` vẫn `TestReady` vì chưa có profiler capture trên Android thấp/trung/cao.

<a id="evidence"></a>

## 5. Evidence kiểm thử

| Gate | Kết quả ngày 16/07/2026 | Kết luận |
| --- | --- | --- |
| EditMode toàn project | 59/59 pass, 1,459 giây | Gồm 15 case catalog mới và regression foundation cũ. |
| PlayMode | 16/16 pass, 2,098 giây | Gồm 4 case fixture load, clean-cache generic load, rejected/missing model fallback, evict và Quick Match/Cup regression. |
| Addressables clean build | Pass | Hai group football local được build; không có entry lỗi. |
| Content-update simulation | 0 asset modified | Content state hiện tại tương thích rollback local. |
| Android Development build | `Succeeded`, Unity exit 0; APK 348.323.263 byte | SHA-256 `5F2D80166A413575A7C93B77939DE84C58DB12F74BAC9BA20CBC20B4C44E2D86`. |
| Android build log | 3 error/13 warning trong `BuildSummary` | Ba error là MCP Hub không kết nối trong batchmode; Gradle/Player build vẫn thành công. Đây là tooling noise, không phải compile/player error; console Editor phải được kiểm tra riêng. |

APK và log là artifact tạm ngoài Git; repository chỉ lưu kết luận, hash và giới hạn. Không có dữ liệu người dùng hoặc production asset trong evidence.

<a id="limits"></a>

## 6. Giới hạn, rollback và quyết định

- `CAT-D02` chuyển `TestReady → InValidation`: staged activation, delta và rollback local đã pass, nhưng còn thiếu remote CDN/operations rehearsal.
- `CAT-D04` chuyển `Blocked → TestReady`: blocker license đã được người dùng giải quyết; capacity, coverage và fallback-quality matrix chưa được kiểm chứng.
- `CAT-D01` giữ `InValidation`: client contract/cache có evidence, còn thiếu backend load/cost/audit và production signing.
- `CAT-D03` giữ `TestReady`: chưa có memory/frame/load-time profiler trên ba device tier.

Tổng lifecycle sau batch: 27 `TestReady`, 6 `InValidation`, 12 `Blocked`, 0 `EvidenceReady`, 0 `Approved`. Rollback runtime là pin catalog N-1, giữ active snapshot cũ, evict model lỗi và dùng generic player. Không có migration phá hủy.

<a id="next-step"></a>

## 7. Bước tiếp theo

P1-03 triển khai theo [kế hoạch player items, skills và progression foundation](p1-player-items-skills-and-progression-plan.md) trên stable catalog ID. Nghiên cứu eFootball đã khóa direct fixture grant, deterministic skill/position path và fusion atomic; random pack, Booster, paid respec và market nằm ngoài batch. Acceptance tối thiểu vẫn là preview deterministic, receipt atomic/idempotent, stale-version rejection, reconnect reconciliation và read-only rollback.
