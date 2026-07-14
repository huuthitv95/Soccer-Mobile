# MATCH SYSTEMS — SET PIECES & CONTROLS

## Input Matrix
| Trạng thái | Cụm trái | Cụm phải |
|---|---|---|
| Có bóng | Joystick/floating stick | Pass, Through, Shoot, Sprint & Skill; contextual Cross/Lob/Clear |
| Không bóng | Joystick/floating stick | Switch, Press/Sprint, Tackle, Slide, Match-Up/Jockey |
| Thủ môn | Joystick + hướng nhảy | Rush, Dive, Catch/Parry, pass/throw distribution |
| Bóng chết | Aim/curl/target selector | Power, pass variant, runner trigger, defensive assignment |

## Control Rules
- Context remap giữ vị trí nút ổn định, chỉ đổi nhãn/hành động theo tình huống.
- Có Classic, Gesture và Advanced; HUD đổi cỡ/vị trí/opacity; có input buffering ngắn và tutorial overlay.
- Assist có 4 mức: Beginner, Assisted, Semi, Manual; mọi ranked competitive queue phải công bố assist policy.

## Set Pieces
- Corner: chọn người đá, target zone, power/curl, gọi người chạy cột gần/xa; phòng ngự phân kèm người/khu vực và người phá bóng.
- Free kick: direct/cross/short routine; target, curve, power, wall positioning.
- Penalty: aim, power, stutter policy, GK direction/commit timing; shoot-out state machine đầy đủ.
- Throw-in/goal kick: lựa chọn nhanh, timeout, chống câu giờ; AI positioning theo tactical profile.

## VAR Presentation
- Trigger: offside sát nút, goal line, penalty, red card; luật engine quyết định, replay chỉ minh họa.
- Có skip setting, không lạm dụng ở ranked, record decision reason trong match log.

## QA Metrics
- Test accidental-tap, command latency, mirrored handedness, 30/60 FPS, screen sizes, touch accessibility và input fairness theo assist level.
