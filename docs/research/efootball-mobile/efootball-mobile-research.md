# Nghiên cứu tham chiếu eFootball Mobile

> [Chỉ mục](../../index.md) · [Sổ nguồn](efootball-mobile-source-register.md) · [Inventory cơ chế](efootball-mobile-feature-mechanic-inventory.md) · [Ma trận rủi ro](efootball-mobile-risk-control-matrix.md) · [Quyết định áp dụng](efootball-mobile-adoption-decision-matrix.md)

**Mốc nghiên cứu:** 16/07/2026  
**Phạm vi hiện hành:** eFootball v5.x trên iOS/Android; trang lịch sử v3/v4 chỉ dùng để theo dõi drift.  
**Authority:** nguồn Konami, Google Play và hỗ trợ Konami chính thức. Không dùng Google Search Console.

## 0. Mục lục

- [1. Kết luận điều hành](#summary)
- [2. Account và bootstrap](#account)
- [3. Dream Team và vòng lặp](#dream-team)
- [4. Cầu thủ và progression](#players)
- [5. Game Plan, manager và tactics](#tactics)
- [6. Chế độ chơi](#modes)
- [7. Gameplay, controls và assist](#gameplay)
- [8. Economy, reward và liveops](#economy)
- [9. Social, integrity và esports](#integrity)
- [10. Live data và vận hành](#operations)
- [11. Ranh giới bằng chứng](#boundaries)
- [12. Tác động tới Soccer Mobile Pro](#impact)

<a id="summary"></a>

## 1. Kết luận điều hành

- **[Thông tin công khai đã xác minh]** eFootball Mobile kết hợp Authentic Team với Dream Team: ký cầu thủ/manager, đăng ký Game Plan, phát triển cầu thủ và tham gia VS AI/PvP. Nguồn: [EF-S01](https://www.konami.com/efootball/en/page/overview), [EF-S02](https://www.konami.com/efootball/en/page/dreamteam), truy cập 16/07/2026. Chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Player development hiện công khai Level Training, Progression Points, tối đa năm Additional Skills, Position Training tối đa hai vị trí ngẫu nhiên và Player Fusion truyền XP/skill. Nguồn: [EF-S02](https://www.konami.com/efootball/en/page/dreamteam), truy cập 16/07/2026. Chắc chắn: **Cao theo trang hiện hành**.
- **[Thông tin công khai đã xác minh]** Smart Assist tự động hỗ trợ một số khía cạnh như lực sút/hướng rê; phạm vi competitive đã thay đổi qua v4.0, v4.4 và v5.5, trong đó v5.5 thử ghép đối thủ Division 1–3 theo setting assist. Nguồn: [EF-S09](https://www.konami.com/efootball/en/page/v4/versioninfo_v4-00), [EF-S10](https://www.konami.com/efootball/en/page/v4/versioninfo_v4-40), [EF-S15](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-50). Chắc chắn: **Cao**, nhưng policy có drift.
- **[Suy luận thiết kế]** Giá trị tham chiếu lớn nhất không nằm ở việc sao chép số cân bằng mà ở cách tách acquisition, immutable player definition, owned instance, development resource, tactical identity và match-mode policy.
- **[Đề xuất cho Soccer Mobile Pro]** P1-03 chỉ áp dụng contract transaction/ledger và progression có thể kiểm chứng. Pack ngẫu nhiên, Booster vượt trần, Position Training ngẫu nhiên, expiry và paid power không được mặc định áp dụng.

<a id="account"></a>

## 2. Account và bootstrap

- **[Thông tin công khai đã xác minh]** Mobile hỗ trợ Link Data qua KONAMI ID hoặc tài khoản nền tảng; KONAMI ID cần thiết khi chuyển giữa iOS và Android. Dữ liệu phải được liên kết trước để tự khôi phục; hỗ trợ thủ công yêu cầu đủ thông tin nhận dạng. Nguồn: [EF-S17](https://www.konami.com/efootball/mobile_support/en/transfer/about), [EF-S18](https://www.konami.com/efootball/mobile_support/en/transfer/about2), [EF-S20](https://us-support.konami.com/hc/en-us/articles/27229187359383-How-do-I-transfer-my-data-to-a-new-device). Chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Liên kết KONAMI ID còn mở quyền dùng eFootball Points; một chiến dịch v5.x thưởng token XP khi hoàn thành Line Objective liên kết dữ liệu. Nguồn: [EF-S19](https://www.konami.com/wepes/efootball_point/en/page/link_konami_id/cs), [EF-S25](https://www.konami.com/efootball/en-us/topic/news/4759). Chắc chắn: **Cao theo chiến dịch**.
- **[Đề xuất cho Soccer Mobile Pro]** Không gắn phần thưởng kinh tế vào thao tác bảo mật bắt buộc nếu làm người chơi hiểu sai consent. Account link, recovery và reward grant phải có idempotency riêng; token không thuộc `PlayerPrefs`.

Không có nguồn authority trong corpus công bố merge policy, token format, session rotation, age gate đầy đủ hoặc schema backend. Các phần đó được ghi là **không công khai**.

<a id="dream-team"></a>

## 3. Dream Team và vòng lặp

Luồng công khai là `Sign player/manager → Register in Game Plan → Play → Earn reward → Develop/adjust team`. Standard Player List cho phép chọn, Standard Player Ticket có kết quả ngẫu nhiên, Special Player List dùng player đặc biệt và Pack có thể chứa player, manager hoặc strip. Nguồn: [EF-S02](https://www.konami.com/efootball/en/page/dreamteam). Chắc chắn: **Cao**.

| Lớp | Input | State/output công khai | Rủi ro cần kiểm soát |
| --- | --- | --- | --- |
| Acquisition | GP, Coin, ticket, pack/contract | Owned player/manager/item | Randomness, odds, duplicate, regret |
| Registration | Player/manager đã ký | Game Plan có lineup/tactic | Invalid roster, stale definition |
| Match | Game Plan + mode policy | Result, objective/match progress | Assist fairness, disconnect |
| Development | Program, point, fusion source | XP/stat/skill/position thay đổi | Power creep, destructive consume |
| Reward | Match Pass, Objective, Event | Currency/item/ticket | Duplicate grant, expiry, FOMO |

<a id="players"></a>

## 4. Cầu thủ và progression

### 4.1 Các trục công khai

| Cơ chế | Input | Output | Giới hạn công khai |
| --- | --- | --- | --- |
| Level Training | Level Training Program hoặc match XP | Level/XP | Phụ thuộc Player Type |
| Player Progression | Progression Points từ level | Phân bổ chỉ số; manual hoặc auto | Cấu hình có thể reset; công thức không công khai |
| Skill Training | Skill Training Program | Additional Skill | Tối đa năm skill bổ sung |
| Position Training | Position Training Program | Tăng proficiency vị trí | Tối đa hai vị trí đủ điều kiện, kết quả ngẫu nhiên |
| Player Fusion | Source player | Truyền XP và Additional Skills | Source bị dùng trong fusion; rule chi tiết theo version |
| Booster | Booster/slot/activation condition | Tăng Ability | Có biến thể vượt 99, crafting và condition theo đội |

Nguồn: [EF-S02](https://www.konami.com/efootball/en/page/dreamteam), [EF-S05](https://www.konami.com/efootball/en/page/2024/versioninfo_v3-00), [EF-S06](https://www.konami.com/efootball/en/page/2024/versioninfo_v3-20), [EF-S09](https://www.konami.com/efootball/en/page/v4/versioninfo_v4-00), [EF-S13](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-20). Chắc chắn: **Cao trong version được nêu**.

### 4.2 Drift quan trọng

- Booster xuất hiện từ v3 với khả năng vượt trần 99; v3.2 thêm loại dựa trên số cầu thủ thỏa điều kiện; v4 thêm Booster Slot/crafting và v5.2 bổ sung biến thể crafting.
- Carryover v3 mô tả currency, contract, player, manager, program, objective và setting có policy riêng; đây là bằng chứng rằng migration không thể chỉ copy toàn snapshot.
- V5.5 bảo vệ bulk release bằng cách chỉ giải phóng player không bị lock, cho thấy lock state phải là invariant ở thao tác hủy hàng loạt.

**[Đề xuất cho Soccer Mobile Pro]** Progression phải tạo preview deterministic, nêu tài nguyên bị tiêu hao, dùng item revision/rules version và trả receipt. Random position/booster không phải mặc định; nếu thử nghiệm phải có pity/choice/duplicate handling, công bố odds và free rollback.

<a id="tactics"></a>

## 5. Game Plan, manager và tactics

- **[Thông tin công khai đã xác minh]** Dream Team yêu cầu đăng ký cầu thủ vào Game Plan; manager có Coaching Affinity/tactical setup. Nguồn: [EF-S02](https://www.konami.com/efootball/en/page/dreamteam). Chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** eFootball công khai năm Team Playstyles; manager proficiency ảnh hưởng phát triển/triển khai chiến thuật. V5.0 thêm Link-up Play cho một số manager. Nguồn: [EF-S01](https://www.konami.com/efootball/en/page/overview), [EF-S11](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-00). Chắc chắn: **Cao theo version**.
- **[Suy luận thiết kế]** Manager, tactic preset, player role và player-owned progression là các aggregate khác nhau. Không nhúng manager modifier vào player instance để tránh migration và rollback dây chuyền.

Các công thức proficiency, player movement, formation modifier, matchmaking rating và AI tactical selection không được công khai.

<a id="modes"></a>

## 6. Chế độ chơi

| Mode | Đối thủ | Đội | Tác động kết quả |
| --- | --- | --- | --- |
| Exhibition | AI | Authentic Team | Giao hữu/PvE |
| Event | AI hoặc PvP | Dream/Authentic theo event | Eligibility và reward theo event |
| eFootball League | PvP division | Dream Team | Division/rating/reward theo phase |
| My League | AI | Dream Team | Season giả lập, My League Points/loan/condition |
| Quick Match | PvP | Dream Team | Không ghi match record theo overview hiện hành |
| Friend Match | 1v1 hoặc Co-op 3v3 | Theo room setting | Match Pass có thể tính theo trang Dream Team |
| Master League Sprint | AI/event | Club management ngắn hạn | Event v5.4, không phải bằng chứng mode thường trực |

Nguồn: [EF-S01](https://www.konami.com/efootball/en/page/overview), [EF-S02](https://www.konami.com/efootball/en/page/dreamteam), [EF-S06](https://www.konami.com/efootball/en/page/2024/versioninfo_v3-20), [EF-S14](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-40). Chắc chắn: **Cao**, riêng Master League Sprint là **theo event/version**.

<a id="gameplay"></a>

## 7. Gameplay, controls và assist

- **[Thông tin công khai đã xác minh]** Mobile controls bao phủ Attack/Defence, pass/through pass/cross/shot, dash, feint, stunning command, Match-up, Pressure, Call for Pressure, shoulder charge, tackle, goalkeeper và set piece. Nguồn: [EF-S03](https://www.konami.com/efootball/en/page/mobile_controller). Chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Bluetooth controller chỉ điều khiển trong match; touch vẫn dùng đồng thời và một số chức năng/mode có giới hạn. Nguồn: [EF-S03](https://www.konami.com/efootball/en/page/mobile_controller). Chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Auto Control v5.2 chỉ dành cho VS AI: AI điều khiển khi không có input và nhường lại ngay khi người chơi thao tác. Nguồn: [EF-S13](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-20). Chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Online PvP có cả P2P và client-server tùy mode; antenna biểu diễn độ ổn định kết nối. Nguồn: [EF-S04](https://www.konami.com/efootball/en/page/online_match). Chắc chắn: **Cao**.

**[Đề xuất cho Soccer Mobile Pro]** Assist là match policy có allowlist, disclosure và matchmaking dimension; không lưu như một setting thuần client. Raw touch/controller phải được adapter thành typed command. Auto Control chỉ thử nghiệm offline và phải có dấu hiệu trạng thái rõ.

<a id="economy"></a>

## 8. Economy, reward và liveops

Các asset công khai gồm GP, eFootball Coins, eFootball Points, training program, ticket/contract/chance deal và reward theo Event/Objective/Match Pass. Campaign Hub v5.2 bổ sung shop/progression riêng; carryover chứng minh từng asset cần migration policy riêng. Nguồn: [EF-S02](https://www.konami.com/efootball/en/page/dreamteam), [EF-S08](https://www.konami.com/efootball/en/page/2024/update_v3_0), [EF-S13](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-20). Chắc chắn: **Cao theo version**.

Google Play công bố in-app purchase và random items nhưng không thay thế disclosure odds theo offer/khu vực. Nguồn: [EF-S22](https://play.google.com/store/apps/details?id=jp.konami.pesam&hl=en_US). Chắc chắn: **Cao cho listing**, không đủ để suy ra economy server.

**[Đề xuất cho Soccer Mobile Pro]** Mọi source/sink/grant có ledger; random offer bị khóa sau age/privacy/store/odds gate; expiry có inbox warning, grace/convert/compensation; không dùng paid Booster tạo lợi thế ranked độc quyền.

<a id="integrity"></a>

## 9. Social, integrity và esports

- **[Thông tin công khai đã xác minh]** Account sharing, bán/chuyển account, chia sẻ credential và chiếm Coin gian lận bị cấm. Nguồn: [EF-S23](https://www.konami.com/efootball/en-us/topic/news/5279). Chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Friend Match hỗ trợ 1v1 tới Co-op 3v3; cross-platform availability thay đổi theo version/platform. Nguồn: [EF-S02](https://www.konami.com/efootball/en/page/dreamteam), [EF-S10](https://www.konami.com/efootball/en/page/v4/versioninfo_v4-40). Chắc chắn: **Cao theo version**.
- **[Thông tin công khai đã xác minh]** Giải International Cup lịch sử dùng Challenge Event, Authentic Team, matchmaking và eligibility theo đại diện khu vực. Nguồn: [EF-S26](https://www.konami.com/efootball/en/page/international_cup/2022/representative_about). Chắc chắn: **Cao cho giải 2022**, không phải luật 2026.

Không có đủ nguồn công khai trong corpus để mô tả detection model, sanction ladder, appeal SLA hoặc retention. Các mục đó giữ decision `Blocked/TestReady` theo owner của Soccer Mobile Pro.

<a id="operations"></a>

## 10. Live data và vận hành

- Live Update có thể đưa game về Title Screen và yêu cầu đăng nhập lại; vì vậy scene/session phải xử lý maintenance boundary. Nguồn: [EF-S24](https://www.konami.com/efootball/en-us/topic/news/5320).
- Season update có thể đổi license, name, emblem, strip, player/manager, photo, commentary và animation; inventory carryover không đồng nghĩa catalog definition bất biến. Nguồn: [EF-S08](https://www.konami.com/efootball/en/page/2024/update_v3_0), [EF-S15](https://www.konami.com/efootball/en/page/v5/versioninfo_v5-50), [EF-S16](https://www.konami.com/efootball/en/page/license_efootball).
- **[Đề xuất cho Soccer Mobile Pro]** Tách catalog manifest, rules version, offer version, owned item revision và ledger. Maintenance phải có drain/reconnect/read-only mode; rollback pin N-1 không sửa receipt đã commit.

<a id="boundaries"></a>

## 11. Ranh giới bằng chứng

- Không nguồn nào trong corpus công bố database endpoint/schema, signing key, matchmaking formula, anti-cheat model, pack RNG implementation, AI architecture/training data hoặc pipeline model 3D nội bộ.
- Patch note mô tả đầu ra và thay đổi hành vi, không chứng minh kiến trúc client/server.
- Cơ chế v3/v4 chỉ được xem là lịch sử nếu trang v5/overview hiện hành không xác nhận tiếp tục tồn tại.
- Không sử dụng community claim trong baseline này vì 26 nguồn chính thức đã đủ cho inventory; khoảng trống được ghi “không công khai” thay vì nâng độ chắc chắn bằng suy đoán.

<a id="impact"></a>

## 12. Tác động tới Soccer Mobile Pro

1. Giữ `OwnedPlayerItem` server-authoritative, immutable catalog reference và lock state ở mọi destructive action.
2. P1-03 triển khai foundation ledger/preview/receipt trước, chưa triển khai pack ngẫu nhiên, market hoặc paid Booster.
3. Tách `PlayerProgressionAllocation`, `AdditionalSkillAssignment` và `PositionProficiency` để migration/rollback độc lập.
4. Manager/tactic là config versioned ngoài player item; assist là match policy, không phải preference tự do trong ranked.
5. Mọi cơ chế tham chiếu phải qua [ma trận quyết định áp dụng](efootball-mobile-adoption-decision-matrix.md), không dùng eFootball làm authority sản phẩm.
