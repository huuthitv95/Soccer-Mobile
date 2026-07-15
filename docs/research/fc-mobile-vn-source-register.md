# Sổ nguồn nghiên cứu FC Mobile VN

> [Chỉ mục](../index.md) · [Nghiên cứu tổng hợp](fc-mobile-vn-research.md) · [Coverage](fc-mobile-vn-coverage-audit.md)

<a id="source-register"></a>

## 1. Mục đích và quy ước

Tài liệu này là danh mục nguồn có thẩm quyền cho nghiên cứu FC Mobile VN, chốt dữ liệu đến **15/07/2026**. Nội dung trang web được truy cập ngày **15/07/2026**; ngày xuất bản được ghi riêng khi trang có cung cấp.

Mọi nhận định trong bộ tài liệu phải mang một trong bốn nhãn:

- **[Thông tin công khai đã xác minh]**: nội dung được nguồn công khai nêu trực tiếp.
- **[Quan sát từ video]**: chi tiết nhìn thấy trong video tham chiếu; không dùng để suy ra backend.
- **[Suy luận thiết kế]**: kết luận hợp lý từ bằng chứng nhưng không được nhà phát hành xác nhận.
- **[Đề xuất cho Soccer Mobile Pro]**: quyết định sản phẩm/kỹ thuật nội bộ, không phải mô tả FC Mobile VN.

Độ chắc chắn dùng ba mức: **Cao** (nguồn chính thức nói trực tiếp), **Trung bình** (nguồn chính thức nhưng có thể khác phiên bản/khu vực), **Thấp** (suy luận hoặc chưa có nguồn xác nhận). Khi EA Help và bản vá mới mô tả khác nhau, tài liệu ưu tiên thông tin mới hơn và ghi rõ nguy cơ lệch phiên bản.

<a id="official-sources"></a>

## 2. Nguồn chính thức

| ID | Cơ quan / ngày đăng | URL | Phạm vi dùng | Giới hạn | Độ chắc chắn |
| --- | --- | --- | --- | --- | --- |
| S01 | Garena, 10/10/2025 | [FC Mobile Việt Nam chính thức ra mắt 16/10/2025](https://fcmobile.garena.vn/fc-mobile-viet-nam-chinh-thuc-ra-mat-ngay-16-10-2025-tren-ios-android/) | Ngày ra mắt, iOS/Android, cấu hình, giao diện tiếng Việt, layout cảm ứng | Nội dung marketing; không phải đặc tả đầy đủ | Cao |
| S02 | Garena, 31/05/2026 | [Siêu cập nhật mùa hè](https://fcmobile.garena.vn/sieu-cap-nhat-mua-he-mo-fc-mobile-mo-hoi-bong-da/) | Chế độ, nội dung mùa, Play's Styles, Quick Match, so sánh cầu thủ | Feature theo phiên bản; có thể thay đổi sau cutoff | Cao |
| S03 | Garena, 20/10/2025 | [Sơ đồ chiến thuật trong Đấu Giả Lập](https://fcmobile.garena.vn/huong-dan-so-do-chien-thuat-trong-che-do-dau-gia-lap/) | Manager Mode/Đấu Giả Lập, sơ đồ chiến thuật, không điều khiển trực tiếp | Không công bố thuật toán AI | Cao |
| S04 | Garena, 07/01/2026 | [Quy tắc chung hệ thống giải đấu](https://fcmobile.garena.vn/quy-tac-chung-khi-tham-gia-giai-dau-thuoc-he-thong-fc-mobile/) | Thiết bị, tài khoản, hành vi thi đấu, xử lý vi phạm | Quy định esports, không mô tả gameplay runtime | Cao |
| S05 | Garena, 09/06/2026 | [Luật và thể thức FVSL Summer 2026](https://fcmobile.garena.vn/luat-va-the-thuc-thi-dau-giai-dau-fvsl-summer-2026/) | Cấu trúc giải, đăng ký, vòng đấu và vận hành | Áp dụng mùa giải cụ thể | Cao |
| S06 | Garena | [Trang hướng dẫn FC Mobile VN](https://fcmobile.garena.vn/huong-dan/) | Chỉ mục hướng dẫn và cập nhật vận hành | Trang tổng hợp thay đổi liên tục | Trung bình |
| S07 | Garena/Google | [FC Mobile VN trên Google Play](https://play.google.com/store/apps/details?id=com.garena.game.fcmobilevn&hl=vi) | Nhà phát hành, nền tảng, mô tả tính năng, an toàn dữ liệu | Listing có thể tùy vùng và được cập nhật | Cao |
| S08 | EA Help, bản hiển thị 2026 | [How to rank up Players](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/how-to-rank-up-players/) | Training XP, Rank Up, OVR, Market, đội hình, thay người, set piece | Một số thuật ngữ vật phẩm có thể lệch bản VN/bản vá mới | Trung bình |
| S09 | EA Help, bản hiển thị 2026 | [How to save your progress](https://help.ea.com/en/articles/ea-sports-fc/fc-mobile/save-your-progress/) | Guest, liên kết tài khoản, khôi phục, tài khoản trẻ vị thành niên | Provider đăng nhập có thể khác bản Garena Việt Nam | Trung bình |
| S10 | EA, 24/09/2025 | [FC Mobile 26 Gameplay Deep Dive](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-update-gameplay) | Chuyền, tạt/đánh đầu, sút, phòng ngự, thủ môn, trọng tài, accessibility, sơ đồ | Không phải bảng mapping đầy đủ của nút cảm ứng | Cao |
| S11 | EA, 2025 | [FC Mobile 26 Patch Notes](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/fc-mobile-26-patch-notes) | Rank Up Currency, Training Level, thay đổi phiên bản | Có thể khác thời điểm triển khai bản VN | Trung bình |
| S12 | EA, 2025 | [Leagues Update](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/leagues-update-2025) | League social, thành viên, quest, leaderboard, tournament | “League” là bang hội xã hội, không đồng nghĩa giải bóng đá bản quyền | Cao |
| S13 | EA, 2025 | [Leagues Deep Dive](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/leagues-deep-dive-2025) | Season Points, quest, tournament và đóng góp Division Rivals | Tính năng có thể thay đổi theo mùa | Cao |
| S14 | EA, 2025 | [UEFA Champions League Deep Dive](https://www.ea.com/games/ea-sports-fc/fc-mobile/news/uefa-champions-league-deep-dive-2025) | Chế độ UCL và phạm vi CLB tham gia | Không chứng minh toàn bộ danh mục license | Cao |
| S15 | EA | [Trang chủ EA SPORTS FC Mobile](https://www.ea.com/games/ea-sports-fc/fc-mobile) | Định vị sản phẩm, Ultimate Team, gameplay mobile | Nội dung marketing tổng quan | Cao |
| S16 | EA Help | [Trung tâm hỗ trợ FC Mobile](https://help.ea.com/en/games/ea-sports-fc/fc-mobile/) | Chỉ mục account, redeem code, squad và troubleshooting | Trang tổng hợp thay đổi liên tục | Trung bình |

<a id="research-boundaries"></a>

## 3. Ranh giới bằng chứng

- Không nguồn S01–S16 công bố schema/API endpoint của player database, cách client gọi catalog, tên CDN, cơ chế ký request hoặc chiến lược cache cụ thể.
- Không nguồn S01–S16 công bố pipeline nội bộ để scan, rig, LOD, đóng gói hay tải model 3D cầu thủ vào game. “New Face Scans” là bằng chứng về nội dung đầu ra, không phải bằng chứng về pipeline.
- S10 xác nhận cải tiến **trọng tài**, nhưng không xác nhận một hệ thống **VAR tương tác** hay dùng replay hình ảnh để quyết định luật.
- S03 xác nhận Đấu Giả Lập không cần điều khiển trực tiếp; không công bố dữ liệu huấn luyện, kiến trúc mô hình, utility score, behavior tree hoặc reinforcement learning.
- License mà người dùng sở hữu cho Soccer Mobile Pro không làm thay đổi mức xác thực của mô tả kỹ thuật EA/Garena. Tài liệu vẫn phải phân biệt dữ kiện và đề xuất.

<a id="research-process"></a>

## 4. Quy trình cập nhật

1. Ưu tiên Garena, EA/EA Help, Google Play và luật giải chính thức.
2. Gắn URL và ngày truy cập sát claim; thêm nguồn mới vào bảng trước khi dùng.
3. Nếu nguồn mới mâu thuẫn, giữ cả hai, ghi phiên bản/khu vực và hạ độ chắc chắn.
4. Không commit cache Firecrawl, HTML thô, token phiên hoặc dữ liệu truy cập.
5. Video chỉ chứng minh điều nhìn/nghe thấy tại timestamp; không dùng giao diện để suy ra API hay schema backend.
