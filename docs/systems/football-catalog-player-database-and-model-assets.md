# Football catalog, player database và model assets

> [Chỉ mục](../index.md) · [Nghiên cứu](../research/fc-mobile-vn-research.md) · [Live data](../operations/live-data-and-operations.md) · [Sổ nguồn](../research/fc-mobile-vn-source-register.md)

## 0. Mục lục

- [1. Mục tiêu và ranh giới bằng chứng](#goal)
- [2. Catalog contracts](#contracts)
- [3. Fetch và asset pipeline](#pipeline)
- [4. Authority, version và migration](#authority)
- [5. Failure, security và accessibility](#failure)
- [6. Analytics, QA và rollback](#quality)
- [7. Decision register](#decisions)

<a id="goal"></a>

## 1. Mục tiêu và ranh giới bằng chứng

Mục tiêu: catalog versioned cho identity/ratings/items và manifest asset 3D có fallback theo tier. Non-goal: mô tả endpoint, database engine, CDN, chữ ký hoặc pipeline face scan nội bộ EA/Garena. Không nguồn S01–S19 công khai các chi tiết đó; kiến trúc dưới đây chỉ dành cho Soccer Mobile Pro, tham chiếu T03/T04.

<a id="contracts"></a>

## 2. Catalog contracts

```text
CatalogManifest { schemaVersion, catalogVersion, region, season,
  effectiveAt, minClientVersion, entityHashes, assetCatalogVersion,
  signature, rollbackVersion }
PlayerIdentity { playerId, canonicalName, birthYear, nationalityIds,
  preferredFoot, identityRightsVersion }
PlayerSeasonRating { playerId, ratingVersion, positions, attributes,
  weakFoot, skillRating, effectiveFrom, effectiveTo }
PlayerItemDefinition { itemDefinitionId, playerId, programId,
  baseOvr, rulesVersion, visualProfileId }
ModelAssetManifest { modelAssetId, body, head, hair, materialSet,
  rigVersion, lods, dependencies, sizeBytes, checksum, fallbackId }
```

`OwnedPlayerItem` không nằm trong catalog public; đó là instance server-authoritative trong inventory. Player identity không chứa gameplay rating; item definition không chứa URL bundle trực tiếp. Input là signed manifest/version và query/filter; output là immutable projection hoặc delta có base version.

<a id="pipeline"></a>

## 3. Fetch và asset pipeline

```text
Bootstrap → verify manifest/signature → compare local version
→ fetch delta/pages → validate references/hash → atomically activate
→ prefetch squad assets → stream optional LOD → last-known-good fallback
```

Model production: rights/source record → ingest mesh/reference → retopology → shared humanoid rig → material/texture budget → LOD0–LOD3 → animation/skin validation → device-tier profile → Addressables build → signed remote catalog → staged QA. Head/kit/hair là dependency riêng để tái sử dụng; generic head/body là fallback. Asset preview có `NotRequested/Downloading/Ready/Failed/Evicted`, độc lập view state.

<a id="authority"></a>

## 4. Authority, version và migration

Publishing service ký manifest; client chỉ kích hoạt sau signature/hash/reference validation. Server match/economy kiểm tra `catalogVersion`/`rulesVersion` và từ chối command không tương thích. ID canonical bất biến; correction tạo version/effective interval. Migration additive trước, deprecate sau backward window; client N-2 được test. Asset catalog có rollback pointer và không xóa bundle client cũ còn tham chiếu.

<a id="failure"></a>

## 5. Failure, security và accessibility

| Failure | Fallback | Security/abuse |
| --- | --- | --- |
| Signature/hash sai | Không activate; dùng last-known-good | Key rotation/revocation, TLS, audit |
| Delta base mismatch | Fetch snapshot đầy đủ | Rate limit/cache stampede control |
| Dangling player/club/item ref | Reject publish | Pre-publish referential validator |
| Storage/memory thấp | LOD thấp/generic asset; cho quản lý pack | Size budget và eviction policy |
| Model/rig lỗi | Generic body/head, animation safe set | Quarantine asset version |
| Rights hết hạn | Hide new acquisition; approved fallback | Effective time/territory enforcement |

Player name có pronunciation/display override, search không dấu, text scale; portrait/model có alt label. Không dùng face/crest làm cách nhận diện duy nhất. Asset download hiển thị dung lượng, mạng, cancel/retry và không bắt người chơi đoán tiến độ.

<a id="quality"></a>

## 6. Analytics, QA và rollback

Event: `catalog_bootstrap_result`, `catalog_delta_result`, `reference_validation_failed`, `asset_download_result`, `asset_fallback_used`, `asset_evicted`, `model_validation_failed`. Không gửi asset source/licensing document hoặc PII.

QA: manifest signature, corrupt/partial download, N-2 migration, locale/search, rights boundary, 10k+ paging, lineup prefetch, memory pressure, missing dependency, animation/LOD pop. Mỗi asset profile ghi device tier, triangle/texture/memory/size/load-time budget và kết quả đo. Acceptance: activation atomic, không dangling ref, lineup luôn render bằng fallback, server từ chối version nguy hiểm và rollback rehearsed. Rollback đổi manifest pointer/disable asset profile; không sửa catalog active tại chỗ.

<a id="decisions"></a>

## 7. Decision register

Trạng thái/evidence chi tiết nằm tại [chương trình kiểm chứng](../implementation/decision-validation-program.md#decision-matrix).

| ID | Quyết định | Owner | Gate | Trạng thái |
| --- | --- | --- | --- | --- |
| CAT-D01 | Backend store/API shape | Backend/Data | Client schema/cache/integrity pass; còn thiếu backend load, audit và cost | `InValidation` |
| CAT-D02 | Addressables group/CDN | Client/DevOps | Staged update + rollback rehearsal | `TestReady` |
| CAT-D03 | Device-tier model budgets | Tech Art | Profiler trên target devices | `TestReady` |
| CAT-D04 | Face/body coverage launch | Production/Licensing | Rights, capacity và fallback quality | `Blocked` |
