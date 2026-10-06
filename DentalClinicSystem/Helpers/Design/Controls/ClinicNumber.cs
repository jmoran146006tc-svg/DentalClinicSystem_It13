namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ClinicNumber : AntdUI.InputNumber
{
    protected override void OnMouseWheel(MouseEventArgs e) => WheelRouting.Forward(this, e);
}
