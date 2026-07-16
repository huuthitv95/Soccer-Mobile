# Audit triển khai Unity và backlog kỹ thuật

> [Chỉ mục](../index.md) · [GDD](../product/gdd-soccer-mobile-pro.md) · [Controls/VAR](../systems/match-controls-set-pieces-and-var.md)

**Mốc kiểm tra:** 16/07/2026. Tài liệu này mô tả trạng thái quan sát được trong checkout hiện tại, không khẳng định kiến trúc nội bộ của FC Mobile VN.

## 0. Mục lục

- [1. Understand brief](#understand-brief)
- [2. Kiểm kê và ranh giới audit](#inventory)
- [3. Bản đồ triển khai hiện tại](#implementation-map)
- [4. Khoảng trống so với hệ thống mục tiêu](#gap-analysis)
- [5. Backlog P0](#backlog-p0)
- [6. Backlog P1](#backlog-p1)
- [7. Backlog P2](#backlog-p2)
- [8. Thứ tự triển khai và Definition of Done](#delivery-order)

<a id="understand-brief"></a>

## 1. Understand brief

**Baseline audit:** HEAD `09af7f3`, kiểm tra lại ngày 16/07/2026 sau P0-01/P0-02/P0-03 và P1-01. Knowledge graph hiện hành tham chiếu content commit ngay trước graph commit; mọi số liệu inventory bên dưới được lấy trực tiếp từ Git, Build Settings, package lock và Unity test runner.

### 1.1 Nhiệm vụ và ranh giới hệ thống

- **Nhiệm vụ:** đối chiếu source Unity với định hướng sản phẩm cho luồng khởi động, menu, trận đấu, điều khiển, dữ liệu bóng đá, tài khoản, thẻ cầu thủ, VAR và AI offline; chuyển khoảng trống thành backlog có thể bàn giao.
- **Baseline trong phạm vi:** 94 script C#, 14 scene, 19 prefab, 7 assembly definition, Build Settings, package/input/addressable configuration và tài liệu sản phẩm liên quan; inventory hiện hành được ghi tại [mục 2](#inventory).
- **Ngoài phạm vi audit:** đổi scene/prefab/physics/reward, tạo backend, nhập toàn bộ dữ liệu/model production hoặc thay đổi cân bằng production.

### 1.2 Chủ sở hữu chính và trách nhiệm hiện tại

| Lớp | Chủ sở hữu quan sát được | Trách nhiệm |
| --- | --- | --- |
| Khởi động | `SplashVideoSequencePlayer` trong `SplashScene` | Phát chuỗi video bằng `VideoPlayer`, quản lý render texture/audio và tải `MainMenu`. |
| Điều hướng | `ButtonAction`, `GUI_MainMenu`, `GameManager` | Chọn quick match/cup, đội, vòng đấu và chuyển scene bằng tên chuỗi. |
| Khởi tạo trận | `InitGame`, `KickOffController` | Áp lựa chọn đội/trận, reset trạng thái hiệp/trận, điều phối kickoff và kết quả. |
| Input và người chơi | `SoccerInput`, `Joystick`, `Player` | Adapter Enhanced Touch + keyboard/gamepad; gameplay keyboard/gamepad chủ yếu nhánh Editor, gamepad Start/Pause dùng runtime; chọn cầu thủ gần bóng và action prototype. |
| AI offline | `AI_Striker`, `AI_MidfielderScript`, `AI_DefenderScript`, goalkeeper scripts | Di chuyển theo khoảng cách/vị trí, lựa chọn chuyền/sút bằng ngưỡng và timer. |
| Bóng và luật cơ bản | `BallScript`, goal/foul/corner handler scripts | Quyền sở hữu bóng, Rigidbody impulse, bàn thắng, foul, corner và goalkeeper restart. |
| Trình bày trận | `animationControllernew`, `RadarSystem`, score/time controllers, `AudioManager` | Legacy `Animation`, HUD/OnGUI, radar, âm thanh và trạng thái pause/result. |
| Lưu cục bộ | `PlayerPrefs` trong `ButtonAction`, `MatchesSceneController`, `InitGame`, audio/result scripts | Lưu giải cup, điểm số và music/SFX; không phải kho dữ liệu có phiên bản hoặc authority trực tuyến. |

### 1.3 Luồng điều khiển và dữ liệu end-to-end

```text
SplashScene / SplashVideoSequencePlayer
  -> MainMenu / ButtonAction
  -> GameSelectionScene -> team/cup scenes -> KickOffScene -> MatchScene
  -> SoccerInput + Joystick
  -> Player hoặc AI role scripts (class ComputerPlayer chỉ còn khối code legacy bị comment)
  -> BallScript + Rigidbody + foul/corner/goal triggers
  -> legacy Animation + HUD/radar/audio
  -> InitGame/MatchResult
  -> PlayerPrefs cup/result -> menu hoặc FinalCeleberation
```

- **UI/input:** scene và `OnGUI`/legacy GUI gọi trực tiếp logic MonoBehaviour; ngữ nghĩa nút thay đổi theo quyền sở hữu bóng trong `Player` nhưng chưa có action map theo context.
- **Domain:** `GameManager` là singleton C# không phải `MonoBehaviour`, giữ cờ trận trong RAM; nhiều script đọc/ghi trực tiếp và dựa vào tag/tên GameObject.
- **Physics/animation:** pass, shoot và goalkeeper dùng lực `Rigidbody`; clip legacy được gọi bằng tên chuỗi và timer, không có lớp command/event xác nhận animation-physics.
- **Persistence/network:** cup/audio legacy còn trong `PlayerPrefs`; P1-01 settings dùng file repository có schema, còn account/catalog mới dừng ở contract/fake repository. Chưa có backend, grant ledger, networking hoặc server authority.
- **Side effect:** scene load, audio, animation, thay component enabled, cập nhật score và ghi `PlayerPrefs` diễn ra trực tiếp từ gameplay scripts.

### 1.4 Trạng thái, invariant và phụ thuộc ẩn

- `IsGameReady`, `IsFirstHalf`, foul/corner/missed-goal flags quyết định gameplay có chạy hay chuyển handler; chúng phải nhất quán giữa cầu thủ, bóng, HUD và kickoff.
- `BallScript.ownerPlayer` là nguồn sự thật cục bộ về quyền sở hữu bóng; tag, khoảng cách và tên GameObject được dùng để tìm đồng đội/đối thủ.
- Scene name, animation clip name, tag, `PlayerPrefs` key và `GameObject.Find(...)` là contract ẩn không được type-check.
- Static flags trong role AI và `Player.noControls` có nguy cơ tồn tại qua vòng đời scene/domain reload không như mong muốn.
- Dữ liệu serialized nằm rải trong scene/prefab/Inspector; P0-01 thêm Match Core, P0-02 thêm Input, P0-03 thêm Platform contract và P1-01 thêm Settings UI/Localization. Legacy còn lại vẫn trong assembly mặc định và chưa có football catalog/model manifest runtime.
- Input System 1.14.2 đã có asset năm context map và adapter typed sau feature flag, trong khi `SoccerInput`/prefab joystick vẫn là runtime mặc định. Addressables 1.25.0 có các group Localization và được build cùng Player; chưa có group football catalog/model, remote catalog hoặc CDN production.

### 1.5 Test, rủi ro và rollback

- Checkout có 7 `.asmdef`, 37 EditMode và 12 PlayMode case do project định nghĩa. Baseline ngày 16/07/2026: toàn EditMode runner 44/44; PlayMode thực thi 12/12, gồm localization/settings và Quick Match/Cup smoke. Backend/provider/secure vault, touch HUD thật, controller reconnect và device-tier profiling vẫn là validation mở.
- Các scene gameplay trọng tâm: `SplashScene`, `MainMenu`, `GameSelectionScene`, hai team-selection scene, `GroupsScene`, `MatchesScene`, `KickOffScene`, `MatchScene`, `FinalCeleberation`.
- Rủi ro chính: coupling theo chuỗi/scene, state global, logic frame-dependent, legacy UI/Animation, thiếu authority và telemetry, thiếu test fixture.
- Kế hoạch xác nhận cho mỗi thay đổi tương lai: EditMode domain tests, PlayMode flow/match tests, console sạch, kiểm tra save migration, input trên touch/gamepad/keyboard và smoke test Android.
- Rollback mặc định: feature flag/config version; giữ adapter về flow prototype và snapshot dữ liệu cũ trong một bản phát hành; không migration phá hủy trước khi reconciliation hoàn tất.
- Tài liệu cần đồng bộ khi triển khai: GDD, UX/wireflow, controls/set pieces/VAR, live data, liveops và competitive integrity.

<a id="inventory"></a>

## 2. Kiểm kê và ranh giới audit

| Hạng mục | Kết quả xác nhận | Ghi chú |
| --- | ---: | --- |
| Unity Editor | 2022.3.62f3 | Từ `ProjectVersion.txt`. |
| Script C# trong `Assets/` | 94 | 79 legacy/khác, 9 foundation runtime và 6 test fixture. |
| Scene `.unity` trong `Assets/` | 14 | 11 scene được bật trong Build Settings; 3 scene test/ngoài danh sách build. |
| Prefab trong `Assets/` | 19 | Chủ yếu cầu thủ/bóng/audience/UI và joystick legacy. |
| Assembly definition | 7 | Match Core, Input, Platform, Settings UI và ba test assembly; legacy vẫn ở assembly mặc định. |
| Input Actions asset | 1 | Năm map `Match_OnBall`, `Match_OffBall`, `SetPiece`, `Goalkeeper`, `UI`; ba scheme Touch/Gamepad/Keyboard. |
| Automated C# test | 37 EditMode + 12 PlayMode | Baseline runner: EditMode 44/44; PlayMode thực thi 12/12. Chênh lệch EditMode đến từ test package/assembly ngoài fixture do project định nghĩa. |
| Package đáng chú ý | Input System 1.14.2, Addressables 1.25.0, Localization 1.5.12, Cinemachine, Test Framework, URP | Cài package không chứng minh có implementation sản phẩm tương ứng. |

`Assets/AddressableAssetsData/AddressableAssetSettings.asset` hiện phục vụ Localization và bật build cùng Player, nhưng chưa có football catalog/model delivery contract nêu trong [catalog spec](../systems/football-catalog-player-database-and-model-assets.md). Input foundation đã có action map theo context, nhưng feature flag mặc định tắt và chưa thay HUD/legacy controller.

Audit dùng Git/source/config tĩnh kết hợp Unity compile/test và Android/Addressables build gate; không sửa scene gameplay trong batch audit.

<a id="implementation-map"></a>

## 3. Bản đồ triển khai hiện tại

Mỗi backlog được chấm độc lập theo bốn gate; `Đạt` không có nghĩa feature production đã hoàn tất nếu gate khác còn thiếu.

| Backlog | Contract | Runtime integration | Automated evidence | Device/operations evidence |
| --- | --- | --- | --- | --- |
| P0-01 Match Core | Đạt | Một phần: shadow adapter, legacy vẫn authoritative | Đạt | Chưa đạt |
| P0-02 Contextual Input | Đạt | Một phần: feature flag mặc định tắt | Đạt | Chưa đạt |
| P0-03 Account/data platform | Đạt | Một phần: fake/offline adapter | Đạt | Chưa đạt backend/security owner gate |
| P1-01 Localization/settings | Đạt | Đạt trong MainMenu; legacy UI chưa chuyển đổi | Đạt | Một phần: Android smoke có, thiếu human/device matrix |
| P1-02 Catalog/model | Một phần: manifest/repository seam | Chưa đạt | Một phần: integrity/cache contract test | Chưa đạt |
| P1-03 Cards/progression/market | Chưa đạt | Chưa đạt | Chưa đạt | Chưa đạt |
| P1-04 Competition/integrity | Chưa đạt | Chưa đạt | Chưa đạt | Chưa đạt |
| P2 VAR/AI/telemetry | Một phần ở Match Core và heuristic AI | Chưa đạt mục tiêu | Một phần | Chưa đạt |

| Hệ thống | Đã có | Giới hạn hiện tại |
| --- | --- | --- |
| Bootstrap/menu | Splash video, menu, scene selection, quick match và cup flow. | Điều hướng hard-code theo scene name; chưa có login/session/loading/error state chuẩn. |
| Match state | Legacy hai hiệp/timer/score/restart + deterministic Match Core cho phase/command/snapshot/event/hash và shadow adapter mặc định tắt. | Core chưa điều khiển physics/HUD/result; legacy state vẫn phân tán; chưa có VAR/replay authority hoặc online server. |
| Điều khiển | Joystick trái; sprint/pass/shoot khi có bóng; sprint/tackle khi không bóng; keyboard trong Editor. | Rect theo pixel/legacy GUI, chưa remap, left-handed layout, assist profile, action context, gamepad scheme hoặc accessibility đầy đủ. |
| Bóng/animation | Rigidbody impulse, ball owner, clip chạy/chuyền/sút/tackle/goalkeeper. | Legacy `Animation`, timer và chuỗi clip; chưa có animation graph, motion matching hoặc gameplay-event synchronization. |
| AI offline | Heuristic theo role, khoảng cách, vùng sân, timer, chuyền/sút và goalkeeper reaction. | Không phải mô hình đã train; không tactical team model, perception/memory, difficulty profile, scenario evaluation hay telemetry tuning. |
| Tournament | Cup cục bộ 8 trận, chọn đội và lưu score bằng `PlayerPrefs`. | Không catalog giải/CLB có version, schedule/rules engine, server result authority, reconnect hoặc anti-cheat. |
| Settings | Music/SFX legacy; P1-01 thêm typed registry, file repository, locale `vi-VN`/`en`, N/N-1 migration và MainMenu panel có safe area. | Chưa localize legacy UI, chưa có cloud backend, voice delivery, production brand/accessibility review hoặc full graphics/control UI. |
| Content/data | Texture/team name gắn trong Inspector và scene/prefab. | Không player/club/league catalog, card instance, skills/PlayStyles, upgrade economy, model manifest hay asset delivery contract. |
| Online/liveops | Không quan sát thấy implementation. | Không account, CMS/config, economy/market, inbox/grant, purchases, telemetry hay support/audit trail. |

`ComputerPlayer` vẫn tồn tại như một type trong source nhưng phần hành vi nằm trong comment; AI đang hoạt động được phân chia cho `AI_Striker`, `AI_MidfielderScript`, `AI_DefenderScript` và các goalkeeper script. Vì vậy sơ đồ mục tiêu `Player/ComputerPlayer` phải được hiểu là ranh giới người chơi/máy, không phải xác nhận `ComputerPlayer` hiện là implementation runtime.

<a id="gap-analysis"></a>

## 4. Khoảng trống so với hệ thống mục tiêu

| Nhu cầu | Trạng thái | Khoảng trống bắt buộc |
| --- | --- | --- |
| Ngôn ngữ | Foundation P1-01 | Có locale/string table `vi-VN`/`en`, QA pseudo-locale và fallback; thiếu legacy-screen coverage, voice/assets delivery, RTL-readiness và production font review. |
| Settings | Foundation P1-01 | Có typed schema/scope, local file persistence, migration N/N-1, allowlist merge và locale UI; thiếu cloud backend, full control/camera/graphics/audio/accessibility UI và device/human validation. |
| Tài khoản | Contract/fake chỉ dùng test | Có immutable session, expiry/revoke/link conflict/retry test và secure-vault seam; chưa có provider/backend/privacy/keystore production. |
| Giải đấu/CLB/cầu thủ | Catalog contract/fake cache | Có manifest version/hash/signature seam, N/N-1 và offline fallback test; chưa có football entities/API/CDN/production signing. |
| Player database/model 3D | Chưa có | API/manifest do Soccer Mobile Pro sở hữu, Addressables/asset bundle pipeline, rig/LOD/material validation và fallback model. |
| Skills/PlayStyles | Chưa có | Taxonomy, gameplay modifier có giới hạn, UI projection, balance version và test determinism. |
| Nâng cấp thẻ/market | Chưa có | Card instance, inventory, upgrade transaction, economy ledger, idempotency, odds/compliance và server authority. |
| Điều khiển theo context | Foundation sau flag | Có năm Input Actions map, typed command, binding override/conflict test và Standard/LeftHanded profile; thiếu HUD production, assist policy, focus/reconnect và device playtest. |
| VAR | Chưa có | Deterministic rule outcome, incident record, presentation-only replay/timeline và fail-safe skip. |
| AI offline | Heuristic | Tactical/role/decision architecture, seeded simulation, difficulty tuning, scenario suite; nếu dùng ML cần dataset governance và model fallback. |
| Telemetry/testability | Chưa có | Event schema, privacy/consent, crash/performance metrics, automated domain/match/UI suites và assembly boundary. |

Authority đặc tả cho các khoảng trống: [account/settings](../systems/account-localization-and-settings.md), [competition/social](../systems/competitions-leagues-clubs-and-social.md), [catalog/model](../systems/football-catalog-player-database-and-model-assets.md), [cards/progression/market](../systems/player-cards-skills-progression-market-and-exchange.md), [offline AI](../systems/offline-ai-tactics-and-difficulty.md), [controls/VAR](../systems/match-controls-set-pieces-and-var.md) và [UI catalogue](../product/ui-design-system-and-screen-catalogue.md). Backlog bên dưới là handoff implementation cho các contract này; trạng thái “decision-complete” trong docs không thay đổi trạng thái code “chưa có”.

<a id="backlog-p0"></a>

## 5. Backlog P0

### P0-01 — Tách domain trận đấu và dựng test foundation

- **Owner layer:** Match Core + QA Automation.
- **Dependencies:** inventory state hiện tại, contract `BallScript`/score/foul/corner, assembly layout và test framework đã cài.
- **Input/output:** input là command + seeded tick + match config; output là immutable match events/snapshot cho presentation.
- **Client/server boundary:** offline client chạy authoritative local simulation; ranked server xác nhận command/result, client không tự cấp thưởng.
- **Acceptance:** state machine xử lý kickoff, in-play, foul, corner, halftime, fulltime; replay cùng seed/command cho cùng kết quả; adapter giữ prototype chạy được.
- **Analytics:** `match_started`, `match_state_changed`, `match_completed`, duration/reason/version; không gửi raw input hoặc PII.
- **Failure/abuse:** reject transition sai, duplicate result và payload/version không tương thích; giới hạn event buffer.
- **Accessibility:** pause và presentation không làm mất command; tốc độ/assist chỉ thay presentation hoặc config được công bố.
- **Tests:** EditMode state transition/property tests; PlayMode quick match/cup smoke; seed determinism và malformed command tests.
- **Rollback:** feature flag quay về controller hiện tại; giữ adapter và snapshot format cũ trong một release.

### P0-02 — Chuẩn hóa input theo context và accessibility

- **Owner layer:** Client Input + Match UX.
- **Dependencies:** P0-01 command contract, Input System, HUD wireflow và device matrix.
- **Input/output:** device controls + active context + assist profile -> typed match/UI commands và prompt glyph.
- **Client/server boundary:** client map thiết bị thành command; server chỉ nhận command hợp lệ, không nhận layout hoặc touch coordinate thô.
- **Acceptance:** action maps riêng on-ball, off-ball, set-piece, goalkeeper, UI; touch/gamepad/keyboard; remap, left-handed, scale/opacity/dead-zone và conflict validation.
- **Analytics:** action latency/error aggregate, remap completion, assist-profile distribution theo consent.
- **Failure/abuse:** mất thiết bị, multi-touch cancel, duplicate binding, focus loss và command spam đều có recovery/rate limit.
- **Accessibility:** target tối thiểu, color-independent state, haptic/audio toggle, one-handed preset và reduced motion.
- **Tests:** EditMode binding validation; PlayMode chuyển quyền sở hữu/context; device simulation, orientation, safe-area và reconnect controller.
- **Rollback:** giữ `SoccerInput`/joystick qua adapter và preset `legacy`; config remote có thể tắt action map mới.

### P0-03 — Account/session và nền tảng dữ liệu có phiên bản

- **Owner layer:** Backend Identity + Client Platform + Data Platform.
- **Dependencies:** privacy/consent policy, secure storage, environment config, stable IDs và API versioning.
- **Input/output:** credential/guest token + client/content version -> session, account profile, entitlement summary và catalog manifest đã ký.
- **Client/server boundary:** server sở hữu account, token, entitlement và catalog version; client cache read-only, không lưu secret trong `PlayerPrefs`.
- **Acceptance:** guest-first, link/recovery/logout/delete, refresh/expiry, maintenance/update-required, offline cached catalog và migration có idempotency.
- **Analytics:** auth outcome/reason, session refresh, catalog sync/version; pseudonymous ID và consent gate.
- **Failure/abuse:** credential stuffing, replay token, clock drift, account-link conflict, tampered manifest và retry storm được rate-limit/audit.
- **Accessibility:** lỗi đăng nhập đọc được bởi screen reader, focus đúng, không chỉ dùng màu, hỗ trợ bàn phím và locale fallback.
- **Tests:** contract/security tests; token expiry/revocation; guest-link matrix; corrupt/offline cache và backward compatibility.
- **Rollback:** remote disable provider/linking; guest offline fallback không cấp economy; hỗ trợ hai schema/catalog version trong cửa sổ rollback.

<a id="backlog-p1"></a>

## 6. Backlog P1

### P1-01 — Localization và settings schema

**Trạng thái:** foundation đã triển khai và automated test đạt; xem [nhật ký P1-01](p1-localization-settings-implementation.md). Product policy, voice/device/human evidence và cloud authority vẫn chưa hoàn tất.

- **Owner layer:** Client Platform + UX/Accessibility.
- **Dependencies:** P0-03 profile sync, string/font inventory, supported-locale policy và device capability tiers.
- **Input/output:** locale/device/account settings -> resolved strings/assets/font, runtime settings snapshot và sync patch.
- **Client/server boundary:** client áp presentation settings; server lưu preference hợp lệ/version, không quyết định match outcome.
- **Acceptance:** chọn ngôn ngữ lần đầu và trong settings; fallback `vi-VN` -> default; typed audio/graphics/control/accessibility settings; migration và conflict rule rõ ràng.
- **Analytics:** locale selected/fallback/missing-key count, settings changed/reset; không log nội dung người dùng.
- **Failure/abuse:** missing table/font, corrupt settings, cloud conflict, unsupported quality tier; dùng default an toàn và cảnh báo một lần.
- **Accessibility:** text scaling, contrast, caption, reduced motion, haptic/audio alternatives, screen-reader labels và không cắt chuỗi dài.
- **Tests:** locale pseudo-localization, diacritics/font fallback, schema migration, cloud conflict, safe-area và low-memory reload.
- **Rollback:** bundle locale trước đó và settings schema N-1; reset riêng field lỗi thay vì xóa toàn bộ preference.

### P1-02 — Catalog giải đấu, CLB, cầu thủ và model 3D

**Trạng thái:** mới có `CatalogManifest`, hash/signature seam và repository in-memory từ P0-03; chưa có football entity, delta installer, file store, Addressables model resolver hoặc fixture catalog.

- **Owner layer:** Football Data + Content Pipeline + Client Rendering.
- **Dependencies:** P0-03 manifest/versioning, Addressables, stable IDs, provenance/`rightsVersion` mapping và rig/LOD/material budgets. Quyền sử dụng toàn bộ giải/CLB/cầu thủ đã được người dùng xác nhận; đây không còn là blocker của batch foundation.
- **Input/output:** signed catalog/asset manifest -> league/club/player definitions, local cache, model address và fallback presentation.
- **Client/server boundary:** server/CMS phát catalog đã duyệt và checksum; client không suy đoán endpoint FC Mobile VN, chỉ đọc contract Soccer Mobile Pro.
- **Acceptance:** delta sync, referential integrity, season/version compatibility; model import validation cho skeleton, avatar, animation, LOD, texture/material và fallback generic.
- **Analytics:** catalog/asset version, download success/bytes/time, fallback reason và rendering tier.
- **Failure/abuse:** ID collision, mapping/provenance sai phiên bản, checksum mismatch, poisoned bundle, partial download và device OOM; quarantine + fallback.
- **Accessibility:** tên hiển thị/localized pronunciation, portrait/model có text alternative trong UI, giảm motion khi preview model.
- **Tests:** schema/foreign-key validation, signature/checksum, Addressables clean-cache load, LOD/device memory budgets và missing-asset fallback.
- **Rollback:** pin manifest/catalog version trước, CDN revoke bundle lỗi, giữ generic model/crest/kit fallback.

### P1-03 — Inventory, skills/PlayStyles và nâng cấp thẻ

- **Owner layer:** Economy Backend + Player Progression + Client Collection UI.
- **Dependencies:** P0-03 account, P1-02 catalog, balance config, ledger/idempotency và compliance decision.
- **Input/output:** card instance + materials/currency + versioned recipe -> authoritative transaction receipt, inventory delta và projected stats/skills.
- **Client/server boundary:** server sở hữu inventory, currency, skill unlock, upgrade result và audit log; client chỉ preview/gửi idempotency key.
- **Acceptance:** ID instance bất biến; preview chính xác; transaction atomic/idempotent; cap/eligibility/version rõ ràng; reconnect reconciliation không nhân đôi hoặc mất vật phẩm.
- **Analytics:** preview/start/success/failure, resource sink/source, skill usage/win-rate aggregate và balance version.
- **Failure/abuse:** replay request, negative balance, stale recipe, clock/receipt tampering, concurrent update và refund dispute đều audit được.
- **Accessibility:** stat delta có text/dấu, xác nhận chi phí, hold-to-confirm tùy chọn, không dựa riêng màu/animation.
- **Tests:** economy invariants/property tests, concurrency/idempotency, stale config, rollback receipt, UI loading/empty/error và localization.
- **Rollback:** disable recipe/skill bằng config; compensating ledger transaction, không sửa/xóa lịch sử; client fallback read-only inventory.

### P1-04 — Luật giải, kết quả, reconnect và competitive integrity

- **Owner layer:** Competition Backend + Match Services + Client Tournament UI.
- **Dependencies:** P0-01 match event, P0-03 identity, P1-02 catalog, anti-cheat/rules config và reward ledger.
- **Input/output:** eligibility + roster + rules + signed match result -> bracket/standing update, reward grant reference và audit record.
- **Client/server boundary:** server quyết định eligibility, schedule, ranked outcome, standing và reward; client hiển thị/cache, offline cup không cấp reward online.
- **Acceptance:** nhiều format giải có rules version; resume/reconnect; duplicate/out-of-order result an toàn; dispute/support correlation ID.
- **Analytics:** enter/match/reconnect/forfeit/complete, queue latency, integrity signal aggregate và rules version.
- **Failure/abuse:** smurf/multi-account, disconnect exploit, result replay, impossible roster, collusion signal; quarantine thay vì tự động cấp thưởng.
- **Accessibility:** bracket/list view tương đương, thời gian theo locale/timezone, thông báo deadline và screen-reader order.
- **Tests:** bracket/ranking property tests, reconnect timeline, duplicate result, season rollover, reward idempotency và degraded network.
- **Rollback:** freeze competition, pin rules version, recompute từ append-only event log và compensating reward transaction.

<a id="backlog-p2"></a>

## 7. Backlog P2

### P2-01 — VAR deterministic và presentation replay

- **Owner layer:** Match Rules + Replay/Presentation.
- **Dependencies:** P0-01 deterministic event/snapshot, incident taxonomy, camera/timeline assets và UX skip policy.
- **Input/output:** authoritative incident + evidence references -> immutable rule decision; presentation nhận decision để phát overlay/replay.
- **Client/server boundary:** server/match authority quyết định online; client replay không được thay đổi hoặc che giấu kết quả, offline dùng cùng rule engine.
- **Acceptance:** goal/offside/foul/penalty review theo phạm vi đã duyệt; timeout/fail-safe; skip presentation vẫn hiện decision và resume đúng tick.
- **Analytics:** incident/review type, decision, duration, skip/fallback và rules version; không dùng footage để tự quyết định luật.
- **Failure/abuse:** missing snapshot, replay desync, repeated trigger, tampered client decision; authority fallback và audit correlation ID.
- **Accessibility:** subtitle/text decision, reduced flash/motion, audio cue thay thế và nút skip có focus/target chuẩn.
- **Tests:** golden incident fixtures, boundary/timestamp tests, replay desync, timeout/skip, network reconnect giữa review.
- **Rollback:** tắt presentation bằng config nhưng giữ rule outcome; fallback banner ngắn và resume snapshot đã xác nhận.

### P2-02 — AI offline theo tactical/utility và pipeline đánh giá

- **Owner layer:** Gameplay AI + Match Simulation + Data Science (chỉ khi ML được duyệt).
- **Dependencies:** P0-01 deterministic core, player attributes/skills, scenario harness, performance budget và difficulty policy.
- **Input/output:** seeded world snapshot + tactic/role/difficulty -> bounded intent/command; output được match core xác nhận.
- **Client/server boundary:** offline inference/quyết định chạy trên client; tuning/model là signed content, không liên quan economy authority.
- **Acceptance:** perception, team shape, role assignment, utility decision, ball prediction, pressing/marking/pass/shot; difficulty không cheat hidden physics; chạy ổn định trên device tier mục tiêu.
- **Analytics:** scenario outcome, decision reason bucket, possession/xG/pass/press aggregate, frame cost và AI version theo consent.
- **Failure/abuse:** invalid state, oscillation, stuck player, model/config tamper, performance spike; timeout về heuristic an toàn.
- **Accessibility:** difficulty/assist mô tả rõ, có preset và adaptive option opt-in; không thay đổi ngoài trận mà không báo.
- **Tests:** seeded scenario suite, golden tactical shapes, invariant/fuzz, difficulty calibration, long-match soak và profiler trên thiết bị mục tiêu.
- **Rollback:** giữ heuristic hiện tại sau interface adapter; remote pin AI version và per-scenario fallback.

### P2-03 — Telemetry, observability và live configuration

- **Owner layer:** Platform/Observability + LiveOps + Privacy.
- **Dependencies:** event taxonomy từ các backlog trên, consent, environment separation, config signing và retention policy.
- **Input/output:** typed event/metric + pseudonymous context -> buffered batch, quality dashboards/alerts; signed config -> validated runtime snapshot.
- **Client/server boundary:** client thu thập tối thiểu và buffer; backend validate/rate-limit/redact; config không trao authority economy cho client.
- **Acceptance:** schema registry/version, offline queue có giới hạn, sampling/consent/delete, health dashboard và kill switch/audit log.
- **Analytics:** đây là nền tảng phát các event đã liệt kê; tự đo drop rate, batch latency, schema reject và config adoption.
- **Failure/abuse:** PII/secrets, event flood, forged economy event, stale/unsigned config, disk growth; redact/drop/quota/fallback default.
- **Accessibility:** telemetry không phụ thuộc thao tác accessibility; consent và privacy controls có screen-reader/keyboard/text rõ ràng.
- **Tests:** schema contract, consent on/off/delete, offline/clock skew, queue cap, signature/expiry, sampling và load tests.
- **Rollback:** kill switch thu thập/upload, pin config N-1, xóa queue lỗi; gameplay vẫn hoạt động với default đóng gói.

<a id="delivery-order"></a>

## 8. Thứ tự triển khai và Definition of Done

1. Dựng P0-01 trước để tạo seam/test cho mọi hệ thống trận; P0-02 và P0-03 có thể triển khai song song sau khi command/version contract ổn định.
2. P1-01 dùng profile contract; P1-02 cung cấp catalog cho P1-03/P1-04; transaction và giải đấu không được dựa vào `PlayerPrefs` làm authority.
3. P2-01 và P2-02 dùng deterministic match core; P2-03 phải có tối thiểu trước production rollout để đo lỗi, adoption và rollback.

Một backlog item chỉ hoàn tất khi contract và owner được duyệt, test nêu trên chạy đạt, Unity console không có lỗi mới, tài liệu authority được cập nhật, migration/rollback được diễn tập, và bằng chứng acceptance được gắn với build/version cụ thể. Mọi thay đổi nhiều file phải có Understand diff trước/sau; knowledge graph chỉ lưu khi ổn định và được yêu cầu review.
