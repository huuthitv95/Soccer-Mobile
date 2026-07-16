# Kiểm toán độ phủ yêu cầu FC Mobile VN

> [Chỉ mục](../index.md) · [Nghiên cứu tổng hợp](fc-mobile-vn-research.md) · [Sổ nguồn](fc-mobile-vn-source-register.md)

## 0. Mục lục

- [1. Mục tiêu kiểm toán](#audit-purpose)
- [2. Ma trận độ phủ](#coverage-matrix)
- [3. Kiểm tra chất lượng bằng chứng](#evidence-quality)
- [4. Khoảng trống ưu tiên](#priority-gaps)
- [5. Điều kiện nghiệm thu](#acceptance)

<a id="audit-purpose"></a>

## 1. Mục tiêu kiểm toán

Ma trận này là authority về độ phủ, được đánh giá ngày **16/07/2026** trên mốc nghiên cứu **15/07/2026**. Ba cột được chấm độc lập: **Research** chỉ hỏi claim có nguồn hoặc kết luận “không công khai”; **Spec** hỏi contract có đủ state/authority/version/failure/QA/rollback; **Unity** chỉ phản ánh code hiện hữu. Một section tồn tại không tự động được tính hoàn thành. Trạng thái phê duyệt của toàn bộ quyết định mở do [chương trình kiểm chứng decision register](../implementation/decision-validation-program.md) quản lý; coverage hoàn chỉnh không đồng nghĩa decision đã `Approved`.

<a id="coverage-matrix"></a>

## 2. Ma trận độ phủ

| Yêu cầu | Research | Spec | Unity | Authority / evidence |
| --- | --- | --- | --- | --- |
| Tính năng tổng thể/core loop | Hoàn thành | Contract/guardrail hoàn chỉnh; tuning gated | Prototype một phần | [GDD](../product/gdd-soccer-mobile-pro.md), [nghiên cứu](fc-mobile-vn-research.md#features-and-modes) |
| Layout UI/UX | Hoàn thành visual-first | Coverage hoàn chỉnh; token/art decisions gated | Chưa có product shell | [Video synthesis](video/ui-pattern-synthesis.md), [UI catalogue](../product/ui-design-system-and-screen-catalogue.md) |
| Ngôn ngữ/cài đặt/tài khoản | Hoàn thành; provider VN còn decision | Contract hoàn chỉnh; provider/legal gated | P1-01 có locale `vi-VN`/`en`, typed registry, file persistence, N/N-1 và MainMenu panel; cloud/backend/voice/device/human gates còn mở | [Account spec](../systems/account-localization-and-settings.md), [evidence P1-01](../implementation/p1-localization-settings-implementation.md) |
| Giải đấu/CLB/League/social | Hoàn thành; license không được dùng để suy diễn kiến trúc FC Mobile | Contract hoàn chỉnh; policy/backend gated | P1-02 có fixture 2 giải/4 CLB; social/competition runtime và production dataset chưa có | [Competition spec](../systems/competitions-leagues-clubs-and-social.md), [evidence P1-02](../implementation/p1-football-catalog-and-model-foundation.md) |
| Cầu thủ/player database | Hoàn thành với kết luận endpoint không công khai | Contract proposal hoàn chỉnh; backend choice gated | P1-02 có entity, 44 cầu thủ hư cấu, snapshot/delta, atomic cache và validator; production API/dataset chưa có | [Catalog spec](../systems/football-catalog-player-database-and-model-assets.md), [evidence P1-02](../implementation/p1-football-catalog-and-model-foundation.md) |
| Model 3D | Hoàn thành với kết luận pipeline nội bộ không công khai | Pipeline proposal hoàn chỉnh; budgets gated | P1-02 có local Addressables resolver và generic fallback; chưa có model production hoặc device profiling | [Catalog/model spec](../systems/football-catalog-player-database-and-model-assets.md), [evidence P1-02](../implementation/p1-football-catalog-and-model-foundation.md) |
| Skills/PlayStyles/nâng cấp thẻ | Hoàn thành FC Mobile; đã bổ sung đối chiếu eFootball hiện hành/lịch sử | Contract hoàn chỉnh; balance/economy gated | P1-03 có owned item, deterministic preview, skill/position/fusion, atomic receipt/ledger và N/N-1 rollback sau diagnostic flag; chưa có backend/economy/UI production | [Cards/progression spec](../systems/player-cards-skills-progression-market-and-exchange.md), [evidence P1-03](../implementation/p1-player-items-skills-and-progression-implementation.md) |
| Controls có/không bóng, set piece | Hoàn thành ở mức public boundary/proposal | Contract hoàn chỉnh; layout/assist gated | P0-02 đã có typed action maps, adapter và test; legacy controller vẫn là mặc định runtime | [Controls spec](../systems/match-controls-set-pieces-and-var.md) |
| VAR | Hoàn thành với kết luận chưa xác nhận VAR tương tác | Presentation contract hoàn chỉnh; eligibility gated | Chưa triển khai | [Controls/VAR spec](../systems/match-controls-set-pieces-and-var.md) |
| AI offline | Hoàn thành với kết luận thuật toán không công khai | Contract/R&D governance hoàn chỉnh; policy gated | Heuristic prototype, không trained model | [AI spec](../systems/offline-ai-tactics-and-difficulty.md) |
| Report/integrity/esports | Hoàn thành, gồm S17 | Case/sanction/appeal contract hoàn chỉnh; policy gated | Chưa có service | [Integrity spec](../systems/competitive-integrity-and-esports.md) |
| Live data/assets/operations | Hoàn thành public boundary | Publish/incident contract hoàn chỉnh; SLA gated | Chưa triển khai | [Operations](../operations/live-data-and-operations.md) |
| Giftcode/liveops/monetization | Hoàn thành, gồm S18/S19 support context | Transaction contract hoàn chỉnh; commercial policy gated | Chưa triển khai | [LiveOps](../operations/liveops-monetization-and-membership.md) |
| Năm video | Hoàn thành visual-first | Chuyển hóa thành catalogue/state | Không phải implementation evidence | [Coverage video](video/ui-pattern-synthesis.md#evidence-coverage) |
| Audit Unity | Không áp dụng | Backlog handoff | P0-01/P0-02/P0-03, P1-01/P1-02/P1-03 foundation có contract/adapter/automated evidence; các product/device/backend gate vẫn mở | [Audit](../implementation/unity-implementation-audit-and-backlog.md) |

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

Lifecycle chuẩn là `Open → TestReady → InValidation → EvidenceReady → Approved | Rejected | Blocked`. Mọi mục dưới đây phải có evidence artifact và rollback trước khi phê duyệt.

### P0 — foundation đã hoàn tất, product gate còn mở

1. P0-01 đã tạo deterministic match seam và automated test; incident taxonomy, VAR eligibility và human/device evidence vẫn phải qua decision lifecycle.
2. P0-02 đã tạo typed action maps/adapter và rollback flag; HUD thật, controller reconnect, Android device matrix và playtest accessibility vẫn chưa qua gate.
3. P0-03 đã tạo account/settings/catalog contract, fake/offline adapter và contract test; provider, privacy/legal, secure storage và backend authority vẫn bị gate.
4. Không dùng trạng thái hoàn thành foundation để suy diễn rằng policy sản phẩm hoặc quyết định mở đã được phê duyệt.

### P1 — cần trước vertical slice meta

1. P1-01 đã triển khai foundation kỹ thuật cho locale bootstrap `vi-VN`/`en`, typed settings registry, file persistence, migration N/N-1 và fallback; đây chưa phải phê duyệt brand font, voice pack, accessibility trên thiết bị hoặc cloud policy production.
2. P1-02 đã triển khai catalog/local Addressables fixture và rehearsal rollback; bước còn lại là backend/CDN production, nhập dataset/model riêng và phê duyệt budget model 3D theo device tier.
3. P1-03 đã hoàn tất [deterministic player item/progression foundation](../implementation/p1-player-items-skills-and-progression-implementation.md): direct grant, lock invariant, preview/receipt, skill/position choice, fusion atomic và read-only rollback; chưa mở backend, economy production, pack/RNG/market hoặc device/human gate.
4. Tạo scenario suite cho AI tactical shape, transition, marking và set piece sau khi contract player attribute/catalog ổn định.

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
