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
            dgvAppointments.Location = new Point(36, 16);
            dgvAppointments.Margin = new Padding(2, 2, 2, 2);
            dgvAppointments.MultiSelect = false;
            dgvAppointments.Name = "dgvAppointments";
            dgvAppointments.ReadOnly = true;
            dgvAppointments.RowHeadersWidth = 62;
            dgvAppointments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAppointments.Size = new Size(631, 136);
            dgvAppointments.TabIndex = 0;
            // 
            // lblPatient
            // 
            lblPatient.AutoSize = true;
            lblPatient.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblPatient.Location = new Point(36, 171);
            lblPatient.Margin = new Padding(2, 0, 2, 0);
            lblPatient.Name = "lblPatient";
            lblPatient.Size = new Size(66, 18);
            lblPatient.TabIndex = 11;
            lblPatient.Text = "Patient:";
            // 
            // cboPatient
            // 
            cboPatient.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPatient.Location = new Point(36, 191);
            cboPatient.Margin = new Padding(2, 2, 2, 2);
            cboPatient.Name = "cboPatient";
            cboPatient.Size = new Size(215, 28);
            cboPatient.TabIndex = 10;
            // 
            // lblDentist
            // 
            lblDentist.AutoSize = true;
            lblDentist.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDentist.Location = new Point(354, 171);
            lblDentist.Margin = new Padding(2, 0, 2, 0);
            lblDentist.Name = "lblDentist";
            lblDentist.Size = new Size(72, 18);
            lblDentist.TabIndex = 9;
            lblDentist.Text = "Dentist: ";
            // 
            // cboDentist
            // 
            cboDentist.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDentist.Location = new Point(354, 191);
            cboDentist.Margin = new Padding(2, 2, 2, 2);
            cboDentist.Name = "cboDentist";
            cboDentist.Size = new Size(190, 28);
            cboDentist.TabIndex = 8;
            // 
            // lblDateTime
            // 
            lblDateTime.AutoSize = true;
            lblDateTime.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDateTime.Location = new Point(36, 223);
            lblDateTime.Margin = new Padding(2, 0, 2, 0);
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Size = new Size(116, 18);
            lblDateTime.TabIndex = 7;
            lblDateTime.Text = "Date and Time";
            // 
            // dtpAppointmentDateTime
            // 
            dtpAppointmentDateTime.CustomFormat = "MM/dd/yyyy hh:mm tt";
            dtpAppointmentDateTime.Format = DateTimePickerFormat.Custom;
            dtpAppointmentDateTime.Location = new Point(36, 243);
            dtpAppointmentDateTime.Margin = new Padding(2, 2, 2, 2);
            dtpAppointmentDateTime.Name = "dtpAppointmentDateTime";
            dtpAppointmentDateTime.ShowUpDown = true;
            dtpAppointmentDateTime.Size = new Size(215, 27);
            dtpAppointmentDateTime.TabIndex = 6;
            // 
            // lblReason
            // 
            lblReason.AutoSize = true;
            lblReason.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblReason.Location = new Point(354, 223);
            lblReason.Margin = new Padding(2, 0, 2, 0);
            lblReason.Name = "lblReason";
            lblReason.Size = new Size(134, 18);
            lblReason.TabIndex = 5;
            lblReason.Text = "Reason for visit:";
            // 
            // txtReason
            // 
            txtReason.Location = new Point(354, 245);
            txtReason.Margin = new Padding(2, 2, 2, 2);
            txtReason.Name = "txtReason";
            txtReason.Size = new Size(190, 27);
            txtReason.TabIndex = 4;
            // 
            // btnSchedule
            // 
            btnSchedule.BackColor = Color.FromArgb(66, 202, 207);
            btnSchedule.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnSchedule.ForeColor = Color.FromArgb(243, 248, 249);
            btnSchedule.Location = new Point(36, 290);
            btnSchedule.Margin = new Padding(2, 2, 2, 2);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.Size = new Size(205, 27);
            btnSchedule.TabIndex = 3;
            btnSchedule.Text = "Schedule Appointment";
            btnSchedule.UseVisualStyleBackColor = false;
            btnSchedule.Click += btnSchedule_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblStatus.Location = new Point(36, 336);
            lblStatus.Margin = new Padding(2, 0, 2, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(96, 18);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Set Status: ";
            // 
            // cboStatus
            // 
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Location = new Point(36, 356);
            cboStatus.Margin = new Padding(2, 2, 2, 2);
            cboStatus.Name = "cboStatus";
            cboStatus.Size = new Size(275, 28);
            cboStatus.TabIndex = 1;
            // 
            // btnUpdateStatus
            // 
            btnUpdateStatus.BackColor = Color.FromArgb(243, 248, 249);
            btnUpdateStatus.Enabled = false;
            btnUpdateStatus.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnUpdateStatus.ForeColor = Color.FromArgb(66, 202, 207);
            btnUpdateStatus.Location = new Point(354, 356);
            btnUpdateStatus.Margin = new Padding(2, 2, 2, 2);
            btnUpdateStatus.Name = "btnUpdateStatus";
            btnUpdateStatus.Size = new Size(72, 27);
            btnUpdateStatus.TabIndex = 0;
            btnUpdateStatus.Text = "Update ";
            btnUpdateStatus.UseVisualStyleBackColor = false;
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            // 
            // ucAppointmentScheduler
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
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
            Margin = new Padding(2, 2, 2, 2);
            Name = "ucAppointmentScheduler";
            Size = new Size(698, 446);
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
