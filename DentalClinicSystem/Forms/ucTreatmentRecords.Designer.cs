namespace DentalClinicSystem.Forms
{
    partial class ucTreatmentRecords
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
            dgvTreatments = new DataGridView();
            cboAppointment = new ComboBox();
            txtToothNumber = new TextBox();
            txtCost = new TextBox();
            txtNotes = new TextBox();
            dtpDatePerformed = new DateTimePicker();
            btnAddTreatment = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvTreatments).BeginInit();
            SuspendLayout();
            // 
            // dgvTreatments
            // 
            dgvTreatments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTreatments.Location = new Point(28, 32);
            dgvTreatments.Name = "dgvTreatments";
            dgvTreatments.RowHeadersWidth = 62;
            dgvTreatments.Size = new Size(702, 225);
            dgvTreatments.TabIndex = 0;
            // 
            // cboAppointment
            // 
            cboAppointment.FormattingEnabled = true;
            cboAppointment.Location = new Point(141, 287);
            cboAppointment.Name = "cboAppointment";
            cboAppointment.Size = new Size(182, 33);
            cboAppointment.TabIndex = 1;
            // 
            // txtToothNumber
            // 
            txtToothNumber.Location = new Point(98, 380);
            txtToothNumber.Name = "txtToothNumber";
            txtToothNumber.Size = new Size(150, 31);
            txtToothNumber.TabIndex = 2;
            // 
            // txtCost
            // 
            txtCost.Location = new Point(371, 380);
            txtCost.Name = "txtCost";
            txtCost.Size = new Size(150, 31);
            txtCost.TabIndex = 3;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(580, 380);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(150, 31);
            txtNotes.TabIndex = 4;
            // 
            // dtpDatePerformed
            // 
            dtpDatePerformed.Location = new Point(207, 542);
            dtpDatePerformed.Name = "dtpDatePerformed";
            dtpDatePerformed.Size = new Size(300, 31);
            dtpDatePerformed.TabIndex = 5;
            // 
            // btnAddTreatment
            // 
            btnAddTreatment.Location = new Point(122, 657);
            btnAddTreatment.Name = "btnAddTreatment";
            btnAddTreatment.Size = new Size(112, 34);
            btnAddTreatment.TabIndex = 8;
            btnAddTreatment.Text = "Add Treatment";
            btnAddTreatment.UseMnemonic = false;
            btnAddTreatment.UseVisualStyleBackColor = true;
            // 
            // ucTreatmentRecords
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnAddTreatment);
            Controls.Add(dtpDatePerformed);
            Controls.Add(txtNotes);
            Controls.Add(txtCost);
            Controls.Add(txtToothNumber);
            Controls.Add(cboAppointment);
            Controls.Add(dgvTreatments);
            Name = "ucTreatmentRecords";
            Size = new Size(770, 750);
            ((System.ComponentModel.ISupportInitialize)dgvTreatments).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvTreatments;
        private ComboBox cboAppointment;
        private TextBox txtToothNumber;
        private TextBox txtCost;
        private TextBox txtNotes;
        private DateTimePicker dtpDatePerformed;
        private Button btnAddTreatment;
    }
}
