# Competitions, leagues, clubs và social

> [Chỉ mục](../index.md) · [Nghiên cứu](../research/fc-mobile-vn-research.md) · [Integrity/esports](competitive-integrity-and-esports.md)

## 0. Mục lục

- [1. Mục tiêu và thuật ngữ](#goal)
- [2. Entities và input/output](#entities)
- [3. State machines](#states)
- [4. Authority, version và migration](#authority)
- [5. Failure, abuse và accessibility](#failure)
- [6. Analytics, QA và rollback](#quality)
- [7. Decision register](#decisions)

<a id="goal"></a>

## 1. Mục tiêu và thuật ngữ

Mục tiêu: tách rõ `Competition` bóng đá, `LeagueGroup` cộng đồng, ranked `Season` và esports `Tournament`; hỗ trợ club/roster có hiệu lực theo mùa và social an toàn. Non-goal: suy toàn bộ license từ một event UCL hoặc gọi League bang hội là giải bóng đá.

<a id="entities"></a>

## 2. Entities và input/output

```text
Competition { id, sportRulesetId, territory, rightsScope, status }
Season { id, competitionId, startsAt, endsAt, rulesVersion }
Club { id, canonicalName, crestAssetId, venueId, rightsVersion }
TeamSeason { id, clubId, seasonId, kitIds, rosterRevision }
LeagueGroup { id, name, ownerId, visibility, memberCap, moderationState }
Tournament { id, rulesVersion, eligibility, bracketVersion, state }
```

`RosterRegistration` liên kết player identity với TeamSeason và effective interval; không ghi đè lịch sử. Input: catalog/rights, membership command, squad/result, moderation signal. Output: projection season/club/standings/group/bracket hoặc receipt/reason code. Client chỉ request/join/display; server xác nhận membership, standings, tournament result và reward.

<a id="states"></a>

## 3. State machines

- Season: `Draft → Scheduled → Active → Locked → Archived`; correction tạo revision mới và audit, không sửa lịch sử im lặng.
- LeagueGroup: `Created → Active → Restricted → Suspended → Archived`; membership `Invited/Requested → Active → Left/Kicked/Banned`.
- Tournament: `Draft → Registration → CheckIn → Running → Verification → Complete/Cancelled`; bracket/result update có revision và dispute window.
- Club Challenge eligibility: catalog rights + season active + roster rule + squad validation; failure nêu item nào vi phạm.

<a id="authority"></a>

## 4. Authority, version và migration

Catalog sports/club/rights version riêng gameplay ratings. Server time quyết định season/check-in; standings/event sourcing giữ correction. Client cache read-only theo ETag/version. Rights revocation có effective time, replacement/fallback asset và không xóa lịch sử giao dịch hợp lệ. Schema migration giữ canonical ID; alias chỉ dành search/display.

<a id="failure"></a>

## 5. Failure, abuse và accessibility

| Tình huống | Xử lý | Abuse control |
| --- | --- | --- |
| Roster/rights hết hiệu lực | Chặn mode mới, giải thích và gợi ý sửa squad | Signed catalog/effective time |
| Join/leave đồng thời | Compare-and-swap membership revision | Cooldown/rate limit |
| Tên/crest vi phạm | Quarantine + generic fallback + appeal | Filter + human moderation |
| Result/bracket tới trễ | Pending verification, không grant sớm | Signed match result/idempotency |
| Leaderboard correction | Hiện revision/reason; reconcile reward | Audit + anomaly detection |
| Harassment/spam | Mute/block/report; preserve scoped evidence | Rate limit, trust/risk tier |

Screen reader đọc tên, thứ hạng và trạng thái; bracket có list alternative; crest/màu không là tín hiệu duy nhất; text scale và timezone locale. Social mặc định privacy-safe, block có hiệu lực ngay trên client projection và server.

<a id="quality"></a>

## 6. Analytics, QA và rollback

Event: `competition_viewed`, `club_selected`, `group_join_result`, `membership_changed`, `tournament_registered`, `checkin_result`, `bracket_updated`, `result_verified`, `social_reported`. Không log chat/PII vào analytics chung.

QA: season boundary/timezone, rights expiry, roster transfer, duplicate join, owner leaves, member cap, block/report, bracket correction, reconnect, cancel/compensation. Acceptance: không lẫn loại League/Competition, mọi result/reward audit được, group không vượt cap, revoked asset có fallback và moderation có acknowledgement. Rollback bằng catalog pointer, tournament pause/cancel policy, feature flag social surface; không xóa sanction/evidence hoặc reward đã grant hợp lệ.

<a id="decisions"></a>

## 7. Decision register

Trạng thái/evidence chi tiết nằm tại [chương trình kiểm chứng](../implementation/decision-validation-program.md#decision-matrix).

| ID | Quyết định | Owner | Gate | Trạng thái |
| --- | --- | --- | --- | --- |
| CLS-D01 | Group cap/roles | Social + Backend | Load/moderation simulation | `TestReady` |
| CLS-D02 | Club Challenge roster policy | Game Design + Licensing | Rights + fairness review | `Blocked` |
| CLS-D03 | Tournament dispute window | Competitive Ops | Operational rehearsal/SLA | `TestReady` |
| CLS-D04 | Public profile defaults | Privacy + UX | Age/region review | `Blocked` |
