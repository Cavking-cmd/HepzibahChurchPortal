-- Clean sheet: permanently deletes ALL reimbursement requests and everything attached
-- to them (items, receipts, history, their notifications and audit entries).
-- Users, roles, departments, budgets, pastors, services and inventory are NOT touched.
--
-- This cannot be undone. Take a backup first (Render dashboard > your database > Backups).
-- Run it against the production database, e.g.:
--   psql "<External Database URL from Render>" -f scripts/reset-reimbursements.sql
-- It runs inside one transaction: if anything fails, nothing is deleted.

BEGIN;

SELECT COUNT(*) AS requests_to_delete FROM "ReimbursementRequests";

DELETE FROM "Notifications" WHERE "ReimbursementRequestId" IS NOT NULL;
DELETE FROM "AuditLogs"      WHERE "EntityName" = 'ReimbursementRequest';
DELETE FROM "ReimbursementHistories";
DELETE FROM "ReimbursementReceipts";
DELETE FROM "ReimbursementItems";
DELETE FROM "ReimbursementRequests";

SELECT COUNT(*) AS requests_left FROM "ReimbursementRequests";

COMMIT;
