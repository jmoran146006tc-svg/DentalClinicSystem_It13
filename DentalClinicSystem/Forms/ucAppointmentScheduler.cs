using System.ComponentModel;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public partial class ucAppointmentScheduler : UserControl
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;

        private int? _selectedAppointmentId;
        private Dictionary<int, string> _patientNamesById = [];
        private Dictionary<int, string> _dentistNamesById = [];

        public ucAppointmentScheduler(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDentistService dentistService)
        {
            InitializeComponent();
            _appointmentService = appointmentService;
            _patientService = patientService;
            _dentistService = dentistService;
        }

        private async void ucAppointmentScheduler_Load(object sender, EventArgs e)
        {
            cboStatus.Items.AddRange(["Scheduled", "Completed", "Cancelled", "NoShow"]);
            dgvAppointments.SelectionChanged += dgvAppointments_SelectionChanged;

            await LoadLookupsAsync();
            await RefreshGridAsync();
        }

        private async Task LoadLookupsAsync()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            cboPatient.DataSource = patients.ToList();
            cboPatient.DisplayMember = nameof(Patient.FullName);
            cboPatient.ValueMember = nameof(Patient.PatientId);
            _patientNamesById = patients.ToDictionary(p => p.PatientId, p => p.FullName);

            var dentists = await _dentistService.GetAllDentistsAsync();
            cboDentist.DataSource = dentists.ToList();
            cboDentist.DisplayMember = nameof(Dentist.FullName);
            cboDentist.ValueMember = nameof(Dentist.DentistId);
            _dentistNamesById = dentists.ToDictionary(d => d.DentistId, d => d.FullName);
        }

        private async Task RefreshGridAsync()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            // Show patient/dentist names instead of raw PatientId/DentistId - the grid
            // used to bind directly to Appointment and show bare numbers for both.
            var rows = appointments.Select(a => new AppointmentRow
            {
                AppointmentId = a.AppointmentId,
                Patient = _patientNamesById.TryGetValue(a.PatientId, out var pn) ? pn : $"#{a.PatientId}",
                Dentist = _dentistNamesById.TryGetValue(a.DentistId, out var dn) ? dn : $"#{a.DentistId}",
                AppointmentDateTime = a.AppointmentDateTime,
                Status = a.Status,
                Reason = a.Reason
            }).ToList();

            dgvAppointments.DataSource = new BindingList<AppointmentRow>(rows);
        }

        private void dgvAppointments_SelectionChanged(object sender, EventArgs e)
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

        private async void btnSchedule_Click(object sender, EventArgs e)
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
                Reason = string.IsNullOrWhiteSpace(txtReason.Text) ? null : txtReason.Text.Trim()
            };

            var result = await _appointmentService.ScheduleAppointmentAsync(appointment);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "Scheduling Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshGridAsync();
            txtReason.Clear();
        }

        private async void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (_selectedAppointmentId is null || cboStatus.SelectedItem is not string status)
            {
                MessageBox.Show("Select an appointment from the grid and a status first.",
                    "Missing Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = await _appointmentService.UpdateAppointmentStatusAsync(_selectedAppointmentId.Value, status);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "Update Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshGridAsync();
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