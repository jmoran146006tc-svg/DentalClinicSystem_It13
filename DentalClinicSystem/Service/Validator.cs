using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DentalClinicSystem.Service
{
    public static class Validator
    {
        public static string Sanitize(string? value) => Regex.Replace(value?.Trim() ?? string.Empty, @"\s+", " ");
        public static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        public static ServiceResult Required(string? value, string field) =>
            string.IsNullOrWhiteSpace(value) ? ServiceResult.Fail($"{field} is required.") : ServiceResult.Ok();
        public static ServiceResult MaxLength(string? value, int maximum, string field) =>
            value?.Length > maximum ? ServiceResult.Fail($"{field} must be at most {maximum} characters.") : ServiceResult.Ok();
        public static ServiceResult Name(string value, string field) => First(
            Required(value, field), MaxLength(value, FieldLimits.Name, field),
            value.All(c => char.IsLetter(c) || " -'.".Contains(c))
                ? ServiceResult.Ok() : ServiceResult.Fail($"{field} may contain letters, spaces, hyphens, apostrophes and periods."));
        public static ServiceResult Phone(string? value, bool required = true)
        {
            if (string.IsNullOrWhiteSpace(value)) return required ? Required(value, "Contact number") : ServiceResult.Ok();
            var digits = value.Count(c => c is >= '0' and <= '9');
            return digits is >= 7 and <= 15 && Regex.IsMatch(value, @"^\+?[0-9 -]+$")
                ? MaxLength(value, FieldLimits.ContactNumber, "Contact number")
                : ServiceResult.Fail("Contact number must be 7 to 15 digits.");
        }
        public static ServiceResult Email(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return ServiceResult.Ok();
            return First(MaxLength(value, FieldLimits.Email, "Email"),
                MailAddress.TryCreate(value, out var address) && address.Address == value
                    ? ServiceResult.Ok() : ServiceResult.Fail("Email must be a valid email address."));
        }
        public static ServiceResult First(params ServiceResult[] results) =>
            results.FirstOrDefault(result => !result.Success) ?? ServiceResult.Ok();
    }
}
