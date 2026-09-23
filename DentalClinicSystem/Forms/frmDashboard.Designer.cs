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
            lblWelcome = new Label();
            pnlSidebar.SuspendLayout();
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
            pnlSidebar.Margin = new Padding(2, 2, 2, 2);
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
            btnDashboard.Margin = new Padding(2, 2, 2, 2);
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
            btnPatients.Location = new Point(0, 40);
            btnPatients.Margin = new Padding(2, 2, 2, 2);
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
            btnDentists.Location = new Point(0, 80);
            btnDentists.Margin = new Padding(2, 2, 2, 2);
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
            btnAppointments.Location = new Point(0, 120);
            btnAppointments.Margin = new Padding(2, 2, 2, 2);
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
            btnTreatments.Location = new Point(0, 160);
            btnTreatments.Margin = new Padding(2, 2, 2, 2);
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
            btnUsers.Location = new Point(0, 200);
            btnUsers.Margin = new Padding(2, 2, 2, 2);
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
            btnLogout.Margin = new Padding(2, 2, 2, 2);
            btnLogout.Name = "btnLogout";
            btnLogout.Padding = new Padding(12, 0, 0, 0);
            btnLogout.Size = new Size(160, 36);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "Logout";
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(243, 248, 249);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(160, 0);
            pnlContent.Margin = new Padding(2, 2, 2, 2);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(640, 520);
            pnlContent.TabIndex = 1;
            pnlContent.Paint += pnlContent_Paint;
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
            // frmDashboard
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 520);
            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2, 2, 2, 2);
            Name = "frmDashboard";
            Text = "Dental Clinic Management System - Dashboard";
            WindowState = FormWindowState.Maximized;
            pnlSidebar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}
