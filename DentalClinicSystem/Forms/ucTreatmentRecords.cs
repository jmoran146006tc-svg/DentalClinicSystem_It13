using System.ComponentModel;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public partial class ucTreatmentRecords : UserControl
    {
        private readonly ITreatmentService _treatmentService;
        private readonly IAppointmentService _appointmentService;
        private readonly ITreatmentTypeService _treatmentTypeService;
        private readonly bool _canEdit;

        private Dictionary<int, TreatmentType> _treatmentTypesById = [];
        private Dictionary<int, string> _appointmentLabelsById = [];

        public ucTreatmentRecords(
            ITreatmentService treatmentService,
            IAppointmentService appointmentService,
            ITreatmentTypeService treatmentTypeService,
            bool canEdit = true)
        {
            InitializeComponent();
            _treatmentService = treatmentService;
            _appointmentService = appointmentService;
            _treatmentTypeService = treatmentTypeService;
            _canEdit = canEdit;
        }

        private async void ucTreatmentRecords_Load(object sender, EventArgs e)
        {
            dtpDatePerformed.MaxDate = DateTime.Today;
            cboTreatmentType.SelectedIndexChanged += cboTreatmentType_SelectedIndexChanged;
            btnAddTreatment.Click += btnAddTreatment_Click;

            // Receptionist gets this tab to see cost/history for checkout, not to
            // enter clinical records - grey out the entry row instead of hiding it,
            // so the fields are still there to read.
            cboAppointment.Enabled = _canEdit;
            cboTreatmentType.Enabled = _canEdit;
            txtToothNumber.Enabled = _canEdit;
            txtCost.Enabled = _canEdit;
            dtpDatePerformed.Enabled = _canEdit;
            txtNotes.Enabled = _canEdit;
            btnAddTreatment.Visible = _canEdit;

            await LoadLookupsAsync();
            await RefreshGridAsync();
        }

        private async Task LoadLookupsAsync()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            var options = appointments
                .Select(a => new AppointmentOption
                {
                    AppointmentId = a.AppointmentId,
                    Display = $"#{a.AppointmentId} - {a.AppointmentDateTime:MM/dd/yyyy hh:mm tt} ({a.Status})"
                })
                .ToList();

            cboAppointment.DataSource = options;
            cboAppointment.DisplayMember = nameof(AppointmentOption.Display);
            cboAppointment.ValueMember = nameof(AppointmentOption.AppointmentId);
            _appointmentLabelsById = options.ToDictionary(o => o.AppointmentId, o => o.Display);

            var treatmentTypes = await _treatmentTypeService.GetAllTreatmentTypesAsync();
            _treatmentTypesById = treatmentTypes.ToDictionary(t => t.TreatmentTypeId);

            cboTreatmentType.DataSource = treatmentTypes.ToList();
            cboTreatmentType.DisplayMember = nameof(TreatmentType.Name);
            cboTreatmentType.ValueMember = nameof(TreatmentType.TreatmentTypeId);
        }

        private async Task RefreshGridAsync()
        {
            var treatments = await _treatmentService.GetAllTreatmentsAsync();

            // Show readable labels instead of raw AppointmentId/TreatmentTypeId - the
            // grid used to bind directly to Treatment and show bare numbers for both.
            var rows = treatments.Select(t => new TreatmentRow
            {
                TreatmentId = t.TreatmentId,
                Appointment = _appointmentLabelsById.TryGetValue(t.AppointmentId, out var label) ? label : $"#{t.AppointmentId}",
                TreatmentType = _treatmentTypesById.TryGetValue(t.TreatmentTypeId, out var type) ? type.Name : $"#{t.TreatmentTypeId}",
                ToothNumber = t.ToothNumber,
                Cost = t.Cost,
                DatePerformed = t.DatePerformed,
                Notes = t.Notes
            }).ToList();

            dgvTreatments.DataSource = new BindingList<TreatmentRow>(rows);
        }

        private void cboTreatmentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboTreatmentType.SelectedValue is int treatmentTypeId &&
                _treatmentTypesById.TryGetValue(treatmentTypeId, out var treatmentType))
            {
                txtCost.Text = treatmentType.DefaultCost.ToString("0.00");
            }
        }

        private async void btnAddTreatment_Click(object sender, EventArgs e)
        {
            if (cboAppointment.SelectedValue is not int appointmentId ||
                cboTreatmentType.SelectedValue is not int treatmentTypeId)
            {
                MessageBox.Show("Pick an appointment and a treatment type first.", "Missing Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!decimal.TryParse(txtCost.Text, out var cost))
            {
                MessageBox.Show("Cost must be a valid amount.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var treatment = new Treatment
            {
                AppointmentId = appointmentId,
                TreatmentTypeId = treatmentTypeId,
                ToothNumber = string.IsNullOrWhiteSpace(txtToothNumber.Text) ? null : txtToothNumber.Text.Trim(),
                Cost = cost,
                DatePerformed = dtpDatePerformed.Value.Date,
                Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim()
            };

            var result = await _treatmentService.AddTreatmentAsync(treatment);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshGridAsync();
            txtToothNumber.Clear();
            txtNotes.Clear();
        }

        // Lightweight display wrapper - Appointment itself has no "show me a readable
        // label" property, so this pairs an AppointmentId with a human-readable string
        // for cboAppointment's DisplayMember/ValueMember, without touching Models/.
        private sealed class AppointmentOption
        {
            public int AppointmentId { get; init; }
            public string Display { get; init; } = string.Empty;
        }

        // Display wrapper for dgvTreatments - shows the appointment's readable label
        // and the treatment type's name instead of the raw foreign-key ints Treatment
        // itself stores.
        private sealed class TreatmentRow
        {
            public int TreatmentId { get; init; }
            public string Appointment { get; init; } = string.Empty;
            public string TreatmentType { get; init; } = string.Empty;
            public string? ToothNumber { get; init; }
            public decimal Cost { get; init; }
            public DateTime DatePerformed { get; init; }
            public string? Notes { get; init; }
        }
    }
}