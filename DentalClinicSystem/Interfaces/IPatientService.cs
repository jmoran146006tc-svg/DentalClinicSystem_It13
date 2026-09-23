using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IPatientService
    {
        Task<IReadOnlyList<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int patientId);
        Task<ServiceResult> AddPatientAsync(Patient patient);
        Task<ServiceResult> UpdatePatientAsync(Patient patient);
        Task<ServiceResult> DeletePatientAsync(int patientId);
    }
}
