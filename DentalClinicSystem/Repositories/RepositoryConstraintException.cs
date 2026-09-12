namespace DentalClinicSystem.Repositories
{
    public class RepositoryConstraintException : Exception
    {
        public RepositoryConstraintException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
