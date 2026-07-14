# KIỂM TOÁN PHẠM VI NGHIÊN CỨU FC MOBILE VN

## Mục đích

Tài liệu này kiểm tra mức độ bao phủ toàn bộ yêu cầu nghiên cứu ban đầu cho Soccer Mobile Pro. Nó không tuyên bố truy cập nội bộ FC Mobile VN/EA/Garena; mọi mục không có tài liệu công khai chính thức được đánh dấu là **đề xuất triển khai**.

## Ma trận bao phủ

| Yêu cầu | Tài liệu nguồn trong repo | Trạng thái | Bổ sung cần làm tiếp theo |
|---|---|---|---|
| Tính năng tổng thể | `FC-Mobile-VN-Research.md` §§1, 15 | Có | Theo dõi update game theo mùa |
| Layout UI/UX | `FC-Mobile-VN-Research.md` §2; `UX-Wireflows-States.md` | Có khung | Tạo wireframe/Figma từng màn hình |
| Chọn ngôn ngữ | `FC-Mobile-VN-Research.md` §3 | Có khung | Chốt localization pipeline và voice-pack vendor |
| Cài đặt | `FC-Mobile-VN-Research.md` §4 | Có khung | Viết schema settings + default theo device tier |
| Tài khoản | `FC-Mobile-VN-Research.md` §5 | Có khung | Chốt identity provider, recovery SLA, consent copy |
| Giải đấu | `FC-Mobile-VN-Research.md` §6; `Competitive-Integrity-Esports.md` | Có khung | Viết bracket/state machine/check-in policy |
| Câu lạc bộ & giải | `FC-Mobile-VN-Research.md` §§6, 16 | Có khung | Chốt licensing catalogue và asset acceptance checklist |
| Cầu thủ & player database | `FC-Mobile-VN-Research.md` §§7, 16, 17; `LiveData-Operations.md` | Có khung kỹ thuật | Chốt source license, API contract, CMS data dictionary |
| Model 3D | `FC-Mobile-VN-Research.md` §7 | Có khung kỹ thuật | Prototype DCC-to-engine, LOD/texture budget, legal likeness clearance |
| Skills/Play’s Styles | `FC-Mobile-VN-Research.md` §8 | Có khung | Chốt catalog, trigger conditions, balance spreadsheet |
| Nâng cấp thẻ | `FC-Mobile-VN-Research.md` §§9, 17; GDD §6 | Có khung | Chốt formula, economy sinks/sources, migration policy |
| Controls trái/phải | `FC-Mobile-VN-Research.md` §10; `Match-Systems-SetPieces-Controls.md` | Có khung | Usability playtest & input-latency benchmark |
| VAR | `FC-Mobile-VN-Research.md` §§11, 18; GDD §16 | Có khung | Prototype deterministic replay và decision logger |
| AI offline | `FC-Mobile-VN-Research.md` §§12, 18; GDD §17 | Có khung | AI prototype, telemetry schema, difficulty validation |

## Khoảng trống cần xử lý trước production

1. **Không có bằng chứng công khai về kiến trúc nội bộ FC Mobile VN:** không được reverse-engineer hoặc mô tả endpoint/API/asset bundles/AI training của EA-Garena như sự thật.
2. **Bản quyền dữ liệu và likeness:** trước khi ingest cầu thủ, CLB, áo đấu, logo hoặc scan gương mặt, cần có quyền sử dụng bằng văn bản và quy trình xóa/thu hồi asset.
3. **Luật loot-box và thanh toán:** cần công bố tỷ lệ nếu có random packs, age-gate, parental controls, refund policy và receipt reconciliation.
4. **Online fairness:** cần server authority, anti-cheat, reconnect, desync handling và replay/audit trail trước ranked launch.
5. **Data operations:** cần CMS, version manifest, approval workflow, rollback drill và on-call ownership trước khi live service.

## Definition of Done cho từng tính năng tham chiếu

Một tính năng chỉ được chuyển từ “nghiên cứu” sang “sẵn sàng sản xuất” khi có đủ: owner, user story, UX flow, data contract, server/client responsibilities, analytics events, abuse cases, accessibility checks, QA cases, rollback/fallback plan, và legal/privacy review.
