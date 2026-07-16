# Account, localization và settings

> [Chỉ mục](../index.md) · [GDD](../product/gdd-soccer-mobile-pro.md) · [UX](../product/ux-wireflows-and-states.md) · [Nguồn](../research/fc-mobile-vn-source-register.md)

## 0. Mục lục

- [1. Mục tiêu và non-goal](#goal)
- [2. Entities và contract](#contracts)
- [3. State machine và flow](#states)
- [4. Authority, version và migration](#authority)
- [5. Failure, abuse và accessibility](#failure)
- [6. Analytics, QA và rollback](#quality)
- [7. Decision register](#decisions)

<a id="goal"></a>

## 1. Mục tiêu và non-goal

Mục tiêu: first launch có locale/consent rõ; Guest nâng cấp an toàn; session phục hồi đa thiết bị; setting có owner/default/scope/version. Non-goal: sao chép provider policy Global sang Garena, lưu credential trong Unity, hoặc tự merge hai inventory có xung đột. S09 là tham chiếu Global; provider Soccer Mobile Pro phải được product/legal khóa riêng.

<a id="contracts"></a>

## 2. Entities và contract

```text
AccountSession { accountId, sessionId, authLevel, provider, issuedAt,
  expiresAt, deviceIdHash, consentVersion, serverRevision }
LocalePreference { locale, voiceLocale, source, changedAt, version }
SettingsSnapshot { accountId, schemaVersion, revision, values,
  deviceOverrides, updatedAt }
```

`AccountSession` không chứa access/refresh token trong log hoặc analytics. `LocalePreference.source` là `device`, `first_launch`, `account` hoặc `fallback`. Mỗi setting definition có `key`, `type`, `defaultByTier`, `scope(local|account|matchPolicy)`, `restartRule`, `min/max/options`, `accessibilityLabelKey`, `owner` và `introducedVersion`.

Input là provider callback/guest intent, device locale/tier, legal version và local snapshot. Output là session authoritative, locale đã resolve, settings projection hoặc error có `reasonCode`/`correlationId`.

<a id="states"></a>

## 3. State machine và flow

```text
Boot → ResolveLocale → ConsentRequired/AccountGate → Authenticating
→ MergeCheck → Syncing → Ready
                 ↘ RecoverableFailure / Blocked
```

- Guest → Linked chỉ chuyển khi provider proof hợp lệ và server hoàn tất merge transaction. `MergeCheck` cho xem nguồn/đích, không cộng currency/item trùng tự động.
- Session: `Active → Refreshing → Active|Expired → Reauthenticate`; revoke trên thiết bị khác làm command ghi thất bại, không xóa cache read-only.
- Locale đổi: validate table/asset → preview → persist local → optional account sync → rerender. Nếu table thiếu, quay `vi` last-known-good và báo lỗi có thể hành động.
- Setting đổi: validate owner/policy → preview → apply local hoặc sync optimistic có revision → commit/conflict. Graphics có safe-mode watchdog; competitive assist bị match policy override và UI phải hiện override.

<a id="authority"></a>

## 4. Authority, version và migration

Server sở hữu account identity, consent, linked provider, entitlement, cloud setting revision và match policy. Client sở hữu token trong secure platform storage, device-only graphics/HUD, preview và cache. Không gửi PII/token trong telemetry.

Schema tăng version theo migration tuần tự; test từ hai version client gần nhất. Unknown key được giữ khi round-trip nhưng không render; key bị bỏ có deprecation window. `SettingsSnapshot` dùng compare-and-swap revision; conflict merge theo field owner, không last-write-wins mù. Legal/consent không rollback xuống version cũ.

<a id="failure"></a>

## 5. Failure, abuse và accessibility

| Tình huống | Hành vi/fallback | Chống abuse |
| --- | --- | --- |
| Provider unavailable/callback trùng | Giữ route, retry/cancel; callback idempotent | nonce/state/PKCE, allowlist |
| Guest mất máy | Cảnh báo trước; support chỉ theo proof hợp lệ | Không phục hồi bằng UID công khai duy nhất |
| Merge conflict | Freeze transaction, support/audit | Không client-selected balance |
| Settings revision conflict | Diff field, reload/merge theo owner | Rate limit sync |
| Locale/voice pack thiếu | Text fallback; subtitle luôn có | Checksum/signed catalog |
| Graphics crash loop | Safe-mode preset sau watchdog | Không sync preset lỗi sang thiết bị khác |

Provider buttons, error và consent có accessible name/focus order; font scale 200%; locale viết bằng native name; screen reader không đọc token/UID đầy đủ. Account delete/export cần re-auth, thời gian xử lý và trạng thái có thể theo dõi.

<a id="quality"></a>

## 6. Analytics, QA và rollback

Event: `locale_resolved`, `consent_result`, `auth_started/result`, `merge_result`, `session_refresh_result`, `setting_changed`, `setting_migration`, `safe_mode_entered`. Không ghi credential, form text hoặc provider identifier thô.

QA: cold/warm boot; vi/en; locale table thiếu; Guest/link/revoke; callback duplicate/late; offline read-only; merge conflict; reinstall/cross-device; schema N-2→N; graphics crash loop; account delete/export. Acceptance: không mất inventory, không dead-end auth, locale luôn render được, mọi setting có owner/default/migration, và transaction auth/merge idempotent. Rollback bằng provider flag, last-known-good tables/schema adapter và reset riêng field lỗi; không hạ consent.

<a id="decisions"></a>

## 7. Decision register

Trạng thái/evidence chi tiết nằm tại [chương trình kiểm chứng](../implementation/decision-validation-program.md#decision-matrix).

| ID | Quyết định | Owner | Gate | Trạng thái |
| --- | --- | --- | --- | --- |
| ALS-D01 | Provider/Guest policy theo khu vực | Product + Legal + Backend | Privacy/age/recovery review | `Blocked` |
| ALS-D02 | Merge rule | Economy + Support | Fake conflict test pass; còn thiếu backend ledger duplicate/loss và audit | `InValidation` |
| ALS-D03 | Voice locale/hot-swap | Audio + Client | Memory/download/resume device matrix | `TestReady` |
| ALS-D04 | Cloud-sync field allowlist | Security + UX | Automated allowlist/conflict pass; còn thiếu backend schema diff, threat review và sign-off | `InValidation` |

Foundation và automated evidence P1-01 được ghi tại [nhật ký localization/settings](../implementation/p1-localization-settings-implementation.md). `ALS-D03` giữ `TestReady` vì chưa có voice download/resume và device profiling; UI/font tự động không thay gate human/accessibility của `UI-D01`.
