using System.Text.RegularExpressions;

namespace DentalClinicSystem.Tests;

// Static contracts catch C#/SQL drift; these tests do not execute MySQL.
public class DatabaseContractTests
{
    private static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DentalClinicSystem.slnx"))) directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Solution root not found.");
        }
    }
    private static string Read(string path) => File.ReadAllText(Path.Combine(Root, path));
    [Fact]
    public void EveryRepositoryCallMatchesItsProcedureAndParameters()
    {
        var sql = Read("Database/02_StoredProcedures.sql");
        var procedures = Regex.Matches(sql, @"CREATE PROCEDURE\s+(\w+)\s*\((.*?)\)\s*BEGIN", RegexOptions.Singleline)
            .ToDictionary(m => m.Groups[1].Value, m => Regex.Matches(m.Groups[2].Value, @"\bIN\s+(p_\w+)").Select(p => p.Groups[1].Value).Order().ToArray());
        var calls = 0;
        foreach (var file in Directory.GetFiles(Path.Combine(Root, "DentalClinicSystem/DBContent"), "MySql*Repository.cs"))
        foreach (Match call in Regex.Matches(File.ReadAllText(file), "(?:QueryAsync|QuerySingleAsync|ExecuteAsync)(?:<[^>]+>)?\\(\"(\\w+)\"(.*?);", RegexOptions.Singleline))
        {
            var name = call.Groups[1].Value; Assert.True(procedures.ContainsKey(name), name);
            var parameters = Regex.Matches(call.Groups[2].Value, "Parameter\\(\"@(p_\\w+)\"").Select(p => p.Groups[1].Value).Order().ToArray();
            Assert.Equal(procedures[name], parameters); calls++;
        }
        Assert.True(calls >= 48, $"Only found {calls} repository calls.");
    }
    [Fact]
    public void ReportsSumNetAndDentistCountUsesWaitingStatuses()
    {
        var sql = Read("Database/02_StoredProcedures.sql");
        foreach (var name in new[] { "RevenueByDay", "TopTreatmentTypes", "DentistWorkload" })
        {
            var body = Regex.Match(sql, $@"CREATE PROCEDURE sp_Report_{name}\(.*?END \$\$", RegexOptions.Singleline).Value;
            Assert.Contains("DiscountPercent / 100", body); Assert.Contains("AS Billed", body); Assert.DoesNotContain("AS Revenue", body);
        }
        var count = Regex.Match(sql, @"CREATE PROCEDURE sp_Appointment_CountUpcomingByDentist\(.*?END \$\$", RegexOptions.Singleline).Value;
        Assert.Contains("AppointmentDateTime >= p_From", count); Assert.Contains("Status IN ('Scheduled','CheckedIn')", count);
    }
    [Fact]
    public void MigrationHasMetadataGuardsAndNoDestructiveDataStatements()
    {
        var sql = Read("Database/04_Migration_RealWorldFixes.sql");
        Assert.Contains("information_schema.COLUMNS", sql); Assert.Contains("information_schema.TABLE_CONSTRAINTS", sql);
        Assert.DoesNotMatch(@"(?i)\b(?:DROP\s+TABLE|DELETE\s+FROM|TRUNCATE)\b", sql);
        Assert.Contains("WHERE DefaultDurationMinutes = 30", sql);
        foreach (var helper in new[] { "sp_Migrate_AddColumn", "sp_Migrate_AddCheck", "sp_Migrate_RealWorldFixes" }) Assert.Contains($"DROP PROCEDURE {helper}$$", sql);
    }
    [Fact]
    public void ApplicationPreservesLayeringAndStoredProcedureOnlyAccess()
    {
        foreach (var file in Directory.GetFiles(Path.Combine(Root, "DentalClinicSystem/Forms"), "*.cs"))
            Assert.DoesNotMatch(@"\b(?:DBContent|I\w+Repository|MySql\w+Repository)\b", File.ReadAllText(file));
        foreach (var file in Directory.GetFiles(Path.Combine(Root, "DentalClinicSystem/Service"), "*.cs"))
        {
            var source = File.ReadAllText(file); Assert.DoesNotMatch(@"DateTime\.(Now|Today)", source); Assert.DoesNotContain("MySqlCommand", source);
        }
        foreach (var file in Directory.GetFiles(Path.Combine(Root, "DentalClinicSystem/DBContent"), "*.cs"))
            Assert.DoesNotContain("CommandType.Text", File.ReadAllText(file));
        Assert.Contains("CommandType.StoredProcedure", Read("DentalClinicSystem/DBContent/StoredProcedureRunner.cs"));
    }
}
