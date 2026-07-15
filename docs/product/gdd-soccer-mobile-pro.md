# Soccer Mobile Pro — Game Design Document

> [Chỉ mục](../index.md) · [Nghiên cứu FC Mobile VN](../research/fc-mobile-vn-research.md) · [Audit Unity](../implementation/unity-implementation-audit-and-backlog.md)

**Chủ đầu tư / Studio:** ZGameVN
**Nền tảng:** Android / iOS
**Thể loại:** Bóng đá mobile online — kết hợp cơ chế xây đội hình thẻ bài (lấy cảm hứng FC Mobile VN) với trải nghiệm thi đấu trực tiếp (lấy cảm hứng eFootball Mobile)
**Trạng thái tài liệu:** Living document — v1.1
**Ngày cập nhật:** 15/07/2026
**Authority:** Quyết định sản phẩm của Soccer Mobile Pro; thông tin tham chiếu phải truy về [sổ nguồn](../research/fc-mobile-vn-source-register.md).

> Tài liệu phân biệt **Thông tin công khai đã xác minh**, **Quan sát từ video**, **Suy luận thiết kế** và **Đề xuất cho Soccer Mobile Pro**. Những gì không có nguồn công khai không được mô tả như kiến trúc nội bộ EA/Garena.

---

## 0. Mục lục

1. [Tổng quan và tầm nhìn](#section-1)
2. [Ngôn ngữ và bản địa hóa](#section-2)
3. [Core loop](#section-3)
4. [Chế độ chơi](#section-4)
5. [Tiền tệ và kinh tế](#section-5)
6. [Thẻ cầu thủ và nâng cấp](#section-6)
7. [Market và contract](#section-7)
8. [Đội hình và PlayStyle](#section-8)
9. [Trận đấu và điều khiển](#section-9)
10. [Screen map](#section-10)
11. [Tài khoản và cài đặt](#section-11)
12. [Tùy biến](#section-12)
13. [Event và season pass](#section-13)
14. [Player database](#section-14)
15. [Model 3D](#section-15)
16. [VAR presentation](#section-16)
17. [AI offline](#section-17)
18. [Tuân thủ](#section-18)
19. [Kiến trúc kỹ thuật](#section-19)
20. [Prototype hiện tại](#section-20)
21. [Backlog nghiên cứu](#section-21)
    - [Đặc tả đã tách file](#section-21a)
22. [Nguồn tham khảo](#section-22)
23. [Decision register và playtest gates](#section-23)

---

<a id="section-1"></a>

## 1. TỔNG QUAN & TẦM NHÌN SẢN PHẨM

Soccer Mobile Pro là game bóng đá mobile online, xây dựng đội hình từ **cầu thủ, CLB và giải đấu đã được ZGameVN cấp phép bản quyền**, phối trộn hai triết lý thiết kế:

- **Từ FC Mobile VN:** hệ thống thẻ bài cạnh tranh (Rank Up / Training / Skill Points), chợ chuyển nhượng giữa người chơi, Division Rivals (3 chế độ xếp hạng song song), sự kiện dạng lộ trình đổi token, bình luận/giao diện tiếng Việt.
- **Từ eFootball Mobile:** mô hình ký hợp đồng cầu thủ đặc biệt/huyền thoại (Dream Team), Progression Points tùy biến chỉ số, Team Playstyle + độ phù hợp HLV, cơ chế phòng ngự Match-Up, chế độ hợp tác 3v3.

**Điểm khác biệt cốt lõi:** Soccer Mobile Pro ưu tiên trận bóng điều khiển trực tiếp, đội hình có bản sắc và live service minh bạch; nội dung có giấy phép được dùng theo catalog của dự án, không biến sản phẩm thành bản sao giao diện hoặc công thức cân bằng của game tham chiếu.

### 1.1 Đối tượng mục tiêu

- Người chơi mobile tại Việt Nam thích bóng đá, phiên 5–15 phút và giao diện tiếng Việt.
- Người mới cần low skill floor, assist rõ và onboarding bằng thực hành.
- Người chơi cạnh tranh cần manual depth, fairness, replay/audit và không bị lợi thế trả phí tuyệt đối.
- Người sưu tầm cần catalog, compare, progression có trade-off và quyền kiểm soát tài nguyên.

**Persona chính — Minh, 22 tuổi, người chơi bóng đá mobile theo phiên ngắn:** Minh dùng Android tầm trung, chơi 2–4 phiên/ngày, mỗi phiên 8–15 phút; hiểu luật bóng đá nhưng không muốn học combo dài. Minh ưu tiên điều khiển phản hồi nhanh, đội hình có cầu thủ/CLB yêu thích, đấu bạn bè và tiến bộ nhờ kỹ năng. Minh sẽ rời game nếu thua vì input mơ hồ, AI “buff ẩn”, tải asset quá lâu hoặc progression bắt buộc quay gói.

**Persona phụ — Huy, 31 tuổi, người quản lý đội hình:** Huy chơi 3–5 phiên/tuần, thích tối ưu role/formation/market và trận offline có thể tạm dừng. Huy cần compare rõ, quyết định có trade-off và không bị FOMO phạt khi nghỉ một tuần. Hai persona cùng yêu cầu UI tiếng Việt, chữ dễ đọc và tiến trình có thể phục hồi trên thiết bị khác.

### 1.2 Trải nghiệm đích và ba trụ cột

1. **Đá bóng có cảm giác:** input rõ, phản hồi nhanh, quyết định chiến thuật có ý nghĩa.
2. **Xây đội có bản sắc:** cầu thủ, role, PlayStyle và chemistry tạo lựa chọn tình huống thay vì một đáp án tối ưu.
3. **Live service đáng tin:** reward, odds, entitlement, event và thay đổi dữ liệu minh bạch, có audit/rollback.

### 1.3 Chỉ số thành công và rủi ro thiết kế

| Nhóm | Tiêu chí trước production |
| --- | --- |
| Core loop | Người chơi mới hoàn thành trận đầu không cần trợ giúp trực tiếp; input lỗi và quit point được đo. |
| Match quality | Input latency, completion, disconnect/desync, perceived fairness và rage-quit nằm trong target đã benchmark theo device tier. |
| Team building | Người chơi hiểu tác động compare/upgrade và không tiêu nhầm thẻ khóa/premium currency. |
| Live service | Không double grant/charge; catalog, event và asset rollback được diễn tập. |

Rủi ro chính: feature creep trước khi core match được chứng minh; progression thay thế fun; UI liveops quá tải; AI tạo cảm giác gian lận; economy/pay-to-win; và tài liệu đi trước implementation quá xa. Mọi milestone phải có prototype/playtest và evidence trước khi mở rộng.

Các ngưỡng sau là **playtest gate**, không phải production policy: ≥80% người mới hoàn thành onboarding match không cần người hướng dẫn; ≥70% gọi đúng ba action cơ bản sau một lần thực hành; median time-to-first-kickoff ≤5 phút; retry sau trận thua đầu ≥50%; lỗi input do nhầm context <3% action; 0 giao dịch test double-charge/double-grant. Owner Product Analytics khóa cohort/device tier trước test và thay ngưỡng bằng baseline quan sát sau tối thiểu 30 người chơi mục tiêu.

### 1.4 MDA và meaningful trade-off

| Aesthetic đích | Dynamic cần tạo | Mechanic hỗ trợ | Dấu hiệu thất bại |
| --- | --- | --- | --- |
| Năng lực và làm chủ | Đọc khoảng trống, chọn nhịp, sửa sai | Input theo context, assist minh bạch, replay decision trace | Auto-play quyết định thay người chơi |
| Bản sắc đội bóng | Chọn role/formation theo đối thủ | PlayStyle, chemistry có cap, compare theo tình huống | Một đội hình/OVR thống trị mọi context |
| Kịch tính công bằng | Chấp nhận rủi ro có thông tin | Stamina, press risk, set piece, rule deterministic | Rubber-band/buff ẩn hoặc kết quả không giải thích được |

Ba lựa chọn cốt lõi phải tạo trade-off: pressing cao đổi lấy stamina/khoảng trống sau lưng; chuyền mạo hiểm đổi lấy tốc độ tiến bóng và nguy cơ mất bóng; dùng cầu thủ chuyên biệt đổi lấy sức mạnh theo tình huống nhưng giảm linh hoạt đội hình. Một option “tốt hơn mọi mặt” là lỗi thiết kế và chặn content gate.

---

<a id="section-2"></a>

## 2. NGÔN NGỮ & BẢN ĐỊA HÓA

| Hạng mục | Quyết định |
| --- | --- |
| Ngôn ngữ hỗ trợ | Tiếng Việt, English |
| Mặc định | **Tiếng Việt** (áp dụng ngay từ màn hình splash đầu tiên) |
| Điểm chuyển đổi | Màn hình chọn ngôn ngữ (lần đầu mở app) + nút đổi ngôn ngữ trong Cài đặt (đổi bất kỳ lúc nào, không cần khởi động lại) |
| Phạm vi bản địa hóa | Toàn bộ UI, tên hệ thống (VSA/H2H/MM…), mô tả vật phẩm, nhiệm vụ sự kiện, bình luận trận đấu (văn bản); tên cầu thủ/CLB/giải đấu giữ nguyên bản gốc quốc tế |
| Kỹ thuật | Toàn bộ chuỗi văn bản tách ra file ngôn ngữ riêng (key-value JSON theo mã ngôn ngữ `vi`, `en`), không hard-code text trong logic hiển thị, để dễ thêm ngôn ngữ thứ 3 sau này |

---

<a id="section-3"></a>

## 3. VÒNG LẶP GAMEPLAY CHÍNH (CORE LOOP)

```text
Xây & nâng cấp đội hình  →  Thi đấu (chọn 1 trong các chế độ)  →  Nhận thưởng (Xu/Kim cương/Token)
        ↑                                                                    │
        └────────────────────────  Tái đầu tư vào đội hình  ◄────────────────┘
```

Vòng lặp phụ theo tuần/mùa: **Sự kiện theo lộ trình** → tích token → đổi cầu thủ/thưởng → tăng sức mạnh đội hình cho vòng lặp chính.

| Nhịp | Người chơi làm gì | Cảm giác phải chứng minh | Gate trước khi mở rộng |
| --- | --- | --- | --- |
| 30 giây — micro | Nhận bóng → đọc áp lực → chọn chuyền/rê/sút → nhận phản hồi → chuyển trạng thái | Input có chủ ý, bóng dễ đọc, thành công/thất bại giải thích được | Gray-box 10 phút vẫn vui khi tắt reward/progression |
| 30 phút — meso | Chuẩn bị đội hình → chơi 2–3 trận → xem result/trace → chỉnh chiến thuật hoặc cầu thủ → chơi lại | Có điểm dừng tự nhiên và “thử thêm một trận” vì học được điều mới | Người chơi tự nêu được một điều chỉnh chiến thuật sau phiên |
| 30 giờ — macro | Làm chủ control/role → mở lựa chọn ngang → xây đội có bản sắc → tham gia season/social | Năng lực và quyền tự chủ, không phải nghĩa vụ/FOMO | Core match đạt gate trước khi progression tăng power được production hóa |

Micro success tạo insight cho meso; meso cung cấp bằng chứng cho macro. Progression chỉ mở cách chơi hoặc biểu đạt mới, không dùng để che core match chưa vui. Tutorial áp dụng `safe practice → low stakes → real match`, dạy đúng lúc bằng thao tác và feedback thay vì chuỗi popup chữ.

---

<a id="section-4"></a>

## 4. CHẾ ĐỘ CHƠI

### 4.1 Division Rivals (trục xếp hạng chính, tham chiếu FC Mobile)

| Chế độ | Cách chơi | Đặc điểm |
| --- | --- | --- |
| **Đấu nhanh** (kiểu VSA) | Chỉ xử lý các tình huống ghi bàn/cơ hội trong thời gian ngắn, đối đầu bất đồng bộ | Phụ thuộc mạnh vào OVR đội hình chính; phù hợp phiên chơi ngắn |
| **Đối kháng trực tiếp** (kiểu H2H) | Trận đấu real-time đầy đủ giữa 2 người chơi, toàn quyền điều khiển tấn công lẫn phòng ngự | Yêu cầu kết nối mạng ổn định; trải nghiệm "đá thật" nhất |
| **Chế độ HLV** (kiểu Manager Mode) | Chọn đội hình + 1 trong 4 chiến thuật preset (Tấn công/Kiểm soát/Phản công/Phòng ngự) hoặc tùy biến, AI thi đấu, có thể đổi chiến thuật/thay người giữa trận | Không đòi hỏi thao tác trực tiếp, phù hợp chơi "chơi khi rảnh" |

Mỗi chế độ có **hệ thống Sao (Star) độc lập**: thắng +1 sao, hòa giữ nguyên, thua −1 sao (có thể dùng vật phẩm "Khiên Sao" để miễn trừ mất sao). Mùa xếp hạng kéo dài theo chu kỳ cố định, kết thúc mùa bị giáng một số bậc so với hạng đạt được.

### 4.2 Club Challenge

Đối kháng H2H nhưng bắt buộc dùng đội hình nguyên bản của 1 CLB được cấp phép (không trộn cầu thủ khác CLB) — tôn vinh tính xác thực của các CLB thật ZVN đã mua bản quyền.

### 4.3 Dream Match (chế độ hợp tác, tham chiếu eFootball Co-op 3v3)

Người chơi rủ bạn bè lập đội 3v3, mỗi người điều khiển 1 phần sân, đấu với đội 3v3 khác hoặc AI.

### 4.4 Đấu giao hữu / VS Bạn bè

Tạo phòng, chia sẻ mã phòng, đấu không tính thứ hạng.

---

<a id="section-5"></a>

## 5. HỆ THỐNG TIỀN TỆ & KINH TẾ

| Loại tiền | Nguồn gốc | Công dụng | Tham chiếu |
| --- | --- | --- | --- |
| **Kim cương** (premium) | Nạp thật; một phần nhỏ miễn phí qua nhiệm vụ/Season Pass/sự kiện | Mở gói cầu thủ cao cấp, ký hợp đồng cầu thủ đặc biệt/huyền thoại, rút ngắn thời gian chờ | FC Points + Gems (FC Mobile), Coins (eFootball) |
| **Xu** (F2P) | Thắng trận, nhiệm vụ ngày, bán cầu thủ trên chợ | Mua bán trên Chợ chuyển nhượng, một phần chi phí Rank Up | Coins (FC Mobile), GP (eFootball) |
| **Token sự kiện** | Chỉ trong thời gian sự kiện đang diễn ra | Đổi lấy cầu thủ/thưởng giới hạn theo lộ trình sự kiện đó | Event Tokens (FC Mobile) |
| **Điểm Nâng Rank** | Thi đấu, sự kiện, đổi vật phẩm dư thừa | Nâng Rank cầu thủ (xem mục 6) | Rank Up Points (FC Mobile 26+) |

**Nguyên tắc thiết kế kinh tế:**

- Xu luôn là con đường F2P khả thi để cạnh tranh (qua giao dịch chợ thông minh), không khóa cứng người không nạp tiền.
- Token sự kiện hết hạn khi sự kiện kết thúc để tránh tồn kho vô hạn.
- Chợ chuyển nhượng thu thuế giao dịch (đề xuất 5–10%) để kiểm soát lạm phát Xu.

---

<a id="section-6"></a>

## 6. THẺ CẦU THỦ: PHÂN LOẠI & HỆ THỐNG NÂNG CẤP

### 6.1 Phân loại thẻ (rarity/loại)

| Loại thẻ | Cách sở hữu | Đặc điểm |
| --- | --- | --- |
| **Thường (Standard)** | Gói cơ bản, chợ, nhiệm vụ | Chỉ số theo phong độ thật hiện tại |
| **Tiêu biểu (Featured)** | Ký hợp đồng bằng Kim cương, sự kiện | Chỉ số nhỉnh hơn bản Thường của cùng cầu thủ (đang có phong độ tốt) |
| **Đặc biệt theo sự kiện (Trending/Highlight)** | Chỉ qua sự kiện đang diễn ra | Boost tạm thời phản ánh phong độ/khoảnh khắc nổi bật gần đây |
| **Huyền thoại (Legend)** | Quay/ký hợp đồng giới hạn, làm mới định kỳ | Cầu thủ đã giải nghệ, chỉ số dựa trên đỉnh cao sự nghiệp |

### 6.2 Hai trục nâng cấp độc lập (điểm thiết kế quan trọng nhất, tách bạch rõ ràng)

**Trục 1 — Rank Up (nâng OVR):**

- Dùng **Điểm Nâng Rank** (tiền tệ tiến trình riêng — không cần thẻ trùng bản) để tăng OVR, tối đa **5 cấp Rank**, mỗi cấp +1 OVR.
- Nâng cấp **luôn thành công** khi đủ điểm — không có tỉ lệ rủi ro, tránh gây ức chế (bài học rút ra từ hệ thống cũ dùng thẻ trùng bản của FC Mobile trước bản 26).
- Rank cao hơn = mở trần Training cao hơn (2 trục liên kết một chiều: Rank quyết định giới hạn của Training, nhưng không tự động tăng chỉ số).

**Trục 2 — Training Level (nâng chỉ số kỹ thuật):**

- Dùng cầu thủ khác (bất kỳ vị trí nào) làm nguyên liệu tăng thanh XP Training.
- Khi đầy thanh XP, cầu thủ lên 1 Training Level, tăng các chỉ số kỹ thuật cụ thể (Sút/Chuyền/Rê bóng/Phòng ngự/Thể lực/Tốc độ) — **không** tăng OVR trực tiếp.
- Trần Training Level bị giới hạn bởi Rank hiện tại (ví dụ: Rank 0 → tối đa Training 10; Rank 5 → tối đa Training 30 — con số cụ thể sẽ cân bằng ở giai đoạn balancing).

**Trục 3 — Skill Points (tùy biến lối chơi):**

- Mỗi lần Rank Up mở khóa **1 Skill Point** (tối đa 5 điểm/thẻ trong vòng đời Rank).
- Mỗi vị trí có bộ kỹ năng riêng: **2 kỹ năng chính** (nâng tối đa cấp 2) + khi hoàn thành kỹ năng chính, mở **1 trong 3 kỹ năng phụ nâng cao** (chỉ 1 cấp) — buộc người chơi chọn hướng phát triển rõ ràng (thiên công/thủ/kiểm soát) thay vì rải đều.
- Có thể reset toàn bộ Skill Points bằng Kim cương để thử nghiệm lại.

**Trục 4 — Position Training (mở vị trí phụ):**

- Ở các mốc Rank nhất định, người chơi có thể "đào tạo" cầu thủ chơi thêm 1 vị trí phụ (ví dụ CM → mở CDM), tăng tính linh hoạt đội hình mà không cần mua thêm thẻ mới.

### 6.3 Bảng tóm tắt quan hệ 4 trục

| Trục | Tăng gì | Nguyên liệu | Giới hạn bởi |
| --- | --- | --- | --- |
| Rank Up | OVR (+1/cấp, tối đa 5) | Điểm Nâng Rank | — |
| Training | Chỉ số kỹ thuật | Thẻ cầu thủ khác (XP) | Rank hiện tại |
| Skill Points | Kỹ năng đặc trưng theo lối chơi | Tự động khi Rank Up | Số lần đã Rank Up |
| Position Training | Vị trí phụ | Mốc Rank cụ thể | Rank hiện tại |

---

<a id="section-7"></a>

## 7. CHỢ CHUYỂN NHƯỢNG & KÝ HỢP ĐỒNG

Thiết kế lai giữa 2 mô hình:

**Tab Chợ (Market) — tham chiếu FC Mobile:**

- Đấu giá/giao dịch cầu thủ **giữa người chơi với nhau** bằng Xu.
- Bộ lọc theo vị trí/OVR/CLB/giải đấu/mức Rank đã nâng.
- Thu thuế giao dịch trên mỗi lượt bán.
- Đây là con đường chính để người chơi F2P nâng cấp đội hình mà không cần nạp tiền.

**Tab Ký hợp đồng (Contract) — tham chiếu eFootball Dream Team:**

- Ký cầu thủ Tiêu biểu/Huyền thoại trực tiếp từ hệ thống (không qua người chơi khác) bằng Kim cương.
- Danh sách làm mới định kỳ (theo tuần), có cơ chế "chỉ định" (nominating) cho phép chọn đúng cầu thủ mong muốn với chi phí cao hơn quay ngẫu nhiên.
- Hợp đồng có thời hạn sử dụng (đề xuất theo mùa giải trong game, không nhất thiết 365 ngày như eFootball — cần cân bằng riêng).

**Tab Đổi (Exchange):**

- Quy đổi cầu thủ dư thừa/không dùng lấy Xu hoặc vật phẩm nâng cấp, giảm tồn kho vô dụng trong túi đồ người chơi.

---

<a id="section-8"></a>

## 8. ĐỘI HÌNH: CHEMISTRY, TEAM BADGE, PLAYSTYLE

| Hệ thống | Mô tả | Tham chiếu |
| --- | --- | --- |
| **Chemistry đội hình** | Chỉ số cộng hưởng dựa trên sự liên kết giữa CLB/giải đấu/quốc tịch của các cầu thủ trong đội hình chính; ảnh hưởng đến hiệu suất thi đấu song song với OVR trung bình | FC Mobile |
| **Huy hiệu đội (Team Badge)** | Slot trang bị (khuyến nghị 3 slot) gắn huy hiệu để cộng chỉ số toàn đội; huy hiệu có được qua sự kiện/thành tích | FC Mobile 26 |
| **Lối chơi đội (Team Playstyle)** | Chọn 1 trong 5 lối chơi (Kiểm soát bóng, Phản công nhanh, Tấn công biên, Pressing cao, Phòng ngự chặt) trong màn Chiến thuật; hiệu quả tăng nếu HLV được chọn có độ phù hợp cao với lối chơi đó | eFootball |
| **PlayStyle cầu thủ** | Mỗi cầu thủ (đặc biệt là thẻ Tiêu biểu/Huyền thoại) có 1–2 "chiêu" đặc trưng kích hoạt tự động trong tình huống phù hợp (vd. dứt điểm xa, tì đè, rê bóng tốc độ), khiến 2 thẻ cùng OVR chơi khác nhau | FC Mobile PlayStyles |
| **HLV (Manager)** | Thẻ HLV riêng biệt, mỗi HLV có 1 lối chơi sở trường + hệ số phù hợp; ảnh hưởng đến hiệu quả chiến thuật đã chọn | eFootball |

---

<a id="section-9"></a>

## 9. TRẢI NGHIỆM TRẬN ĐẤU & ĐIỀU KHIỂN

### 9.1 Sơ đồ điều khiển

- **Joystick ảo (trái):** di chuyển cầu thủ đang điều khiển.
- **Cụm nút ngữ cảnh (phải):** Chuyền / Chuyền dài (giữ/vuốt để chọn kiểu chuyền bổng) / Sút (giữ để tích lực, vuốt để sút vô lê/sút xoáy) / Rướn & Kỹ thuật (giữ để chạy nước rút, vuốt để thực hiện động tác kỹ thuật).
- **Chuyển đổi ngữ cảnh:** khi đội nhà đang có bóng trong phần sân nhà, nút Sút tự động đổi thành nút **Phá bóng** để tránh thao tác sai nguy hiểm.
- **Cơ chế phòng ngự Match-Up:** giữ nút Áp sát để hậu vệ "kèm bước chân" đối thủ (di chuyển lùi/ngang, giữ trọng tâm thấp), tự động cắt đường chuyền gần đó — thay thế việc lao vào tắc bóng liều lĩnh.
- **2 chế độ điều khiển:** Nút bấm (Classic — ổn định, khuyến nghị cho thi đấu cạnh tranh) và Cử chỉ (Gesture/Touch & Flick — mượt hơn khi rê bóng, khó hơn khi phòng ngự); người chơi chọn tự do trong Cài đặt.
- **Hỗ trợ người mới (tùy chọn bật/tắt):** tự động hỗ trợ thực hiện động tác rê bóng/chuyền bóng phức tạp mà không cần lệnh thủ công chính xác — dành cho người chơi mới.

### 9.2 HUD trong trận

- Tỉ số + đồng hồ dạng tối giản ở trên cùng.
- Radar/minimap vị trí cầu thủ (tùy chọn bật/tắt trong Cài đặt).
- Chỉ báo PlayStyle kích hoạt (icon nhỏ nổi lên trên đầu cầu thủ khi chiêu đặc trưng được kích hoạt).
- Góc camera: hỗ trợ tối thiểu 3 góc quay (Rộng/Chuẩn/Sân vận động), chọn trong menu tạm dừng hoặc Cài đặt trước trận.

---

<a id="section-10"></a>

## 10. BẢN ĐỒ MÀN HÌNH (SCREEN MAP)

| # | Màn hình | Nội dung chính | Điều hướng đến |
| --- | ---------- | ---------------- | ----------------- |
| 0 | Chọn ngôn ngữ (splash) | VI (mặc định) / EN | → Trang chủ |
| 1 | **Trang chủ** | Nav dưới: Trang chủ / Đội hình / Thi đấu / Chợ / Hồ sơ; thanh Xu/Kim cương, banner sự kiện, nút Đấu nhanh nổi bật | Tất cả màn hình khác |
| 2 | **Đội hình** | Sân bóng sơ đồ, kéo-thả cầu thủ, chỉ số OVR + Chemistry, slot Huy hiệu đội, chọn Team Playstyle + HLV | Chi tiết cầu thủ, Rank Up/Training |
| 2a | Chi tiết cầu thủ | Rank Up / Training / Skill Points / Position Training | — |
| 3 | **Chợ** | Tab Market (đấu giá) / Ký hợp đồng / Đổi (Exchange) | Chi tiết cầu thủ |
| 4 | **Sảnh trận đấu** | Đội trưởng 2 bên, chọn áo đấu, xem sơ lược đội hình đối thủ, chọn chế độ điều khiển & camera | Thi đấu |
| 5 | **Thi đấu** | Gameplay chính (xem mục 9) | Kết quả trận đấu |
| 6 | Kết quả trận đấu | Tỉ số, MOTM, thưởng Xu/Kim cương/token | Trang chủ / Đấu lại |
| 7 | **Sự kiện** | Lộ trình đổi token, nhiệm vụ ngày/tuần, Star Pass | Chợ (đổi thưởng) |
| 8 | Tùy biến CLB | Áo đấu, logo, huy hiệu, sân vận động | — |
| 9 | Cài đặt | Ngôn ngữ, kiểu điều khiển, camera, âm thanh, hỗ trợ người mới | — |
| 10 | Hồ sơ | Thống kê cá nhân, lịch sử trận đấu, thành tích | — |

---

### 10.1 Cụm màn hình theo vòng đời người dùng

| Cụm | Màn hình chính | Mục tiêu UX |
| --- | --- | --- |
| First Launch | Splash, chọn ngôn ngữ, tải gói tài nguyên cơ bản | Vào game nhanh, mặc định tiếng Việt, giảm bỏ game sớm |
| Login / Binding | Guest login, liên kết tài khoản, điều khoản | Cho phép chơi ngay nhưng khuyến khích bind tài khoản |
| Home LiveOps | Banner sự kiện, inbox, nhiệm vụ, điểm danh, shortcut chế độ | Luôn đẩy nội dung đang chạy và CTA giá trị nhất lên đầu |
| Team Building | Đội hình, cầu thủ, nâng cấp, so sánh, chemistry/play style | Đây là trung tâm tiến trình meta |
| Match Flow | Chuẩn bị trận, loading, HUD, tạm dừng, kết quả | Đọc được nhanh, ít chạm nhầm, ưu tiên độ rõ |
| Social / Economy | Chợ, cửa hàng, bạn bè, guild/clan (nếu có), giao hữu | Hỗ trợ vòng lặp giữ chân và cạnh tranh |
| Support / Settings | Cài đặt, ngôn ngữ, âm thanh, tài khoản, pháp lý | Tự phục vụ tốt, hỗ trợ nhanh khi có lỗi |

### 10.2 Quy tắc điều hướng

- Từ Home phải vào được ngay: **Play**, **Squad**, **Events**, **Store/Market**, **Settings**.
- Mọi trang cầu thủ đều phải có đường tắt tới **Compare**, **Upgrade**, **Add to Squad**.
- Màn hình sự kiện không được tách rời đội hình; luôn có shortcut tới đội để tránh đứt mạch nâng cấp.
- Match result phải nối thẳng sang 3 việc: chơi tiếp, nâng đội hình, nhận thưởng/nhiệm vụ.

<a id="section-11"></a>

## 11. TÀI KHOẢN, CÀI ĐẶT & HỖ TRỢ

### 11.1 Hệ thống tài khoản

- Hỗ trợ **Guest Account** ở lần đầu vào game để giảm ma sát onboarding.
- Cho phép liên kết lên tài khoản định danh (Google/Apple/Facebook/nhà phát hành) mà không mất tiến trình.
- Mọi giao dịch nạp, entitlement và tiến trình sự kiện phải được lưu ở account-level thay vì device-level.
- Có trang hỗ trợ account recovery, lịch sử đăng nhập gần đây và yêu cầu xóa dữ liệu theo quy định bảo mật.

### 11.2 Hệ thống cài đặt

| Nhóm | Hạng mục chính |
| --- | --- |
| Language & Audio | Ngôn ngữ UI, ngôn ngữ bình luận, tải/xóa voice pack |
| Controls | Kiểu joystick, kích thước/opacity HUD, hỗ trợ chuyền/sút, auto-switch |
| Gameplay | Camera, radar, rung máy, hỗ trợ phòng ngự, preview control preset |
| Graphics | FPS mode, chất lượng model/sân/khán giả/đổ bóng |
| Network | Ping indicator, cảnh báo mạng yếu, chẩn đoán kết nối |
| Support & Legal | Trung tâm hỗ trợ, FAQ, điều khoản, chính sách dữ liệu |

### 11.3 Nguyên tắc UX cho cài đặt

- Các tùy chọn ảnh hưởng trực tiếp gameplay phải có chế độ **thử nhanh** trong sân tập.
- Voice pack và gói tài nguyên tải thêm phải hiển thị rõ dung lượng.
- Các thay đổi đồ họa nặng cần có cơ chế Apply/Revert để tránh khóa thiết bị yếu.

<a id="section-12"></a>

## 12. TÙY BIẾN & TRÌNH BÀY TRỰC QUAN

- **Áo đấu:** chọn theo CLB thật đã cấp phép, tự tránh trùng màu ở sảnh trận đấu.
- **Logo/Huy hiệu người chơi:** mở khóa qua sự kiện, Star Pass, hoặc thành tích bảng xếp hạng.
- **Sân vận động tùy biến:** màu sân, họa tiết mặt cỏ, băng rôn khán đài, hiệu ứng ăn mừng — mở khóa qua chiến dịch hoặc mua bằng Kim cương.
- **Phong cách hình ảnh chung:** gradient xanh navy/vàng neon, layout dạng thẻ, HUD tối giản, chuyển cảnh trước/sau trận để tăng cảm giác "truyền hình trực tiếp".

---

<a id="section-13"></a>

## 13. SỰ KIỆN, MÙA GIẢI & SEASON PASS

- **Sự kiện theo lộ trình:** chơi trận hằng ngày → nhận token sự kiện → dùng token quay Draft hoặc đổi trực tiếp lấy cầu thủ/thưởng; chia theo tuần/giai đoạn (ví dụ tuần 1: Tiền đạo, tuần 2: Tiền vệ...).
- **Season Pass (Vé Mùa):** có nhánh miễn phí và trả phí, tích điểm qua nhiệm vụ/trận đấu, phần thưởng cao nhất là cầu thủ/vật phẩm độc quyền mùa đó.
- **Chiến dịch theo mùa bóng thật:** đồng bộ nội dung sự kiện với các mốc bóng đá thực tế (khai mạc giải, vòng loại, chung kết) mà ZVN có bản quyền.
- **Bảng xếp hạng tuần:** nhóm người chơi cùng hạng (ví dụ 20 người/nhóm), xếp theo số trận thắng trong tuần, thưởng theo thứ hạng cuối tuần.

---

<a id="section-14"></a>

## 14. PLAYER DATABASE — NGUỒN DỮ LIỆU & VẬN HÀNH

**Ghi chú quan trọng:** phần endpoint/backend nội bộ của FC Mobile VN không có tài liệu công khai; nội dung dưới đây là kiến trúc đề xuất cho Soccer Mobile Pro dựa trên yêu cầu sản phẩm cùng các mẫu thiết kế phổ biến của live-service football game.

### 14.1 Quy trình vận hành dữ liệu (đề xuất cho ZVN)

1. **Nguồn dữ liệu gốc:** xác định trong hợp đồng bản quyền ZVN đã ký — liệu có bao gồm feed chỉ số cầu thủ từ đối tác dữ liệu bóng đá chuyên nghiệp, hay ZVN phải tự biên soạn dựa trên phong độ thực tế và nguồn công khai.
2. **Đội ngũ duy trì dữ liệu:** một nhóm nhỏ (đề xuất khởi điểm 3–5 người) rà soát và cập nhật chỉ số theo chu kỳ **hằng tháng**, phản ánh phong độ mùa giải thật; có thể mở rộng theo mô hình "cộng tác viên tình nguyện" ở quy mô nhỏ hơn khi game phát triển.
3. **Cơ chế cập nhật tức thời:** áp dụng dạng "Cầu thủ đang lên" (Trending) — tăng chỉ số tạm thời cho cầu thủ vừa có màn trình diễn nổi bật, làm mới theo tuần.
4. **Cơ chế refresh lớn theo mùa:** đầu mỗi mùa giải thật (ví dụ đầu mùa bóng mới), thực hiện đợt cập nhật chỉ số tổng thể.

### 14.2 Schema dữ liệu kỹ thuật (đề xuất cho đội lập trình)

```json
{
  "player_id": "string (unique)",
  "name": "string",
  "club_id": "string",
  "league_id": "string",
  "nationality": "string",
  "position_main": "string",
  "position_alt": ["string"],
  "card_type": "standard | featured | trending | legend",
  "ovr_base": "number",
  "stats": {
    "pace": "number", "shooting": "number", "passing": "number",
    "dribbling": "number", "defending": "number", "physical": "number"
  },
  "playstyle": ["string"],
  "rank": "number (0-5)",
  "training_level": "number",
  "skill_points_allocated": { "primary": {}, "secondary": {} },
  "contract_expiry": "date | null",
  "data_version": "string (theo mùa/tháng)",
  "asset_tier": "star_head | generic_head | placeholder"
}
```

**Nguyên tắc kiến trúc:** tách **dữ liệu tĩnh** (tên, CLB, giải đấu, hình ảnh — ít đổi) khỏi **dữ liệu động** (chỉ số, giá thị trường, phiên bản — đổi thường xuyên), cho phép cập nhật qua backend live-ops **không cần** người chơi tải bản cập nhật app mới.

---

<a id="section-15"></a>

## 15. MODEL 3D CẦU THỦ — PIPELINE SẢN XUẤT

**Ghi chú:** chưa có nguồn công khai mô tả chính xác pipeline import model 3D của FC Mobile VN; vì vậy mục này là pipeline đích đề xuất cho Soccer Mobile Pro.

| Cấp độ tài sản | Quy trình | Chi phí/thời gian | Áp dụng cho |
| --- | --- | --- | --- |
| **Star Head (scan thật)** | Chụp ảnh nhiều góc bằng giàn máy ảnh chuyên dụng tại buổi tập/sự kiện CLB → phần mềm dựng mesh 3D thô → nghệ sĩ 3D tinh chỉnh texture/tóc | Cao — làm theo đợt, ưu tiên cầu thủ/CLB chủ lực | Ngôi sao chủ lực trong chiến dịch ra mắt |
| **Generic Head (dựng thủ công)** | Nghệ sĩ dựng khuôn mặt gần đúng dựa trên ảnh tham khảo công khai + bộ công cụ sculpt có sẵn (preset + slider) | Trung bình | Phần lớn cầu thủ trong danh sách ban đầu |
| **Placeholder (giai đoạn hiện tại)** | Thẻ 2D dạng minh họa/silhouette + huy hiệu CLB dạng chữ viết tắt trên nền màu | Thấp — dùng ngay | Bản prototype/demo |

**Lộ trình đề xuất:** Prototype (Placeholder) → Alpha (Generic Head cho đội hình chính) → Live-service (bổ sung Star Head theo đợt, ưu tiên theo mức độ nổi tiếng/yêu cầu cộng đồng — tương tự cách FC Mobile công bố "đợt cập nhật Face Scan" định kỳ).

---

<a id="section-16"></a>

## 16. LUẬT TRẬN ĐẤU ĐẶC BIỆT: VAR PRESENTATION LAYER

- Soccer Mobile Pro không nên coi VAR là hệ thống luật độc lập, mà là lớp **trình bày quyết định** cho các tình huống nhạy cảm như việt vị sát nút, penalty, bóng qua vạch vôi và thẻ đỏ trực tiếp.
- Quyết định cuối cùng vẫn do match engine và luật hiện hành xử lý; VAR chỉ bổ sung replay, overlay “VAR Check” và đường minh họa để tăng tính truyền hình.
- Ở ranked online, cần giới hạn tần suất để không kéo dài trận; trong cài đặt cho phép tắt cutscene nhưng không tắt logic luật.

<a id="section-17"></a>

## 17. AI ĐÁ OFFLINE VỚI MÁY

### 17.1 Mục tiêu

AI offline phải tạo cảm giác “đá với đội bóng có chiến thuật”, không phải bot gian lận chỉ cộng chỉ số ẩn.

### 17.2 Kiến trúc đề xuất

1. **Tactical Layer:** chọn tempo, width, line height, pressing, risk profile.
2. **Role Layer:** state machine riêng cho CB/FB/DM/CM/AM/Winger/ST/GK.
3. **Decision Layer:** utility scoring cho pass, shoot, dribble, clear, press, jockey.
4. **Animation Layer:** ánh xạ quyết định sang locomotion và animation profile phù hợp.

### 17.3 Dữ liệu huấn luyện và tinh chỉnh

- Dùng telemetry replay nội bộ để học phân bố chuyền, sút, pressing và chuyển trạng thái.
- Dùng kịch bản mẫu bóng đá (counter-attack, low block, wing overload, set-piece defense) để regression test.
- Điều chỉnh độ khó bằng decision latency, sai số thao tác, mức mạo hiểm chuyền và chất lượng first touch, thay vì buff chỉ số thô.

<a id="section-18"></a>

## 18. TUÂN THỦ PHÁP LÝ & ĐẠO ĐỨC

- **Công bố tỉ lệ mở gói (loot box odds):** hiển thị rõ tỉ lệ % nhận từng loại thẻ trong màn Store trước khi mua, tuân thủ theo chuẩn phổ biến ở các cửa hàng ứng dụng (App Store/Google Play đều yêu cầu công bố xác suất với cơ chế gacha).
- **Khu vực có quy định loot box nghiêm ngặt:** một số thị trường quốc tế cấm/hạn chế bán vật phẩm ngẫu nhiên bằng tiền thật — nếu Soccer Mobile Pro phát hành ra ngoài Việt Nam, cần cơ chế khóa tính năng mở gói bằng Kim cương theo khu vực (learned từ cách các game gacha bóng đá quốc tế xử lý).
- **Bảo vệ người chơi vị thành niên:** giới hạn chi tiêu, cảnh báo trước giao dịch giá trị lớn, không thiết kế cơ chế gây áp lực tâm lý kiểu FOMO quá mức nhắm vào trẻ vị thành niên.
- **Minh bạch hợp đồng cầu thủ:** nếu áp dụng cơ chế "hết hạn hợp đồng" như eFootball, cần thông báo rõ thời hạn còn lại trước khi hết hạn, tránh gây bất ngờ khó chịu cho người chơi.

---

<a id="section-19"></a>

## 19. KIẾN TRÚC KỸ THUẬT: PROTOTYPE → PRODUCTION

| Giai đoạn | Công nghệ | Phạm vi |
| --- | --- | --- |
| **Prototype hiện tại** | Unity 2022.3.62f3, C#, legacy GUI/touch wrapper, scene-based flow | Match prototype, chọn mode/đội, kickoff, set-piece cơ bản, kết quả và AI heuristic. |
| **Foundation/P0** | Assembly boundaries, Input System actions, domain state, tests, localization/settings persistence | Giữ match playable trong khi tách input, state và data khỏi scene/GameObject lookup. |
| **Alpha/P1** | Versioned catalog, account mock, squad/card progression, Addressables, telemetry | Team-building và offline match loop có dữ liệu/test/rollback. |
| **Beta/Live service** | Server-authoritative account/economy/market/match result, CMS, entitlement và observability | Closed beta theo region/device tier; staged rollout và incident response. |

Chi tiết ownership, dependency và acceptance nằm trong [audit Unity và backlog](../implementation/unity-implementation-audit-and-backlog.md).

---

<a id="section-20"></a>

## 20. PHẠM VI BẢN PROTOTYPE HIỆN TẠI (v1)

**Đã quan sát trong source:** 78 script C#, 14 scene, 19 prefab; flow splash/menu/chọn mode/chọn đội/kickoff/match/result; legacy touch/joystick; player/ball ownership; AI theo vai trò với heuristic; corner/free-kick/goalkeeper/foul ở mức prototype.

**Chưa được triển khai như hệ thống sản phẩm:** localization/language selection, account/login service, settings schema, catalog player/club/league, squad/card progression, market/exchange, event/pass, VAR presentation, telemetry, server authority, Input Actions asset, assembly definitions và automated tests.

Các video tham chiếu không phải bằng chứng rằng các màn hình tương ứng đã tồn tại trong source Unity.

---

<a id="section-21"></a>

## 21. BACKLOG NGHIÊN CỨU & LỘ TRÌNH MỞ RỘNG

Các câu hỏi chưa khóa không còn là checkbox vô chủ. Chúng được quản lý tại [decision register](#section-23); backlog triển khai nằm ở [tài liệu implementation](../implementation/unity-implementation-audit-and-backlog.md). Số cân bằng trong GDD là hypothesis/config cho đến khi vượt playtest gate và có owner phê duyệt.

---

<a id="section-21a"></a>

## 21A. ĐẶC TẢ BỔ SUNG ĐÃ TÁCH FILE

- [Coverage audit](../research/fc-mobile-vn-coverage-audit.md): ma trận bao phủ, ranh giới bằng chứng và Definition of Done.
- [Nghiên cứu FC Mobile VN](../research/fc-mobile-vn-research.md): data governance, card model, controls, VAR và AI.
- [Sổ nguồn](../research/fc-mobile-vn-source-register.md): URL, ngày truy cập và claim được hỗ trợ.

Các hệ thống dưới đây được tách khỏi GDD tổng để đội sản phẩm, kỹ thuật và vận hành có tài liệu triển khai chuyên biệt:

- [LiveOps, monetization và membership](../operations/liveops-monetization-and-membership.md)
- [Competitive integrity và esports](../systems/competitive-integrity-and-esports.md)
- [Match controls, set pieces và VAR](../systems/match-controls-set-pieces-and-var.md)
- [Account, localization và settings](../systems/account-localization-and-settings.md)
- [Competitions, leagues, clubs và social](../systems/competitions-leagues-clubs-and-social.md)
- [Football catalog, player database và model assets](../systems/football-catalog-player-database-and-model-assets.md)
- [Player cards, skills, progression, market và exchange](../systems/player-cards-skills-progression-market-and-exchange.md)
- [Offline AI, tactics và difficulty](../systems/offline-ai-tactics-and-difficulty.md)
- [UX wireflows và states](ux-wireflows-and-states.md)
- [Live data và operations](../operations/live-data-and-operations.md)

<a id="section-22"></a>

## 22. PHỤ LỤC: NGUỒN THAM KHẢO

- [Nghiên cứu chi tiết](../research/fc-mobile-vn-research.md)
- [Sổ nguồn chính thức và mức độ chắc chắn](../research/fc-mobile-vn-source-register.md)
- [Bằng chứng video có timestamp](../research/video/ui-pattern-synthesis.md)

Các công thức, schema và pipeline trong GDD là quyết định hoặc đề xuất của Soccer Mobile Pro trừ khi đoạn văn gắn nhãn và dẫn nguồn khác.

---

<a id="section-23"></a>

## 23. DECISION REGISTER VÀ PLAYTEST GATES

| ID | Quyết định mở | Owner | Dependency/phạm vi thử | Metric và gate | Trạng thái |
| --- | --- | --- | --- | --- | --- |
| D01 | Công thức chemistry và cap | Game Design | Core match + catalog role | Không dominant strategy; ≥3 formation khả dụng trong scenario suite | Chờ core gate |
| D02 | Thuế/limit market | Economy + Backend | Ledger, price band, anti-abuse | Sink/source ratio và false-positive manipulation trong simulation | Chờ economy prototype |
| D03 | Preset AI Manager | AI/Gameplay | Tactical state/decision trace | Shape error, pass diversity, exploit rate theo seed | Chờ AI scenario suite |
| D04 | Chi phí Rank/Training | Economy + Product | Core loop đạt gate, upgrade receipt | Time-to-goal, regret/undo request, power gap cohort | Không production hóa |
| D05 | Esports dài hạn | Competitive Ops | Deterministic result, reconnect, audit | Dispute SLA, verified result và tournament completion | Chờ integrity P1 |
| D06 | Push/retention | LiveOps + Privacy | Consent, quiet hours, preference | Opt-out/complaint; không dùng mất streak làm phạt | Chờ privacy review |
| D07 | Giá bundle/LTV | Product + Compliance | Odds/entitlement/refund | Không pay-to-win; refund/reconcile pass; age review | Chờ compliance gate |

Mỗi decision chuyển sang `Approved` chỉ khi có build/config version, cohort/device tier, câu hỏi nghiên cứu, raw metric, biên bản rủi ro và rollback. Playtest quan sát hành vi (time, mis-input, retry, quit point) trước survey; ba người lặp cùng lỗi tạo design issue. Khi core match thất bại gate, D01–D07 không được dùng làm lý do mở thêm feature.
