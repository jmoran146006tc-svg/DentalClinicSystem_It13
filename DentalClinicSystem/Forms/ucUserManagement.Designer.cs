namespace DentalClinicSystem.Forms
{
    partial class ucUserManagement
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
            dgvUsers = new DataGridView();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblRole = new Label();
            cboRole = new ComboBox();
            lblDentist = new Label();
            cboDentist = new ComboBox();
            btnAdd = new Button();
            btnUpdate = new Button();
            btnDeactivate = new Button();
            btnClear = new Button();
            lblAdminOnly = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvUsers).BeginInit();
            SuspendLayout();
            // 
            // dgvUsers
            // 
            dgvUsers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsers.Location = new Point(24, 14);
            dgvUsers.MultiSelect = false;
            dgvUsers.Name = "dgvUsers";
            dgvUsers.ReadOnly = true;
            dgvUsers.RowHeadersWidth = 62;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.Size = new Size(664, 170);
            dgvUsers.TabIndex = 0;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblUsername.Location = new Point(45, 210);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(119, 22);
            lblUsername.TabIndex = 12;
            lblUsername.Text = "USERNAME:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(45, 235);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(245, 31);
            txtUsername.TabIndex = 11;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblPassword.Location = new Point(347, 210);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(116, 22);
            lblPassword.TabIndex = 10;
            lblPassword.Text = "PASSWORD:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(348, 235);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(252, 31);
            txtPassword.TabIndex = 9;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblRole
            // 
            lblRole.AutoSize = true;
            lblRole.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblRole.Location = new Point(45, 279);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(64, 22);
            lblRole.TabIndex = 8;
            lblRole.Text = "ROLE:";
            // 
            // cboRole
            // 
            cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRole.Location = new Point(45, 304);
            cboRole.Name = "cboRole";
            cboRole.Size = new Size(245, 33);
            cboRole.TabIndex = 7;
            // 
            // lblDentist
            // 
            lblDentist.AutoSize = true;
            lblDentist.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            lblDentist.Location = new Point(348, 279);
            lblDentist.Name = "lblDentist";
            lblDentist.Size = new Size(92, 22);
            lblDentist.TabIndex = 6;
            lblDentist.Text = "DENTIST:";
            // 
            // cboDentist
            // 
            cboDentist.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDentist.Enabled = false;
            cboDentist.Location = new Point(348, 304);
            cboDentist.Name = "cboDentist";
            cboDentist.Size = new Size(260, 33);
            cboDentist.TabIndex = 5;
            // 
            // btnAdd
            // 
            btnAdd.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnAdd.Location = new Point(24, 384);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(112, 34);
            btnAdd.TabIndex = 4;
            btnAdd.Text = "ADD";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnUpdate.Location = new Point(197, 384);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(112, 34);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "UPDATE";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnDeactivate
            // 
            btnDeactivate.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnDeactivate.Location = new Point(370, 384);
            btnDeactivate.Name = "btnDeactivate";
            btnDeactivate.Size = new Size(137, 34);
            btnDeactivate.TabIndex = 2;
            btnDeactivate.Text = "DEACTIVATE";
            btnDeactivate.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            btnClear.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnClear.Location = new Point(549, 384);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(112, 34);
            btnClear.TabIndex = 1;
            btnClear.Text = "CLEAR";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // lblAdminOnly
            // 
            lblAdminOnly.AutoSize = true;
            lblAdminOnly.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold | FontStyle.Italic);
            lblAdminOnly.ForeColor = Color.Firebrick;
            lblAdminOnly.Location = new Point(24, 373);
            lblAdminOnly.Name = "lblAdminOnly";
            lblAdminOnly.Size = new Size(0, 22);
            lblAdminOnly.TabIndex = 0;
            lblAdminOnly.Visible = false;
            // 
            // ucUserManagement
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 248, 249);
            Controls.Add(lblAdminOnly);
            Controls.Add(btnClear);
            Controls.Add(btnDeactivate);
            Controls.Add(btnUpdate);
            Controls.Add(btnAdd);
            Controls.Add(cboDentist);
            Controls.Add(lblDentist);
            Controls.Add(cboRole);
            Controls.Add(lblRole);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblUsername);
            Controls.Add(dgvUsers);
            Name = "ucUserManagement";
            Size = new Size(723, 484);
            Load += ucUserManagement_Load;
            ((System.ComponentModel.ISupportInitialize)dgvUsers).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvUsers;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblRole;
        private ComboBox cboRole;
        private Label lblDentist;
        private ComboBox cboDentist;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDeactivate;
        private Button btnClear;
        private Label lblAdminOnly;
    }
}