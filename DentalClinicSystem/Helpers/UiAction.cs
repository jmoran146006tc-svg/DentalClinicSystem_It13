using DentalClinicSystem.Helpers.Design.Controls;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers
{
    public static class UiAction
    {
        public static async Task RunAsync(Control owner, Func<Task> action, AppButton? trigger = null)
        {
            if (owner.IsDisposed || !owner.Enabled) return;
            using var scope = UiMessages.UseOwner(owner);
            LoadingOverlay? overlay = null;
            var enabled = owner.Enabled;
            var waitCursor = owner.UseWaitCursor;
            if (trigger is not null) trigger.IsBusy = true;
            owner.Enabled = false;
            owner.UseWaitCursor = true;
            MotionSystem.Animator.Schedule(owner, "loading-delay", MotionSystem.LoadingDelay, () =>
            {
                if (owner.IsDisposed || !owner.Visible || owner.FindForm() is null || owner.Width <= 0 || owner.Height <= 0) return;
                overlay = new LoadingOverlay(owner);
                owner.Controls.Add(overlay); overlay.BringToFront();
            });
            try { await action(); }
            catch (Exception error)
            {
                AppLog.Write(error);
                if (!owner.IsDisposed) UiMessages.ShowUnexpectedError();
            }
            finally
            {
                MotionSystem.Animator.Cancel(owner, "loading-delay");
                overlay?.Dispose();
                if (trigger is { IsDisposed: false }) trigger.IsBusy = false;
                if (!owner.IsDisposed)
                {
                    owner.UseWaitCursor = waitCursor;
                    owner.Enabled = enabled;
                }
            }
        }
    }
}
