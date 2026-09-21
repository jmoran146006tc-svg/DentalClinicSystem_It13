using System.ComponentModel;
using DentalClinicSystem.Models;
using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucTreatmentRecords : UserControl
    {
        private readonly ITreatmentService _treatmentService;
        private readonly IAppointmentService _appointmentService;
        private readonly ITreatmentTypeService _treatmentTypeService;

        private Dictionary<int, TreatmentType> _treatmentTypesById = [];

        public ucTreatmentRecords(
            ITreatmentService treatmentService,
            IAppointmentService appointmentService,
            ITreatmentTypeService treatmentTypeService)
        {
            InitializeComponent();
            _treatmentService = treatmentService;
            _appointmentService = appointmentService;
            _treatmentTypeService = treatmentTypeService;
        }

        private async void ucTreatmentRecords_Load(object sender, EventArgs e)
        {
            dtpDatePerformed.MaxDate = DateTime.Today;
            cboTreatmentType.SelectedIndexChanged += cboTreatmentType_SelectedIndexChanged;
            btnAddTreatment.Click += btnAddTreatment_Click;

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

            var treatmentTypes = await _treatmentTypeService.GetAllTreatmentTypesAsync();
            _treatmentTypesById = treatmentTypes.ToDictionary(t => t.TreatmentTypeId);

            cboTreatmentType.DataSource = treatmentTypes.ToList();
            cboTreatmentType.DisplayMember = nameof(TreatmentType.Name);
            cboTreatmentType.ValueMember = nameof(TreatmentType.TreatmentTypeId);
        }

        private async Task RefreshGridAsync()
        {
            var treatments = await _treatmentService.GetAllTreatmentsAsync();
            dgvTreatments.DataSource = new BindingList<Treatment>(treatments.ToList());
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
        // for cboAppointment's DisplayMember/ValueMember, without touching Models.cs.
        private sealed class AppointmentOption
        {
            public int AppointmentId { get; init; }
            public string Display { get; init; } = string.Empty;
        }
    }
}