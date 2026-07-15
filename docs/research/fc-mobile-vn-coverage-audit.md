# Kiểm toán độ phủ yêu cầu FC Mobile VN

> [Chỉ mục](../index.md) · [Nghiên cứu tổng hợp](fc-mobile-vn-research.md) · [Sổ nguồn](fc-mobile-vn-source-register.md)

<a id="audit-purpose"></a>

## 1. Mục tiêu kiểm toán

Ma trận này là authority về độ phủ tại mốc **15/07/2026**. Ba cột được chấm độc lập: **Research** chỉ hỏi claim có nguồn hoặc kết luận “không công khai”; **Spec** hỏi contract có đủ state/authority/version/failure/QA/rollback; **Unity** chỉ phản ánh code hiện hữu. Một section tồn tại không tự động được tính hoàn thành.

<a id="coverage-matrix"></a>

## 2. Ma trận độ phủ

| Yêu cầu | Research | Spec | Unity | Authority / evidence |
| --- | --- | --- | --- | --- |
| Tính năng tổng thể/core loop | Hoàn thành | Contract/guardrail hoàn chỉnh; tuning gated | Prototype một phần | [GDD](../product/gdd-soccer-mobile-pro.md), [nghiên cứu](fc-mobile-vn-research.md#features-and-modes) |
| Layout UI/UX | Hoàn thành visual-first | Coverage hoàn chỉnh; token/art decisions gated | Chưa có product shell | [Video synthesis](video/ui-pattern-synthesis.md), [UI catalogue](../product/ui-design-system-and-screen-catalogue.md) |
| Ngôn ngữ/cài đặt/tài khoản | Hoàn thành; provider VN còn decision | Contract hoàn chỉnh; provider/legal gated | Chưa triển khai | [Account spec](../systems/account-localization-and-settings.md) |
| Giải đấu/CLB/League/social | Hoàn thành; license catalog không suy diễn | Contract hoàn chỉnh; policy/licensing gated | Chưa triển khai | [Competition spec](../systems/competitions-leagues-clubs-and-social.md) |
| Cầu thủ/player database | Hoàn thành với kết luận endpoint không công khai | Contract proposal hoàn chỉnh; backend choice gated | Chưa triển khai | [Catalog spec](../systems/football-catalog-player-database-and-model-assets.md) |
| Model 3D | Hoàn thành với kết luận pipeline nội bộ không công khai | Pipeline proposal hoàn chỉnh; budgets gated | Asset hiện tại không phải pipeline sản phẩm | [Catalog/model spec](../systems/football-catalog-player-database-and-model-assets.md) |
| Skills/PlayStyles/nâng cấp thẻ | Hoàn thành, ghi drift S08/S11 | Contract hoàn chỉnh; balance/economy gated | Chưa triển khai | [Cards/progression spec](../systems/player-cards-skills-progression-market-and-exchange.md) |
| Controls có/không bóng, set piece | Hoàn thành ở mức public boundary/proposal | Contract hoàn chỉnh; layout/assist gated | Legacy prototype một phần | [Controls spec](../systems/match-controls-set-pieces-and-var.md) |
| VAR | Hoàn thành với kết luận chưa xác nhận VAR tương tác | Presentation contract hoàn chỉnh; eligibility gated | Chưa triển khai | [Controls/VAR spec](../systems/match-controls-set-pieces-and-var.md) |
| AI offline | Hoàn thành với kết luận thuật toán không công khai | Contract/R&D governance hoàn chỉnh; policy gated | Heuristic prototype, không trained model | [AI spec](../systems/offline-ai-tactics-and-difficulty.md) |
| Report/integrity/esports | Hoàn thành, gồm S17 | Case/sanction/appeal contract hoàn chỉnh; policy gated | Chưa có service | [Integrity spec](../systems/competitive-integrity-and-esports.md) |
| Live data/assets/operations | Hoàn thành public boundary | Publish/incident contract hoàn chỉnh; SLA gated | Chưa triển khai | [Operations](../operations/live-data-and-operations.md) |
| Giftcode/liveops/monetization | Hoàn thành, gồm S18/S19 support context | Transaction contract hoàn chỉnh; commercial policy gated | Chưa triển khai | [LiveOps](../operations/liveops-monetization-and-membership.md) |
| Năm video | Hoàn thành visual-first | Chuyển hóa thành catalogue/state | Không phải implementation evidence | [Coverage video](video/ui-pattern-synthesis.md#evidence-coverage) |
| Audit Unity | Không áp dụng | Backlog handoff | Đã audit HEAD theo batch cuối | [Audit](../implementation/unity-implementation-audit-and-backlog.md) |

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
