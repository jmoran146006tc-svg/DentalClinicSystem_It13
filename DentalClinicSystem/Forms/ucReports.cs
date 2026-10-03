using DentalClinicSystem.Helpers;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public sealed class ucReports : UserControl
    {
        private readonly IReportService _reports;
        private readonly User _currentUser;
        private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short };
        private readonly DataGridView _status = new();
        private readonly DataGridView _revenue = new();
        private readonly DataGridView _types = new();
        private readonly DataGridView _workload = new();

        public ucReports(IReportService reports, User currentUser)
        {
            _reports = reports;
            _currentUser = currentUser;
            AutoScroll = true;
            BackColor = Color.FromArgb(243, 248, 249);
            _from.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _to.Value = DateTime.Today;
            BuildLayout();
            Load += OnLoad;
        }

        private void BuildLayout()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(16) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            var refresh = new Button { Text = "Refresh", AutoSize = true };
            refresh.Click += OnRefresh;
            toolbar.Controls.AddRange([
                new Label { Text = "From", AutoSize = true }, _from,
                new Label { Text = "To", AutoSize = true }, _to, refresh
            ]);
            var sections = new TabControl { Dock = DockStyle.Fill };
            AddSection(sections, "Appointments by status", _status);
            AddSection(sections, "Revenue by day", _revenue);
            AddSection(sections, "Top treatment types", _types);
            AddSection(sections, "Dentist workload", _workload);
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(sections, 0, 1);
            Controls.Add(layout);
        }

        private static void AddSection(TabControl sections, string title, DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            var section = new TabPage(title);
            section.Controls.Add(grid);
            sections.TabPages.Add(section);
        }

        private async void OnLoad(object? sender, EventArgs e) => await UiAction.RunAsync(this, RefreshAsync);
        private async void OnRefresh(object? sender, EventArgs e) => await UiAction.RunAsync(this, RefreshAsync);

        private async Task RefreshAsync()
        {
            var from = _from.Value.Date;
            var to = _to.Value.Date;
            var status = await _reports.GetAppointmentStatusCountsAsync(_currentUser, from, to);
            if (IsDisposed) return;
            if (!status.Success) { UiMessages.ShowError(status); return; }
            var revenueTask = _reports.GetRevenueByDayAsync(_currentUser, from, to);
            var typesTask = _reports.GetTopTreatmentTypesAsync(_currentUser, from, to);
            var workloadTask = _reports.GetDentistWorkloadAsync(_currentUser, from, to);
            await Task.WhenAll(revenueTask, typesTask, workloadTask);
            if (IsDisposed) return;
            GridHelper.Bind(_status, UiMessages.Items(status));
            GridHelper.Bind(_revenue, UiMessages.Items(await revenueTask));
            GridHelper.Bind(_types, UiMessages.Items(await typesTask));
            GridHelper.Bind(_workload, UiMessages.Items(await workloadTask));
        }
    }
}
