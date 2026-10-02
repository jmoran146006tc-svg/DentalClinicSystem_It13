namespace DentalClinicSystem.Forms
{
    partial class frmDashboard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnPatients;
        private System.Windows.Forms.Button btnDentists;
        private System.Windows.Forms.Button btnAppointments;
        private System.Windows.Forms.Button btnTreatments;
        private System.Windows.Forms.Button btnUsers;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblWelcome;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDashboard));
            pnlSidebar = new Panel();
            btnDashboard = new Button();
            btnPatients = new Button();
            btnDentists = new Button();
            btnAppointments = new Button();
            btnTreatments = new Button();
            btnUsers = new Button();
            btnLogout = new Button();
            pnlContent = new Panel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblWc = new Label();
            _lblClock = new Label();
            lblDateTime = new Label();
            lblWelcome = new Label();
            timerClock = new System.Windows.Forms.Timer(components);
            pnlSidebar.SuspendLayout();
            pnlContent.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(230, 236, 245);
            pnlSidebar.Controls.Add(btnDashboard);
            pnlSidebar.Controls.Add(btnPatients);
            pnlSidebar.Controls.Add(btnDentists);
            pnlSidebar.Controls.Add(btnAppointments);
            pnlSidebar.Controls.Add(btnTreatments);
            pnlSidebar.Controls.Add(btnUsers);
            pnlSidebar.Controls.Add(btnLogout);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Margin = new Padding(2);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(160, 520);
            pnlSidebar.TabIndex = 0;
            // 
            // btnDashboard
            // 
            btnDashboard.BackColor = Color.FromArgb(66, 202, 207);
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDashboard.ForeColor = Color.White;
            btnDashboard.Location = new Point(0, 0);
            btnDashboard.Margin = new Padding(2);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Padding = new Padding(12, 0, 0, 0);
            btnDashboard.Size = new Size(160, 36);
            btnDashboard.TabIndex = 0;
            btnDashboard.Text = "Dashboard";
            btnDashboard.TextAlign = ContentAlignment.MiddleLeft;
            btnDashboard.UseVisualStyleBackColor = false;
            // 
            // btnPatients
            // 
            btnPatients.BackColor = Color.FromArgb(66, 202, 207);
            btnPatients.FlatStyle = FlatStyle.Flat;
            btnPatients.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPatients.ForeColor = Color.White;
            btnPatients.Location = new Point(0, 35);
            btnPatients.Margin = new Padding(2);
            btnPatients.Name = "btnPatients";
            btnPatients.Padding = new Padding(12, 0, 0, 0);
            btnPatients.Size = new Size(160, 36);
            btnPatients.TabIndex = 1;
            btnPatients.Text = "Patients";
            btnPatients.TextAlign = ContentAlignment.MiddleLeft;
            btnPatients.UseVisualStyleBackColor = false;
            // 
            // btnDentists
            // 
            btnDentists.BackColor = Color.FromArgb(66, 202, 207);
            btnDentists.FlatStyle = FlatStyle.Flat;
            btnDentists.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDentists.ForeColor = Color.White;
            btnDentists.Location = new Point(0, 70);
            btnDentists.Margin = new Padding(2);
            btnDentists.Name = "btnDentists";
            btnDentists.Padding = new Padding(12, 0, 0, 0);
            btnDentists.Size = new Size(160, 36);
            btnDentists.TabIndex = 2;
            btnDentists.Text = "Dentists";
            btnDentists.TextAlign = ContentAlignment.MiddleLeft;
            btnDentists.UseVisualStyleBackColor = false;
            // 
            // btnAppointments
            // 
            btnAppointments.BackColor = Color.FromArgb(66, 202, 207);
            btnAppointments.FlatStyle = FlatStyle.Flat;
            btnAppointments.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAppointments.ForeColor = Color.White;
            btnAppointments.Location = new Point(0, 105);
            btnAppointments.Margin = new Padding(2);
            btnAppointments.Name = "btnAppointments";
            btnAppointments.Padding = new Padding(12, 0, 0, 0);
            btnAppointments.Size = new Size(160, 36);
            btnAppointments.TabIndex = 3;
            btnAppointments.Text = "Appointments";
            btnAppointments.TextAlign = ContentAlignment.MiddleLeft;
            btnAppointments.UseVisualStyleBackColor = false;
            // 
            // btnTreatments
            // 
            btnTreatments.BackColor = Color.FromArgb(66, 202, 207);
            btnTreatments.FlatStyle = FlatStyle.Flat;
            btnTreatments.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTreatments.ForeColor = Color.White;
            btnTreatments.Location = new Point(0, 140);
            btnTreatments.Margin = new Padding(2);
            btnTreatments.Name = "btnTreatments";
            btnTreatments.Padding = new Padding(12, 0, 0, 0);
            btnTreatments.Size = new Size(160, 36);
            btnTreatments.TabIndex = 4;
            btnTreatments.Text = "Treatments";
            btnTreatments.TextAlign = ContentAlignment.MiddleLeft;
            btnTreatments.UseVisualStyleBackColor = false;
            // 
            // btnUsers
            // 
            btnUsers.BackColor = Color.FromArgb(66, 202, 207);
            btnUsers.FlatStyle = FlatStyle.Flat;
            btnUsers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnUsers.ForeColor = Color.White;
            btnUsers.Location = new Point(2, 175);
            btnUsers.Margin = new Padding(2);
            btnUsers.Name = "btnUsers";
            btnUsers.Padding = new Padding(12, 0, 0, 0);
            btnUsers.Size = new Size(160, 36);
            btnUsers.TabIndex = 5;
            btnUsers.Text = "Users";
            btnUsers.TextAlign = ContentAlignment.MiddleLeft;
            btnUsers.UseVisualStyleBackColor = false;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.FromArgb(66, 202, 207);
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogout.ForeColor = Color.White;
            btnLogout.Location = new Point(0, 484);
            btnLogout.Margin = new Padding(2);
            btnLogout.Name = "btnLogout";
            btnLogout.Padding = new Padding(12, 0, 0, 0);
            btnLogout.Size = new Size(160, 36);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "Logout";
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.UseVisualStyleBackColor = false;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(243, 248, 249);
            pnlContent.Controls.Add(tableLayoutPanel2);
            pnlContent.Controls.Add(tableLayoutPanel1);
            pnlContent.Controls.Add(lblDateTime);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(160, 0);
            pnlContent.Margin = new Padding(2);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(640, 520);
            pnlContent.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 5;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.Location = new Point(51, 70);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(487, 125);
            tableLayoutPanel2.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(lblWc, 0, 0);
            tableLayoutPanel1.Controls.Add(_lblClock, 1, 0);
            tableLayoutPanel1.Location = new Point(18, 21);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(599, 35);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // lblWc
            // 
            lblWc.AutoSize = true;
            lblWc.Location = new Point(3, 0);
            lblWc.Name = "lblWc";
            lblWc.Size = new Size(78, 20);
            lblWc.TabIndex = 0;
            lblWc.Text = "Welcome, ";
            // 
            // _lblClock
            // 
            _lblClock.AutoSize = true;
            _lblClock.Location = new Point(302, 0);
            _lblClock.Name = "_lblClock";
            _lblClock.Size = new Size(202, 20);
            _lblClock.TabIndex = 2;
            _lblClock.Text = "Monday, September 28, 2026";
            // 
            // lblDateTime
            // 
            lblDateTime.AutoSize = true;
            lblDateTime.Font = new Font("Segoe UI", 10F);
            lblDateTime.Location = new Point(538, 19);
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Size = new Size(0, 23);
            lblDateTime.TabIndex = 1;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 18F, FontStyle.Bold | FontStyle.Italic);
            lblWelcome.Location = new Point(30, 30);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(0, 32);
            lblWelcome.TabIndex = 0;
            // 
            // timerClock
            // 
            timerClock.Enabled = true;
            timerClock.Interval = 1000;
            timerClock.Tick += timer1_Tick;
            // 
            // frmDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 520);
            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            Name = "frmDashboard";
            Text = "Dental Clinic Management System - Dashboard";
            WindowState = FormWindowState.Maximized;
            pnlSidebar.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblDateTime;
        private System.Windows.Forms.Timer timerClock;
        private Label lblWc;
        private Label _lblClock;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
    }
}
