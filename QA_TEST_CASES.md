# IMTS QA Supervision Test Cases

## How this document is used

The implementing developer/agent changes feature code. The QA supervisor does not modify that code. At each checkpoint, QA runs the gates below and records Pass, Fail, Blocked, or Not Run with evidence.

Severity:

- **P0**: cannot build, start, migrate, or safely continue.
- **P1**: security breach, data loss, cross-user access, or critical workflow failure.
- **P2**: major feature failure with a workaround or incorrect important behavior.
- **P3**: visual, accessibility, wording, or minor usability defect.

## Gate 0 - Repository integrity

| ID | Test | Steps | Expected result | Severity if failed |
|---|---|---|---|---|
| QA-GIT-001 | No unresolved merge entries | Run `git diff --name-only --diff-filter=U` | No output | P0 |
| QA-GIT-002 | No conflict markers | Run `rg -n "<<<<<<<|=======|>>>>>>>" Template.Common Template.Data Template.Core Template.Web` | No source-code matches | P0 |
| QA-GIT-003 | Clean patch formatting | Run `git diff --check` | No errors | P2 |
| QA-GIT-004 | Intentional file scope | Review `git status --short` | Every file is explained by the active task | P2 |
| QA-GIT-005 | No generated output committed | Check for `bin/`, `obj/`, logs, temporary uploads | None staged/tracked | P2 |

Stop immediately if QA-GIT-001 or QA-GIT-002 fails.

## Gate 1 - Build and startup

| ID | Test | Steps | Expected result | Severity |
|---|---|---|---|---|
| QA-BLD-001 | Restore | `dotnet restore Template.sln` | Restore succeeds | P0 |
| QA-BLD-002 | Compile | `dotnet build Template.sln --no-restore` | 0 errors; no new warnings | P0 |
| QA-BLD-003 | LocalDB | `sqllocaldb start MSSQLLocalDB` | Instance starts | P0 |
| QA-BLD-004 | Migration/startup | Start `Template.Web` in Development | Migrations/seeding complete; Kestrel listens | P0 |
| QA-BLD-005 | Anonymous protection | Request `/` without a cookie | 302 to `/Account/Login` | P1 |
| QA-BLD-006 | Static assets | Open page and inspect network/console | CSS/JS load; no blocking errors | P2 |

## Gate 2 - Authentication and role isolation

Use three separate browser sessions for `staff`, `innovation`, and `admin`.

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-AUTH-001 | Valid Staff login | Staff dashboard only | P1 |
| QA-AUTH-002 | Valid Innovation Team login | Innovation Team dashboard only | P1 |
| QA-AUTH-003 | Valid Admin login | Admin dashboard only | P1 |
| QA-AUTH-004 | Invalid password | Generic error; no username/password disclosure | P1 |
| QA-AUTH-005 | Logout | Cookie/session invalidated; protected page redirects to login | P1 |
| QA-AUTH-006 | Staff requests `/Category` | 403/Access Denied | P1 |
| QA-AUTH-007 | Staff requests Innovation review URL | 403/404 without data leakage | P1 |
| QA-AUTH-008 | Innovation Team requests account administration | 403 | P1 |
| QA-AUTH-009 | Admin requests business-review action without that role | Denied by defined policy | P1 |

## Gate 3 - Dashboards

| ID | Scenario | Expected result |
|---|---|---|
| QA-DSH-001 | Staff dashboard counts | Match only authenticated Staff user's records |
| QA-DSH-002 | Staff recent ideas | Maximum five; no other user's ideas |
| QA-DSH-003 | Staff notifications | Maximum five; only current user |
| QA-DSH-004 | Innovation dashboard queue | Active, non-deleted, non-retracted ideas only |
| QA-DSH-005 | Deadline list | Dates and overdue labels match `IdeaTimeline` |
| QA-DSH-006 | Admin user counts | Match active/disabled/locked database states |
| QA-DSH-007 | Mutually exclusive UI | Exactly one role dashboard renders |

## Gate 4 - Staff idea lifecycle

| ID | Scenario | Steps | Expected result | Severity |
|---|---|---|---|---|
| QA-IDEA-001 | Valid submission | Complete required fields and submit | One idea, initial timeline, notification, audit record | P1 |
| QA-IDEA-002 | Missing required field | Leave each required field blank | Form rejected with field message | P2 |
| QA-IDEA-003 | Active category only | Open category choices | Inactive categories absent | P2 |
| QA-IDEA-004 | Valid attachment | Upload allowed file under 10 MB | Stored outside web root and downloadable | P1 |
| QA-IDEA-005 | Oversized attachment | Upload file over limit | Rejected; no orphan DB/file record | P1 |
| QA-IDEA-006 | Disallowed file | Upload executable/renamed file | Rejected | P1 |
| QA-IDEA-007 | Duplicate refresh | Refresh after successful submission | No duplicate idea | P1 |
| QA-IDEA-008 | Ownership list | Compare two Staff accounts | Each sees only own ideas | P1 |
| QA-IDEA-009 | Ownership details | Staff A requests Staff B's ID | 404/403; no data displayed | P1 |
| QA-IDEA-010 | Ownership attachment | Staff A requests Staff B's attachment ID | 404/403; no download | P1 |
| QA-IDEA-011 | Comment | Owner posts non-empty comment | Comment saved once and displayed safely | P2 |
| QA-IDEA-012 | Empty comment | Submit whitespace | Rejected with message | P3 |
| QA-IDEA-013 | Retract allowed idea | Submit retract POST | Idea removed from active review; history retained | P2 |
| QA-IDEA-014 | Retract prohibited idea | Retract approved/closed idea | Server rejects operation | P1 |
| QA-IDEA-015 | Cancel precondition | Cancel non-retracted idea | Server rejects operation | P1 |

## Gate 5 - Innovation Team review

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-REV-001 | Search/filter | Results match title/reference/status/category/department/date | P2 |
| QA-REV-002 | Review details | Submitter, idea fields, and attachments are accurate | P2 |
| QA-REV-003 | Valid transition | Stage/status, history, timeline, audit, notification update atomically | P1 |
| QA-REV-004 | Invalid transition | Rejected without partial writes | P1 |
| QA-REV-005 | Decline reason | Required and stored for decline | P2 |
| QA-REV-006 | Reviewer assignment | Authorized reviewer stored and visible | P2 |
| QA-REV-007 | Public comment | Staff receives and can view it | P2 |
| QA-REV-008 | Internal comment | Never visible to Staff | P1 |
| QA-REV-009 | Concurrency | Two reviewers save same original version | Second receives conflict; first is not overwritten | P1 |
| QA-REV-010 | Deadline default | New stage receives configured SRS duration | P2 |

## Gate 6 - Categories

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-CAT-001 | List | Paginated categories load | P2 |
| QA-CAT-002 | Create | Valid unique category persists | P2 |
| QA-CAT-003 | Required name | Blank name rejected | P2 |
| QA-CAT-004 | Duplicate exact name | Rejected | P2 |
| QA-CAT-005 | Duplicate case variant | Rejected (`Process` vs `process`) | P2 |
| QA-CAT-006 | Edit | Name/description/status persist | P2 |
| QA-CAT-007 | Remove unused | Category is removed according to policy | P2 |
| QA-CAT-008 | Remove used | Category is deactivated; old ideas retain category | P1 |
| QA-CAT-009 | Staff direct POST | 403; database unchanged | P1 |
| QA-CAT-010 | Filter preservation | Search/status/page links preserve filters | P3 |

## Gate 7 - Notifications

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-NOT-001 | User isolation | Only current user's notifications returned | P1 |
| QA-NOT-002 | Read one | Only selected owned notification changes | P1 |
| QA-NOT-003 | Read foreign ID | 404/403; foreign record unchanged | P1 |
| QA-NOT-004 | Mark all | Only current user's unread records change | P1 |
| QA-NOT-005 | Filters | All/read/unread lists and counts agree | P2 |
| QA-NOT-006 | Reminder idempotency | Run reminder job twice | No duplicate reminder | P2 |

## Gate 8 - Resources and files

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-RES-001 | Authorized upload | Metadata and file saved atomically | P1 |
| QA-RES-002 | Unauthorized upload | 403; no file created | P1 |
| QA-RES-003 | Download active resource | Correct content type and original download name | P2 |
| QA-RES-004 | Path traversal | Crafted `../` path/ID request | 404; no server path disclosure | P1 |
| QA-RES-005 | Retired resource | Hidden from normal Staff listing/download | P2 |

## Gate 9 - Reports

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-RPT-001 | Filtered HTML report | Results match database query | P2 |
| QA-RPT-002 | Excel parity | Same rows/totals as UI filters | P2 |
| QA-RPT-003 | PDF parity | Same rows/totals; readable pages | P2 |
| QA-RPT-004 | Activity performer | Comes from history/audit, not current user | P1 |
| QA-RPT-005 | Unauthorized export | 403 | P1 |
| QA-RPT-006 | Empty report | Valid empty output, no exception | P3 |

## Gate 10 - Admin and audit

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-ADM-001 | Create user | User created with intended role and secure initial state | P1 |
| QA-ADM-002 | Reset password | Old password invalid; reset event audited | P1 |
| QA-ADM-003 | Lock/unlock | Login behavior follows account state | P1 |
| QA-ADM-004 | Enable/disable | Disabled account cannot authenticate | P1 |
| QA-ADM-005 | Final admin | Cannot disable/remove final active Admin | P1 |
| QA-ADM-006 | Role change | Takes effect and is audited | P1 |
| QA-AUD-001 | Login success/failure/logout | Each event recorded exactly once | P1 |
| QA-AUD-002 | Business changes | Submission/edit/review/admin actions recorded | P1 |
| QA-AUD-003 | Audit access | Admin only | P1 |
| QA-AUD-004 | Audit immutability | No normal edit/delete endpoint | P1 |

## Gate 11 - UI, offline, and accessibility

| ID | Scenario | Expected result | Severity |
|---|---|---|---|
| QA-UI-001 | Shared layout | No nested `<html>`, `<head>`, `<body>` in MVC views | P2 |
| QA-UI-002 | MVC routing | No active MVC view uses `@page`/`asp-page-handler` | P2 |
| QA-UI-003 | Responsive | Usable at 360, 768, 1024, and desktop widths | P3 |
| QA-UI-004 | Keyboard | Forms, menus, modal/dialog controls keyboard reachable | P3 |
| QA-UI-005 | Labels/errors | Inputs have labels; errors identify field/problem | P3 |
| QA-UI-006 | Offline assets | Active pages do not depend on unavailable public CDNs | P2 |
| QA-UI-007 | Empty states | Lists show useful empty message, not blank/broken table | P3 |
| QA-UI-008 | XSS output | Script-like titles/comments render as text, not execute | P1 |

## Checkpoint report template

```text
Checkpoint:
Commit/diff tested:
Environment:

Gate results:
- Repository integrity:
- Build/startup:
- Authentication/authorization:
- Feature tests:
- UI/accessibility:

Defects:
- ID:
- Severity:
- Exact steps:
- Expected:
- Actual:
- Evidence/log:
- Suspected file (if known):

Decision: PASS / FAIL / BLOCKED
Required action before next checkpoint:
```

## Current baseline result

- Decision: **BLOCKED**
- Severity: **P0**
- Cause: unresolved merge entries and conflict markers exist in database entities, DbContext, seeding, `Program.cs`, controllers, and views.
- Required action: implementing agent resolves the merge, then requests a new QA checkpoint beginning at Gate 0.
