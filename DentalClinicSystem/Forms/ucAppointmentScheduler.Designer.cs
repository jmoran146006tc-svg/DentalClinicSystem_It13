namespace DentalClinicSystem.Forms
{
    partial class ucAppointmentScheduler
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvAppointments = new DataGridView();
            dtpAppointmentDateTime = new DateTimePicker();
            txtReason = new TextBox();
            label1 = new Label();
            comboBox1 = new ComboBox();
            label2 = new Label();
            btnSchedule = new Button();
            btnUpdateStatus = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvAppointments).BeginInit();
            SuspendLayout();
            // 
            // dgvAppointments
            // 
            dgvAppointments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAppointments.Location = new Point(40, 41);
            dgvAppointments.Name = "dgvAppointments";
            dgvAppointments.RowHeadersWidth = 62;
            dgvAppointments.Size = new Size(654, 225);
            dgvAppointments.TabIndex = 0;
            // 
            // dtpAppointmentDateTime
            // 
            dtpAppointmentDateTime.Format = DateTimePickerFormat.Custom;
            dtpAppointmentDateTime.Location = new Point(40, 302);
            dtpAppointmentDateTime.Name = "dtpAppointmentDateTime";
            dtpAppointmentDateTime.Size = new Size(300, 31);
            dtpAppointmentDateTime.TabIndex = 1;
            // 
            // txtReason
            // 
            txtReason.Location = new Point(263, 367);
            txtReason.Name = "txtReason";
            txtReason.Size = new Size(150, 31);
            txtReason.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(40, 367);
            label1.Name = "label1";
            label1.Size = new Size(217, 25);
            label1.TabIndex = 3;
            label1.Text = "Reason for Appointment: ";
            label1.Click += label1_Click;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(115, 443);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(182, 33);
            comboBox1.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(40, 443);
            label2.Name = "label2";
            label2.Size = new Size(69, 25);
            label2.TabIndex = 5;
            label2.Text = "Status: ";
            label2.Click += label2_Click;
            // 
            // btnSchedule
            // 
            btnSchedule.Location = new Point(40, 544);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.Size = new Size(112, 34);
            btnSchedule.TabIndex = 6;
            btnSchedule.Text = "Schedule";
            btnSchedule.UseVisualStyleBackColor = true;
            // 
            // btnUpdateStatus
            // 
            btnUpdateStatus.Location = new Point(301, 544);
            btnUpdateStatus.Name = "btnUpdateStatus";
            btnUpdateStatus.Size = new Size(176, 34);
            btnUpdateStatus.TabIndex = 7;
            btnUpdateStatus.Text = "Update Schedule";
            btnUpdateStatus.UseVisualStyleBackColor = true;
            // 
            // ucAppointmentScheduler
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnUpdateStatus);
            Controls.Add(btnSchedule);
            Controls.Add(label2);
            Controls.Add(comboBox1);
            Controls.Add(label1);
            Controls.Add(txtReason);
            Controls.Add(dtpAppointmentDateTime);
            Controls.Add(dgvAppointments);
            Name = "ucAppointmentScheduler";
            Size = new Size(733, 607);
            Load += ucAppointmentScheduler_Load;
            ((System.ComponentModel.ISupportInitialize)dgvAppointments).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvAppointments;
        private DateTimePicker dtpAppointmentDateTime;
        private TextBox txtReason;
        private Label label1;
        private ComboBox comboBox1;
        private Label label2;
        private Button btnSchedule;
        private Button btnUpdateStatus;
    }
}
