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
            dgvTreatments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTreatments.Location = new Point(45, 20);
            dgvTreatments.Name = "dgvTreatments";
            dgvTreatments.RowHeadersWidth = 62;
            dgvTreatments.Size = new Size(650, 160);
            dgvTreatments.TabIndex = 0;
            dgvTreatments.ReadOnly = true;
            dgvTreatments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTreatments.MultiSelect = false;
            dgvTreatments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            //
            // lblAppointment
            //
            lblAppointment.AutoSize = true;
            lblAppointment.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblAppointment.Location = new Point(45, 200);
            lblAppointment.Name = "lblAppointment";
            lblAppointment.Text = "APPOINTMENT:";
            //
            // cboAppointment
            //
            cboAppointment.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAppointment.Location = new Point(180, 196);
            cboAppointment.Name = "cboAppointment";
            cboAppointment.Size = new Size(280, 31);
            //
            // lblTreatmentType
            //
            lblTreatmentType.AutoSize = true;
            lblTreatmentType.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblTreatmentType.Location = new Point(480, 200);
            lblTreatmentType.Name = "lblTreatmentType";
            lblTreatmentType.Text = "TYPE:";
            //
            // cboTreatmentType
            //
            cboTreatmentType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTreatmentType.Location = new Point(535, 196);
            cboTreatmentType.Name = "cboTreatmentType";
            cboTreatmentType.Size = new Size(160, 31);
            //
            // lblToothNumber
            //
            lblToothNumber.AutoSize = true;
            lblToothNumber.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblToothNumber.Location = new Point(45, 250);
            lblToothNumber.Name = "lblToothNumber";
            lblToothNumber.Text = "TOOTH #:";
            //
            // txtToothNumber
            //
            txtToothNumber.Location = new Point(150, 246);
            txtToothNumber.Name = "txtToothNumber";
            txtToothNumber.Size = new Size(100, 31);
            //
            // lblCost
            //
            lblCost.AutoSize = true;
            lblCost.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblCost.Location = new Point(280, 250);
            lblCost.Name = "lblCost";
            lblCost.Text = "COST:";
            //
            // txtCost
            //
            txtCost.Location = new Point(340, 246);
            txtCost.Name = "txtCost";
            txtCost.Size = new Size(120, 31);
            //
            // lblDatePerformed
            //
            lblDatePerformed.AutoSize = true;
            lblDatePerformed.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDatePerformed.Location = new Point(480, 250);
            lblDatePerformed.Name = "lblDatePerformed";
            lblDatePerformed.Text = "DATE:";
            //
            // dtpDatePerformed
            //
            dtpDatePerformed.Format = DateTimePickerFormat.Short;
            dtpDatePerformed.Location = new Point(535, 246);
            dtpDatePerformed.Name = "dtpDatePerformed";
            dtpDatePerformed.Size = new Size(160, 31);
            //
            // lblNotes
            //
            lblNotes.AutoSize = true;
            lblNotes.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblNotes.Location = new Point(45, 300);
            lblNotes.Name = "lblNotes";
            lblNotes.Text = "NOTES:";
            //
            // txtNotes
            //
            txtNotes.Location = new Point(150, 296);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(545, 60);
            //
            // btnAddTreatment
            //
            btnAddTreatment.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnAddTreatment.Location = new Point(45, 375);
            btnAddTreatment.Name = "btnAddTreatment";
            btnAddTreatment.Size = new Size(160, 34);
            btnAddTreatment.Text = "ADD TREATMENT";
            btnAddTreatment.UseVisualStyleBackColor = true;
            btnAddTreatment.Click += btnAddTreatment_Click;
            //
            // ucTreatmentRecords
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
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
            Size = new Size(733, 440);
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