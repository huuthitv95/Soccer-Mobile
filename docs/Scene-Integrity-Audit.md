# Kiểm tra tính toàn vẹn Unity Scene

## Mục tiêu

Đảm bảo mọi Scene trong `Assets/` tải được và không chứa Missing Script, object reference hỏng hoặc GUID asset không còn được Unity Asset Database phân giải.

## Kết quả ban đầu

Đợt kiểm tra ngày 2026-07-15 trên Unity 2022.3.62f3 đã mở đủ 14 Scene. Không Scene nào lỗi tải hoặc có Missing Script.

| Scene | Lỗi ban đầu | Xử lý |
|---|---:|---|
| `KickOffScene` | 34 object reference hỏng: 2 Avatar, 16 Mesh, 16 Material | Thay `Team1Player` và `Team2Player` bằng hai prefab kickoff hợp lệ, giữ nguyên tên và transform root |
| `MatchScene` | 12 controller, 12 clip và 12 array-size override trỏ tới asset đã mất | Xóa riêng 36 override cũ; giữ nguyên 24 Legacy Animation clip đang hoạt động |
| `_LifeBar` | 3 Mesh và 3 Material bị mất | Dùng built-in Quad và ba material URP Unlit mới cho fill, background và outer |

Mười một Scene còn lại không có lỗi reference tại thời điểm kiểm tra.

## Kiểm tra tự động

`SceneIntegrityTests` thực hiện các bước sau cho toàn bộ Scene dưới `Assets/`:

1. Đọc YAML và xác nhận mọi GUID ngoài nhóm built-in đều được Asset Database phân giải.
2. Mở lần lượt từng Scene và duyệt cả GameObject không active.
3. Báo lỗi khi component bị Missing Script hoặc serialized object reference có instance ID nhưng không còn object.
4. Khôi phục Scene setup ban đầu sau khi hoàn tất, kể cả khi test thất bại.

## Tiêu chí nghiệm thu

- 14/14 Scene tải thành công.
- Không có Missing Script, broken object reference hoặc missing GUID.
- EditMode test pass và Console không có Error/Exception liên quan đến project.
- `KickOffScene` hiển thị đủ hai cầu thủ; `MatchScene` giữ đủ 24 clip hiện hành; `_LifeBar` hiển thị đúng ba lớp màu.

## Rollback

Nếu thay đổi hình ảnh hoặc animation không đạt yêu cầu, revert commit sửa Scene để khôi phục đồng thời Scene YAML, material và test.
