using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IPatientHistoryService
    {
        Task<ServiceResult<PatientHistory>> GetHistoryAsync(User actor, int appointmentId);
    }
}
