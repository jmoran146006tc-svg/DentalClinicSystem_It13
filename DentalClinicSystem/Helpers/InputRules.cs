using DentalClinicSystem.Service;

namespace DentalClinicSystem.Helpers
{
    public static class InputRules
    {
        public static string? NullIfBlank(string value) => Validator.Optional(value);
        public static bool TryCost(string text, out decimal cost) => decimal.TryParse(text,
            System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite,
            System.Globalization.CultureInfo.InvariantCulture, out cost) && Validator.Cost(cost).Success;
        public static void PhoneKeyPress(object? sender, KeyPressEventArgs e) =>
            e.Handled = !char.IsControl(e.KeyChar) && !(e.KeyChar is >= '0' and <= '9') && !"+- ".Contains(e.KeyChar);
        public static void PhoneVerifyChar(object sender, AntdUI.InputVerifyCharEventArgs e) =>
            e.Result = char.IsControl(e.Char) || e.Char is >= '0' and <= '9' || "+- ".Contains(e.Char);
        public static void ApplyMaxLengths(params (TextBoxBase Input, int Maximum)[] fields)
        {
            foreach (var (input, maximum) in fields) input.MaxLength = maximum;
        }
        public static void SetDate(DateTimePicker picker, DateTime date) =>
            picker.Value = date < picker.MinDate ? picker.MinDate : date > picker.MaxDate ? picker.MaxDate : date;
    }
}
