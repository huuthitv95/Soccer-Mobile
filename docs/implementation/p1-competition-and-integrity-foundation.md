# P1-04 — Competition và integrity foundation

> [Chỉ mục](../index.md) · [Audit Unity](unity-implementation-audit-and-backlog.md) · [Competition spec](../systems/competitions-leagues-clubs-and-social.md) · [Integrity spec](../systems/competitive-integrity-and-esports.md)

## 0. Mục lục

- [1. Kết quả](#result)
- [2. Contract và authority](#contracts)
- [3. Luồng deterministic](#flow)
- [4. Automated evidence](#evidence)
- [5. Giới hạn và rủi ro mở](#limitations)
- [6. Rollback](#rollback)
- [7. Decision lifecycle](#decisions)

<a id="result"></a>

## 1. Kết quả

Ngày kiểm tra: 16/07/2026. Assembly thuần C# `SoccerMobilePro.Competition` đã cung cấp foundation cho Single Elimination và Round Robin, roster lock, kết quả có sequence/idempotency/payload hash, standing deterministic, reward-once qua gateway, reconnect timeline và dispute/correction append-only. Feature gate mặc định tắt; Cup offline không cấp reward online.

Đây là fake/local authority phục vụ contract test, không phải competition backend production. Legacy Cup scene vẫn dùng flow hiện hành và không bị Competition domain tiếp quản.

<a id="contracts"></a>

## 2. Contract và authority

| Nhóm | Contract | Authority hiện tại |
| --- | --- | --- |
| Rules/tournament | `CompetitionRules`, `TournamentDefinition`, `TournamentSnapshot` | Fake/file repository; rules, catalog và revision được pin theo snapshot |
| Roster | `RosterSubmission` | Domain kiểm participant, size, duplicate, catalog/rules/revision trước roster lock |
| Result | `MatchResultSubmission`, `AuthoritativeResultReceipt`, `IResultVerifier` | Fake verifier; reject stale, out-of-order, payload sai và idempotency conflict |
| Reward | `IRewardGrantGateway` | Gateway idempotent bằng reference ổn định; không tham chiếu implementation `PlayerItems` |
| Reconnect | `ReconnectTimeline`, `ReconnectTransition` | State machine append-only theo match/participant |
| Dispute | `DisputeRecord` | Submission/correction append-only; projection standing được rebuild |
| Persistence | `ICompetitionRepository`, `FileCompetitionRepository`, `CompetitionCodec` | Optimistic revision, atomic staging/replace, backup read-only và schema N/N-1 |

Client chỉ gửi intent/result candidate. Ranked eligibility, result, standing và reward phải thuộc server authority khi có backend thật. `MatchEventDigest` là seam để nối incident/event contract của Match Core; foundation chưa ký digest hoặc nhận network packet.

<a id="flow"></a>

## 3. Luồng deterministic

```text
TournamentDefinition + versioned rules/catalog
  -> roster validation và lock
  -> deterministic fixture generation
  -> result verifier
  -> sequence + idempotency + payload-hash gate
  -> bracket/standing projection
  -> stable reward reference qua idempotent gateway
  -> optimistic atomic snapshot commit
```

Single Elimination yêu cầu số participant là lũy thừa của hai trong fixture foundation; mỗi vòng kế tiếp chỉ được tạo sau khi cả cặp trận cha hoàn tất. Round Robin tạo mỗi cặp đúng một lần và sắp hạng theo `TieBreakOrder`, luôn có participant ID ordinal làm fallback ổn định.

<a id="evidence"></a>

## 4. Automated evidence

| Gate | Kết quả |
| --- | --- |
| EditMode | `102/102` đạt; P1-04 đóng góp 19 case |
| PlayMode | `19/19` đạt; case mới xác nhận feature gate mặc định tắt và Cup scene vẫn load |
| Format | Bracket Single Elimination, Round Robin và tie-break deterministic đạt |
| Result integrity | Stale rules/catalog/revision, out-of-order, replay và idempotency conflict đều không mutation ngoài ý muốn |
| Reward | Retry cùng payload trả receipt cũ; stable reward reference chỉ grant một lần; offline Cup không grant online |
| Recovery | Reconnect transition hợp lệ theo sequence; transition tắt bị reject |
| Dispute | Bản ghi gốc không bị sửa; correction append record mới và rebuild projection |
| Persistence | JSON unknown-field round-trip, schema N/N-1, atomic reload, corrupt-active fallback và read-only rollback đạt |
| Regression | Quick Match/Cup nằm trong PlayMode suite và tiếp tục xanh; Unity Console không có error sau compile/test |

Android development smoke chưa đạt: build trước đó hoàn tất Addressables/Bee nhưng Editor dừng tiến triển trước khi tạo APK. Đây là blocker tooling/device evidence đang mở, không được suy thành runtime pass.

<a id="limitations"></a>

## 5. Giới hạn và rủi ro mở

- Chưa có backend/network, signature cho result/event digest, identity provider hoặc anti-cheat signal.
- Chưa có packet-loss/device matrix, reconnect grace timer thực, forfeit policy hoặc operational tournament rehearsal.
- Correction sau reward cần compensating receipt trong transaction ledger production; foundation chỉ giữ audit append-only và không tự đảo reward.
- Chưa có tournament UI, season rollover, timezone/deadline, social/moderation hoặc sanction automation.
- Gateway idempotent phải được triển khai cùng transaction store phía server; fake gateway không chứng minh atomicity liên dịch vụ.
- Bracket foundation chưa hỗ trợ bye, double elimination, Swiss hoặc seeding policy production.

<a id="rollback"></a>

## 6. Rollback

`CompetitionFeatureOptions.Enabled` mặc định `false`; rollback client giữ legacy Quick Match/Cup. File store dùng last-known-good backup ở chế độ read-only và chỉ hỗ trợ schema hiện tại/N-1. Production rollback phải freeze giải, pin rules/catalog version, recompute từ event log và phát compensating receipt; không sửa/xóa receipt hoặc dispute lịch sử.

<a id="decisions"></a>

## 7. Decision lifecycle

| ID | Trạng thái mới | Evidence hiện có | Gate còn thiếu |
| --- | --- | --- | --- |
| `CIE-D03` | `InValidation` | Reconnect state machine, sequence/idempotency tests, Cup regression | Packet-loss/device matrix, grace/forfeit fairness và exploit rehearsal |
| `CLS-D03` | `InValidation` | Dispute/correction append-only, correlation ID và deterministic projection | Operational SLA, reviewer separation, support export và compensating reward rehearsal |

Không decision nào đạt `EvidenceReady` hoặc `Approved` trong batch này.
