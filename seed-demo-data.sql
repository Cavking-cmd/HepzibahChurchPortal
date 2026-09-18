-- Demo seed data for Church Portal (PostgreSQL)
-- Idempotent: each INSERT is guarded by WHERE NOT EXISTS.
-- Requires: pgcrypto for gen_random_uuid(); user id 99999999-9999-9999-9999-999999999999 (admin).

-- ==========================================================
-- 1) INVENTORY ITEMS (fills Description, SerialNumber, Custodian)
-- ==========================================================
INSERT INTO "InventoryItems"
  ("Id", "ItemName", "Description", "Category", "Quantity", "Location",
   "Condition", "PurchaseDate", "Value", "Custodian", "SerialNumber",
   "LastVerifiedDate", "IsDeleted", "CreatedDate")
SELECT gen_random_uuid(), 'Sound Mixer', 'WING personal mixing console with 48 channels',
  'Audio Equipment', 1, 'Technical', 0, TIMESTAMPTZ '2023-06-10 00:00:00', 3200,
  'Church Administration', 'WING-2023-0048', TIMESTAMPTZ '2026-08-15 00:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "InventoryItems" WHERE "ItemName" = 'Sound Mixer'
  AND "Description" = 'WING personal mixing console with 48 channels');

INSERT INTO "InventoryItems"
  ("Id", "ItemName", "Description", "Category", "Quantity", "Location",
   "Condition", "PurchaseDate", "Value", "Custodian", "SerialNumber",
   "LastVerifiedDate", "IsDeleted", "CreatedDate")
SELECT gen_random_uuid(), 'Stage Lights', 'Full-color LED stage lighting kit',
  'Lighting', 8, 'Main auditorium stage', 0, TIMESTAMPTZ '2022-11-01 00:00:00', 900,
  'Church Administration', 'LED-2022-0008', TIMESTAMPTZ '2026-08-20 00:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "InventoryItems" WHERE "ItemName" = 'Stage Lights'
  AND "Description" = 'Full-color LED stage lighting kit');

INSERT INTO "InventoryItems"
  ("Id", "ItemName", "Description", "Category", "Quantity", "Location",
   "Condition", "PurchaseDate", "Value", "Custodian", "SerialNumber",
   "LastVerifiedDate", "IsDeleted", "CreatedDate")
SELECT gen_random_uuid(), 'Projector', 'HD projector with ceiling mount and cables',
  'Electronics', 1, 'Main auditorium stage', 1, TIMESTAMPTZ '2019-03-15 00:00:00', 750,
  'Church Administration', 'PROJ-2019-0001', TIMESTAMPTZ '2026-07-01 00:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "InventoryItems" WHERE "ItemName" = 'Projector'
  AND "Description" = 'HD projector with ceiling mount and cables');

INSERT INTO "InventoryItems"
  ("Id", "ItemName", "Description", "Category", "Quantity", "Location",
   "Condition", "PurchaseDate", "Value", "Custodian", "SerialNumber",
   "LastVerifiedDate", "IsDeleted", "CreatedDate")
SELECT gen_random_uuid(), 'Amplifier', 'Power amplifier for main speaker system',
  'Audio Equipment', 2, 'Stage/right', 2, TIMESTAMPTZ '2016-09-20 00:00:00', 1100,
  'Church Administration', 'AMP-2016-0002', NULL, false, now()
WHERE NOT EXISTS (SELECT 1 FROM "InventoryItems" WHERE "ItemName" = 'Amplifier'
  AND "Description" = 'Power amplifier for main speaker system');

INSERT INTO "InventoryItems"
  ("Id", "ItemName", "Description", "Category", "Quantity", "Location",
   "Condition", "PurchaseDate", "Value", "Custodian", "SerialNumber",
   "LastVerifiedDate", "IsDeleted", "CreatedDate")
SELECT gen_random_uuid(), 'Cable Drum', 'Professional cable drum 25m with stage-grade XLR',
  'Audio Equipment', 6, 'Storage room', 0, TIMESTAMPTZ '2024-01-05 00:00:00', 120,
  'Church Administration', 'CBL-2024-0025', TIMESTAMPTZ '2026-08-25 00:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "InventoryItems" WHERE "ItemName" = 'Cable Drum'
  AND "Description" = 'Professional cable drum 25m with stage-grade XLR');

INSERT INTO "InventoryItems"
  ("Id", "ItemName", "Description", "Category", "Quantity", "Location",
   "Condition", "PurchaseDate", "Value", "Custodian", "SerialNumber",
   "LastVerifiedDate", "IsDeleted", "CreatedDate")
SELECT gen_random_uuid(), 'Piano', 'Digital stage piano, black, with stand',
  'Musical Instruments', 1, 'Main auditorium stage', 0, TIMESTAMPTZ '2021-05-18 00:00:00', 1500,
  'Church Administration', 'DP-2021-0001', TIMESTAMPTZ '2026-09-01 00:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "InventoryItems" WHERE "ItemName" = 'Piano'
  AND "Description" = 'Digital stage piano, black, with stand');

-- ==========================================================
-- 2) SERVICES - one per month for the last 3 months
-- ==========================================================
INSERT INTO "Services" ("Id", "Date", "Day", "ServiceType", "Theme",
  "ScriptureText", "Preacher", "OnlineAttendance", "IsDeleted", "CreatedDate")
SELECT '44440000-0000-0000-0000-000000000001', TIMESTAMPTZ '2026-07-05 09:00:00',
  'Sunday', 0, 'Walking in Newness of Life', 'Romans 6:4', 'PLA', 85, false, now()
WHERE NOT EXISTS (SELECT 1 FROM "Services" WHERE "Id" = '44440000-0000-0000-0000-000000000001');

INSERT INTO "Services" ("Id", "Date", "Day", "ServiceType", "Theme",
  "ScriptureText", "Preacher", "OnlineAttendance", "IsDeleted", "CreatedDate")
SELECT '44440000-0000-0000-0000-000000000002', TIMESTAMPTZ '2026-08-02 09:00:00',
  'Sunday', 0, 'The Power of Unity', 'Psalm 133:1', 'PLA', 95, false, now()
WHERE NOT EXISTS (SELECT 1 FROM "Services" WHERE "Id" = '44440000-0000-0000-0000-000000000002');

INSERT INTO "Services" ("Id", "Date", "Day", "ServiceType", "Theme",
  "ScriptureText", "Preacher", "OnlineAttendance", "IsDeleted", "CreatedDate")
SELECT '44440000-0000-0000-0000-000000000003', TIMESTAMPTZ '2026-09-06 09:00:00',
  'Sunday', 0, 'Faith That Moves Mountains', 'Matthew 17:20', 'PLA', 110, false, now()
WHERE NOT EXISTS (SELECT 1 FROM "Services" WHERE "Id" = '44440000-0000-0000-0000-000000000003');

-- ==========================================================
-- 3) ATTENDANCES linked to those services (Total = Men+Women+Children)
--    Approved+locked so they can be viewed and counted.
-- ==========================================================
INSERT INTO "Attendances" ("Id", "ServiceId", "Men", "Women", "Children",
  "SundaySchool", "NewConverts", "FirstTimers", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '55550000-0000-0000-0000-000000000001', '44440000-0000-0000-0000-000000000001',
  240, 210, 90, 150, 12, 8, 540, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-07-05 12:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "Attendances" WHERE "Id" = '55550000-0000-0000-0000-000000000001');

INSERT INTO "Attendances" ("Id", "ServiceId", "Men", "Women", "Children",
  "SundaySchool", "NewConverts", "FirstTimers", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '55550000-0000-0000-0000-000000000002', '44440000-0000-0000-0000-000000000002',
  260, 230, 100, 170, 15, 11, 590, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-08-02 12:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "Attendances" WHERE "Id" = '55550000-0000-0000-0000-000000000002');

INSERT INTO "Attendances" ("Id", "ServiceId", "Men", "Women", "Children",
  "SundaySchool", "NewConverts", "FirstTimers", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '55550000-0000-0000-0000-000000000003', '44440000-0000-0000-0000-000000000003',
  280, 245, 110, 185, 20, 14, 635, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-09-06 12:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "Attendances" WHERE "Id" = '55550000-0000-0000-0000-000000000003');

-- ==========================================================
-- 4) FELLOWSHIP ATTENDANCE - 3 months for the existing centers
--    Center 1 declines (100 -> 80 -> 60) to trigger the dashboard alert.
-- ==========================================================
INSERT INTO "FellowshipAttendances" ("Id", "FellowshipCenterId", "Date", "Men",
  "Women", "Children", "NewConverts", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '66660000-0000-0000-0000-000000000001', '535c47d9-ad0c-4335-a149-f8be7851fbe8',
  TIMESTAMPTZ '2026-07-12 18:00:00', 40, 35, 25, 2, 100, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-07-13 08:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "FellowshipAttendances" WHERE "Id" = '66660000-0000-0000-0000-000000000001');

INSERT INTO "FellowshipAttendances" ("Id", "FellowshipCenterId", "Date", "Men",
  "Women", "Children", "NewConverts", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '66660000-0000-0000-0000-000000000002', '535c47d9-ad0c-4335-a149-f8be7851fbe8',
  TIMESTAMPTZ '2026-08-09 18:00:00', 32, 28, 20, 1, 80, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-08-10 08:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "FellowshipAttendances" WHERE "Id" = '66660000-0000-0000-0000-000000000002');

INSERT INTO "FellowshipAttendances" ("Id", "FellowshipCenterId", "Date", "Men",
  "Women", "Children", "NewConverts", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '66660000-0000-0000-0000-000000000003', '535c47d9-ad0c-4335-a149-f8be7851fbe8',
  TIMESTAMPTZ '2026-09-06 18:00:00', 24, 21, 15, 1, 60, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-09-07 08:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "FellowshipAttendances" WHERE "Id" = '66660000-0000-0000-0000-000000000003');

-- Center 2 grows (50 -> 70 -> 90) - healthy, no alert.
INSERT INTO "FellowshipAttendances" ("Id", "FellowshipCenterId", "Date", "Men",
  "Women", "Children", "NewConverts", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '66660000-0000-0000-0000-000000000004', '232dfe86-43f8-4cad-b132-2493243cc345',
  TIMESTAMPTZ '2026-07-19 17:00:00', 20, 18, 12, 2, 50, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-07-20 08:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "FellowshipAttendances" WHERE "Id" = '66660000-0000-0000-0000-000000000004');

INSERT INTO "FellowshipAttendances" ("Id", "FellowshipCenterId", "Date", "Men",
  "Women", "Children", "NewConverts", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '66660000-0000-0000-0000-000000000005', '232dfe86-43f8-4cad-b132-2493243cc345',
  TIMESTAMPTZ '2026-08-16 17:00:00', 28, 25, 17, 3, 70, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-08-17 08:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "FellowshipAttendances" WHERE "Id" = '66660000-0000-0000-0000-000000000005');

INSERT INTO "FellowshipAttendances" ("Id", "FellowshipCenterId", "Date", "Men",
  "Women", "Children", "NewConverts", "Total", "IsApproved", "IsLocked",
  "ApprovedByUserId", "ApprovedDate", "IsDeleted", "CreatedDate")
SELECT '66660000-0000-0000-0000-000000000006', '232dfe86-43f8-4cad-b132-2493243cc345',
  TIMESTAMPTZ '2026-09-13 17:00:00', 36, 32, 22, 4, 90, true, true,
  '99999999-9999-9999-9999-999999999999', TIMESTAMPTZ '2026-09-14 08:00:00', false, now()
WHERE NOT EXISTS (SELECT 1 FROM "FellowshipAttendances" WHERE "Id" = '66660000-0000-0000-0000-000000000006');
