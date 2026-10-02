namespace DentalClinicSystem.Helpers
{
    public static class UiAction
    {
        public static async Task RunAsync(Control owner, Func<Task> action)
        {
            if (owner.IsDisposed || !owner.Enabled) return;
            owner.Enabled = false;
            owner.UseWaitCursor = true;
            try { await action(); }
            catch (Exception)
            {
                if (!owner.IsDisposed) UiMessages.ShowInfo("The action could not be completed. Check the database connection and try again.");
            }
            finally
            {
                if (!owner.IsDisposed)
                {
                    owner.UseWaitCursor = false;
                    owner.Enabled = true;
                }
            }
        }
    }
}
