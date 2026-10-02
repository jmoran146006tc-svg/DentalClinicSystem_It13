using DentalClinicSystem.Helpers;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms
{
    public partial class ucAppointmentScheduler : UserControl
    {
        private readonly User _currentUser;
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;
        private readonly TextBox _cancellationReason = new() { Width = 280, MaxLength = FieldLimits.Reason };

        private int? _selectedAppointmentId;
        private Dictionary<int, string> _patientNamesById = [];
        private Dictionary<int, string> _dentistNamesById = [];

        public ucAppointmentScheduler(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDentistService dentistService, User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _appointmentService = appointmentService;
            _patientService = patientService;
            _dentistService = dentistService;
            var reasonRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(12) };
            reasonRow.Controls.Add(new Label { Text = "Cancellation / no-show reason:", AutoSize = true });
            reasonRow.Controls.Add(_cancellationReason);
            Controls.Add(reasonRow);
        }

        private async void ucAppointmentScheduler_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, ucAppointmentScheduler_LoadAsync);

        private async Task ucAppointmentScheduler_LoadAsync()
        {
            cmbReason.DropDownStyle = ComboBoxStyle.DropDown;
            cmbReason.Items.Clear();
            cmbReason.Items.AddRange(["Oral Prophylaxis Package", "Consultation & Check up", "Promo Bundles", "Tooth Extraction", "Filling / Restoration", "Root Canal", "Braces Adjustment", "Dentures", "Teeth Whitening", "Other"]);
            cboStatus.Items.Clear();
            cboStatus.Items.AddRange([AppointmentStatus.Scheduled, AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow]);
            dgvAppointments.SelectionChanged += dgvAppointments_SelectionChanged;

            await LoadLookupsAsync();
            await RefreshGridAsync();
        }

        private async Task LoadLookupsAsync()
        {
            var patients = RoleAccess.Can(_currentUser, Permission.ViewPatients)
                ? UiMessages.Items(await _patientService.GetAllPatientsAsync(_currentUser)) : Array.Empty<Patient>();
            cboPatient.DataSource = patients.ToList();
            cboPatient.DisplayMember = nameof(Patient.FullName);
            cboPatient.ValueMember = nameof(Patient.PatientId);
            _patientNamesById = patients.ToDictionary(p => p.PatientId, p => p.FullName);

            var dentists = UiMessages.Items(await _dentistService.GetAllDentistsAsync(_currentUser));
            cboDentist.DataSource = dentists.ToList();
            cboDentist.DisplayMember = nameof(Dentist.FullName);
            cboDentist.ValueMember = nameof(Dentist.DentistId);
            _dentistNamesById = dentists.ToDictionary(d => d.DentistId, d => d.FullName);
        }

        private async Task RefreshGridAsync()
        {
            var appointments = UiMessages.Items(await _appointmentService.GetAllAppointmentsAsync(_currentUser));

            foreach (var appointment in appointments.Where(a => !_patientNamesById.ContainsKey(a.PatientId)))
            {
                var details = await _appointmentService.GetDetailsAsync(_currentUser, appointment.AppointmentId);
                if (details.Data is { } data) _patientNamesById[data.Patient.PatientId] = data.Patient.FullName;
            }
            var rows = appointments.Select(a => new AppointmentRow
            {
                AppointmentId = a.AppointmentId,
                Patient = _patientNamesById.TryGetValue(a.PatientId, out var pn) ? pn : $"#{a.PatientId}",
                Dentist = _dentistNamesById.TryGetValue(a.DentistId, out var dn) ? dn : $"#{a.DentistId}",
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status,
                Reason = a.Reason
            }).ToList();

            GridHelper.Bind(dgvAppointments, rows);
        }

        private void dgvAppointments_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvAppointments.CurrentRow?.DataBoundItem is not AppointmentRow row)
            {
                _selectedAppointmentId = null;
                btnUpdateStatus.Enabled = false;
                return;
            }

            _selectedAppointmentId = row.AppointmentId;
            cboStatus.SelectedItem = row.Status;
            btnUpdateStatus.Enabled = true;
        }

        private async void btnSchedule_Click(object? sender, EventArgs e) => await UiAction.RunAsync(this, btnSchedule_ClickAsync);

        private async Task btnSchedule_ClickAsync()
        {
            if (cboPatient.SelectedValue is not int patientId || cboDentist.SelectedValue is not int dentistId)
            {
                MessageBox.Show("Pick a patient and a dentist first.", "Missing Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DentistId = dentistId,
                AppointmentDateTime = dtpAppointmentDateTime.Value,
                Reason = string.IsNullOrWhiteSpace(cmbReason.Text) ? null : cmbReason.Text.Trim()
            };

            var result = await _appointmentService.ScheduleAppointmentAsync(_currentUser, appointment);
            if (!result.Success)
            {
                UiMessages.ShowError(result);
                return;
            }

            await RefreshGridAsync();
            cmbReason.SelectedIndex = -1;
            cmbReason.Text = string.Empty;
        }

        private async void btnUpdateStatus_Click(object? sender, EventArgs e) => await UiAction.RunAsync(this, btnUpdateStatus_ClickAsync);

        private async Task btnUpdateStatus_ClickAsync()
        {
            if (_selectedAppointmentId is null || cboStatus.SelectedItem is not string status)
            {
                MessageBox.Show("Select an appointment from the grid and a status first.",
                    "Missing Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var reason = status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow ? _cancellationReason.Text : null;
            var result = await _appointmentService.UpdateAppointmentStatusAsync(_currentUser, _selectedAppointmentId.Value, status, reason);
            if (!result.Success)
            {
                UiMessages.ShowError(result);
                return;
            }

            await RefreshGridAsync();
            _cancellationReason.Clear();
        }

        // Display wrapper for dgvAppointments - shows patient/dentist names instead of
        // the raw foreign-key ints Appointment itself stores.
        private sealed class AppointmentRow
        {
            public int AppointmentId { get; init; }
            public string Patient { get; init; } = string.Empty;
            public string Dentist { get; init; } = string.Empty;
            public DateTime AppointmentDateTime { get; init; }
            public string Status { get; init; } = string.Empty;
            public string? Reason { get; init; }
        }

    }
}
