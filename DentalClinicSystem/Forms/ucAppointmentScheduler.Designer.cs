namespace DentalClinicSystem.Forms
{
    partial class ucAppointmentScheduler
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            dgvAppointments = new DataGridView();
            lblPatient = new Label();
            cboPatient = new ComboBox();
            lblDentist = new Label();
            cboDentist = new ComboBox();
            lblDateTime = new Label();
            dtpAppointmentDateTime = new DateTimePicker();
            lblReason = new Label();
            txtReason = new TextBox();
            btnSchedule = new Button();
            lblStatus = new Label();
            cboStatus = new ComboBox();
            btnUpdateStatus = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvAppointments).BeginInit();
            SuspendLayout();
            // 
            // dgvAppointments
            // 
            dgvAppointments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvAppointments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAppointments.Location = new Point(45, 20);
            dgvAppointments.MultiSelect = false;
            dgvAppointments.Name = "dgvAppointments";
            dgvAppointments.ReadOnly = true;
            dgvAppointments.RowHeadersWidth = 62;
            dgvAppointments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAppointments.Size = new Size(789, 170);
            dgvAppointments.TabIndex = 0;
            // 
            // lblPatient
            // 
            lblPatient.AutoSize = true;
            lblPatient.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblPatient.Location = new Point(45, 214);
            lblPatient.Name = "lblPatient";
            lblPatient.Size = new Size(80, 22);
            lblPatient.TabIndex = 11;
            lblPatient.Text = "Patient:";
            lblPatient.Click += lblPatient_Click;
            // 
            // cboPatient
            // 
            cboPatient.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPatient.Location = new Point(45, 239);
            cboPatient.Name = "cboPatient";
            cboPatient.Size = new Size(268, 33);
            cboPatient.TabIndex = 10;
            // 
            // lblDentist
            // 
            lblDentist.AutoSize = true;
            lblDentist.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDentist.Location = new Point(443, 214);
            lblDentist.Name = "lblDentist";
            lblDentist.Size = new Size(86, 22);
            lblDentist.TabIndex = 9;
            lblDentist.Text = "Dentist: ";
            // 
            // cboDentist
            // 
            cboDentist.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDentist.Location = new Point(443, 239);
            cboDentist.Name = "cboDentist";
            cboDentist.Size = new Size(237, 33);
            cboDentist.TabIndex = 8;
            // 
            // lblDateTime
            // 
            lblDateTime.AutoSize = true;
            lblDateTime.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDateTime.Location = new Point(45, 279);
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Size = new Size(140, 22);
            lblDateTime.TabIndex = 7;
            lblDateTime.Text = "Date and Time";
            // 
            // dtpAppointmentDateTime
            // 
            dtpAppointmentDateTime.CustomFormat = "MM/dd/yyyy hh:mm tt";
            dtpAppointmentDateTime.Format = DateTimePickerFormat.Custom;
            dtpAppointmentDateTime.Location = new Point(45, 304);
            dtpAppointmentDateTime.Name = "dtpAppointmentDateTime";
            dtpAppointmentDateTime.ShowUpDown = true;
            dtpAppointmentDateTime.Size = new Size(268, 31);
            dtpAppointmentDateTime.TabIndex = 6;
            // 
            // lblReason
            // 
            lblReason.AutoSize = true;
            lblReason.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblReason.Location = new Point(443, 279);
            lblReason.Name = "lblReason";
            lblReason.Size = new Size(160, 22);
            lblReason.TabIndex = 5;
            lblReason.Text = "Reason for visit:";
            // 
            // txtReason
            // 
            txtReason.Location = new Point(443, 306);
            txtReason.Name = "txtReason";
            txtReason.Size = new Size(237, 31);
            txtReason.TabIndex = 4;
            // 
            // btnSchedule
            // 
            btnSchedule.BackColor = Color.FromArgb(66, 202, 207);
            btnSchedule.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnSchedule.ForeColor = Color.FromArgb(243, 248, 249);
            btnSchedule.Location = new Point(45, 363);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.Size = new Size(256, 34);
            btnSchedule.TabIndex = 3;
            btnSchedule.Text = "Schedule Appointment";
            btnSchedule.UseVisualStyleBackColor = false;
            btnSchedule.Click += btnSchedule_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblStatus.Location = new Point(45, 420);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(115, 22);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Set Status: ";
            // 
            // cboStatus
            // 
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Location = new Point(45, 445);
            cboStatus.Name = "cboStatus";
            cboStatus.Size = new Size(343, 33);
            cboStatus.TabIndex = 1;
            // 
            // btnUpdateStatus
            // 
            btnUpdateStatus.BackColor = Color.FromArgb(243, 248, 249);
            btnUpdateStatus.Enabled = false;
            btnUpdateStatus.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnUpdateStatus.ForeColor = Color.FromArgb(66, 202, 207);
            btnUpdateStatus.Location = new Point(443, 444);
            btnUpdateStatus.Name = "btnUpdateStatus";
            btnUpdateStatus.Size = new Size(97, 34);
            btnUpdateStatus.TabIndex = 0;
            btnUpdateStatus.Text = "Update ";
            btnUpdateStatus.UseVisualStyleBackColor = false;
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            // 
            // ucAppointmentScheduler
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 248, 249);
            Controls.Add(btnUpdateStatus);
            Controls.Add(cboStatus);
            Controls.Add(lblStatus);
            Controls.Add(btnSchedule);
            Controls.Add(txtReason);
            Controls.Add(lblReason);
            Controls.Add(dtpAppointmentDateTime);
            Controls.Add(lblDateTime);
            Controls.Add(cboDentist);
            Controls.Add(lblDentist);
            Controls.Add(cboPatient);
            Controls.Add(lblPatient);
            Controls.Add(dgvAppointments);
            Name = "ucAppointmentScheduler";
            Size = new Size(872, 558);
            Load += ucAppointmentScheduler_Load;
            ((System.ComponentModel.ISupportInitialize)dgvAppointments).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvAppointments;
        private Label lblPatient;
        private ComboBox cboPatient;
        private Label lblDentist;
        private ComboBox cboDentist;
        private Label lblDateTime;
        private DateTimePicker dtpAppointmentDateTime;
        private Label lblReason;
        private TextBox txtReason;
        private Button btnSchedule;
        private Label lblStatus;
        private ComboBox cboStatus;
        private Button btnUpdateStatus;
    }
}