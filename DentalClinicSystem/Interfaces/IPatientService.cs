using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IPatientService
    {
        Task<ServiceResult<IReadOnlyList<Patient>>> GetAllPatientsAsync(User actor);
        Task<ServiceResult<IReadOnlyList<Patient>>> GetAllIncludingInactiveAsync(User actor);
        Task<ServiceResult<Patient>> GetPatientByIdAsync(User actor, int patientId);
        Task<ServiceResult> AddPatientAsync(User actor, Patient patient);
        Task<ServiceResult> UpdatePatientAsync(User actor, Patient patient);
        Task<ServiceResult> ReactivatePatientAsync(User actor, int patientId);
    }
}
