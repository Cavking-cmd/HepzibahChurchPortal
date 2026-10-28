# Reimbursements, budgets and financial reports

## Who does what

| Stage | Status | Who acts | Possible actions |
|---|---|---|---|
| Preparing | Draft | Member (owner) | Edit, delete, submit |
| 1 | Under Review | **Accounts Officer** or **Accounts Admin** | Officer: forward → Pending Approval (optional help step) · Admin: approve directly → Approved · Either: return for correction, reject |
| 2 | Pending Approval | **Accounts Admin** | Approve → Approved · Return for correction · Reject |
| 3 | Approved (not paid) | **Accounts Admin** | Confirm payment → Paid |
| — | Returned for Correction | Member (owner) | Edit and resubmit → Under Review |
| End | Paid / Rejected | — | None |

Rules enforced on the server (not just hidden buttons):
- Nobody can review, approve, return, reject or pay their **own** request.
- Only an Accounts Officer can forward a request; only an Accounts Admin can approve or confirm payment. The Accounts Admin does not have to wait for the officer.
- The person who forwarded a request cannot also approve it.
- Approval never marks a request as paid. Payment is a separate step, and can only be done once.
- Two people acting on the same request at once: the second gets "changed by someone else, refresh".
- Self-registered members must verify their email before they can submit.
- Every action is written to the request history and the audit log, with who, when, from/to status and notes.

## Physical items and the Inventory Officer

- Every line item is marked **Physical item** or **Service**. Serial numbers are optional and only for items that already have one.
- When a request is submitted, its physical items are set to *waiting for verification* and every Inventory Officer is emailed.
- On the Inventory dashboard (**Items to Verify**) the Inventory Officer either:
  - **Verifies** the item: confirms it was seen, fills in its inventory details, and it is added to Inventory. The Accounts Officer and Accounts Admin are emailed.
  - **Rejects** it with a reason. The Accounts Officer, Accounts Admin and the member are emailed.
- While any physical item is waiting or rejected, the Accounts Officer **cannot forward** the request (they can still return or reject it).
- The Accounts Admin can still approve, but must enter an **inventory override reason**, saved on the request, in the history and in the audit log.
- If a returned request is resubmitted, items that were not verified go back to the Inventory Officer. Unchanged verified items keep their result, so no duplicate inventory entry is made.

The Pastor login role is retired. Pastors are now name records used when creating services.

### Status numbers (stored in the database)

0 Draft · 1 Under Review (was "Pending") · 2 legacy minister-approved (moved to 1 at startup) · 3 Approved · 4 Paid · 5 Rejected · 6 Returned for Correction (was "Info Requested") · 7 Pending Approval

## Departments and Personal Use

- Each request is either **Department** (must point to an active department record) or **Personal Use** (no department, never charged to a department budget).
- Departments have a required **annual budget** (calendar year, Jan–Dec) and an optional **monthly guide**.
- Departments are never deleted; deactivate them instead. Inactive departments keep their history but don't appear on the form.
- Older requests with typed department names show as "not linked" until an Accounts Admin links them (Departments → Needs linking). Nothing is guessed.

## Budget formulas

A request is in exactly one bucket at a time, so nothing is counted twice.

| Bucket | Statuses | Amount | Counted in the period of |
|---|---|---|---|
| Paid | Paid | amount actually paid | payment date (entered at payment confirmation) |
| Committed | Approved | approved amount (else total) | approval date |
| Pending | Under Review, Pending Approval | requested total | first submission date |

- **Remaining** = budget − paid − committed. Pending is shown separately and **not** subtracted.
- **Utilisation** = (paid + committed) ÷ budget.
- **Projection for a request** = remaining − this request's amount, as if approved now.
- Rejected, returned and draft requests are never counted.

Alerts (calculated on the server): over annual budget, at or above the warning threshold (default 80%, `Budget:WarningThresholdPercent`), over the monthly guide, and pending requests ≥ 50% of what's left (`Budget:HighPendingSharePercent`).

**Over-budget policy:** the budget informs the decision and never blocks it. If approving would exceed the annual budget or monthly guide, the Accounts Admin must enter a justification. It is saved on the request, in the history and in the audit log.

## Reports (`/api/Reports/Finance/...`, Accounts Officer and Accounts Admin only)

- `summary`: paid, approved, requested and rejected in a date range; what is in each stage now; budget utilisation for active departments.
- `monthly`: paid (by payment date), approved, requested, awaiting payment at month end, change vs last month, average and largest payment, breakdown by department and Personal Use.
- `departments`: per department budget, paid, committed, pending, remaining, utilisation, counts, average, largest, rejected, and flags.
- `trends`: monthly paid, request counts, average size, year-to-date utilisation, and lines for the biggest departments.
- `workflow`: counts and amounts per status, items waiting with days in stage, stale items, and average processing times from the history.
- `forecast`: run-rate projections (only with ≥3 months and ≥3 payments), overrun if pending is approved, rising spend, unusually large payments. **Estimates only.**

Every report response includes a `definitions` list describing its calculations. CSV exports in the app are built from these same responses.
