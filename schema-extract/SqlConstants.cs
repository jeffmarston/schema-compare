namespace eze.schema.extract
{
    public static class SqlConstants
    {
        public static readonly string GetObjects = @"
SELECT Name, Type,
CHECKSUM(OBJECT_DEFINITION(object_id)) as [Checksum], 
OBJECT_DEFINITION(object_id) as [Definition]
FROM sys.objects
WHERE type in ('P', 'TR', 'U', 'V')
";
        public static readonly string GetClientInfo = @"
SELECT 'Client' as Setting, Value FROM tc_settings WHERE userid = '*' and groupId='Global' AND itemID='Company'
UNION ALL
SELECT 'Entitled', CONVERT(varchar(200), COUNT(itemId)) FROM tc_settings WHERE userid = '*' and groupId='Database' AND itemID='EntitlementVersion'
UNION ALL 
SELECT 'Version', Value as DBVersion FROM tc_settings WHERE userid = '*' and groupId='Database' AND itemID='Version'         
";

        public static readonly string GetVersionDetails = @"
SELECT ProductName, Max(ReleaseVersion) as ReleaseVersion, count(distinct MachineName) as NumMachines
FROM EzeInstallHistory
WHERE ReleaseVersion = (SELECT TOP 1 ReleaseVersion FROM EzeInstallHistory ORDER BY Created DESC)
GROUP BY ProductName  
";

    }
}
