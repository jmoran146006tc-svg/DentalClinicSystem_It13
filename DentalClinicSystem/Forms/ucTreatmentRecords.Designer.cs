namespace DentalClinicSystem.Forms
{
    partial class ucTreatmentRecords
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
            dgvTreatments = new DataGridView();
            lblAppointment = new Label();
            cboAppointment = new ComboBox();
            lblTreatmentType = new Label();
            cboTreatmentType = new ComboBox();
            lblToothNumber = new Label();
            txtToothNumber = new TextBox();
            lblCost = new Label();
            txtCost = new TextBox();
            lblDatePerformed = new Label();
            dtpDatePerformed = new DateTimePicker();
            lblNotes = new Label();
            txtNotes = new TextBox();
            btnAddTreatment = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvTreatments).BeginInit();
            SuspendLayout();
            // 
            // dgvTreatments
            // 
            dgvTreatments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvTreatments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTreatments.Location = new Point(36, 16);
            dgvTreatments.Margin = new Padding(2, 2, 2, 2);
            dgvTreatments.MultiSelect = false;
            dgvTreatments.Name = "dgvTreatments";
            dgvTreatments.ReadOnly = true;
            dgvTreatments.RowHeadersWidth = 62;
            dgvTreatments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTreatments.Size = new Size(582, 128);
            dgvTreatments.TabIndex = 0;
            // 
            // lblAppointment
            // 
            lblAppointment.AutoSize = true;
            lblAppointment.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblAppointment.Location = new Point(36, 160);
            lblAppointment.Margin = new Padding(2, 0, 2, 0);
            lblAppointment.Name = "lblAppointment";
            lblAppointment.Size = new Size(108, 18);
            lblAppointment.TabIndex = 12;
            lblAppointment.Text = "Appointment:";
            // 
            // cboAppointment
            // 
            cboAppointment.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAppointment.Location = new Point(36, 180);
            cboAppointment.Margin = new Padding(2, 2, 2, 2);
            cboAppointment.Name = "cboAppointment";
            cboAppointment.Size = new Size(225, 28);
            cboAppointment.TabIndex = 11;
            // 
            // lblTreatmentType
            // 
            lblTreatmentType.AutoSize = true;
            lblTreatmentType.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblTreatmentType.Location = new Point(310, 160);
            lblTreatmentType.Margin = new Padding(2, 0, 2, 0);
            lblTreatmentType.Name = "lblTreatmentType";
            lblTreatmentType.Size = new Size(45, 18);
            lblTreatmentType.TabIndex = 10;
            lblTreatmentType.Text = "Type:";
            // 
            // cboTreatmentType
            // 
            cboTreatmentType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTreatmentType.Location = new Point(310, 180);
            cboTreatmentType.Margin = new Padding(2, 2, 2, 2);
            cboTreatmentType.Name = "cboTreatmentType";
            cboTreatmentType.Size = new Size(218, 28);
            cboTreatmentType.TabIndex = 9;
            // 
            // lblToothNumber
            // 
            lblToothNumber.AutoSize = true;
            lblToothNumber.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblToothNumber.Location = new Point(36, 225);
            lblToothNumber.Margin = new Padding(2, 0, 2, 0);
            lblToothNumber.Name = "lblToothNumber";
            lblToothNumber.Size = new Size(67, 18);
            lblToothNumber.TabIndex = 8;
            lblToothNumber.Text = "Tooth #:";
            // 
            // txtToothNumber
            // 
            txtToothNumber.Location = new Point(114, 221);
            txtToothNumber.Margin = new Padding(2, 2, 2, 2);
            txtToothNumber.Name = "txtToothNumber";
            txtToothNumber.Size = new Size(81, 27);
            txtToothNumber.TabIndex = 7;
            // 
            // lblCost
            // 
            lblCost.AutoSize = true;
            lblCost.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblCost.Location = new Point(211, 225);
            lblCost.Margin = new Padding(2, 0, 2, 0);
            lblCost.Name = "lblCost";
            lblCost.Size = new Size(46, 18);
            lblCost.TabIndex = 6;
            lblCost.Text = "Cost:";
            // 
            // txtCost
            // 
            txtCost.Location = new Point(265, 221);
            txtCost.Margin = new Padding(2, 2, 2, 2);
            txtCost.Name = "txtCost";
            txtCost.Size = new Size(97, 27);
            txtCost.TabIndex = 5;
            // 
            // lblDatePerformed
            // 
            lblDatePerformed.AutoSize = true;
            lblDatePerformed.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDatePerformed.Location = new Point(379, 225);
            lblDatePerformed.Margin = new Padding(2, 0, 2, 0);
            lblDatePerformed.Name = "lblDatePerformed";
            lblDatePerformed.Size = new Size(47, 18);
            lblDatePerformed.TabIndex = 4;
            lblDatePerformed.Text = "Date:";
            // 
            // dtpDatePerformed
            // 
            dtpDatePerformed.Format = DateTimePickerFormat.Short;
            dtpDatePerformed.Location = new Point(434, 221);
            dtpDatePerformed.Margin = new Padding(2, 2, 2, 2);
            dtpDatePerformed.Name = "dtpDatePerformed";
            dtpDatePerformed.Size = new Size(129, 27);
            dtpDatePerformed.TabIndex = 3;
            // 
            // lblNotes
            // 
            lblNotes.AutoSize = true;
            lblNotes.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblNotes.Location = new Point(36, 270);
            lblNotes.Margin = new Padding(2, 0, 2, 0);
            lblNotes.Name = "lblNotes";
            lblNotes.Size = new Size(57, 18);
            lblNotes.TabIndex = 2;
            lblNotes.Text = "Notes:";
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(36, 290);
            txtNotes.Margin = new Padding(2, 2, 2, 2);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(437, 98);
            txtNotes.TabIndex = 1;
            // 
            // btnAddTreatment
            // 
            btnAddTreatment.BackColor = Color.FromArgb(66, 202, 207);
            btnAddTreatment.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnAddTreatment.ForeColor = Color.FromArgb(243, 248, 249);
            btnAddTreatment.Location = new Point(36, 400);
            btnAddTreatment.Margin = new Padding(2, 2, 2, 2);
            btnAddTreatment.Name = "btnAddTreatment";
            btnAddTreatment.Size = new Size(181, 27);
            btnAddTreatment.TabIndex = 0;
            btnAddTreatment.Text = "Add Treatment";
            btnAddTreatment.UseVisualStyleBackColor = false;
            btnAddTreatment.Click += btnAddTreatment_Click;
            // 
            // ucTreatmentRecords
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 248, 249);
            Controls.Add(btnAddTreatment);
            Controls.Add(txtNotes);
            Controls.Add(lblNotes);
            Controls.Add(dtpDatePerformed);
            Controls.Add(lblDatePerformed);
            Controls.Add(txtCost);
            Controls.Add(lblCost);
            Controls.Add(txtToothNumber);
            Controls.Add(lblToothNumber);
            Controls.Add(cboTreatmentType);
            Controls.Add(lblTreatmentType);
            Controls.Add(cboAppointment);
            Controls.Add(lblAppointment);
            Controls.Add(dgvTreatments);
            Margin = new Padding(2, 2, 2, 2);
            Name = "ucTreatmentRecords";
            Size = new Size(648, 470);
            Load += ucTreatmentRecords_Load;
            ((System.ComponentModel.ISupportInitialize)dgvTreatments).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvTreatments;
        private Label lblAppointment;
        private ComboBox cboAppointment;
        private Label lblTreatmentType;
        private ComboBox cboTreatmentType;
        private Label lblToothNumber;
        private TextBox txtToothNumber;
        private Label lblCost;
        private TextBox txtCost;
        private Label lblDatePerformed;
        private DateTimePicker dtpDatePerformed;
        private Label lblNotes;
        private TextBox txtNotes;
        private Button btnAddTreatment;
    }
}
