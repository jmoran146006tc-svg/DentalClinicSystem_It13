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
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(200, 650);
            pnlSidebar.TabIndex = 0;
            // 
            // btnDashboard
            // 
            btnDashboard.FlatStyle = FlatStyle.Flat;
            btnDashboard.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDashboard.Location = new Point(0, 0);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Padding = new Padding(15, 0, 0, 0);
            btnDashboard.Size = new Size(200, 45);
            btnDashboard.TabIndex = 0;
            btnDashboard.Text = "Dashboard";
            btnDashboard.TextAlign = ContentAlignment.MiddleLeft;
            btnDashboard.UseVisualStyleBackColor = true;
            // 
            // btnPatients
            // 
            btnPatients.FlatStyle = FlatStyle.Flat;
            btnPatients.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPatients.Location = new Point(0, 50);
            btnPatients.Name = "btnPatients";
            btnPatients.Padding = new Padding(15, 0, 0, 0);
            btnPatients.Size = new Size(200, 45);
            btnPatients.TabIndex = 1;
            btnPatients.Text = "Patients";
            btnPatients.TextAlign = ContentAlignment.MiddleLeft;
            btnPatients.UseVisualStyleBackColor = true;
            // 
            // btnDentists
            // 
            btnDentists.FlatStyle = FlatStyle.Flat;
            btnDentists.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDentists.Location = new Point(0, 100);
            btnDentists.Name = "btnDentists";
            btnDentists.Padding = new Padding(15, 0, 0, 0);
            btnDentists.Size = new Size(200, 45);
            btnDentists.TabIndex = 2;
            btnDentists.Text = "Dentists";
            btnDentists.TextAlign = ContentAlignment.MiddleLeft;
            btnDentists.UseVisualStyleBackColor = true;
            // 
            // btnAppointments
            // 
            btnAppointments.FlatStyle = FlatStyle.Flat;
            btnAppointments.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAppointments.Location = new Point(0, 150);
            btnAppointments.Name = "btnAppointments";
            btnAppointments.Padding = new Padding(15, 0, 0, 0);
            btnAppointments.Size = new Size(200, 45);
            btnAppointments.TabIndex = 3;
            btnAppointments.Text = "Appointments";
            btnAppointments.TextAlign = ContentAlignment.MiddleLeft;
            btnAppointments.UseVisualStyleBackColor = true;
            // 
            // btnTreatments
            // 
            btnTreatments.FlatStyle = FlatStyle.Flat;
            btnTreatments.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTreatments.Location = new Point(0, 200);
            btnTreatments.Name = "btnTreatments";
            btnTreatments.Padding = new Padding(15, 0, 0, 0);
            btnTreatments.Size = new Size(200, 45);
            btnTreatments.TabIndex = 4;
            btnTreatments.Text = "Treatments";
            btnTreatments.TextAlign = ContentAlignment.MiddleLeft;
            btnTreatments.UseVisualStyleBackColor = true;
            // 
            // btnUsers
            // 
            btnUsers.FlatStyle = FlatStyle.Flat;
            btnUsers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnUsers.Location = new Point(0, 250);
            btnUsers.Name = "btnUsers";
            btnUsers.Padding = new Padding(15, 0, 0, 0);
            btnUsers.Size = new Size(200, 45);
            btnUsers.TabIndex = 5;
            btnUsers.Text = "Users";
            btnUsers.TextAlign = ContentAlignment.MiddleLeft;
            btnUsers.UseVisualStyleBackColor = true;
            // 
            // btnLogout
            // 
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogout.Location = new Point(0, 605);
            btnLogout.Name = "btnLogout";
            btnLogout.Padding = new Padding(15, 0, 0, 0);
            btnLogout.Size = new Size(200, 45);
            btnLogout.TabIndex = 6;
            btnLogout.Text = "Logout";
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlContent
            // 
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(200, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(800, 650);
            pnlContent.TabIndex = 1;
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
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 650);
            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Name = "frmDashboard";
            Text = "Dental Clinic Management System";
            WindowState = FormWindowState.Maximized;
            pnlSidebar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}