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
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblWelcome;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.btnDashboard = new System.Windows.Forms.Button();
            this.btnPatients = new System.Windows.Forms.Button();
            this.btnDentists = new System.Windows.Forms.Button();
            this.btnAppointments = new System.Windows.Forms.Button();
            this.btnTreatments = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.lblWelcome = new System.Windows.Forms.Label();

            this.pnlSidebar.SuspendLayout();
            this.SuspendLayout();

            // pnlSidebar
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Width = 200;
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(230, 236, 245);
            this.pnlSidebar.Controls.Add(this.btnDashboard);
            this.pnlSidebar.Controls.Add(this.btnPatients);
            this.pnlSidebar.Controls.Add(this.btnDentists);
            this.pnlSidebar.Controls.Add(this.btnAppointments);
            this.pnlSidebar.Controls.Add(this.btnTreatments);
            this.pnlSidebar.Controls.Add(this.btnLogout);

            var sidebarButtons = new[]
            {
                this.btnDashboard, this.btnPatients, this.btnDentists,
                this.btnAppointments, this.btnTreatments, this.btnLogout
            };
            string[] labels = { "Dashboard", "Patients", "Dentists", "Appointments", "Treatments", "Logout" };
            for (int i = 0; i < sidebarButtons.Length; i++)
            {
                sidebarButtons[i].Text = labels[i];
                sidebarButtons[i].Location = new System.Drawing.Point(0, i * 50);
                sidebarButtons[i].Size = new System.Drawing.Size(200, 45);
                sidebarButtons[i].FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                sidebarButtons[i].TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                sidebarButtons[i].Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
                sidebarButtons[i].Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            }

            // pnlContent
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;

            // lblWelcome - default content shown before any sidebar button is clicked
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 18F,
                System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic);
            this.lblWelcome.Location = new System.Drawing.Point(30, 30);

            // frmDashboard
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlSidebar);
            this.Text = "Dental Clinic Management System";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            this.pnlSidebar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}