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
            dgvAppointments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAppointments.Location = new Point(45, 20);
            dgvAppointments.Name = "dgvAppointments";
            dgvAppointments.RowHeadersWidth = 62;
            dgvAppointments.Size = new Size(650, 170);
            dgvAppointments.TabIndex = 0;
            dgvAppointments.ReadOnly = true;
            dgvAppointments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAppointments.MultiSelect = false;
            dgvAppointments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            //
            // lblPatient
            //
            lblPatient.AutoSize = true;
            lblPatient.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblPatient.Location = new Point(45, 210);
            lblPatient.Name = "lblPatient";
            lblPatient.Text = "PATIENT:";
            //
            // cboPatient
            //
            cboPatient.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPatient.Location = new Point(150, 206);
            cboPatient.Name = "cboPatient";
            cboPatient.Size = new Size(220, 31);
            //
            // lblDentist
            //
            lblDentist.AutoSize = true;
            lblDentist.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDentist.Location = new Point(420, 210);
            lblDentist.Name = "lblDentist";
            lblDentist.Text = "DENTIST:";
            //
            // cboDentist
            //
            cboDentist.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDentist.Location = new Point(520, 206);
            cboDentist.Name = "cboDentist";
            cboDentist.Size = new Size(180, 31);
            //
            // lblDateTime
            //
            lblDateTime.AutoSize = true;
            lblDateTime.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDateTime.Location = new Point(45, 265);
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Text = "DATE/TIME:";
            //
            // dtpAppointmentDateTime
            //
            dtpAppointmentDateTime.Format = DateTimePickerFormat.Custom;
            dtpAppointmentDateTime.CustomFormat = "MM/dd/yyyy hh:mm tt";
            dtpAppointmentDateTime.ShowUpDown = true;
            dtpAppointmentDateTime.Location = new Point(150, 261);
            dtpAppointmentDateTime.Name = "dtpAppointmentDateTime";
            dtpAppointmentDateTime.Size = new Size(220, 31);
            //
            // lblReason
            //
            lblReason.AutoSize = true;
            lblReason.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblReason.Location = new Point(420, 265);
            lblReason.Name = "lblReason";
            lblReason.Text = "REASON:";
            //
            // txtReason
            //
            txtReason.Location = new Point(520, 261);
            txtReason.Name = "txtReason";
            txtReason.Size = new Size(180, 31);
            //
            // btnSchedule
            //
            btnSchedule.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnSchedule.Location = new Point(45, 315);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.Size = new Size(150, 34);
            btnSchedule.Text = "SCHEDULE";
            btnSchedule.UseVisualStyleBackColor = true;
            btnSchedule.Click += btnSchedule_Click;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblStatus.Location = new Point(45, 380);
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "SET STATUS:";
            //
            // cboStatus
            //
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Location = new Point(180, 376);
            cboStatus.Name = "cboStatus";
            cboStatus.Size = new Size(150, 31);
            //
            // btnUpdateStatus
            //
            btnUpdateStatus.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnUpdateStatus.Location = new Point(360, 372);
            btnUpdateStatus.Name = "btnUpdateStatus";
            btnUpdateStatus.Size = new Size(150, 34);
            btnUpdateStatus.Text = "UPDATE STATUS";
            btnUpdateStatus.UseVisualStyleBackColor = true;
            btnUpdateStatus.Enabled = false;
            btnUpdateStatus.Click += btnUpdateStatus_Click;
            //
            // ucAppointmentScheduler
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
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
            Size = new Size(733, 440);
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