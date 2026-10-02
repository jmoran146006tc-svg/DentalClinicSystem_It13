using DentalClinicSystem.Service;

namespace DentalClinicSystem.Helpers
{
    public static class UiMessages
    {
        public static void ShowError(ServiceResult result) =>
            MessageBox.Show(result.ErrorMessage, "Dental clinic", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        public static void ShowInfo(string text) =>
            MessageBox.Show(text, "Dental clinic", MessageBoxButtons.OK, MessageBoxIcon.Information);
        public static bool Confirm(string text, string title = "Please confirm") =>
            MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
