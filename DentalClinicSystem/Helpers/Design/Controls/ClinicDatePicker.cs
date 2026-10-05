using System.ComponentModel;

namespace DentalClinicSystem.Helpers.Design.Controls;

// Clinic dates are required. A picker cannot clear its value or bypass the
// minimum/maximum constraints enforced by the existing forms.
public sealed class ClinicDatePicker : AntdUI.DatePicker
{
    private DateTime _value = DateTime.Now;
    private DateTimePickerFormat _format = DateTimePickerFormat.Short;
    private string _customFormat = DisplayFormat.DatePattern;
    public ClinicDatePicker()
    {
        Font = Typography.Body; Radius = Metrics.ControlRadius;
        Height = Metrics.ControlHeight; Width = Metrics.FormWidth;
        base.Value = _value; base.Format = DisplayFormat.DatePattern;
        AllowClear = false; EnabledValueTextChange = false; CaretVisible = false;
        base.ValueChanged += (_, _) =>
        {
            if (base.Value is not DateTime date) { base.Value = _value; return; }
            date = Clamp(date);
            if (base.Value != date) { base.Value = date; return; }
            if (_value == date) return;
            _value = date; ValueChanged?.Invoke(this, EventArgs.Empty);
        };
    }
    // AntdUI ReadOnly also closes the calendar. BanInput is its separate native
    // text-entry guard (also used by Select.List) and leaves the popup usable.
    protected override bool BanInput => true;
    private DateTime Clamp(DateTime value) => value < MinDate ? MinDate : value > MaxDate ? MaxDate : value;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DateTime Value { get => _value; set => base.Value = Clamp(value); }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DateTime MinDate { get => base.MinDate ?? new DateTime(1900, 1, 1); set { base.MinDate = value; Value = Clamp(_value); } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DateTime MaxDate { get => base.MaxDate ?? new DateTime(9998, 12, 31); set { base.MaxDate = value; Value = Clamp(_value); } }
    [DefaultValue(DateTimePickerFormat.Short)]
    public new DateTimePickerFormat Format { get => _format; set { _format = value; UpdateFormat(); } }
    [DefaultValue("dd MMM yyyy")]
    public string CustomFormat { get => _customFormat; set { _customFormat = value; UpdateFormat(); } }
    private void UpdateFormat()
    {
        var format = _format == DateTimePickerFormat.Custom ? _customFormat : DisplayFormat.DatePattern;
        // AntdUI exposes its hour selector only for uppercase H. Keep the clinic's
        // 12-hour table formatting while the editable picker uses a 24-hour clock.
        base.Format = format == DisplayFormat.DateTimePattern ? "MMM d, yyyy HH:mm" : format;
    }
    public new event EventHandler? ValueChanged;
}
