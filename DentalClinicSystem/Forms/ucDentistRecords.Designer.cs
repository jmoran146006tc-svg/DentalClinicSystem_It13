namespace DentalClinicSystem.Forms
{
    partial class ucDentistRecords
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
            dgvDentists = new DataGridView();
            label2 = new Label();
            label1 = new Label();
            txtLastName = new TextBox();
            txtFirstName = new TextBox();
            label3 = new Label();
            label4 = new Label();
            txtContactNumber = new TextBox();
            txtSpecialization = new TextBox();
            label5 = new Label();
            txtLicenseNumber = new TextBox();
            btnClear = new Button();
            btnDelete = new Button();
            btnUpdate = new Button();
            btnAdd = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvDentists).BeginInit();
            SuspendLayout();
            // 
            // dgvDentists
            // 
            dgvDentists.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDentists.Location = new Point(51, 39);
            dgvDentists.Name = "dgvDentists";
            dgvDentists.RowHeadersWidth = 62;
            dgvDentists.Size = new Size(629, 142);
            dgvDentists.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label2.Location = new Point(405, 234);
            label2.Name = "label2";
            label2.Size = new Size(126, 22);
            label2.TabIndex = 11;
            label2.Text = "LAST NAME: ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label1.Location = new Point(52, 232);
            label1.Name = "label1";
            label1.Size = new Size(133, 22);
            label1.TabIndex = 10;
            label1.Text = "FIRST NAME: ";
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(531, 227);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(150, 31);
            txtLastName.TabIndex = 9;
            // 
            // txtFirstName
            // 
            txtFirstName.Location = new Point(187, 227);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(150, 31);
            txtFirstName.TabIndex = 8;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label3.Location = new Point(394, 300);
            label3.Name = "label3";
            label3.Size = new Size(131, 22);
            label3.TabIndex = 15;
            label3.Text = "CONTACT NO.";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label4.Location = new Point(52, 300);
            label4.Name = "label4";
            label4.Size = new Size(166, 22);
            label4.TabIndex = 14;
            label4.Text = "SPECIALIZATION:";
            // 
            // txtContactNumber
            // 
            txtContactNumber.Location = new Point(531, 295);
            txtContactNumber.Name = "txtContactNumber";
            txtContactNumber.Size = new Size(150, 31);
            txtContactNumber.TabIndex = 13;
            // 
            // txtSpecialization
            // 
            txtSpecialization.Location = new Point(224, 297);
            txtSpecialization.Name = "txtSpecialization";
            txtSpecialization.Size = new Size(150, 31);
            txtSpecialization.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label5.Location = new Point(52, 367);
            label5.Name = "label5";
            label5.Size = new Size(180, 22);
            label5.TabIndex = 17;
            label5.Text = "LICENSE NUMBER:";
            // 
            // txtLicenseNumber
            // 
            txtLicenseNumber.Location = new Point(238, 362);
            txtLicenseNumber.Name = "txtLicenseNumber";
            txtLicenseNumber.Size = new Size(150, 31);
            txtLicenseNumber.TabIndex = 16;
            // 
            // btnClear
            // 
            btnClear.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnClear.Location = new Point(568, 505);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(112, 34);
            btnClear.TabIndex = 21;
            btnClear.Text = "CLEAR";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnDelete.Location = new Point(394, 505);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(112, 34);
            btnDelete.TabIndex = 20;
            btnDelete.Text = "DELETE";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnUpdate.Location = new Point(224, 505);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(112, 34);
            btnUpdate.TabIndex = 19;
            btnUpdate.Text = "UPDATE";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            btnAdd.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnAdd.Location = new Point(51, 505);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(112, 34);
            btnAdd.TabIndex = 18;
            btnAdd.Text = "ADD";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // ucDentistRecords
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnClear);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(btnAdd);
            Controls.Add(label5);
            Controls.Add(txtLicenseNumber);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(txtContactNumber);
            Controls.Add(txtSpecialization);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtLastName);
            Controls.Add(txtFirstName);
            Controls.Add(dgvDentists);
            Name = "ucDentistRecords";
            Size = new Size(729, 586);
            Load += ucDentistRecords_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDentists).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvDentists;
        private Label label2;
        private Label label1;
        private TextBox txtLastName;
        private TextBox txtFirstName;
        private Label label3;
        private Label label4;
        private TextBox txtContactNumber;
        private TextBox txtSpecialization;
        private Label label5;
        private TextBox txtLicenseNumber;
        private Button btnClear;
        private Button btnDelete;
        private Button btnUpdate;
        private Button btnAdd;
    }
}
