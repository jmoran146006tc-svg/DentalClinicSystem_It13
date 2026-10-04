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
        public static ServiceResult Patient(Patient patient, DateTime now)
        {
            patient.FirstName = Sanitize(patient.FirstName);
            patient.LastName = Sanitize(patient.LastName);
            patient.ContactNumber = Sanitize(patient.ContactNumber);
            patient.Email = Optional(patient.Email);
            patient.Address = Optional(patient.Address);
            patient.GuardianName = Optional(Sanitize(patient.GuardianName)); patient.GuardianContact = Optional(Sanitize(patient.GuardianContact));
            patient.Allergies = Optional(patient.Allergies); patient.MedicalNotes = Optional(patient.MedicalNotes);
            var minor = IsMinor(patient.DateOfBirth, now);
            return First(Name(patient.FirstName, "First name"), Name(patient.LastName, "Last name"),
                Phone(patient.ContactNumber), Email(patient.Email), MaxLength(patient.Address, FieldLimits.Address, "Address"),
                patient.DateOfBirth.Date > now.Date ? ServiceResult.Fail("Date of birth cannot be in the future.") : ServiceResult.Ok(),
                DatabaseDate(patient.DateOfBirth, "Date of birth"),
                minor ? Required(patient.GuardianName, "Guardian name") : ServiceResult.Ok(),
                MaxLength(patient.GuardianName, FieldLimits.GuardianName, "Guardian name"),
                minor ? Required(patient.GuardianContact, "Guardian contact") : ServiceResult.Ok(),
                Phone(patient.GuardianContact, minor), MaxLength(patient.GuardianContact, FieldLimits.GuardianContact, "Guardian contact"),
                MaxLength(patient.Allergies, FieldLimits.Allergies, "Allergies"), MaxLength(patient.MedicalNotes, FieldLimits.MedicalNotes, "Medical notes"));
        }
        public static bool IsMinor(DateTime birthDate, DateTime now) => now.Year - birthDate.Year < 18 || now.Year - birthDate.Year == 18 && birthDate.Date.AddYears(18) > now.Date;
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
        public static ServiceResult Appointment(Appointment appointment, DateTime now)
        {
            appointment.Reason = Optional(appointment.Reason);
            appointment.Notes = Optional(appointment.Notes);
            return First(Duration(appointment.DurationMinutes), DatabaseDate(appointment.AppointmentDateTime, "Appointment date"), MaxLength(appointment.Reason, FieldLimits.Reason, "Reason"),
                MaxLength(appointment.Notes, FieldLimits.Notes, "Notes"),
                appointment.AppointmentDateTime < now.AddMinutes(-ClinicRules.PastGraceMinutes) ? ServiceResult.Fail($"Appointment time cannot be more than {ClinicRules.PastGraceMinutes} minutes in the past.") : ServiceResult.Ok());
        }
        public static ServiceResult StatusChange(string current, string status, string? reason, DateTime appointmentDateTime, DateTime now)
        {
            if (!AppointmentStatus.IsValid(status)) return ServiceResult.Fail("Select a valid appointment status.");
            if (!AppointmentStatus.CanTransition(current, status)) return ServiceResult.Fail("That appointment status can't be changed.");
            if (status == AppointmentStatus.CheckedIn && appointmentDateTime.Date != now.Date)
                return ServiceResult.Fail("Check-in is only available on the appointment date.");
            if (status == AppointmentStatus.Completed && appointmentDateTime.Date > now.Date)
                return ServiceResult.Fail("A future appointment can't be completed yet.");
            if (status == AppointmentStatus.NoShow && appointmentDateTime > now)
                return ServiceResult.Fail("Wait until the appointment time to mark a no-show.");
            if (status == AppointmentStatus.Cancelled)
                return First(Required(reason, "Cancellation reason"), MaxLength(Optional(reason), FieldLimits.Reason, "Cancellation reason"));
            return MaxLength(Optional(reason), FieldLimits.Reason, "Cancellation reason");
        }
        public static ServiceResult Treatment(Treatment treatment, DateTime now)
        {
            treatment.ToothNumber = Optional(treatment.ToothNumber);
            treatment.Notes = Optional(treatment.Notes);
            return First(Cost(treatment.Cost), Discount(treatment.DiscountType, treatment.DiscountPercent), MaxLength(treatment.ToothNumber, FieldLimits.ToothNumber, "Tooth number"),
                ToothNumbering.IsValid(treatment.ToothNumber) ? ServiceResult.Ok() : ServiceResult.Fail("Enter a valid FDI tooth number (for example 11 to 48) or leave it blank."),
                MaxLength(treatment.Notes, FieldLimits.Notes, "Notes"), DatabaseDate(treatment.DatePerformed, "Date performed"),
                treatment.DatePerformed.Date > now.Date ? ServiceResult.Fail("Date performed cannot be in the future.") : ServiceResult.Ok());
        }
        public static ServiceResult TreatmentType(TreatmentType type)
        {
            type.Name = Sanitize(type.Name);
            type.Description = Optional(type.Description);
            return First(Required(type.Name, "Treatment type name"), MaxLength(type.Name, FieldLimits.TreatmentTypeName, "Treatment type name"),
                Duration(type.DefaultDurationMinutes), Cost(type.DefaultCost), MaxLength(type.Description, FieldLimits.Description, "Description"));
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
        public static ServiceResult Duration(int minutes) => minutes >= ClinicRules.MinDurationMinutes && minutes <= ClinicRules.MaxDurationMinutes && minutes % ClinicRules.DurationStepMinutes == 0
            ? ServiceResult.Ok() : ServiceResult.Fail($"Duration must be {ClinicRules.MinDurationMinutes} to {ClinicRules.MaxDurationMinutes} minutes in {ClinicRules.DurationStepMinutes}-minute steps.");
        public static ServiceResult Discount(string type, decimal percent)
        {
            if (!DiscountTypes.All.Contains(type)) return ServiceResult.Fail("Select a valid discount type.");
            if (percent < 0 || percent > 100) return ServiceResult.Fail("Discount must be between 0% and 100%.");
            if (type == DiscountTypes.None && percent != 0) return ServiceResult.Fail("Choose a discount type or set the discount to 0%.");
            if (decimal.Round(percent, 2) != percent) return ServiceResult.Fail("Use at most two decimal places for the discount.");
            return ServiceResult.Ok();
        }
        public static ServiceResult TimeOff(DentistTimeOff timeOff)
        {
            timeOff.StartDate = timeOff.StartDate.Date; timeOff.EndDate = timeOff.EndDate.Date; timeOff.Reason = Optional(timeOff.Reason);
            return First(DateRange(timeOff.StartDate, timeOff.EndDate), MaxLength(timeOff.Reason, FieldLimits.TimeOffReason, "Reason"));
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
