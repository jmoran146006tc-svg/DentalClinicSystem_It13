using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent;

public class MySqlDentistTimeOffRepository : IDentistTimeOffRepository
{
    public Task<IReadOnlyList<DentistTimeOff>> GetByDentistAsync(int dentistId) =>
        QueryAsync("sp_DentistTimeOff_GetByDentist", Map, Parameter("@p_DentistId", dentistId));
    public Task<IReadOnlyList<DentistTimeOff>> GetOverlappingAsync(int dentistId, DateTime from, DateTime to) =>
        QueryAsync("sp_DentistTimeOff_GetOverlapping", Map, Parameter("@p_DentistId", dentistId), Parameter("@p_From", from.Date), Parameter("@p_To", to.Date));
    public Task AddAsync(DentistTimeOff timeOff) => ExecuteAsync("sp_DentistTimeOff_Add",
        Parameter("@p_DentistId", timeOff.DentistId), Parameter("@p_StartDate", timeOff.StartDate.Date),
        Parameter("@p_EndDate", timeOff.EndDate.Date), Parameter("@p_Reason", timeOff.Reason));
    public Task DeleteAsync(int timeOffId) => ExecuteAsync("sp_DentistTimeOff_Delete", Parameter("@p_TimeOffId", timeOffId));
    private static DentistTimeOff Map(MySqlDataReader reader) => new()
    {
        TimeOffId = reader.GetInt32("TimeOffId"), DentistId = reader.GetInt32("DentistId"),
        StartDate = reader.GetDateTime("StartDate"), EndDate = reader.GetDateTime("EndDate"), Reason = reader.GetNullableString("Reason")
    };
}
