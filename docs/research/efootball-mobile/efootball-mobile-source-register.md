# Sổ nguồn và claim register eFootball Mobile

> [Chỉ mục](../../index.md) · [Nghiên cứu](efootball-mobile-research.md) · [Inventory](efootball-mobile-feature-mechanic-inventory.md) · [Rủi ro](efootball-mobile-risk-control-matrix.md)

## 0. Mục lục

- [1. Quy ước](#conventions)
- [2. Nguồn chính thức](#sources)
- [3. Claim register](#claims)
- [4. Coverage và khoảng trống](#coverage)
- [5. Quy trình cập nhật](#update-process)

<a id="conventions"></a>

## 1. Quy ước

Corpus chốt ngày **16/07/2026**, ưu tiên Global English và ghi platform/version cạnh claim. Nhãn dùng chung: **Thông tin công khai đã xác minh**, **Suy luận thiết kế**, **Đề xuất cho Soccer Mobile Pro**. Baseline không dùng nguồn cộng đồng; nếu bổ sung sau này, một claim chỉ đạt `Medium` khi có hai nguồn độc lập và không mâu thuẫn nguồn chính thức.

Độ chắc chắn: `Cao` khi authority nói trực tiếp; `Trung bình` khi đúng trong một version/khu vực nhưng có drift; `Thấp` cho suy luận hoặc dữ kiện chưa xác nhận. `Hiện hành` nghĩa trang v5.x/overview còn hiệu lực tại cutoff; `Lịch sử` không được dùng như policy hiện tại.

<a id="sources"></a>

## 2. Nguồn chính thức

| ID | Nguồn | Version/ngày | Claim chính | Giới hạn |
| --- | --- | --- | --- | --- |
| EF-S01 | Konami — [Overview](https://www.konami.com/efootball/en/page/overview) | Hiện hành | Tutorial, Authentic/Dream Team, mode, Team Playstyle | Marketing/overview |
| EF-S02 | Konami — [Dream Team](https://www.konami.com/efootball/en/page/dreamteam) | Hiện hành | Acquisition, Game Plan, modes, rewards, development | Không công bố backend/odds |
| EF-S03 | Konami — [Mobile Controls Manual](https://www.konami.com/efootball/en/page/mobile_controller) | Hiện hành | Touch/controller, attack/defence/GK/set piece | Mapping có thể đổi theo setting |
| EF-S04 | Konami — [Online PvP Setup](https://www.konami.com/efootball/en/page/online_match) | Hiện hành | P2P/client-server, mode mapping, antenna | Không công bố netcode |
| EF-S05 | Konami — [v3.0.0](https://www.konami.com/efootball/en/page/2024/versioninfo_v3-00) | Lịch sử | Gameplay, Booster vượt 99 | Không mặc định còn hiện hành |
| EF-S06 | Konami — [v3.2.0](https://www.konami.com/efootball/en/page/2024/versioninfo_v3-20) | Lịch sử | My League, Booster condition, rewards | Version-specific |
| EF-S07 | Konami — [v3.6.0](https://www.konami.com/efootball/en/page/2024/versioninfo_v3-60) | Lịch sử | Gameplay/mode refinement | Version-specific |
| EF-S08 | Konami — [v3 carryover](https://www.konami.com/efootball/en/page/2024/update_v3_0) | Lịch sử | Migration currency/player/manager/item/settings | Không phải policy v5 |
| EF-S09 | Konami — [v4.0.0](https://www.konami.com/efootball/en/page/v4/versioninfo_v4-00) | Lịch sử | Smart Assist trial, Booster crafting | Policy assist đã đổi |
| EF-S10 | Konami — [v4.4.0](https://www.konami.com/efootball/en/page/v4/versioninfo_v4-40) | Lịch sử gần | Smart Assist League, backup slot, cross-platform | Platform-specific |
| EF-S11 | Konami — [v5.0.0](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-00) | v5.0 | Manager Link-up Play, gameplay/Match-up | Không công bố formula |
| EF-S12 | Konami — [v5.1.0](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-10) | v5.1 | Fix stamina/set piece/Smart Assist/UI | Patch delta, không phải full spec |
| EF-S13 | Konami — [v5.2.0](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-20) | v5.2 | Auto Control, Campaign Hub, Booster crafting | Feature có thể theo platform |
| EF-S14 | Konami — [v5.4.0](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-40) | v5.4 | Master League Sprint, gameplay | Event/version-specific |
| EF-S15 | Konami — [v5.5.0](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-50) | v5.5 | Assist matchmaking test, bulk release lock, content update | Thử nghiệm có thể thay đổi |
| EF-S16 | Konami — [Licenses](https://www.konami.com/efootball/en/page/license_efootball) | Hiện hành | Club/team availability và giới hạn license | Danh sách có thể thay đổi |
| EF-S17 | Konami — [Data transfer support](https://www.konami.com/efootball/mobile_support/en/transfer/about) | Mobile | Backup/restore qua KONAMI ID/platform | Không công bố session design |
| EF-S18 | Konami — [Manual transfer policy](https://www.konami.com/efootball/mobile_support/en/transfer/about2) | Mobile | Điều kiện hỗ trợ transfer thủ công | Không công bố merge |
| EF-S19 | Konami — [Link KONAMI ID](https://www.konami.com/wepes/efootball_point/en/page/link_konami_id/cs) | Mobile/console | Link flow và eFootball Points | Có thể khác region |
| EF-S20 | Konami Support — [Transfer to new device](https://us-support.konami.com/hc/en-us/articles/27229187359383-How-do-I-transfer-my-data-to-a-new-device) | Mobile | Cross-OS cần KONAMI ID | US support wording |
| EF-S21 | Konami Support — [Restore game data](https://us-support.konami.com/hc/en-us/articles/27229097140247-How-can-I-restore-my-game-data) | Mobile | Recovery khi chưa cấu hình transfer | Yêu cầu xác minh thủ công |
| EF-S22 | Google Play/Konami — [Store listing](https://play.google.com/store/apps/details?id=jp.konami.pesam&hl=en_US) | Android hiện hành | IAP/random items, features, data safety | Listing theo vùng |
| EF-S23 | Konami — [Account sharing prohibited](https://www.konami.com/efootball/en-us/topic/news/5279) | 02/06/2026 | Cấm share/sale/transfer account, fraud Coin | Không mô tả detection/sanction |
| EF-S24 | Konami — [Live Update disconnection](https://www.konami.com/efootball/en-us/topic/news/5320) | 01/06/2026 | Có thể về Title Screen, cần login lại | Incident cụ thể |
| EF-S25 | Konami — [Using Link Data Setting](https://www.konami.com/efootball/en-us/topic/news/4759) | 17/02/2026 | Link data và campaign reward | Reward theo campaign |
| EF-S26 | Konami — [International Cup rules](https://www.konami.com/efootball/en/page/international_cup/2022/representative_about) | 2022, lịch sử | Authentic Team, Challenge Event, eligibility | Không phải luật hiện hành |

<a id="claims"></a>

## 3. Claim register

| Claim ID | Claim | Nguồn | Version/platform | Chắc chắn |
| --- | --- | --- | --- | --- |
| EFC-001 | Dream Team cho ký player/manager và đăng ký Game Plan | EF-S02 | Hiện hành | Cao |
| EFC-002 | Standard ticket ký player ngẫu nhiên; pack có thể chứa player/manager/strip | EF-S02 | Hiện hành | Cao |
| EFC-003 | Event, League, My League, Quick/Friend/Co-op là các mode công khai | EF-S01, EF-S02, EF-S06 | Hiện hành + lịch sử xác lập | Cao |
| EFC-004 | Match Pass và Objective cấp reward; Friend Match có thể tính Match Pass | EF-S02 | Hiện hành | Cao |
| EFC-005 | Progression gồm Level, Points, Skill, Position và Fusion | EF-S02 | Hiện hành | Cao |
| EFC-006 | Additional Skill tối đa năm, Position Training tối đa hai vị trí ngẫu nhiên | EF-S02 | Hiện hành | Cao |
| EFC-007 | Booster có nhiều thế hệ, gồm vượt 99/crafting/condition | EF-S05, EF-S06, EF-S09, EF-S13 | v3–v5 | Trung bình do drift |
| EFC-008 | Game có năm Team Playstyles; manager mang affinity/skill chiến thuật | EF-S01, EF-S02, EF-S11 | Hiện hành/v5 | Cao |
| EFC-009 | Mobile manual có context Attack/Defence/GK/set piece | EF-S03 | Mobile hiện hành | Cao |
| EFC-010 | Smart Assist tự động hỗ trợ một số input và policy competitive thay đổi theo version | EF-S09, EF-S10, EF-S15 | v4–v5 | Cao |
| EFC-011 | Auto Control chỉ dùng VS AI và nhường control khi có user input | EF-S13 | v5.2 | Cao |
| EFC-012 | Online PvP dùng P2P hoặc client-server tùy mode | EF-S04 | Hiện hành | Cao |
| EFC-013 | Link Data qua KONAMI ID/platform; cross-OS cần KONAMI ID | EF-S17, EF-S20 | Mobile | Cao |
| EFC-014 | Carryover áp policy riêng cho currency, player, manager, item và setting | EF-S08 | Migration v3 | Cao cho lịch sử |
| EFC-015 | Bulk release v5.5 bỏ qua player đang lock | EF-S15 | v5.5 | Cao |
| EFC-016 | Account sharing/sale/credential sharing và Coin fraud bị cấm | EF-S23 | 2026 | Cao |
| EFC-017 | Live Update có thể ngắt session và trả về Title Screen | EF-S24 | 2026 | Cao |
| EFC-018 | Store listing công bố IAP gồm random items | EF-S22 | Android/region | Cao |
| EFC-019 | License/content data có thể đổi theo season update | EF-S08, EF-S15, EF-S16 | Seasonal | Cao |
| EFC-020 | Database, RNG, AI, matchmaking, anti-cheat và model pipeline nội bộ không được công khai | Tổng corpus | Cutoff | Cao về giới hạn corpus |

<a id="coverage"></a>

## 4. Coverage và khoảng trống

| Domain | Nguồn authority | Coverage | Khoảng trống công khai |
| --- | --- | --- | --- |
| Account/data transfer | EF-S17–EF-S21, EF-S25 | Đủ cho user flow | Merge/session/token schema |
| Locale/settings/accessibility | EF-S03, EF-S09–EF-S15, EF-S22 | Một phần | Full settings registry, screen reader |
| Home/objective/inbox | EF-S02, EF-S13, EF-S15 | Đủ cơ chế cấp cao | Full screen/state inventory |
| Squad/tactic/manager | EF-S01, EF-S02, EF-S11 | Đủ entity/intent | Modifier formula, AI tactic |
| Acquisition/player type | EF-S02, EF-S08, EF-S22 | Đủ taxonomy cấp cao | Odds/pity/duplicate theo offer |
| Progression/skill/position/fusion | EF-S02, EF-S05–EF-S15 | Đủ cơ chế/version drift | Server transaction/schema |
| Currency/shop/pass/reward | EF-S02, EF-S08, EF-S13, EF-S22 | Đủ taxonomy | Source/sink rate, entitlement ledger |
| Modes/social/esports | EF-S01, EF-S02, EF-S06, EF-S14, EF-S26 | Đủ mode inventory | Matchmaking/rules hiện hành đầy đủ |
| Gameplay/controls/assist | EF-S03, EF-S09–EF-S15 | Đủ command/policy drift | Physics/input algorithm |
| AI/difficulty | EF-S06, EF-S13, EF-S14 | Một phần | Architecture, training, difficulty formula |
| Live data/integrity | EF-S04, EF-S08, EF-S15, EF-S23, EF-S24 | Đủ risk signals | Detection, sanction, appeal, retention |

Coverage chỉ được coi `Đủ` khi claim có authority hoặc kết luận “không công khai”; không suy ra backend từ UI/patch note.

<a id="update-process"></a>

## 5. Quy trình cập nhật

1. Theo dõi tối đa năm bề mặt: Overview, Dream Team, Mobile Controls, trang version v5 hiện hành và announcements.
2. Alert chỉ dành cho thay đổi cơ chế, policy competitive, currency/reward, mode, platform hoặc migration; bỏ qua marketing copy.
3. Ghi source/version/date trước, sau đó cập nhật claim và mechanic; không sửa policy Soccer Mobile Pro ngầm.
4. Khi claim drift, giữ history và đánh dấu `Superseded`; không xóa bằng chứng cũ.
5. Firecrawl cache nằm ngoài repository. Google Search Console không thuộc quy trình này.
