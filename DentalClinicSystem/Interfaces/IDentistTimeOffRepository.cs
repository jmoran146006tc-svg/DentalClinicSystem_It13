using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces;

public interface IDentistTimeOffRepository
{
    Task<IReadOnlyList<DentistTimeOff>> GetByDentistAsync(int dentistId);
    Task<IReadOnlyList<DentistTimeOff>> GetOverlappingAsync(int dentistId, DateTime from, DateTime to);
    Task AddAsync(DentistTimeOff timeOff);
    Task DeleteAsync(int timeOffId);
}
