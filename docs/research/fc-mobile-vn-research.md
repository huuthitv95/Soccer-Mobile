# Nghiên cứu tham chiếu FC Mobile VN

> [Chỉ mục](../index.md) · [Sổ nguồn](fc-mobile-vn-source-register.md) · [Coverage](fc-mobile-vn-coverage-audit.md) · [Video UI](video/ui-pattern-synthesis.md) · [Audit Unity](../implementation/unity-implementation-audit-and-backlog.md)

**Mốc nghiên cứu:** 15/07/2026  
**Phạm vi:** tính năng, UI/UX, ngôn ngữ, cài đặt, tài khoản, giải/CLB/cầu thủ, kỹ năng, nâng cấp thẻ, điều khiển, VAR và AI offline.  
**Nguyên tắc:** mọi thông tin kỹ thuật không được công khai đều được ghi là suy luận hoặc đề xuất; xem [ranh giới bằng chứng](./fc-mobile-vn-source-register.md#research-boundaries).

<a id="evidence-summary"></a>

## 1. Kết luận điều hành

- **[Thông tin công khai đã xác minh]** FC Mobile VN là bản Garena phát hành trên iOS/Android, mở server 16/10/2025, có giao diện tiếng Việt và layout cảm ứng được tối ưu. Nguồn: [S01](https://fcmobile.garena.vn/fc-mobile-viet-nam-chinh-thuc-ra-mat-ngay-16-10-2025-tren-ios-android/), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Vòng lặp sản phẩm kết hợp xây Ultimate Team, thu thập/nâng cấp cầu thủ, PvP/PvE, sự kiện mùa và hệ thống League xã hội. Nguồn: [S02](https://fcmobile.garena.vn/sieu-cap-nhat-mua-he-mo-fc-mobile-mo-hoi-bong-da/), [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), [S12](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/leagues-update-2025), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** Ba vòng lặp tham chiếu là: thi đấu/ra quyết định theo giây; hoàn thành trận–nhận thưởng–chỉnh đội theo phiên; sưu tập–nâng cấp–xếp hạng theo mùa. Đây là phân tích MDA/30-30-30, không phải thuật ngữ EA/Garena. Độ chắc chắn: **Trung bình**.
- **[Đề xuất cho Soccer Mobile Pro]** Chỉ đưa meta progression vào production sau khi core match prototype chứng minh được khả năng điều khiển, phản hồi bóng và quyết định chiến thuật có chất lượng. Tránh dùng FOMO hoặc OVR tăng đơn thuần để thay thế gameplay.

<a id="features-and-modes"></a>

## 2. Tính năng và chế độ

### 2.1 Vòng lặp trận đấu và meta

- **[Thông tin công khai đã xác minh]** Người chơi có thể nhận cầu thủ từ Store pack, Campaign/Event và mua/bán qua Market; My Team hỗ trợ Auto Build, dự bị, tối đa bảy lựa chọn thay người, vị trí, người đá set piece, đội trưởng và formation. Nguồn: [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), truy cập 15/07/2026. Độ chắc chắn: **Trung bình** vì UI/bản VN có thể khác.
- **[Thông tin công khai đã xác minh]** EA mô tả Head to Head, VS Attack, Manager Mode/AI Matches và League Tournaments; Garena mô tả Đấu Giả Lập là chế độ không điều khiển trực tiếp, trọng tâm ở sơ đồ và quản lý đội. Nguồn: [S03](https://fcmobile.garena.vn/huong-dan-so-do-chien-thuat-trong-che-do-dau-gia-lap/), [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** Bản cập nhật mùa hè VN quảng bá nội dung mùa, The World's Game, Quick Match, Play's Styles và so sánh cầu thủ. Nguồn: [S02](https://fcmobile.garena.vn/sieu-cap-nhat-mua-he-mo-fc-mobile-mo-hoi-bong-da/), truy cập 15/07/2026. Độ chắc chắn: **Cao**.

### 2.2 League xã hội và esports

- **[Thông tin công khai đã xác minh]** Leagues Update mô tả League tối đa 100 thành viên, quest, achievement, leaderboard và tournament; Season Points đến từ quest, tournament và đóng góp Division Rivals. Nguồn: [S12](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/leagues-update-2025), [S13](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/leagues-deep-dive-2025), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Thông tin công khai đã xác minh]** FVSL có luật đăng ký, thiết bị, tài khoản, vòng đấu và xử lý vi phạm riêng; đây là lớp vận hành giải, không phải hệ thống giải tự động trong client. Nguồn: [S04](https://fcmobile.garena.vn/quy-tac-chung-khi-tham-gia-giai-dau-thuoc-he-thong-fc-mobile/), [S05](https://fcmobile.garena.vn/luat-va-the-thuc-thi-dau-giai-dau-fvsl-summer-2026/), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Đề xuất cho Soccer Mobile Pro]** Tách rõ “giải bóng đá/licensed competition”, “League bang hội”, “ranked season” và “esports tournament” trong terminology, data model và navigation để tránh một từ “giải đấu” sở hữu nhiều authority.

<a id="ui-ux-layout"></a>

## 3. Layout UI/UX

### 3.1 Kiến trúc thông tin tham chiếu

- **[Thông tin công khai đã xác minh]** Luồng EA Help bắt đầu từ Home → Club → Player → Training/Rank Up; My Team chứa formation, lineup, dự bị, set piece và captain. Nguồn: [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), truy cập 15/07/2026. Độ chắc chắn: **Trung bình**.
- **[Suy luận thiết kế]** Kiến trúc màn hình hợp lý gồm: bootstrap/login; Home/liveops; Play/mode select; Club/My Team/Inventory; Market/Store; League/social; Settings/support. Đây là tổng hợp từ các entry point công khai, không phải sitemap chính thức.
- **[Quan sát từ video]** Chi tiết pixel/layout, motion, modal, loading và menu phải lấy từ tài liệu phân tích video có timestamp; nghiên cứu web này không thay thế bằng chứng hình ảnh.

### 3.2 Nguyên tắc trải nghiệm

- **[Đề xuất cho Soccer Mobile Pro]** Giữ CTA “Play” và “My Team” ở tầng đầu; badge/sự kiện không được che nhiệm vụ chính. Luồng nâng cấp phải hiển thị trước–sau, chi phí, vật liệu bị tiêu hao và điều kiện rollback trước xác nhận.
- **[Đề xuất cho Soccer Mobile Pro]** Mọi màn async có loading, empty, stale/offline và retry; back navigation giữ nguyên filter/scroll; modal phá hủy vật phẩm cần xác nhận chống double-submit.
- **[Đề xuất cho Soccer Mobile Pro]** Skill floor thấp, skill ceiling cao: cho phép HUD scale/left-handed layout/assist, nhưng không tạo assist bí mật làm thay đổi kết quả competitive.

<a id="language-system"></a>

## 4. Hệ thống ngôn ngữ

- **[Thông tin công khai đã xác minh]** Garena tuyên bố giao diện tiếng Việt 100%, nhấn mạnh chiến thuật, đội hình và nâng cấp cầu thủ dễ tiếp cận. Nguồn: [S01](https://fcmobile.garena.vn/fc-mobile-viet-nam-chinh-thuc-ra-mat-ngay-16-10-2025-tren-ios-android/), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** Nguồn không xác nhận FC Mobile VN có màn chọn nhiều ngôn ngữ, hot-swap locale, fallback chain hay downloadable voice pack. Không được mô tả các khả năng này như hiện trạng.
- **[Đề xuất cho Soccer Mobile Pro]** First launch dùng locale thiết bị nhưng cho chọn Tiếng Việt/English; Settings cho đổi text không restart. Dữ liệu cầu thủ/CLB/giải dùng ID ổn định và display name theo license; fallback `vi-VN → en → key`, kiểm tra tràn chữ và font fallback.
- **[Đề xuất cho Soccer Mobile Pro]** Commentary là gói asset có version/hash/size, download-resume và subtitle fallback; accessibility không phụ thuộc audio.

<a id="settings-system"></a>

## 5. Hệ thống cài đặt

- **[Thông tin công khai đã xác minh]** Bản VN có layout cảm ứng tối ưu; EA công bố Ball Visibility Toggle và các điều chỉnh tăng quyền kiểm soát phòng ngự. Nguồn: [S01](https://fcmobile.garena.vn/fc-mobile-viet-nam-chinh-thuc-ra-mat-ngay-16-10-2025-tren-ios-android/), [S10](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-update-gameplay), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** Không có nguồn trong sổ nguồn xác nhận đầy đủ taxonomy Settings của FC Mobile VN. Các tab graphics/audio/camera/control/privacy nhìn thấy trong video phải được ghi ở tài liệu video, không suy ra từ marketing.
- **[Đề xuất cho Soccer Mobile Pro]** Nhóm Settings: Account; Language/Audio; Controls/HUD; Gameplay/Camera; Graphics/Performance; Network; Notifications; Accessibility; Privacy/Legal/Support. Cài đặt gameplay competitive phải được server policy cho phép và audit được.
- **[Đề xuất cho Soccer Mobile Pro]** Có preset theo tier thiết bị, preview HUD, reset từng nhóm, cloud sync cho preference không nhạy cảm và local safe-mode nếu graphics khiến app crash.

<a id="account-system"></a>

## 6. Hệ thống tài khoản

- **[Thông tin công khai đã xác minh]** EA Help cho phép Guest và liên kết EA Account, Google Play, Apple Account/Game Center, LINE hoặc Facebook; account đã link không thể unlink theo hướng dẫn này. Liên kết hỗ trợ đa thiết bị, chuyển/khôi phục tiến trình; Guest chỉ lưu trên thiết bị. Nguồn: [S09](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/save-your-progress/), truy cập 15/07/2026. Độ chắc chắn: **Trung bình**, vì provider/chính sách bản Garena có thể khác.
- **[Thông tin công khai đã xác minh]** Tài khoản dưới tuổi đồng ý số tối thiểu có thể bị giới hạn ở Guest theo EA Help. Nguồn: [S09](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/save-your-progress/), truy cập 15/07/2026. Độ chắc chắn: **Trung bình theo khu vực**.
- **[Đề xuất cho Soccer Mobile Pro]** Server authority sở hữu account ID, entitlement, inventory, currency, ranked result và grant ledger. Guest có upgrade transaction idempotent; không merge hai inventory tự động khi xung đột.
- **[Đề xuất cho Soccer Mobile Pro]** Có consent/age gate, session revoke, recovery, delete/export request, device history và support correlation ID; tuyệt đối không ghi token hay PII vào telemetry client.

<a id="competitions-clubs"></a>

## 7. Giải đấu và câu lạc bộ

- **[Thông tin công khai đã xác minh]** EA công bố chế độ UEFA Champions League với 34 CLB khả dụng trong phạm vi nội dung được mô tả; đây không phải bằng chứng cho toàn bộ license catalog. Nguồn: [S14](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/uefa-champions-league-deep-dive-2025), truy cập 15/07/2026. Độ chắc chắn: **Cao trong phạm vi bài**.
- **[Thông tin công khai đã xác minh]** Formation và vị trí đội hình tác động đến lựa chọn chiến thuật/boost; cầu thủ có thể đổi vị trí trừ thủ môn theo hướng dẫn hiện hành. Nguồn: [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), truy cập 15/07/2026. Độ chắc chắn: **Trung bình**.
- **[Suy luận thiết kế]** Không thể lấy một tổng số đội/giải từ marketing làm schema license. Catalog phải có effective date, territory, competition season và quyền dùng crest/kit/player riêng.
- **[Đề xuất cho Soccer Mobile Pro]** Entity tối thiểu: `Competition`, `Season`, `Club`, `TeamSeason`, `Venue`, `Kit`, `RosterRegistration`; lịch và bảng xếp hạng là dữ liệu theo season, không ghi đè master club.

<a id="players-and-assets"></a>

## 8. Cầu thủ, player database và model 3D

### 8.1 Catalog và card instance

- **[Thông tin công khai đã xác minh]** EA Help phân biệt Player dùng trong lineup/reserve/Market/training; mỗi Player mang Training XP/OVR/attribute và có thể bị tiêu hao khi dùng làm XP. Nguồn: [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), truy cập 15/07/2026. Độ chắc chắn: **Trung bình theo phiên bản**.
- **[Suy luận thiết kế]** Public UI không chứng minh cách FC Mobile VN “gọi player database”. Không có bằng chứng cho endpoint, payload, database engine, GraphQL/REST, encryption, CDN hoặc cache invalidation cụ thể.
- **[Đề xuất cho Soccer Mobile Pro]** Tách `PlayerIdentity` (người thật), `PlayerSeasonRating` (chỉ số theo version), `PlayerItemDefinition` (thẻ phát hành) và `OwnedPlayerItem` (instance inventory). Client tải manifest ký số theo `catalogVersion`, sau đó delta/cache; server xác thực mọi grant, upgrade và market transaction.

### 8.2 Pipeline model 3D

- **[Thông tin công khai đã xác minh]** EA công bố “New Face Scans” trong FC Mobile 26 update, chỉ xác nhận có nội dung khuôn mặt mới ở đầu ra. Nguồn: [S10](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-update-gameplay), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** Không có nguồn công khai mô tả quy trình capture, topology, rig, blendshape, texture, LOD hay cách đóng bundle của EA/Garena.
- **[Đề xuất cho Soccer Mobile Pro]** Pipeline an toàn: source/licensing record → scan/reference ingest → retopology + shared humanoid rig → material/texture budget → LOD0–LOD3 → animation/skin validation → Addressables theo player asset ID → signed remote catalog → QA trên device tier → fallback generic head. Catalog gameplay chỉ giữ `modelAssetId`; không nhúng URL bundle vào player record.

<a id="skills-system"></a>

## 9. Skills và PlayStyles

- **[Thông tin công khai đã xác minh]** Bản mùa hè VN quảng bá Play's Styles; gameplay deep dive cho thấy kết quả chịu ảnh hưởng bởi attribute, vị trí, áp lực, weak foot, Curve/Long Passing và hành vi phòng ngự. Nguồn: [S02](https://fcmobile.garena.vn/sieu-cap-nhat-mua-he-mo-fc-mobile-mo-hoi-bong-da/), [S10](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-update-gameplay), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** “Skills” có thể chỉ skill move input, rating sao, trait hoặc PlayStyle; nguồn công khai không đủ để đồng nhất chúng thành một bảng duy nhất.
- **[Đề xuất cho Soccer Mobile Pro]** Tách `SkillMoveDefinition` (input/animation/risk), `TraitDefinition` (modifier tình huống), `PlayStyleDefinition` (identity chiến thuật) và `SkillRating` (điều kiện thực thi). Mọi modifier có cap, context và telemetry; không tạo lựa chọn “luôn tốt hơn”.

<a id="card-upgrade"></a>

## 10. Nâng cấp thẻ cầu thủ

- **[Thông tin công khai đã xác minh]** Hướng dẫn EA Help: Training tăng chỉ số qua Training XP; Rank Up tăng OVR và mở khả năng train thêm; cầu thủ cùng nhóm vị trí có thể làm XP, và vật liệu bị tiêu hao. Nguồn: [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), truy cập 15/07/2026. Độ chắc chắn: **Trung bình**.
- **[Thông tin công khai đã xác minh]** Patch Notes FC Mobile 26 mô tả Rank Up Currency thay vật phẩm rank cũ và Training Level tách khỏi Rank Up. Nguồn: [S11](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-patch-notes), truy cập 15/07/2026. Độ chắc chắn: **Trung bình cho bản VN**; đây là dấu hiệu drift so với S08.
- **[Đề xuất cho Soccer Mobile Pro]** Contract upgrade server-authoritative gồm preview deterministic, cost/version, idempotency key, inventory revision và receipt. UI phải nêu rõ OVR/stat delta, item bị khóa/tiêu hao, cap và khả năng transfer; không dùng random outcome nếu chưa có quyết định compliance/odds.

<a id="mobile-controls"></a>

## 11. Điều khiển trái/phải khi có bóng và không bóng

- **[Thông tin công khai đã xác minh]** Garena xác nhận layout cảm ứng mới; EA xác nhận gesture shot, kiểm soát phòng ngự, auto-tackle tuning, chuyền, tạt, đánh đầu và skill move ảnh hưởng gameplay. Nguồn: [S01](https://fcmobile.garena.vn/fc-mobile-viet-nam-chinh-thuc-ra-mat-ngay-16-10-2025-tren-ios-android/), [S10](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-update-gameplay), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** S01/S10 không cung cấp bảng mapping hoàn chỉnh theo trạng thái. Không khẳng định vị trí/tên chính xác từng nút FC Mobile VN nếu chưa có video timestamp hoặc hướng dẫn chính thức.
- **[Đề xuất cho Soccer Mobile Pro]** Bên trái là joystick di chuyển/chọn hướng. Bên phải đổi action map theo state: có bóng gồm Pass/Through/Shoot/Sprint-Skill; không bóng gồm Switch/Sprint-Tackle/Second Defender/Slide. Set piece, goalkeeper và UI dùng map riêng; action semantics không tái sử dụng mơ hồ.
- **[Đề xuất cho Soccer Mobile Pro]** Hỗ trợ tap/hold/swipe, vùng chết và cancel rõ; hiển thị cooldown/charge; remap, HUD scale/opacity, left-handed, color/ball visibility và haptic toggle. Competitive telemetry ghi action intent chứ không ghi raw touch có thể nhận diện cá nhân.

<a id="var-system"></a>

## 12. VAR và trọng tài

- **[Thông tin công khai đã xác minh]** EA công bố cải thiện referee decision-making và collision detection; va chạm nhẹ hoặc skill move lao vào hậu vệ không tự động tạo foul. Nguồn: [S10](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-update-gameplay), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** Không tìm thấy trong S01–S16 bằng chứng FC Mobile VN có quy trình “check VAR” tương tác. Referee tuning không đồng nghĩa VAR; replay hình ảnh không chứng minh engine dùng video để quyết định luật.
- **[Đề xuất cho Soccer Mobile Pro]** Rule engine deterministic chốt goal/offside/foul/penalty từ match state authoritative; VAR chỉ là presentation state (`ReviewRequested → Reviewing → DecisionShown → Resume`). Replay camera không được thay đổi decision. Có timeout, skip presentation, audit event và fallback instant decision trên thiết bị yếu.

<a id="offline-ai"></a>

## 13. AI offline và “train trí thông minh cho máy”

- **[Thông tin công khai đã xác minh]** Đấu Giả Lập không cần điều khiển trực tiếp và nhấn mạnh sơ đồ/đội hình; EA Help nhắc AI Matches. Nguồn: [S03](https://fcmobile.garena.vn/huong-dan-so-do-chien-thuat-trong-che-do-dau-gia-lap/), [S08](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/), truy cập 15/07/2026. Độ chắc chắn: **Cao**.
- **[Suy luận thiết kế]** Không có bằng chứng công khai rằng FC Mobile VN dùng machine learning/reinforcement learning để chạy AI trận offline. Không biết training corpus, reward function, model architecture hay inference runtime.
- **[Đề xuất cho Soccer Mobile Pro]** Bắt đầu bằng AI deterministic nhiều tầng: tactical shape → role assignment → perception/blackboard → utility scoring → action planner → locomotion/animation. Dùng scenario tests và telemetry để tune tham số; ML chỉ được thử offline để gợi ý tuning, không tự học từ người chơi trên thiết bị.
- **[Đề xuất cho Soccer Mobile Pro]** Difficulty điều chỉnh reaction delay, perception noise, tactical risk và execution error trong biên minh bạch; không tăng tốc/OVR bí mật. Acceptance gồm build-up, pressing, marking, transition, set piece, goalkeeper, time wasting và recovery shape; seed replayable để debug.

<a id="research-risks"></a>

## 14. Rủi ro và câu hỏi cần xác minh

- Phiên bản Global, Garena VN và thời điểm update có thể khác; đặc biệt Rank Up/Training và provider account.
- Tên Play's Styles trên trang VN cần đối chiếu terminology trong client trước khi dùng làm type chính thức.
- Cần video timestamp hoặc hướng dẫn chính thức để khóa mapping nút, taxonomy Settings và luồng language selection.
- Cần quyết định backend/Addressables/licensing của Soccer Mobile Pro trước khi chốt schema player/3D asset.
- Cần prototype/playtest core match trước khi cân bằng progression, AI difficulty và liveops; đo completion, mis-input, quit/retry và thời gian trong menu thay vì chỉ hỏi cảm nhận.
