using DentalClinicSystem.Models;
using System.Text;
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
        public static ServiceResult Patient(Patient patient)
        {
            patient.FirstName = Sanitize(patient.FirstName);
            patient.LastName = Sanitize(patient.LastName);
            patient.ContactNumber = Sanitize(patient.ContactNumber);
            patient.Email = Optional(patient.Email);
            patient.Address = Optional(patient.Address);
            return First(Name(patient.FirstName, "First name"), Name(patient.LastName, "Last name"),
                Phone(patient.ContactNumber), Email(patient.Email), MaxLength(patient.Address, FieldLimits.Address, "Address"),
                patient.DateOfBirth.Date > DateTime.Today ? ServiceResult.Fail("Date of birth cannot be in the future.") : ServiceResult.Ok(),
                DatabaseDate(patient.DateOfBirth, "Date of birth"));
        }
        public static ServiceResult Dentist(Dentist dentist)
        {
            dentist.FirstName = Sanitize(dentist.FirstName);
            dentist.LastName = Sanitize(dentist.LastName);
            dentist.ContactNumber = Optional(dentist.ContactNumber);
            dentist.Specialization = Optional(dentist.Specialization);
            dentist.LicenseNumber = Optional(dentist.LicenseNumber);
            return First(Name(dentist.FirstName, "First name"), Name(dentist.LastName, "Last name"),
                Phone(dentist.ContactNumber, false), MaxLength(dentist.Specialization, FieldLimits.Specialization, "Specialization"),
                MaxLength(dentist.LicenseNumber, FieldLimits.LicenseNumber, "License number"));
        }
        public static ServiceResult Appointment(Appointment appointment)
        {
            appointment.Reason = Optional(appointment.Reason);
            appointment.Notes = Optional(appointment.Notes);
            return First(MaxLength(appointment.Reason, FieldLimits.Reason, "Reason"),
                MaxLength(appointment.Notes, FieldLimits.Notes, "Notes"),
                appointment.AppointmentDateTime < DateTime.Now ? ServiceResult.Fail("Appointment time cannot be in the past.") : ServiceResult.Ok());
        }
        public static ServiceResult StatusChange(string current, string status, string? reason)
        {
            if (!AppointmentStatus.IsValid(status)) return ServiceResult.Fail("Select a valid appointment status.");
            if (current == AppointmentStatus.Cancelled && status != current) return ServiceResult.Fail("Cancelled appointments cannot be reopened.");
            if (status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            {
                if (current == AppointmentStatus.Completed) return ServiceResult.Fail("Completed appointments cannot be cancelled.");
                return First(Required(reason, "Cancellation reason"), MaxLength(Optional(reason), FieldLimits.Reason, "Cancellation reason"));
            }
            return ServiceResult.Ok();
        }
        public static ServiceResult Treatment(Treatment treatment)
        {
            treatment.ToothNumber = Optional(treatment.ToothNumber);
            treatment.Notes = Optional(treatment.Notes);
            return First(Cost(treatment.Cost), MaxLength(treatment.ToothNumber, FieldLimits.ToothNumber, "Tooth number"),
                MaxLength(treatment.Notes, FieldLimits.Notes, "Notes"), DatabaseDate(treatment.DatePerformed, "Date performed"),
                treatment.DatePerformed.Date > DateTime.Today ? ServiceResult.Fail("Date performed cannot be in the future.") : ServiceResult.Ok());
        }
        public static ServiceResult TreatmentType(TreatmentType type)
        {
            type.Name = Sanitize(type.Name);
            type.Description = Optional(type.Description);
            return First(Required(type.Name, "Treatment type name"), MaxLength(type.Name, FieldLimits.TreatmentTypeName, "Treatment type name"),
                Cost(type.DefaultCost), MaxLength(type.Description, FieldLimits.Description, "Description"));
        }
        public static ServiceResult User(User user, string? password, bool isNew)
        {
            user.Username = Sanitize(user.Username);
            user.Role = Sanitize(user.Role);
            if (!RoleAccess.RequiresDentist(user.Role)) user.DentistId = null;
            return First(Required(user.Username, "Username"), MaxLength(user.Username, FieldLimits.Username, "Username"),
                Roles.All.Contains(user.Role) ? ServiceResult.Ok() : ServiceResult.Fail("Select a valid role."),
                isNew ? Required(password, "Password") : ServiceResult.Ok(),
                !string.IsNullOrEmpty(password) && (password.Length < 6 || Encoding.UTF8.GetByteCount(password) > FieldLimits.Password)
                    ? ServiceResult.Fail("Password must be at least 6 characters and at most 72 UTF-8 bytes.") : ServiceResult.Ok());
        }
        public static ServiceResult Cost(decimal cost) => cost < 0 || cost > FieldLimits.MaximumCost
            ? ServiceResult.Fail("Cost must be between 0 and 99,999,999.99.") : ServiceResult.Ok();
        public static ServiceResult DatabaseDate(DateTime date, string field) => date.Year < 1000
            ? ServiceResult.Fail($"{field} must be in year 1000 or later.") : ServiceResult.Ok();
        public static ServiceResult DateRange(DateTime from, DateTime to) => First(
            DatabaseDate(from, "From date"), DatabaseDate(to, "To date"),
            from.Date > to.Date ? ServiceResult.Fail("From must be on or before To.") : ServiceResult.Ok());

        public static ServiceResult First(params ServiceResult[] results) =>
            results.FirstOrDefault(result => !result.Success) ?? ServiceResult.Ok();
    }
}
