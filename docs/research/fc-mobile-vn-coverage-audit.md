# Kiểm toán độ phủ yêu cầu FC Mobile VN

> [Chỉ mục](../index.md) · [Nghiên cứu tổng hợp](fc-mobile-vn-research.md) · [Sổ nguồn](fc-mobile-vn-source-register.md)

<a id="audit-purpose"></a>

## 1. Mục tiêu kiểm toán

Ma trận này là authority về độ phủ của yêu cầu nghiên cứu tại mốc **15/07/2026**. “Đủ” nghĩa là đã ghi rõ bằng chứng công khai, giới hạn và đề xuất cần thiết; không có nghĩa đã biết nội bộ EA/Garena. “Một phần” nghĩa là thiếu nguồn chính thức hoặc còn phụ thuộc phân tích video/implementation.

<a id="coverage-matrix"></a>

## 2. Ma trận độ phủ

| Yêu cầu | Trạng thái | Bằng chứng / tài liệu | Khoảng trống còn lại | Hành động khóa quyết định |
| --- | --- | --- | --- | --- |
| Tính năng tổng thể | Đủ nghiên cứu | [Mục 2](./fc-mobile-vn-research.md#features-and-modes), S02/S08/S12 | Feature thay đổi theo mùa | Revalidate trước production milestone |
| Layout UI/UX | Đủ nghiên cứu và video | [Mục 3](fc-mobile-vn-research.md#ui-ux-layout), [tổng hợp video](video/ui-pattern-synthesis.md) | Cần usability test trên UI riêng của dự án | Chuyển pattern thành wireframe/prototype Soccer Mobile Pro |
| Chọn ngôn ngữ | Đủ về ranh giới | [Mục 4](fc-mobile-vn-research.md#language-system), S01 xác nhận UI tiếng Việt; [video Settings](video/03-profile-settings.md) | Video không chứng minh first-launch locale/hot-swap | Giữ first-launch selector và hot-swap dưới nhãn đề xuất |
| Cài đặt | Đủ nghiên cứu và video | [Mục 5](fc-mobile-vn-research.md#settings-system), S01/S10, [video Settings](video/03-profile-settings.md) | Default theo device tier chưa được playtest | Chốt settings schema trong P1 |
| Tài khoản | Đủ ở mức tham chiếu | [Mục 6](./fc-mobile-vn-research.md#account-system), S09 | Provider/policy bản Garena có thể khác Global | Xác minh policy Garena trước implementation |
| Giải đấu | Đủ nghiên cứu | [Mục 7](./fc-mobile-vn-research.md#competitions-clubs), S04/S05/S14 | License catalog toàn bộ không được suy từ một mode | Xây license register riêng theo territory/season |
| Câu lạc bộ | Đủ kiến trúc tham chiếu | [Mục 7](./fc-mobile-vn-research.md#competitions-clubs), S08/S12 | Master data và license nội bộ chưa có | Chốt schema Competition/Season/Club |
| Chi tiết cầu thủ | Đủ khái niệm | [Mục 8](./fc-mobile-vn-research.md#players-and-assets), S08/S10 | Không có schema nội bộ | Tách identity/rating/item definition/owned instance |
| Cách gọi player database | Đủ về ranh giới, chưa thể xác minh nội bộ | [Mục 8.1](./fc-mobile-vn-research.md#players-and-assets), **[Suy luận thiết kế]** | Endpoint/payload/CDN/cache của FC Mobile VN không công khai | Chỉ đặc tả API của Soccer Mobile Pro sau khi chốt backend |
| Cách thêm model 3D | Đủ về ranh giới và proposal | [Mục 8.2](./fc-mobile-vn-research.md#players-and-assets), S10 chỉ xác nhận face scan đầu ra | Pipeline EA/Garena không công khai | Prototype Addressables + LOD/fallback riêng |
| Skills/PlayStyles | Đủ ở mức taxonomy | [Mục 9](./fc-mobile-vn-research.md#skills-system), S02/S10 | Terminology/rule chi tiết có thể lệch bản | Đối chiếu client và data spec trước code |
| Nâng cấp thẻ | Đủ nhưng có version drift | [Mục 10](./fc-mobile-vn-research.md#card-upgrade), S08/S11 | Rank item và currency mô tả khác nhau | Ưu tiên data/config version của sản phẩm, không hard-code |
| Nút trái/phải có bóng | Đủ ở mức proposal | [Mục 11](fc-mobile-vn-research.md#mobile-controls), [controls spec](../systems/match-controls-set-pieces-and-var.md); mapping là **Đề xuất** | Bộ video không có trận đấu | Playtest Input Actions riêng của dự án trước khi khóa layout |
| Nút trái/phải không bóng | Đủ ở mức proposal | [Mục 11](fc-mobile-vn-research.md#mobile-controls), [controls spec](../systems/match-controls-set-pieces-and-var.md) | Mapping FC Mobile VN chính xác chưa được nguồn công khai xác nhận | Giữ action map riêng on/off-ball và công bố assist policy |
| Check VAR | Đủ về kết luận “chưa xác nhận” | [Mục 12](./fc-mobile-vn-research.md#var-system), S10 chỉ xác nhận referee | Không có bằng chứng VAR tương tác | Giữ VAR là presentation trên rule engine deterministic |
| Train AI offline | Đủ về kết luận “không biết nội bộ” | [Mục 13](./fc-mobile-vn-research.md#offline-ai), S03/S08 | Không công bố ML/model/training data | Dùng AI phân tầng + scenario telemetry; ML là R&D riêng |
| Phân tích 5 video | Đủ | [Tổng hợp pattern](video/ui-pattern-synthesis.md) và năm timeline có key frame | Không có gameplay controls; không transcription audio | Dùng làm evidence UI, không dùng để suy nội bộ |
| Đối chiếu source Unity | Đủ | [Audit Unity và backlog](../implementation/unity-implementation-audit-and-backlog.md) | Chưa triển khai backlog | Thực hiện theo P0 → P1 → P2 |

<a id="evidence-quality"></a>

## 3. Kiểm tra chất lượng bằng chứng

- [x] Nguồn ưu tiên Garena, EA/EA Help, Google Play và luật giải chính thức.
- [x] Claim công khai có URL và ngày truy cập gần nội dung.
- [x] Phân biệt League bang hội, competition bóng đá, ranked season và esports.
- [x] Không gọi referee tuning là VAR.
- [x] Không khẳng định endpoint/player DB, AssetBundle/model pipeline hoặc thuật toán AI nội bộ.
- [x] Ghi rõ drift giữa hướng dẫn Rank Up và Patch Notes mới.
- [x] Proposal player catalog, model 3D, controls, VAR và AI được nhận diện là thiết kế Soccer Mobile Pro.
- [x] Video observation có timestamp và keyframe cho toàn bộ màn hình quan sát được.
- [x] Unity audit xác nhận trạng thái implementation và test.

<a id="priority-gaps"></a>

## 4. Khoảng trống ưu tiên

### P0 — cần trước khi chốt interface

1. Khóa account provider, age/consent và recovery policy cho bản phát hành Soccer Mobile Pro.
2. Khóa data authority và version contract cho player/club/competition/card instance.
3. Khóa action map có bóng/không bóng/set piece/goalkeeper cùng accessibility HUD.
4. Xác nhận rule engine deterministic và ranh giới VAR presentation.

### P1 — cần trước vertical slice meta

1. Chọn Addressables/catalog strategy và budget model 3D theo device tier.
2. Prototype transaction nâng cấp server-authoritative với idempotency/receipt.
3. Tạo scenario suite cho AI tactical shape, transition, marking và set piece.
4. Đưa bằng chứng video vào screen inventory và UX state matrix.

### P2 — cần trước live operation

1. License register theo territory/season/asset right.
2. League/social moderation, esports rules và dispute/audit flow.
3. Localization QA, commentary pack delivery và fallback.
4. Telemetry/experimentation policy tôn trọng privacy và competitive integrity.

<a id="acceptance"></a>

## 5. Điều kiện nghiệm thu tài liệu nghiên cứu

- Mọi hàng trong ma trận dẫn đến section cụ thể, nguồn hoặc nhãn khoảng trống.
- Không có claim nội bộ EA/Garena chỉ dựa trên UI, video hay license của người dùng.
- Link nội bộ và fragment ASCII hoạt động trong GitHub/VS Code preview.
- Video audit và Unity audit được liên kết bằng đường dẫn tương đối; không sao chép nội dung sang file này.
- Mỗi lần thay đổi nguồn phải cập nhật [sổ nguồn](./fc-mobile-vn-source-register.md) và ngày cutoff trong cùng change set.
