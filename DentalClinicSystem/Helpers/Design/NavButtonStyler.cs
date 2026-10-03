using System.Runtime.CompilerServices;
using DentalClinicSystem.Helpers.Design.Controls;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design;

public static class NavButtonStyler
{
    private sealed class State(IconKind icon)
    {
        public IconKind Icon { get; } = icon;
        public bool Selected { get; set; }
        public bool Hover { get; set; }
        public Color Fill { get; set; } = Palette.Surface;
    }
    private static readonly ConditionalWeakTable<Button, State> States = new();
    public static void Attach(Button button, IconKind icon)
    {
        if (States.TryGetValue(button, out _)) return;
        var state = new State(icon); States.Add(button, state); Theme.MarkPrimitive(button);
        DesignPaint.Enable(button); button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = 0;
        button.Font = Typography.Body; button.Height = Metrics.NavHeight; button.Width = Metrics.SidebarWidth - Space.Xl * 2;
        button.Margin = new Padding(0, Space.Xs, 0, 0); button.Cursor = Cursors.Hand;
        button.MouseEnter += (_, _) => { state.Hover = true; Change(button, state); };
        button.MouseLeave += (_, _) => { state.Hover = false; Change(button, state); };
        button.Disposed += (_, _) => MotionSystem.Animator.Cancel(button);
        button.GotFocus += (_, _) => button.Invalidate(); button.LostFocus += (_, _) => button.Invalidate();
        button.Paint += (_, e) =>
        {
            e.Graphics.Clear(Palette.Surface);
            var bounds = Rectangle.Inflate(button.ClientRectangle, -Metrics.FocusRing, -Metrics.FocusRing);
            DesignPaint.Surface(e.Graphics, bounds, Metrics.ControlRadius, state.Fill,
                button.Focused ? Palette.BrandAccent : null);
            var color = state.Selected ? Palette.BrandSoftText : Palette.Ink700;
            Icons.Draw(e.Graphics, state.Icon, new(Space.Md, (button.Height - Metrics.IconSize) / 2, Metrics.IconSize, Metrics.IconSize), color);
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font, new Rectangle(Space.Xxxl, 0, button.Width - Space.Xxxl - Space.Sm, button.Height), color, DesignPaint.TextFlags);
        };
    }
    public static void Select(Button button, bool selected)
    {
        if (States.TryGetValue(button, out var state)) { state.Selected = selected; Change(button, state); }
    }
    private static void Change(Button button, State state) => MotionSystem.Animator.RunColor(button, "nav-fill", state.Fill,
        state.Selected ? Palette.BrandSoft : state.Hover ? Palette.SurfaceAlt : Palette.Surface, MotionSystem.Fast, color => { state.Fill = color; button.Invalidate(); });
}
