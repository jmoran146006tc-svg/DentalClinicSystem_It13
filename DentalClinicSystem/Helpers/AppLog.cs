namespace DentalClinicSystem.Helpers;

public static class AppLog
{
    public static void Write(Exception error)
    {
        try
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DentalClinicSystem", "logs");
            Directory.CreateDirectory(directory);
            // Messages and arguments can contain credentials or SQL. Record only safe diagnostic metadata.
            File.AppendAllText(Path.Combine(directory, $"{DateTime.Today:yyyy-MM-dd}.log"), $"{DateTimeOffset.Now:O} {error.GetType().FullName} HResult={error.HResult:X8}{Environment.NewLine}");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
