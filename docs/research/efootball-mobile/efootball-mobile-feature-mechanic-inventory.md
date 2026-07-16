# Inventory tính năng và cơ chế eFootball Mobile

> [Chỉ mục](../../index.md) · [Nghiên cứu](efootball-mobile-research.md) · [Sổ nguồn](efootball-mobile-source-register.md) · [Rủi ro](efootball-mobile-risk-control-matrix.md) · [Adoption](efootball-mobile-adoption-decision-matrix.md)

## 0. Mục lục

- [1. Contract record](#record-contract)
- [2. Account, bootstrap và shell](#shell)
- [3. Acquisition, collection và progression](#collection)
- [4. Squad, tactic và manager](#squad)
- [5. Match modes và social](#match-modes)
- [6. Gameplay và controls](#controls)
- [7. Economy, liveops và operations](#liveops)
- [8. Coverage summary](#inventory-coverage)

<a id="record-contract"></a>

## 1. Contract record

Mỗi hàng là một mechanic có ID ổn định. `Entry/Input → State/Output` mô tả hành vi quan sát công khai, không phải API Konami. `Authority` là nơi Soccer Mobile Pro phải đặt quyền quyết định nếu áp dụng.

```text
MechanicRecord {
  id, name, statusVersionPlatform, entryCondition, input,
  stateTransition, output, costReward, limitExpiryRandomness,
  uiFailureAccessibility, proposedAuthority, evidenceIds,
  driftNote, dependency, adoptionLink
}
```

Quy ước status: `Current`, `Versioned`, `Historical`, `NotPublic`. Mọi số cân bằng, odds, entitlement và algorithm không có trong nguồn được để trống thay vì suy đoán.

<a id="shell"></a>

## 2. Account, bootstrap và shell

| EFM-ID | Cơ chế / status | Entry/Input → State/Output | Limit, failure, accessibility | Authority đề xuất | Evidence |
| --- | --- | --- | --- | --- | --- |
| EFM-ACC-01 | Link Data / Current mobile | Extras/Support hoặc Title → chọn KONAMI ID/platform → linked recovery path | Cross-OS cần KONAMI ID; lỗi credential/recovery; cần copy có thể đọc | Identity backend | EF-S17–EF-S20 |
| EFM-ACC-02 | Manual recovery / Current | Không tự transfer được → gửi inquiry → support xác minh/restore hoặc reject | Thiếu dữ liệu nhận dạng; SLA không công khai | Support + Identity | EF-S18, EF-S21 |
| EFM-ACC-03 | Link reward / Versioned | Link KONAMI ID → Line Objective → XP token | Reward campaign có thể hết hạn; double grant | Objective ledger | EF-S25 |
| EFM-UI-01 | Tutorial/Practice / Current | First launch/Practice → task/input → learned state/reward tùy campaign | Skip/retry, locale, assist disclosure | Client flow + profile flag | EF-S01, EF-S15 |
| EFM-UI-02 | Home/Dream Team navigation / Current | Home → Contract/Game Plan/Match/My Team → screen state | Loading/error/empty không được mô tả đầy đủ | Client navigation | EF-S01, EF-S02 |
| EFM-UI-03 | Settings/Smart Assist / Versioned | Extras → Game Settings → Play Settings → setting/mode policy | Một số match khóa thay đổi; disclosure competitive | Match policy + profile setting | EF-S09, EF-S10, EF-S15 |
| EFM-UI-04 | Controller setup / Current mobile | Bluetooth controller → detailed settings → binding active in match | Chỉ match; touch dùng đồng thời; reconnect | Client input | EF-S03 |
| EFM-UI-05 | Locale/accessibility / NotPublic partial | Store/app locale và assist setting → localized UI | Full locale list, screen reader, text scale không công khai | Client/platform | EF-S22 |

<a id="collection"></a>

## 3. Acquisition, collection và progression

| EFM-ID | Cơ chế / status | Entry/Input → State/Output | Limit/randomness/failure | Authority đề xuất | Evidence |
| --- | --- | --- | --- | --- | --- |
| EFM-ACQ-01 | Standard Player List / Current | Filter + GP/eligible resource → chọn player → owned player | Giá/version thay đổi; stale offer | Catalog + economy server | EF-S02 |
| EFM-ACQ-02 | Special Player List / Current | Limited list + Coin/deal → result player | Randomness/odds/duplicate theo offer không đủ nguồn | Offer + grant ledger | EF-S02, EF-S22 |
| EFM-ACQ-03 | Standard Player Ticket / Current | Ticket → random signing → owned player | Random result, duplicate handling không công khai | Offer + RNG service | EF-S02 |
| EFM-ACQ-04 | Pack / Current | Pack purchase/open → player/manager/strip bundle | IAP/random item, expiry/refund | Commerce + entitlement ledger | EF-S02, EF-S22 |
| EFM-ACQ-05 | Nominating/Selection/Chance Deal / Versioned | Contract/deal → choose hoặc draw → grant | Carryover/expiry và taxonomy drift | Offer + grant ledger | EF-S08, EF-S06 |
| EFM-COL-01 | My Team player inventory / Current | Acquired player → list/filter/lock/select → projection | Bulk release phải bỏ qua locked player | Inventory server | EF-S02, EF-S15 |
| EFM-COL-02 | Player release / Current | Select unlocked players → confirm → removed/compensation nếu có | Destructive, bulk error, locked/in-squad | Inventory + ledger | EF-S15 |
| EFM-PRG-01 | Level Training / Current | Program/match XP → XP/level | Player Type eligibility, cap/rule version | Progression service | EF-S02 |
| EFM-PRG-02 | Progression allocation / Current | Progression Points → manual/auto allocation → stat projection | Reset/cap/formula không công khai; stale rule | Progression service | EF-S02, EF-S22 |
| EFM-PRG-03 | Skill Training / Current | Training Program → Additional Skill assignment | Tối đa 5; duplicate/overwrite policy không công khai | Progression service | EF-S02 |
| EFM-PRG-04 | Position Training / Current | Eligible player + Program → random proficiency increase | Tối đa 2 vị trí; randomness/regret | Progression service | EF-S02 |
| EFM-PRG-05 | Player Fusion / Current | Source + target → transfer XP/Additional Skills → target update | Source consume, compatibility/version | Atomic progression transaction | EF-S02 |
| EFM-PRG-06 | Booster / Versioned | Player slot/condition + token/config → ability modifier | Vượt 99, random craft, team condition, power creep | Rules config + progression | EF-S05, EF-S06, EF-S09, EF-S13 |
| EFM-PRG-07 | Carryover/migration / Historical authority | Pre-update asset → per-type rule → carry/reset/convert | Partial migration, expiry, changed licenses | Migration service + ledger | EF-S08 |

<a id="squad"></a>

## 4. Squad, tactic và manager

| EFM-ID | Cơ chế / status | Entry/Input → State/Output | Limit/failure | Authority đề xuất | Evidence |
| --- | --- | --- | --- | --- | --- |
| EFM-SQD-01 | Game Plan / Current | Owned roster + formation → registered lineup/bench | Invalid/duplicate/ineligible roster | Squad service | EF-S02 |
| EFM-SQD-02 | Game Plan Lists / Current v5.5 | Save preset → named list → activate | V5.5 tăng tối đa 20; conflict/revision | Profile/squad service | EF-S15 |
| EFM-SQD-03 | Manager signing / Current | Manager List + resource → owned manager | Affinity/skill version, duplicate | Catalog + inventory | EF-S02 |
| EFM-TAC-01 | Team Playstyle / Current | Chọn một tactical identity → team behavior projection | 5 style công khai; formula không công khai | Match rules config | EF-S01 |
| EFM-TAC-02 | Manager affinity / Current | Manager + tactical setup → proficiency effect | Modifier/cap không công khai | Match rules config | EF-S02 |
| EFM-TAC-03 | Manager Link-up Play / v5 | Eligible manager + conditions → tactical behavior | Chỉ manager được chọn; trigger/counterplay | Match rules config | EF-S11 |
| EFM-TAC-04 | Player role/form/condition / Versioned | Player state + match context → role/condition projection | Stamina/condition drift; formula không công khai | Match simulation/rules | EF-S06, EF-S12 |

<a id="match-modes"></a>

## 5. Match modes và social

| EFM-ID | Mode/status | Entry/Input → State/Output | Limit/reward/network | Evidence |
| --- | --- | --- | --- | --- |
| EFM-MOD-01 | Authentic Exhibition / Current | Authentic Team + settings → VS AI match → result | Offline/PvE scope theo platform | EF-S01 |
| EFM-MOD-02 | Dream Team Event / Current | Eligibility + Game Plan → VS AI/PvP → progress/reward | Event/rules/expiry versioned | EF-S02 |
| EFM-MOD-03 | eFootball League / Current | Division queue + Game Plan → PvP → rating/division/reward | Phase reset, assist matchmaking test | EF-S02, EF-S15 |
| EFM-MOD-04 | My League / Current | Team/season → VS AI fixtures → My League Point/loan/condition | Reset/event drift; no online reward inference | EF-S06, EF-S15 |
| EFM-MOD-05 | Quick Match / Current | Casual queue → PvP → result không tính record | Matchmaking/net policy | EF-S02, EF-S04 |
| EFM-MOD-06 | Friend Match 1v1 / Current | Room/settings/friend → PvP → friendly result | Room/reconnect; Match Pass eligibility | EF-S02 |
| EFM-MOD-07 | Co-op 3v3 / Current | Room + tối đa 3 người/side → cooperative match | Cross-platform/version; griefing/role clarity | EF-S02, EF-S10 |
| EFM-MOD-08 | Master League Sprint / v5.4 event | Club manager event → sign/train/lineup → condensed campaign | Không coi là permanent mode | EF-S14 |
| EFM-MOD-09 | International Cup / Historical | Region/team eligibility → Challenge Event → representative result | Luật 2022, không dùng cho 2026 | EF-S26 |

<a id="controls"></a>

## 6. Gameplay và controls

| EFM-ID | Cơ chế/status | Entry/Input → State/Output | Failure/fairness/accessibility | Evidence |
| --- | --- | --- | --- | --- |
| EFM-CTL-01 | On-ball attack / Current mobile | Stick/gesture/button → move/pass/through/cross/shot/dash/feint | Context ambiguity, latency, reach | EF-S03 |
| EFM-CTL-02 | Stunning commands / Current | Modified input → stronger action có wind-up | Risk/reward và mis-input | EF-S03 |
| EFM-CTL-03 | Off-ball defence / Current | Match-up/Pressure/Call/Tackle/Charge → defensive intent | Assist strength, foul, dominant input | EF-S03, EF-S11 |
| EFM-CTL-04 | Goalkeeper / Current | GK context + command → rush/position/distribute | Context switch/focus | EF-S03 |
| EFM-CTL-05 | Set pieces / Current | Restart context + direction/power/receiver → restart | Seamless transition bugs/version drift | EF-S03, EF-S12 |
| EFM-CTL-06 | Smart Assist / Versioned current | Setting + user input → auto direction/power/context support | Ranked allowlist, disclosure, matchmaking | EF-S09, EF-S10, EF-S15 |
| EFM-CTL-07 | Auto Control / v5.2 VS AI | Không input → AI controls; user input → immediate handoff | Chỉ VS AI; phải báo trạng thái | EF-S13 |
| EFM-NET-01 | PvP transport selection / Current | Mode/region → P2P hoặc client-server → session | Disconnect, latency, host advantage | EF-S04 |
| EFM-NET-02 | Connection antenna / Current | Match network sample → stability graph/icon | Không công bố metric/formula | EF-S04 |

<a id="liveops"></a>

## 7. Economy, liveops và operations

| EFM-ID | Cơ chế/status | Entry/Input → State/Output | Risk/authority | Evidence |
| --- | --- | --- | --- | --- |
| EFM-ECO-01 | GP / Current | Match/event/objective → balance → signing/training use | Inflation; economy ledger | EF-S02, EF-S08 |
| EFM-ECO-02 | eFootball Coins / Current | Purchase/reward → premium balance → offer | Fraud/refund/age/odds; commerce ledger | EF-S08, EF-S22, EF-S23 |
| EFM-ECO-03 | eFootball Points / Current | Linked account/activity → points → exchange/use | Account-link/region/expiry | EF-S19 |
| EFM-LIV-01 | Match Pass / Current | Completed eligible match → pass progress → reward | FOMO/paid tier/expiry | EF-S02 |
| EFM-LIV-02 | Objectives/Line Objective / Current | Challenge event → progress → reward | Duplicate/event expiry | EF-S02, EF-S25 |
| EFM-LIV-03 | Campaign Hub / v5.2 | Campaign progress/currency → hub/shop → reward | Temporary economy/migration | EF-S13 |
| EFM-OPS-01 | Live Update / Current | Scheduled update → session/title transition → re-login | Disconnect/result recovery | EF-S24 |
| EFM-OPS-02 | Seasonal content update / Current | Catalog/license/content version → migrate/download → new projection | Removed/renamed content, rollback | EF-S08, EF-S15, EF-S16 |
| EFM-INT-01 | Account conduct / Current | Risk report/evidence → review/sanction | Detection/appeal/SLA not public | EF-S23 |

<a id="inventory-coverage"></a>

## 8. Coverage summary

Inventory có **56 mechanic record**: account/UI 8, acquisition/collection/progression 14, squad/tactics 7, mode/social 9, controls/network 9 và economy/liveops/operations 9. Mỗi domain yêu cầu ban đầu có ít nhất một record hoặc trạng thái `NotPublic`.

Các khoảng trống không công khai được chuyển thành risk/gate, không thành claim: full UI state map, locale/accessibility taxonomy, odds/pity, server schema, AI architecture, matchmaking formula, anti-cheat detection, sanction/appeal, telemetry/retention và model pipeline.
