-- ============================================================
-- Maono Editorial Ops — Row Level Security (PostgreSQL)
-- ============================================================
-- Apply this script AFTER running EF Core migrations.
-- This provides database-level tenant isolation as a safety net
-- on top of the EF Core Global Query Filters.
-- ============================================================

-- Enable RLS on all tenant-scoped tables
-- Each table that inherits from TenantEntity has a WorkspaceId column.

-- 1. Create application role (used by the API connection string)
-- DO NOT run as superuser in production; use a dedicated role.
-- CREATE ROLE maono_app LOGIN PASSWORD 'your_secure_password';

-- 2. Create the session variable function
CREATE OR REPLACE FUNCTION current_workspace_id()
RETURNS uuid AS $$
BEGIN
    RETURN NULLIF(current_setting('app.current_workspace_id', TRUE), '')::uuid;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE;

-- 3. Apply RLS policies to tenant-scoped tables
-- Template: for each table with WorkspaceId

DO $$
DECLARE
    tbl TEXT;
    tables TEXT[] := ARRAY[
        -- Clients
        '"ClientOrganizations"',
        '"ClientContacts"',
        '"BrandProfiles"',
        -- Campaigns
        '"Campaigns"',
        '"CampaignKpis"',
        '"CampaignTags"',
        '"CampaignClosureRecords"',
        -- Planning
        '"CalendarEntries"',
        '"ResourceCapacities"',
        '"Assignments"',
        '"SavedViews"',
        -- Content
        '"ContentItems"',
        '"ContentDependencies"',
        '"Briefs"',
        '"TaskChecklistItems"',
        '"Tags"',
        -- Assets
        '"Assets"',
        '"AssetVersions"',
        '"AssetPreviews"',
        '"AssetRestoreRecords"',
        -- Approval
        '"ApprovalCycles"',
        '"ApprovalDecisions"',
        '"ClientPortalSessions"',
        '"ContentMessages"',
        '"ContentAnnotations"',
        -- Publications
        '"Publications"',
        '"PublicationVariants"',
        '"PublicationAttempts"',
        '"SocialConnections"',
        '"PublicationLogs"',
        -- Performance
        '"PerformanceSnapshots"',
        '"CampaignPerformanceAggregates"',
        '"ReportExports"',
        -- Missions
        '"Missions"',
        '"MissionMembers"',
        '"MissionMilestones"',
        '"MissionDeliveries"',
        '"MissionTasks"',
        '"DeliveryNotes"',
        '"BillingRecords"',
        -- Notifications
        '"Notifications"',
        '"EscalationRules"',
        -- Audit
        '"AuditLogs"',
        '"WebhookSubscriptions"',
        -- Identity (tenant-scoped)
        '"ClientAccessTokens"'
    ];
BEGIN
    FOREACH tbl IN ARRAY tables
    LOOP
        -- Enable RLS
        EXECUTE format('ALTER TABLE %s ENABLE ROW LEVEL SECURITY', tbl);

        -- Drop existing policy if any
        EXECUTE format('DROP POLICY IF EXISTS tenant_isolation ON %s', tbl);

        -- Create isolation policy
        EXECUTE format(
            'CREATE POLICY tenant_isolation ON %s
             USING (
                current_workspace_id() IS NULL
                OR "WorkspaceId" = current_workspace_id()
             )
             WITH CHECK (
                "WorkspaceId" = current_workspace_id()
             )',
            tbl
        );

        RAISE NOTICE 'RLS enabled on %', tbl;
    END LOOP;
END $$;

-- 4. Grant usage to the application role
-- GRANT ALL ON ALL TABLES IN SCHEMA public TO maono_app;

-- ============================================================
-- USAGE: Before each request, set the session variable:
-- SET app.current_workspace_id = '<workspace-uuid>';
-- 
-- In EF Core, this is done via a connection interceptor
-- that sets the variable before each query.
-- ============================================================

-- 5. Indexes for performance (if not already created by EF migrations)
-- These ensure RLS filter scans are efficient.
-- Most are already created in EF Core Configurations.

-- Verify RLS is active:
-- SELECT tablename, rowsecurity FROM pg_tables WHERE schemaname = 'public' AND rowsecurity = true;
