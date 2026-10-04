using DentalClinicSystem.Service;

namespace DentalClinicSystem.Helpers;

// Deliberately stores one username, only after authentication has succeeded.
public sealed class RememberedUsernameStore(string directory)
{
    private readonly string _file = Path.Combine(directory, "remembered-username.txt");
    public static RememberedUsernameStore ForCurrentUser() => new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DentalClinicSystem"));

    public string Read()
    {
        try
        {
            if (!File.Exists(_file) || new FileInfo(_file).Length > FieldLimits.Username * 4) return string.Empty;
            var username = File.ReadAllText(_file).Trim();
            return username.Length <= FieldLimits.Username ? username : string.Empty;
        }
        catch (IOException error) { AppLog.Write(error); return string.Empty; }
        catch (UnauthorizedAccessException error) { AppLog.Write(error); return string.Empty; }
    }

    public void SaveAfterSuccessfulLogin(string username, bool remember)
    {
        try
        {
            if (!remember) { File.Delete(_file); return; }
            Directory.CreateDirectory(directory);
            File.WriteAllText(_file, username.Trim());
        }
        catch (IOException error) { AppLog.Write(error); }
        catch (UnauthorizedAccessException error) { AppLog.Write(error); }
    }
}
