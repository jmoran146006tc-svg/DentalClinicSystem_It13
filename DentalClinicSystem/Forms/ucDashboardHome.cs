using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public sealed class ucDashboardHome : UserControl
    {
        public ucDashboardHome(User currentUser)
        {
            AutoScroll = true;
            BackColor = Color.FromArgb(243, 248, 249);
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(24)
            };
            layout.Controls.Add(new Label
            {
                Text = "Dental clinic overview",
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 16)
            });
            layout.Controls.Add(new Label
            {
                Text = $"Hello, {currentUser.Username}. Choose a page from the sidebar to begin.",
                AutoSize = true
            });
            Controls.Add(layout);
        }
    }
}
