namespace DentalClinicSystem.Forms
{
    partial class ucPatientRecords
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
            dgvPatients = new DataGridView();
            txtFirstName = new TextBox();
            txtEmail = new TextBox();
            txtAddress = new TextBox();
            txtContactNumber = new TextBox();
            txtLastName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            dtpDateOfBirth = new DateTimePicker();
            label6 = new Label();
            btnAdd = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvPatients).BeginInit();
            SuspendLayout();
            // 
            // dgvPatients
            // 
            dgvPatients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPatients.Location = new Point(45, 42);
            dgvPatients.Name = "dgvPatients";
            dgvPatients.RowHeadersWidth = 62;
            dgvPatients.Size = new Size(629, 142);
            dgvPatients.TabIndex = 0;
            // 
            // txtFirstName
            // 
            txtFirstName.Location = new Point(45, 223);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(285, 31);
            txtFirstName.TabIndex = 1;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(45, 292);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(285, 31);
            txtEmail.TabIndex = 2;
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(45, 369);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(518, 31);
            txtAddress.TabIndex = 4;
            // 
            // txtContactNumber
            // 
            txtContactNumber.Location = new Point(359, 292);
            txtContactNumber.Name = "txtContactNumber";
            txtContactNumber.Size = new Size(315, 31);
            txtContactNumber.TabIndex = 3;
            // 
            // txtLastName
            // 
            txtLastName.Location = new Point(359, 225);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(315, 31);
            txtLastName.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label1.Location = new Point(45, 200);
            label1.Name = "label1";
            label1.Size = new Size(122, 22);
            label1.TabIndex = 6;
            label1.Text = "First Name: ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label2.Location = new Point(358, 200);
            label2.Name = "label2";
            label2.Size = new Size(112, 22);
            label2.TabIndex = 7;
            label2.Text = "Last Name:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label3.Location = new Point(45, 267);
            label3.Name = "label3";
            label3.Size = new Size(66, 22);
            label3.TabIndex = 8;
            label3.Text = "Email:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label4.Location = new Point(358, 267);
            label4.Name = "label4";
            label4.Size = new Size(102, 22);
            label4.TabIndex = 9;
            label4.Text = "Phone No.";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label5.Location = new Point(45, 344);
            label5.Name = "label5";
            label5.Size = new Size(90, 22);
            label5.TabIndex = 10;
            label5.Text = "Address:";
            // 
            // dtpDateOfBirth
            // 
            dtpDateOfBirth.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            dtpDateOfBirth.Format = DateTimePickerFormat.Short;
            dtpDateOfBirth.Location = new Point(45, 448);
            dtpDateOfBirth.MaxDate = new DateTime(2026, 9, 15, 13, 9, 7, 0);
            dtpDateOfBirth.Name = "dtpDateOfBirth";
            dtpDateOfBirth.Size = new Size(257, 29);
            dtpDateOfBirth.TabIndex = 11;
            dtpDateOfBirth.Value = new DateTime(2026, 9, 15, 0, 0, 0, 0);
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            label6.Location = new Point(45, 414);
            label6.Name = "label6";
            label6.Size = new Size(133, 22);
            label6.TabIndex = 12;
            label6.Text = "Date of Birth:";
            // 
            // btnAdd
            // 
            btnAdd.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnAdd.Location = new Point(36, 518);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(112, 34);
            btnAdd.TabIndex = 13;
            btnAdd.Text = "ADD";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnUpdate.Location = new Point(209, 518);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(112, 34);
            btnUpdate.TabIndex = 14;
            btnUpdate.Text = "UPDATE";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            btnDelete.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnDelete.Location = new Point(379, 518);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(112, 34);
            btnDelete.TabIndex = 15;
            btnDelete.Text = "DELETE";
            btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            btnClear.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnClear.Location = new Point(553, 518);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(112, 34);
            btnClear.TabIndex = 16;
            btnClear.Text = "CLEAR";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // ucPatientRecords
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 248, 249);
            Controls.Add(btnClear);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(btnAdd);
            Controls.Add(label6);
            Controls.Add(dtpDateOfBirth);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtLastName);
            Controls.Add(txtAddress);
            Controls.Add(txtContactNumber);
            Controls.Add(txtEmail);
            Controls.Add(txtFirstName);
            Controls.Add(dgvPatients);
            Name = "ucPatientRecords";
            Size = new Size(733, 603);
            Load += ucPatientRecords_Load;
            ((System.ComponentModel.ISupportInitialize)dgvPatients).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvPatients;
        private TextBox txtFirstName;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private TextBox txtContactNumber;
        private TextBox txtLastName;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private DateTimePicker dtpDateOfBirth;
        private Label label6;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
    }
}
