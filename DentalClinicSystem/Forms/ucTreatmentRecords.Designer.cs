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
            dgvTreatments.Location = new Point(45, 20);
            dgvTreatments.MultiSelect = false;
            dgvTreatments.Name = "dgvTreatments";
            dgvTreatments.ReadOnly = true;
            dgvTreatments.RowHeadersWidth = 62;
            dgvTreatments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTreatments.Size = new Size(727, 160);
            dgvTreatments.TabIndex = 0;
            // 
            // lblAppointment
            // 
            lblAppointment.AutoSize = true;
            lblAppointment.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblAppointment.Location = new Point(45, 200);
            lblAppointment.Name = "lblAppointment";
            lblAppointment.Size = new Size(130, 22);
            lblAppointment.TabIndex = 12;
            lblAppointment.Text = "Appointment:";
            // 
            // cboAppointment
            // 
            cboAppointment.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAppointment.Location = new Point(45, 225);
            cboAppointment.Name = "cboAppointment";
            cboAppointment.Size = new Size(280, 33);
            cboAppointment.TabIndex = 11;
            // 
            // lblTreatmentType
            // 
            lblTreatmentType.AutoSize = true;
            lblTreatmentType.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblTreatmentType.Location = new Point(387, 200);
            lblTreatmentType.Name = "lblTreatmentType";
            lblTreatmentType.Size = new Size(56, 22);
            lblTreatmentType.TabIndex = 10;
            lblTreatmentType.Text = "Type:";
            // 
            // cboTreatmentType
            // 
            cboTreatmentType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTreatmentType.Location = new Point(387, 225);
            cboTreatmentType.Name = "cboTreatmentType";
            cboTreatmentType.Size = new Size(272, 33);
            cboTreatmentType.TabIndex = 9;
            // 
            // lblToothNumber
            // 
            lblToothNumber.AutoSize = true;
            lblToothNumber.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblToothNumber.Location = new Point(45, 281);
            lblToothNumber.Name = "lblToothNumber";
            lblToothNumber.Size = new Size(82, 22);
            lblToothNumber.TabIndex = 8;
            lblToothNumber.Text = "Tooth #:";
            // 
            // txtToothNumber
            // 
            txtToothNumber.Location = new Point(143, 276);
            txtToothNumber.Name = "txtToothNumber";
            txtToothNumber.Size = new Size(100, 31);
            txtToothNumber.TabIndex = 7;
            // 
            // lblCost
            // 
            lblCost.AutoSize = true;
            lblCost.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblCost.Location = new Point(264, 281);
            lblCost.Name = "lblCost";
            lblCost.Size = new Size(55, 22);
            lblCost.TabIndex = 6;
            lblCost.Text = "Cost:";
            // 
            // txtCost
            // 
            txtCost.Location = new Point(331, 276);
            txtCost.Name = "txtCost";
            txtCost.Size = new Size(120, 31);
            txtCost.TabIndex = 5;
            // 
            // lblDatePerformed
            // 
            lblDatePerformed.AutoSize = true;
            lblDatePerformed.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDatePerformed.Location = new Point(474, 281);
            lblDatePerformed.Name = "lblDatePerformed";
            lblDatePerformed.Size = new Size(57, 22);
            lblDatePerformed.TabIndex = 4;
            lblDatePerformed.Text = "Date:";
            // 
            // dtpDatePerformed
            // 
            dtpDatePerformed.Format = DateTimePickerFormat.Short;
            dtpDatePerformed.Location = new Point(542, 276);
            dtpDatePerformed.Name = "dtpDatePerformed";
            dtpDatePerformed.Size = new Size(160, 31);
            dtpDatePerformed.TabIndex = 3;
            // 
            // lblNotes
            // 
            lblNotes.AutoSize = true;
            lblNotes.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblNotes.Location = new Point(45, 337);
            lblNotes.Name = "lblNotes";
            lblNotes.Size = new Size(68, 22);
            lblNotes.TabIndex = 2;
            lblNotes.Text = "Notes:";
            lblNotes.Click += lblNotes_Click;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(45, 362);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(545, 122);
            txtNotes.TabIndex = 1;
            // 
            // btnAddTreatment
            // 
            btnAddTreatment.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnAddTreatment.ForeColor = Color.FromArgb(66, 202, 207);
            btnAddTreatment.Location = new Point(45, 500);
            btnAddTreatment.Name = "btnAddTreatment";
            btnAddTreatment.Size = new Size(226, 34);
            btnAddTreatment.TabIndex = 0;
            btnAddTreatment.Text = "Add Treatment";
            btnAddTreatment.UseVisualStyleBackColor = true;
            btnAddTreatment.Click += btnAddTreatment_Click;
            // 
            // ucTreatmentRecords
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
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
            Name = "ucTreatmentRecords";
            Size = new Size(810, 588);
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
