# Soccer Mobile Pro — chỉ mục tài liệu

> Cập nhật: 16/07/2026 · Mốc FC Mobile VN: 15/07/2026 · Mốc eFootball Mobile: 16/07/2026 · Ngôn ngữ chuẩn: tiếng Việt · [Mở thư mục dự án](../README.md)

Đây là cổng vào duy nhất của knowledge wiki. Tài liệu được chia theo domain để mỗi quyết định chỉ có một nơi chịu trách nhiệm, còn các trang khác dùng liên kết tương đối thay vì sao chép nội dung.

## 1. Quy ước bằng chứng

| Nhãn | Ý nghĩa |
| --- | --- |
| **Thông tin công khai đã xác minh** | Có nguồn chính thức hoặc nguồn công khai đáng tin cậy, kèm URL và ngày truy cập. |
| **Quan sát từ video** | Nhìn thấy trực tiếp trong video người dùng cung cấp, kèm tên video và timestamp. |
| **Suy luận thiết kế** | Diễn giải hợp lý từ bằng chứng nhưng không phải xác nhận kiến trúc nội bộ. |
| **Đề xuất cho Soccer Mobile Pro** | Quyết định hoặc hướng triển khai của dự án, không được gán cho EA/Garena. |

## 2. Product

- [Game Design Document](product/gdd-soccer-mobile-pro.md)
- [UX wireflows và trạng thái UI](product/ux-wireflows-and-states.md)
- [UI design system và screen catalogue](product/ui-design-system-and-screen-catalogue.md)

## 3. Research

- [Nghiên cứu FC Mobile VN](research/fc-mobile-vn-research.md)
- [Sổ nguồn FC Mobile VN](research/fc-mobile-vn-source-register.md)
- [Kiểm toán độ bao phủ](research/fc-mobile-vn-coverage-audit.md)
- eFootball Mobile: [nghiên cứu](research/efootball-mobile/efootball-mobile-research.md), [sổ nguồn](research/efootball-mobile/efootball-mobile-source-register.md), [inventory cơ chế](research/efootball-mobile/efootball-mobile-feature-mechanic-inventory.md), [risk controls](research/efootball-mobile/efootball-mobile-risk-control-matrix.md), [adoption decisions](research/efootball-mobile/efootball-mobile-adoption-decision-matrix.md)
- [Tổng hợp pattern UI từ video](research/video/ui-pattern-synthesis.md)
- Phân tích từng video: [login/account](research/video/01-login-account.md), [Home/LiveOps](research/video/02-home-liveops.md), [profile/settings](research/video/03-profile-settings.md), [club/customization](research/video/04-club-squad-customization.md), [exchange/events](research/video/05-exchange-missions-events.md)

## 4. Systems

- [Account, localization và settings](systems/account-localization-and-settings.md)
- [Competitions, leagues, clubs và social](systems/competitions-leagues-clubs-and-social.md)
- [Football catalog, player database và model assets](systems/football-catalog-player-database-and-model-assets.md)
- [Player cards, skills, progression, market và exchange](systems/player-cards-skills-progression-market-and-exchange.md)
- [Offline AI, tactics và difficulty](systems/offline-ai-tactics-and-difficulty.md)
- [Match controls, set pieces và VAR](systems/match-controls-set-pieces-and-var.md)
- [Competitive integrity và esports](systems/competitive-integrity-and-esports.md)

## 5. Operations

- [Live data và operations](operations/live-data-and-operations.md)
- [LiveOps, monetization và membership](operations/liveops-monetization-and-membership.md)

## 6. Implementation

- [Audit Unity và backlog triển khai](implementation/unity-implementation-audit-and-backlog.md)
- [Chương trình kiểm chứng decision register](implementation/decision-validation-program.md)
- [Nhật ký P0 foundation](implementation/p0-foundation-implementation.md)
- [Nhật ký P1-01 localization và settings](implementation/p1-localization-settings-implementation.md)
- [Nhật ký P1-02 football catalog và model foundation](implementation/p1-football-catalog-and-model-foundation.md)
- [Kế hoạch P1-03 player items, skills và progression foundation](implementation/p1-player-items-skills-and-progression-plan.md)
- [Nhật ký P1-03 player items, skills và progression](implementation/p1-player-items-skills-and-progression-implementation.md)
- [Sổ migration tên và cấu trúc Unity asset](implementation/asset-naming-migration-register.md)

## 7. Bản đồ phụ thuộc

```text
Nguồn chính thức + video
          ↓
Nghiên cứu + coverage audit
          ↓
GDD + UX + system specs
          ↓
Unity implementation audit + backlog
          ↓
Knowledge graph và dashboard
```

## 8. Điều hướng và đóng góp

- Mỗi file chỉ có một tiêu đề H1; các mục H2 dùng số thứ tự.
- Liên kết nội bộ phải là Markdown tương đối và trỏ tới file hoặc anchor tồn tại.
- Claim nghiên cứu phải dùng một trong bốn nhãn ở mục 1.
- Spec triển khai phải có input/output, trách nhiệm client/server, analytics, accessibility, QA và rollback.
- Graph canonical nằm tại [`.understand-anything/knowledge-graph.json`](../.understand-anything/knowledge-graph.json); không tạo graph lồng trong `docs/`.
