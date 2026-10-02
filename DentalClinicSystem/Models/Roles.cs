namespace DentalClinicSystem.Models
{
    public static class Roles
    {
        public const string Admin = "Admin", Receptionist = "Receptionist", Dentist = "Dentist";
        public static readonly string[] All = [Admin, Receptionist, Dentist];
    }
}
