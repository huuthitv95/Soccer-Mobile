# Sổ migration tên và cấu trúc Unity asset

> [Chỉ mục](../index.md) · [Audit Unity](unity-implementation-audit-and-backlog.md) · Cập nhật: 16/07/2026

## 0. Mục lục

- [1. Phạm vi và bất biến](#scope-invariants)
- [2. Cấu trúc đích](#target-layout)
- [3. Mapping có ngữ nghĩa](#semantic-mapping)
- [4. Mapping asset mơ hồ](#generic-mapping)
- [5. Serialization và compatibility](#serialization)
- [6. Gate nghiệm thu](#acceptance-gate)

<a id="scope-invariants"></a>

## 1. Phạm vi và bất biến

Sổ này là authority cho migration first-party asset từ cấu trúc legacy sang `Assets/SoccerMobilePro`. Vendor/generated roots `AddressableAssetsData`, `Plugins`, `Polybrush Data`, `TextMesh Pro`, `Standard Assets (Mobile)` và thư mục `.fbm` do importer tạo không bị chuẩn hóa tên nội bộ.

- GUID của mọi asset file tồn tại trước migration phải được giữ nguyên.
- Catalog ID, Addressable address, localization key, persistence key, analytics event và save schema không thay đổi.
- `Assets/Resources`, `Assets/StreamingAssets` và `Assets/csc.rsp` giữ nguyên special root.
- Folder GUID có thể thay đổi khi thay hierarchy; chúng không phải runtime identity. Mọi thay đổi folder nằm trong commit migration và có thể revert độc lập.

<a id="target-layout"></a>

## 2. Cấu trúc đích

```text
Assets/SoccerMobilePro/
  Art/{Characters,Stadiums,Environment,UI,Vfx}
  Animations/{Players,Goalkeepers,Celebrations,UI}
  Audio/{Music,Sfx,Ambience}
  Prefabs/{Characters,Gameplay,Environment,UI}
  Scenes/{Bootstrap,Menus,Match,Tests}
  Settings/{Rendering,Input,Localization}
  Runtime/{Catalog,CatalogUnity,Input,MatchCore,Platform,PlayerItems,PlayerItemsUnity,SettingsUI,Legacy}
  Editor
  Tests/{EditMode,PlayMode}
```

<a id="semantic-mapping"></a>

## 3. Mapping có ngữ nghĩa

| Source | Target | Identity |
| --- | --- | --- |
| `Assets/Scenes/SplashScene.unity` | `Assets/SoccerMobilePro/Scenes/Bootstrap/SplashScene.unity` | Giữ GUID |
| `Assets/Scenes/MainMenu.unity` | `Assets/SoccerMobilePro/Scenes/Menus/MainMenuScene.unity` | Giữ GUID |
| `Assets/Scenes/GameSelectionScene.unity` | `Assets/SoccerMobilePro/Scenes/Menus/GameModeSelectionScene.unity` | Giữ GUID |
| `Assets/Scenes/1stTeamSelection.unity` | `Assets/SoccerMobilePro/Scenes/Menus/HomeTeamSelectionScene.unity` | Giữ GUID |
| `Assets/Scenes/2ndTeamSelection.unity` | `Assets/SoccerMobilePro/Scenes/Menus/AwayTeamSelectionScene.unity` | Giữ GUID |
| `Assets/Scenes/GroupsScene.unity` | `Assets/SoccerMobilePro/Scenes/Menus/TournamentGroupsScene.unity` | Giữ GUID |
| `Assets/Scenes/MatchesScene.unity` | `Assets/SoccerMobilePro/Scenes/Menus/TournamentMatchesScene.unity` | Giữ GUID |
| `Assets/Scenes/FinalCeleberation.unity` | `Assets/SoccerMobilePro/Scenes/Menus/TournamentCelebrationScene.unity` | Giữ GUID |
| `Assets/Scenes/KickOffScene.unity` | `Assets/SoccerMobilePro/Scenes/Match/KickoffScene.unity` | Giữ GUID |
| `Assets/Scenes/MatchScene.unity` | `Assets/SoccerMobilePro/Scenes/Match/MatchScene.unity` | Giữ GUID |
| `Assets/Scenes/IntroScene.unity` | `Assets/SoccerMobilePro/Scenes/Match/MatchIntroScene.unity` | Giữ GUID |
| `Assets/testScene.unity` | `Assets/SoccerMobilePro/Scenes/Tests/LegacyGameplayTestScene001.unity` | Giữ GUID |
| `Assets/_LifeBar.unity` | `Assets/SoccerMobilePro/Scenes/Tests/LegacyHudTestScene002.unity` | Giữ GUID |
| `Assets/Scene/Test_Scene.unity` | `Assets/SoccerMobilePro/Scenes/Tests/LegacyGameplayTestScene003.unity` | Giữ GUID |
| `Assets/Scripts` | `Assets/SoccerMobilePro/Runtime/Legacy/Gameplay/LegacyScripts` | Giữ GUID con |
| `Assets/Standard Assets` | `Assets/SoccerMobilePro/Runtime/Legacy/Compatibility/StandardAssets` | Giữ GUID con |
| `Assets/Tests` | `Assets/SoccerMobilePro/Tests` | Giữ GUID con |

Prefab được đổi theo vai trò thành `AudienceBasePrefab`, `AudienceVariant001Prefab` đến `AudienceVariant005Prefab`, `AudioManagerPrefab`, `FireFlameVfxPrefab`, `FootballPrefab`, `OpponentKickoffPlayerPrefab`, `PlayerKickoffPlayerPrefab`, `CrowdGroupPrefab`, `ScreenTitlePrefab`, `ComputerPlayerPrefab` và `LegacyTouchJoystickPrefab`; mọi prefab giữ GUID cũ.

<a id="generic-mapping"></a>

## 4. Mapping asset mơ hồ

616 asset art/audio/animation/rendering có tên mơ hồ được cấp tên ổn định bằng thuật toán sau; source/target cụ thể là rename pair trong commit `refactor: standardize first-party Unity asset layout and names`.

1. Chọn leaf domain đích từ loại nội dung: `Characters`, `Environment`, `UI`, `Players`, `Music`, `Sfx` hoặc `Rendering`.
2. Nhóm theo `domain + asset type`; type gồm `Model`, `AnimationModel`, `Texture`, `Material`, `AnimationClip`, `AnimatorController`, `AudioClip`, `Video`, `Shader`, `Font`, `Settings`, `Data` và `Documentation`.
3. Sort tăng dần theo GUID cũ rồi cấp số ba chữ số, ví dụ `PlayerAnimationModel001.fbx`.
4. Collision được bỏ qua bằng cách tăng số; extension được chuẩn hóa lowercase. GUID và subasset identity không đổi.

Các asset có nghĩa rõ như `SplashScreen`, các scene, prefab và assembly definition không bị thay bằng tên generic. Folder `.fbm`, `Materials` do importer quản lý và tên bone/rig nội bộ được giữ nguyên.

<a id="serialization"></a>

## 5. Serialization và compatibility

- Legacy type chuyển vào namespace `SoccerMobilePro.Legacy.Gameplay`, `.Compatibility`, `.Input` hoặc `.TeamSelection` và có `MovedFrom` trỏ về namespace/assembly cũ.
- Field serialized đổi tên phải có `FormerlySerializedAs`; Unity message và production key giữ nguyên chữ ký/giá trị.
- Prefab legacy vẫn dùng serialized format cũ. Không bulk-save để ép nâng format vì Unity 2022 có thể đổi
  local file ID và làm mất scene override. Gate migration dùng load/inspect không save; chỉ save từng asset
  khi diff chứng minh toàn bộ object reference được bảo toàn.
- Inline `GUIText` được mở/lưu qua Unity trước; metadata còn lại được chuyển từ `Assembly-CSharp-firstpass` sang namespace compatibility trong `Assembly-CSharp`.
- Scene load dùng `SoccerMobilePro.Platform.SceneIds`; Build Settings trỏ đến path mới.

<a id="acceptance-gate"></a>

## 6. Gate nghiệm thu

- Không mất GUID asset file, không duplicate GUID và không collision target.
- 14 scene và 19 prefab load được, không có Missing Script; 44 field/array đã đổi tên resolve qua
  `FormerlySerializedAs` mà không làm scene dirty; material/texture/animation resolve.
- Build Settings, Addressables, Localization, Resources và StreamingAssets resolve.
- EditMode 83/83, PlayMode 18/18, Quick Match/Cup regression, console sạch và Android development smoke đạt hoặc blocker được ghi rõ.
- Có checkpoint commit riêng cho layout, symbol/serialization và tài liệu; rollback dùng revert checkpoint, không sửa snapshot production tại chỗ.

Evidence ngày 16/07/2026: GUID inventory giữ nguyên 1.082 meta asset, không duplicate; 14 scene và 19
prefab load không có Missing Script; 44 field/array migration giữ reference; EditMode 83/83 và PlayMode
18/18 đạt. Android smoke đã build xong Addressables/Bee nhưng Unity Editor dừng tiến triển trước bước tạo
APK; không còn build worker hoặc thay đổi `Library/Bee`, nên Editor được restart và mọi artifact sinh tạm
được loại bỏ. Gate Android vẫn mở, không được trình bày như đã đạt.
