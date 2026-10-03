using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using System.Runtime.CompilerServices;

namespace DentalClinicSystem.Helpers
{
    public static class UiMessages
    {
        private static readonly AsyncLocal<Control?> Current = new();
        private static readonly ConditionalWeakTable<Control, InlineAlert> Alerts = new();
        private sealed class Scope(Control? previous) : IDisposable { public void Dispose() => Current.Value = previous; }
        public static IDisposable UseOwner(Control owner) { var previous = Current.Value; Current.Value = owner; return new Scope(previous); }
        public static void RegisterAlertHost(Control page, InlineAlert alert) { Alerts.Remove(page); Alerts.Add(page, alert); }
        public static IReadOnlyList<T> Items<T>(ServiceResult<IReadOnlyList<T>> result)
        {
            if (!result.Success) ShowError(result);
            return result.Data ?? Array.Empty<T>();
        }

        public static void ShowError(ServiceResult result)
        {
            var page = Current.Value;
            if (page is { IsDisposed: false } && Alerts.TryGetValue(page, out var alert) && !alert.IsDisposed)
            {
                alert.ShowMessage(result.ErrorMessage);
                if (alert.Parent is { } formBody) Design.Motion.Motion.Shake(formBody);
            }
            else Feedback(result.ErrorMessage, Semantic.Danger);
        }
        public static void ShowSuccess(string text) => Feedback(text, Semantic.Success);
        public static void ShowInfo(string text) => Feedback(text, Semantic.Info);
        public static void ShowUnexpectedError() => ShowError(ServiceResult.Fail("The action could not be completed. Check the database connection and try again."));
        public static void ShowFatal(string text) => MessageBox.Show(text, "Dental clinic", MessageBoxButtons.OK, MessageBoxIcon.Error);
        private static Form? Owner() => Current.Value?.FindForm() ?? Form.ActiveForm ?? Application.OpenForms.Cast<Form>().FirstOrDefault(form => form.Visible);
        private static void Feedback(string text, Semantic semantic)
        {
            if (Owner() is { IsDisposed: false, Visible: true } form) ToastHost.Show(form, text, semantic);
            else ShowFatal(text);
        }
        public static bool Confirm(string text, string title = "Please confirm")
        {
            using var dialog = UiFactory.Confirm(text, title);
            return dialog.ShowDialog(Owner()) == DialogResult.OK;
        }
    }
}
