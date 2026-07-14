# FC MOBILE VN — NGHIÊN CỨU THAM CHIẾU CHI TIẾT

**Mục đích:** tài liệu nghiên cứu tham chiếu để bổ sung cho `GDD-Soccer-Mobile-Pro.md`, tập trung vào các nhóm tính năng người dùng yêu cầu và chỉ rõ đâu là dữ liệu xác nhận được từ nguồn công khai, đâu là suy luận thiết kế dùng cho Soccer Mobile Pro.

**Phạm vi nguồn công khai đã xác nhận:** Google Play listing của FC Mobile VN và bài công bố Siêu cập nhật mùa hè từ Garena. Các phần liên quan đến kiến trúc nội bộ như player database backend, pipeline thêm model 3D, logic VAR và huấn luyện AI offline **không được Garena/EA công bố chi tiết công khai**; vì vậy tài liệu này tách rõ phần “xác nhận được” và “đề xuất suy luận sản phẩm/kỹ thuật”.

---

## 1. TÍNH NĂNG TỔNG THỂ ĐÃ XÁC NHẬN CÔNG KHAI

### 1.1 Tính năng lõi
- Hơn 19.000 cầu thủ, 690+ đội bóng, 35+ giải đấu.
- Có hơn 50 đội tuyển quốc gia chính thức trong chế độ **The World’s Game**.
- Có chế độ **Quick Match** để giao hữu nhanh với bạn bè.
- Có hệ thống **PVP xếp hạng**, bảng xếp hạng tuần, và ghép trận được cải thiện.
- Có bình luận tiếng Việt và mùa thẻ **Vietnam All-Star** với cầu thủ Việt Nam.
- Gameplay được nhấn mạnh ở chuyền bóng, điều khiển, phòng ngự phạt góc, cân bằng kỹ năng và chỉ số cầu thủ.
- Bản cập nhật hè 2026 bổ sung/tăng cường: **The World’s Game**, **Play’s Styles**, **Quick Match có hiệp phụ + penalty**, và **hệ thống so sánh cầu thủ**.

### 1.2 Hàm ý thiết kế rút ra cho Soccer Mobile Pro
- FC Mobile VN định vị là game live service xoay quanh 3 trục: **sưu tầm cầu thủ**, **thi đấu PvP/PvE**, **sự kiện/mùa thẻ**.
- Sản phẩm ưu tiên độ phủ bản quyền rất lớn, nội dung cập nhật liên tục, và bản địa hóa riêng cho Việt Nam.
- Các hệ thống meta quan trọng nhất cần xuất hiện trong GDD của Soccer Mobile Pro là: đội tuyển/CLB thật, bảng xếp hạng, sự kiện theo mùa, so sánh cầu thủ, phong cách chơi, và social match với bạn bè.

---

## 2. LAYOUT UI/UX THAM CHIẾU

### 2.1 Các lớp màn hình nên có
Dựa trên mô hình vận hành của FC Mobile VN, layout chuẩn nên chia thành 5 lớp:
1. **Lớp mở đầu**: splash, tải tài nguyên, chọn ngôn ngữ lần đầu.
2. **Lớp vào game**: đăng nhập/tài khoản khách, đồng bộ dữ liệu, thông báo sự kiện đang chạy.
3. **Lớp sảnh chính**: banner sự kiện, truy cập đội hình, thị trường/gói, chế độ chơi, nhiệm vụ, cài đặt.
4. **Lớp meta team-building**: đội hình, cầu thủ, nâng cấp, so sánh, CLB, giải đấu, inventory.
5. **Lớp thi đấu**: tiền trận, chọn đội/chế độ, HUD in-match, tạm dừng, kết quả, MVP, thưởng.

### 2.2 Nguyên tắc UI/UX nên bổ sung vào GDD
- **Home hub nhiều shortcut**: vì game live service luôn có nhiều entry points theo sự kiện.
- **Banner ưu tiên theo mùa**: nội dung đang chạy phải đứng đầu luồng điều hướng.
- **Bottom nav hoặc tab lớn** cho các cụm chính: Home, Club/Team, Market/Store, Play, Events.
- **Một thao tác đến đội hình** từ mọi vị trí quan trọng vì đội hình là trung tâm tiến trình.
- **Luồng nâng cấp nhiều bước nhưng không sâu quá 3 lớp** để tránh người chơi bị lạc giữa thẻ, vật phẩm, chỉ số và xác nhận.
- **So sánh cầu thủ dạng side-by-side** là bắt buộc vì Garena đã đưa đây thành tính năng công khai nổi bật.

### 2.3 Màn hình cần tách thành tài liệu riêng
Nên tạo sau này các spec riêng:
- `docs/ui/Home-LiveOps.md`
- `docs/ui/Player-Upgrade-Flows.md`
- `docs/ui/Match-HUD.md`
- `docs/ui/Settings-Account-Language.md`

---

## 3. HỆ THỐNG CHỌN NGÔN NGỮ

### 3.1 Điều xác nhận được
- FC Mobile VN là bản phát hành riêng tại Việt Nam và nhấn mạnh **bình luận tiếng Việt**.
- Đây là chỉ dấu rõ ràng rằng trò chơi có lớp bản địa hóa sâu hơn bản global, ít nhất ở text, voice và event framing.

### 3.2 Thiết kế tham chiếu cho Soccer Mobile Pro
- Chọn ngôn ngữ ở lần mở đầu tiên: **Tiếng Việt / English**.
- Ngôn ngữ UI, mô tả vật phẩm, onboarding, thông báo hệ thống dùng bảng chuỗi tách riêng.
- Riêng tên cầu thủ/CLB/giải đấu giữ nguyên bản quyền quốc tế; chỉ dịch mô tả, tooltip, và hướng dẫn.
- Gói voice/bình luận cần tách riêng thành downloadable pack để giảm dung lượng cài đặt ban đầu.
- Đổi ngôn ngữ không cần restart nếu chỉ là text; riêng audio pack có thể yêu cầu tải/xóa gói theo ngôn ngữ.

### 3.3 Checklist UX
- Màn first launch có CTA rõ: “Chơi ngay bằng Tiếng Việt”.
- Có preview dung lượng audio trước khi tải.
- Có fallback nếu gói âm thanh lỗi: dùng text subtitle + commentary mặc định.

---

## 4. HỆ THỐNG CÀI ĐẶT

### 4.1 Điều xác nhận được
- Google Play listing và mô tả cộng đồng chỉ ra game có cài đặt liên quan đến bình luận tiếng Việt / tải gói âm thanh.
- Game có các yếu tố điều khiển và gameplay được tối ưu nên về mặt sản phẩm chắc chắn phải có các nhóm setting cho controls, camera, audio, và đồ họa.

### 4.2 Cấu trúc Settings đề xuất
1. **Tài khoản**: liên kết, đăng xuất, xóa dữ liệu, trung tâm hỗ trợ.
2. **Ngôn ngữ & âm thanh**: text language, commentary language, tải/xóa voice pack.
3. **Điều khiển**: độ nhạy joystick, auto-switch, auto-pass assist, hiển thị nút bấm, cỡ HUD.
4. **Gameplay**: camera, radar, hỗ trợ chuyền/sút/lọc khe, rung, gợi ý phòng ngự.
5. **Đồ họa**: FPS mode, chất lượng sân/cầu thủ/đám đông/đổ bóng.
6. **Mạng**: cảnh báo ping, chọn vùng máy chủ nếu có, kiểm tra kết nối.
7. **Riêng tư & pháp lý**: điều khoản, chính sách dữ liệu, yêu cầu xóa dữ liệu.

### 4.3 UX rules
- Chia settings theo **task-oriented groups**, không nhồi tất cả vào một trang dài.
- Các setting ảnh hưởng trực tiếp trận đấu phải cho xem trước bằng mini preview clip hoặc sân tập.
- Các thay đổi nặng như graphics preset có thể cần nút Apply/Revert trong 10 giây để tránh khóa máy yếu.

---

## 5. HỆ THỐNG TÀI KHOẢN

### 5.1 Điều xác nhận được
- Bản Google Play nêu ứng dụng có thể thu thập thông tin cá nhân, thông tin tài chính và hỗ trợ yêu cầu xóa dữ liệu.
- Có email hỗ trợ và policy riêng, đồng nghĩa game vận hành theo mô hình account service + live support.

### 5.2 Thiết kế tham chiếu
- Hỗ trợ **Guest Account** để vào game nhanh.
- Khuyến khích liên kết Garena / Google / Apple / Facebook (hoặc nhà phát hành tương đương ở Soccer Mobile Pro).
- Có cơ chế **account upgrade**: từ khách lên tài khoản định danh mà không mất tiến trình.
- Có trang **Security & Recovery**: thiết bị đăng nhập gần đây, đổi email, xác nhận xóa dữ liệu.
- Mọi purchase phải gắn với account-level entitlement, không gắn local device.

### 5.3 Rủi ro cần ghi rõ
- Không khóa người chơi vào duy nhất một phương thức đăng nhập.
- Phải có anti-abuse cho reroll tài khoản khách.
- Support flow phải nằm ngay trong phần account, không chôn sâu dưới legal text.

---

## 6. GIẢI ĐẤU VÀ CLB

### 6.1 Điều xác nhận được
- FC Mobile VN công khai nêu các giải: Premier League, LALIGA EA SPORTS, Bundesliga, Serie A, Ligue 1 McDonald’s, UEFA Champions League.
- Trò chơi cũng công khai có 50+ đội tuyển quốc gia và 690+ đội bóng.

### 6.2 Cấu trúc dữ liệu nên có cho Soccer Mobile Pro
**League entity**
- league_id
- display_name
- region
- season_id
- logo_asset
- competition_type (domestic, continental, international)
- licensing_scope

**Club entity**
- club_id
- league_id
- full_name / short_name
- crest_asset
- kit_home / away / gk
- stadium_ref
- nation
- popularity_tier

### 6.3 Hệ thống UX liên quan
- Mỗi CLB phải có landing card riêng: logo, bộ áo, giải đấu, đội hình nổi bật.
- Mỗi giải đấu phải có trang browse để lọc cầu thủ và sự kiện liên quan.
- Các chế độ event nên dùng bộ lọc theo giải/CLB để tăng giá trị bản quyền.

---

## 7. CẦU THỦ, PLAYER DATABASE, VÀ MODEL 3D

### 7.1 Điều xác nhận được công khai
- FC Mobile VN công bố có hơn 19.000 cầu thủ.
- Có hệ thống so sánh cầu thủ với cả chỉ số chi tiết và chỉ số ẩn/phong cách thi đấu theo mô tả công bố mùa hè.

### 7.2 Những gì **không được công khai đầy đủ**
Không có nguồn công khai chính thức trong phạm vi nghiên cứu hiện tại mô tả:
- FC Mobile VN gọi API player database theo endpoint nào.
- Cấu trúc payload thực tế của player database backend.
- Pipeline import model 3D cầu thủ vào engine sản phẩm.
- Asset bundle naming convention, streaming strategy, LOD policy nội bộ.

### 7.3 Suy luận kỹ thuật hợp lý cho Soccer Mobile Pro
Để xây dựng sản phẩm tương đương, nên thiết kế player database như sau:

**Player master data**
- player_id toàn cục.
- tên hiển thị, tên áo đấu, quốc tịch, chân thuận, chiều cao/cân nặng.
- vị trí chính, vị trí phụ.
- club_id, league_id, nation_team_id.
- rarity/card_program/version.
- chỉ số base và các derived attributes.
- weak_foot, skill_move_rating, work_rate, traits, play_styles.
- portrait_asset, bust_asset, body_model_ref, animation_profile_ref.

**Runtime fetch layers**
1. **Bootstrap catalog** khi login: version manifest, season tables, event catalog.
2. **Lazy fetch player pages** ở market/club/squad theo paging.
3. **Prefetch team lineup** trước trận.
4. **On-demand detail fetch** khi mở trang compare/upgrade/scout.

**Caching**
- local SQLite hoặc binary cache cho dữ liệu catalog.
- CDN manifest versioning để invalidate theo mùa thẻ.
- tách player static data khỏi dynamic economy data (price, supply, ranking).

### 7.4 Pipeline model 3D đề xuất
Vì pipeline nội bộ của FC Mobile VN không công khai, Soccer Mobile Pro nên dùng pipeline rõ ràng:
1. **DCC source**: Maya/Blender/ZBrush cho base head + body archetype.
2. **Retopo + skinning** theo skeleton chuẩn chung cho toàn bộ cầu thủ.
3. **Face variation**: blendshapes / morph targets cho likeness.
4. **Kit fitting**: bind áo/shorts/socks vào body archetype theo club kit set.
5. **LOD generation**: LOD0 close-up replay, LOD1 gameplay gần, LOD2 gameplay xa, billboard/crowd fallback.
6. **Texture packing**: albedo, normal, ORM packed texture.
7. **Export**: glTF/FBX vào build pipeline, convert sang engine-native asset bundles.
8. **Runtime binding**: player record chứa `body_model_ref`, `face_model_ref`, `hair_variant_ref`, `animation_profile_ref`, `kit_slot_ref`.
9. **Patch strategy**: tách asset bundle cầu thủ ngôi sao/đội mới để hot-update không cần full app patch.

### 7.5 Quy tắc sản phẩm nên thêm
- Không bắt buộc mọi cầu thủ đều có face scan riêng; chia tier visual fidelity.
- Cầu thủ top-tier, icon, cover athlete có intro, close-up và animation signature.
- Cầu thủ thường có generic face archetype nhưng vẫn đúng chiều cao, body type, skin tone, tóc và chân thuận.

---

## 8. HỆ THỐNG SKILLS / PLAY’S STYLES

### 8.1 Điều xác nhận được
- Garena công bố **Play’s Styles** làm trận đấu đa dạng hơn và cải thiện trực tiếp các chỉ số chuyên biệt.
- Mô tả trên Google Play cũng nhấn mạnh yếu tố cân bằng giữa kỹ năng và chỉ số cầu thủ.

### 8.2 Cách nên mô hình hóa cho Soccer Mobile Pro
Tách làm 3 lớp:
1. **Skill Moves**: động tác tay/chân do người chơi kích hoạt khi cầm bóng.
2. **Traits / Passive Tendencies**: xu hướng ẩn như cắt bóng, chọn vị trí, chạy chỗ, đánh đầu.
3. **Play Styles / Signature Archetypes**: gói thuộc tính chủ đạo như Deep Playmaker, Box Finisher, Pressing Destroyer.

### 8.3 Gợi ý data schema
- skill_move_rating: 1–5 sao.
- passive_traits: array.
- play_style_primary: enum.
- play_style_secondary: enum nullable.
- triggered_bonus_context: sprint / finesse / through_ball / crossing / pressing.

### 8.4 UX triển khai
- Ở thẻ cầu thủ phải có block riêng cho **Play Style** và **Traits**.
- Màn hình compare cần so sánh cả “phù hợp meta nào”, không chỉ cộng/trừ chỉ số.
- Trong trận, chỉ hiển thị feedback tinh gọn như icon nhỏ hoặc banner ngắn khi style kích hoạt.

---

## 9. HỆ THỐNG NÂNG CẤP THẺ

### 9.1 Điều xác nhận được
- Bản cập nhật hè công bố tính năng so sánh cầu thủ nhằm hỗ trợ quyết định nâng cấp và tối ưu đội hình.
- Mô tả cộng đồng và positioning sản phẩm cho thấy loop nâng cấp thẻ là trung tâm của tiến trình.

### 9.2 Thiết kế hoàn chỉnh cần có
- **Rank** tăng trần phát triển.
- **Training** tăng chỉ số chi tiết.
- **Skill Points / Play Style Nodes** mở hướng build.
- **Position Training** mở vị trí phụ.
- **Compare Before Commit**: luôn cho xem chênh lệch trước khi xác nhận nâng cấp.
- **Material Lock**: khóa thẻ quan trọng để tránh dùng nhầm làm nguyên liệu.

### 9.3 UX an toàn
- Mọi bước tiêu tốn premium currency phải có confirm 2 lớp.
- Nếu nâng cấp làm thay đổi chemistry/play style của đội hình, phải hiện cảnh báo trước.

---

## 10. NÚT ĐIỀU KHIỂN TRÁI / PHẢI TRONG TRẬN

### 10.1 Mô hình tham chiếu nên dùng
Vì FC Mobile VN nhấn mạnh điều khiển mượt mà và gameplay chân thực, sơ đồ controls chuẩn nên là:

**Bên trái**
- Joystick ảo di chuyển.
- Có thể hỗ trợ fixed / floating / locked region.

**Bên phải khi có bóng**
- Shoot.
- Pass ngắn.
- Through pass / chọc khe.
- Sprint & Skill.
- Cross / Lob (qua gesture hoặc modifier).
- Clear biến thành context action khi ở sân nhà.

**Bên phải khi không có bóng**
- Sprint & Press.
- Tackle đứng.
- Tackle trượt.
- Switch player.
- Match-Up / jockey / contain.

### 10.2 Contextual remap
- Cùng một vị trí nút nhưng đổi nhãn/chức năng theo trạng thái có bóng/không bóng để giảm tải thị giác.
- Nút lớn nhất luôn là hành động có xác suất dùng cao nhất ở ngữ cảnh hiện tại.

### 10.3 Trợ năng
- Cho đổi kích thước, opacity, vị trí cụm nút.
- Có preset 2 nút / 3 nút / nâng cao.
- Có training overlay hiển thị tên động tác trong vài trận đầu.

---

## 11. HỆ THỐNG CHECK VAR

### 11.1 Điều xác nhận được
Không có xác nhận công khai trong phạm vi nguồn hiện tại rằng FC Mobile VN có một hệ thống VAR độc lập, tương tác như module luật trận đấu riêng.

### 11.2 Định hướng khả thi cho Soccer Mobile Pro
Nên hiểu “VAR” như **presentation system** nằm trên luật sẵn có, không phải hệ thống luật mới:
- Trigger ở các tình huống bàn thắng sát việt vị, penalty nhạy cảm, bóng qua vạch vôi, thẻ đỏ trực tiếp.
- Engine vẫn chốt quyết định bằng luật/tracking chính; VAR chỉ đóng vai trò **giải thích trực quan**.
- Dùng camera cutscene 2–4 giây, overlay “VAR Check”, line visualization cho việt vị, slow-motion replay.

### 11.3 Quy tắc thiết kế
- Chỉ kích hoạt ở khoảnh khắc có giá trị cảm xúc cao.
- Không được lạm dụng trong online ranked vì làm tăng thời lượng trận và bực người chơi.
- Có tùy chọn tắt cutscene VAR ở setting, nhưng vẫn giữ kết quả luật.

---

## 12. TRAIN AI OFFLINE KHI NGƯỜI CHƠI ĐÁ VỚI MÁY

### 12.1 Điều xác nhận được
Không có công bố công khai chi tiết về cách FC Mobile VN huấn luyện AI offline.

### 12.2 Hướng triển khai khả thi cho Soccer Mobile Pro
Nên dùng kiến trúc **hybrid football AI** thay vì full reinforcement learning end-to-end:
1. **Layer chiến thuật đội**: chọn nhịp độ, pressing line, block height, width, risk appetite.
2. **Layer vai trò vị trí**: CB, FB, DM, AM, ST có state machine riêng.
3. **Layer quyết định ngắn hạn**: utility scoring cho pass, shoot, dribble, clear, press.
4. **Layer animation/locomotion**: motion matching hoặc blend tree để ra hành vi mượt.

### 12.3 Dữ liệu train/tune
- Telemetry từ người chơi thật: vị trí sút, tỷ lệ chuyền, bản đồ mất bóng, vùng pressing.
- Heatmaps theo chiến thuật và cấp độ khó.
- Tập thư viện kịch bản bóng đá: phản công 3v2, build-up từ sân nhà, low block, chống tạt.

### 12.4 Khó khăn và cách giải
- **Không gian trạng thái lớn** → dùng heuristic + behavior trees trước, ML chỉ hỗ trợ tuning weights.
- **Người chơi cảm thấy AI gian lận** → tuyệt đối tránh buff ẩn kiểu tăng tốc/đè vật lý vô lý.
- **Độ khó không ổn định** → xây difficulty profiles bằng decision time, sai số first touch, độ mạo hiểm chuyền, tốc độ chuyển trạng thái.

### 12.5 Lộ trình thực tế
- Prototype: behavior tree + utility AI.
- Alpha: imitation từ replay người chơi nội bộ.
- Live: dùng telemetry để cân lại shot selection, pressing timing, pass safety.

---

## 13. KHOẢNG TRỐNG TÀI LIỆU HIỆN TẠI CỦA REPO

Sau khi đối chiếu với `GDD-Soccer-Mobile-Pro.md`, các khoảng trống đáng chú ý là:
- Chưa có mục riêng mô tả **layout UI/UX** theo cụm màn hình và quy tắc điều hướng.
- Chưa có mục riêng mô tả **Settings**, **Account**, **Language UX** đủ sâu.
- Chưa có mô tả rõ “FC Mobile VN công khai đến đâu” và “đâu là suy luận kỹ thuật” cho **player database**, **model 3D**, **VAR**, **offline AI**.
- Chưa có phân rã chi tiết controls theo trạng thái **có bóng / không bóng**.
- Chưa tách tài liệu nghiên cứu tham chiếu để GDD chính đỡ bị quá tải.

---

## 14. HƯỚNG BỔ SUNG VÀO GDD CHÍNH

Đề xuất cập nhật GDD chính theo hướng:
- Thêm mục **Tài khoản & Cài đặt** vào Mục lục.
- Mở rộng phần **Ngôn ngữ & Bản địa hóa** thành flow hoàn chỉnh.
- Mở rộng phần **Screen Map** để phản ánh layout home/event/store/club/player/settings.
- Mở rộng phần **Trải nghiệm trận đấu & Điều khiển** bằng bảng controls theo ngữ cảnh.
- Giữ tài liệu này làm **phụ lục nghiên cứu tham chiếu**, để đội thiết kế và kỹ thuật cùng dùng.


---

## 15. MA TRẬN TRUY XUẤT NGUỒN VÀ MỨC ĐỘ CHẮC CHẮN

| Chủ đề người dùng yêu cầu | Bằng chứng công khai hiện có | Mức độ | Ranh giới cần tôn trọng |
|---|---|---|---|
| Tính năng/game modes/live events | Trang chính thức Garena, Google Play, bài cập nhật và hướng dẫn sự kiện | Xác nhận | Chỉ dùng các mode/tính năng có thông báo chính thức làm “đã có” |
| UI/UX, ngôn ngữ, settings, account | Có chỉ dấu công khai về bản VN, bình luận tiếng Việt, web ingame, FAQ/hỗ trợ | Một phần xác nhận | Layout màn hình, schema localization và settings cụ thể là đề xuất thiết kế, không phải reverse-engineering |
| Giải đấu, CLB, cầu thủ, mùa thẻ | Trang sự kiện/cập nhật, giải đấu cộng đồng, mùa thẻ, BDR list | Xác nhận một phần | Không suy luận quyền sở hữu dữ liệu/giấy phép vượt quá nội dung công bố |
| Skills/Play’s Styles, compare player | Siêu cập nhật hè công bố Play’s Styles và compare có chỉ số ẩn | Xác nhận | Công thức kích hoạt, tỉ lệ, logic server chưa được công bố |
| Nâng cấp thẻ | Bài mùa thẻ và event đề cập nâng cấp/thẻ mùa mới | Xác nhận một phần | Không khẳng định công thức Rank/Training/Skill Point nội bộ của FC Mobile VN |
| Điều khiển có/không có bóng, VAR | Không có đặc tả control-map/VAR kỹ thuật công khai đầy đủ | Đề xuất | Các input matrix/VAR presentation là đặc tả Soccer Mobile Pro |
| Player database API, model 3D pipeline, AI offline | Không thấy tài liệu kỹ thuật chính thức công bố endpoint, bundle, model pipeline hay thuật toán train AI | Không công khai | Tuyệt đối không mô tả như dữ liệu nội bộ EA/Garena; chỉ ghi là kiến trúc đề xuất |

## 16. BỔ SUNG: DATA GOVERNANCE CHO PLAYER, CLUB VÀ LEAGUE

### 16.1 Vòng đời dữ liệu cầu thủ
1. **Ingest**: nhận bộ dữ liệu được cấp phép từ nguồn bản quyền/đối tác đã được phê duyệt.
2. **Validation**: kiểm tra khóa định danh, club/league mapping, ảnh, vị trí, chỉ số, quyền sử dụng asset.
3. **Staging**: dữ liệu được review trên CMS nội bộ; chưa hiện trong game.
4. **Publish**: phát hành bằng version manifest, có effective date và audit log.
5. **Deprecate**: cầu thủ bị chuyển nhượng, giải nghệ, hạn chế hoặc hết quyền được chuyển trạng thái thay vì xóa thẳng.
6. **Rollback**: khi phát hiện sai dữ liệu/asset, quay lại manifest ổn định trước đó.

### 16.2 Danh sách hạn chế / retire / ban
Các cập nhật công khai của FC Mobile VN cho thấy có khái niệm BDR (Ban/Dead/Retire): cầu thủ bị hạn chế có thể không còn mở thêm hoặc giao dịch được. Soccer Mobile Pro cần một state machine rõ ràng: `active`, `restricted_opening`, `restricted_trade`, `retired`, `removed_by_license`, cùng thông báo rõ cho người chơi.

### 16.3 Quy tắc không gây hại kinh tế
- Mọi thay đổi trạng thái thẻ phải có effective date, thông báo trước và FAQ.
- Nếu một card bị ảnh hưởng sau khi người chơi sở hữu, phải xác định chính sách bồi hoàn/giữ lại/quy đổi minh bạch.
- Giá thị trường, giới hạn giao dịch và nguồn cung phải có audit log để điều tra khi có biến động bất thường.

## 17. BỔ SUNG: MÔ HÌNH THẺ, CHỈ SỐ VÀ SO SÁNH

### 17.1 Phân tách player identity và card instance
- **Player identity**: người/cầu thủ thật, dữ liệu tiểu sử, team/league/nation, body/face reference.
- **Card program**: mùa thẻ hoặc chương trình (TOTY, TOTS, VNAS, Legends…).
- **Card instance**: phiên bản người chơi sở hữu, gồm Rank, Training, skill build, khóa giao dịch, acquisition source.

Cấu trúc này ngăn việc nhầm “một cầu thủ” với “một thẻ”, và cho phép cùng một cầu thủ xuất hiện nhiều mùa thẻ mà vẫn truy vết được toàn bộ thay đổi.

### 17.2 Compare Player — yêu cầu tối thiểu
- Chỉ số tổng và chỉ số theo nhóm.
- Chỉ số ẩn, traits, Play Style/role suitability.
- Chân thuận/yếu, skill moves, vị trí và vị trí phụ.
- Rank/Training hiện tại và chênh lệch sau nâng cấp dự kiến.
- Chênh lệch chemistry/formation fit khi thay vào đội hình đang dùng.

## 18. BỔ SUNG: MẪU KIỂM THỬ CHO CONTROLS, VAR VÀ AI OFFLINE

### 18.1 Controls
- Mỗi action phải có input buffering, cancellation rule và feedback hình/âm thanh.
- Test thiết bị cảm ứng nhỏ/lớn, left-handed layout, joystick fixed/floating và HUD opacity.
- Đo input-to-action latency, tỷ lệ tap nhầm, tỷ lệ auto-switch hợp lý và tỷ lệ bàn thua từ sai input.

### 18.2 VAR
- Mọi VAR trigger phải replay xác định (deterministic) cùng dữ liệu luật trận đấu.
- QA kiểm tra: offside line, penalty contact, goal-line decision, card decision, skip cutscene và timeout mạng.
- Không dùng VAR để che lỗi physics/animation; nếu match engine chưa đủ tin cậy thì ưu tiên sửa luật/physics trước.

### 18.3 AI offline
- Regression suite theo kịch bản: build-up, high press, low block, counter, set piece, bảo toàn tỷ số.
- KPI: pass completion theo độ khó, shot quality, defensive shape error, foul rate, rage-quit rate và perceived fairness survey.
- AI chỉ được tăng độ khó qua quyết định/thời gian phản ứng/sai số, không qua buff tốc độ hoặc va chạm ẩn.
