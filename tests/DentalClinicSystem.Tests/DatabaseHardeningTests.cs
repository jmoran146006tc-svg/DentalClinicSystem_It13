using System.Text.RegularExpressions;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

// Source contracts only. These checks do not execute SQL or prove MySQL locking.
public sealed class DatabaseHardeningTests
{
    private static string Script(string name)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "Database", name);
            if (File.Exists(path)) return File.ReadAllText(path);
        }
        throw new FileNotFoundException(name);
    }
    [Fact]
    public void ReportsRetainTheirContractsAndUseTheUnroundedNetFunction()
    {
        var procedures = Script("02_StoredProcedures.sql"); var objects = Script("02b_FunctionsTriggersEvents.sql");
        Assert.Contains("CREATE PROCEDURE sp_Report_RevenueByDay(IN p_From DATE, IN p_To DATE)", procedures);
        Assert.Contains("CREATE PROCEDURE sp_Report_TopTreatmentTypes(IN p_From DATE, IN p_To DATE, IN p_Top INT)", procedures);
        Assert.Contains("CREATE PROCEDURE sp_Report_DentistWorkload(IN p_From DATE, IN p_To DATE)", procedures);
        Assert.Equal(3, Regex.Matches(procedures, @"SUM\(fn_TreatmentNet\(").Count);
        Assert.Contains("RETURNS DECIMAL(16,6) DETERMINISTIC NO SQL", objects);
    }
    [Fact]
    public void ObjectsAreReplaceableAndMissedAppointmentEventShipsDisabled()
    {
        var sql = Script("02b_FunctionsTriggersEvents.sql");
        foreach (Match create in Regex.Matches(sql, @"CREATE (FUNCTION|TRIGGER|EVENT) (\w+)"))
            Assert.Contains($"DROP {create.Groups[1].Value} IF EXISTS {create.Groups[2].Value}", sql[..create.Index]);
        Assert.Equal(3, Regex.Matches(sql, @"CREATE FUNCTION").Count);
        Assert.Equal(2, Regex.Matches(sql, @"SELECT DentistId INTO v_lock FROM Dentists WHERE DentistId = NEW.DentistId FOR UPDATE").Count);
        Assert.Contains("LIMIT 1 FOR SHARE", sql); Assert.Contains("OLD.AppointmentId)", sql);
        Assert.Matches(@"CREATE EVENT ev_flag_missed_appointments[\s\S]*?DISABLE[\s\S]*?Enable only after the clinic/adviser confirms this rule", sql);
        Assert.Contains("-- SET GLOBAL event_scheduler = ON;", sql);
    }
    [Fact]
    public void AuditKeepsOldAndNewValuesWithoutCredentialHashes()
    {
        var sql = Script("02b_FunctionsTriggersEvents.sql");
        Assert.DoesNotContain("PasswordHash", sql);
        foreach (var table in new[] { "Patients", "Treatments", "Appointments", "Users" })
            Assert.Contains($"CREATE TRIGGER trg_{table}_AU_Audit", sql);
        Assert.Contains("NOT (OLD.Role <=> NEW.Role)", sql); Assert.Contains("NOT (OLD.IsActive <=> NEW.IsActive)", sql);
        Assert.Contains("JSON_OBJECT", sql);
        foreach (var script in new[] { "01_Schema.sql", "04_Migration_RealWorldFixes.sql" })
            Assert.Contains("CREATE TABLE IF NOT EXISTS AuditLog", Script(script));
        var seedHash = Regex.Match(Script("03_SeedData.sql"), @"SELECT 'drsantos', '([^']+)'").Groups[1].Value;
        Assert.NotEmpty(seedHash); Assert.Contains(seedHash, Script("05_DemoData.sql"));
        Assert.DoesNotContain("RAND(", Script("05_DemoData.sql"));
    }
    [Fact]
    public void BaselineSeedUsesDatesRatherThanSessionStringCollations()
    {
        var sql = Script("03_SeedData.sql");
        var dateVariables = Regex.Matches(sql, @"@(week_start|previous_open_day|demo_day)\b");
        foreach (Match variable in dateVariables)
        {
            // Assignments become strings even if their input is a DATE. Every
            // subsequent use must restore the temporal type at the call site.
            if (sql[..variable.Index].EndsWith("SET ", StringComparison.Ordinal)) continue;
            Assert.EndsWith("CAST(", sql[..variable.Index]);
            Assert.StartsWith(" AS DATE)", sql[(variable.Index + variable.Length)..]);
        }
        Assert.Equal(14, dateVariables.Count); // Three assignments, eleven uses.
    }
    [Fact]
    public void BaselineSeedSlotsStayOpenAndNonOverlappingOnEveryWeekday()
    {
        var rows = Regex.Matches(Script("03_SeedData.sql"),
            @"SELECT (?<patient>@\w+), (?<dentist>@\w+), (?<start>.+?), (?<minutes>\d+), '(?<status>\w+)', 'Demo consultation', .+? FROM DUAL WHERE @seed_appointments;");
        Assert.Equal(10, rows.Count);
        for (var weekday = 0; weekday < 7; weekday++)
        {
            var today = new DateTime(2026, 10, 5).AddDays(weekday);
            var monday = today.AddDays(-weekday); var previous = today.AddDays(weekday == 0 ? -2 : -1);
            var visits = rows.Select(row =>
            {
                var expression = row.Groups["start"].Value;
                var offset = Regex.Match(expression, @"DATE_ADD\(CAST\(@week_start AS DATE\), INTERVAL (\d+) DAY\)");
                var day = expression.Contains("@demo_day") ? today.AddDays(weekday == 6 ? 1 : 0) : monday.AddDays(int.Parse(offset.Groups[1].Value));
                if (expression.Contains("LEAST")) day = new[] { day, expression.Contains("@previous_open_day") ? previous : today }.Min();
                var hour = int.Parse(Regex.Match(expression, @"INTERVAL (\d+) HOUR").Groups[1].Value);
                return (Dentist: row.Groups["dentist"].Value, Patient: row.Groups["patient"].Value,
                    Start: day.AddHours(hour), Minutes: int.Parse(row.Groups["minutes"].Value));
            }).ToArray();
            Assert.All(visits, v => Assert.True(ClinicRules.IsOpen(v.Start, v.Minutes)));
            for (var i = 0; i < visits.Length; i++)
            for (var j = i + 1; j < visits.Length; j++)
                if (visits[i].Dentist == visits[j].Dentist || visits[i].Patient == visits[j].Patient)
                    Assert.False(visits[i].Start < visits[j].Start.AddMinutes(visits[j].Minutes) && visits[j].Start < visits[i].Start.AddMinutes(visits[i].Minutes));
        }
    }
}
