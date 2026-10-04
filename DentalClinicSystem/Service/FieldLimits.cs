namespace DentalClinicSystem.Service
{
    public static class FieldLimits
    {
        public const int Name = 50, ContactNumber = 20, Email = 100, Address = 200;
        public const int Specialization = 100, LicenseNumber = 50, TreatmentTypeName = 100;
        public const int Description = 255, Reason = 255, Notes = 500, ToothNumber = 2;
        public const int Username = 50, PasswordHash = 255, Role = 20, Status = 20;
        public const int TimeOffReason = 100;
        public const int Password = 72;
        public const decimal MaximumCost = 99999999.99m;
    }
}
