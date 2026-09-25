namespace DentalClinicSystem.Forms
{
    partial class frmUserManagement
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel pnlHost;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnClose;

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblTitle = new Label();
            pnlHost = new Panel();
            pnlFooter = new Panel();
            btnClose = new Button();
            pnlHeader.SuspendLayout();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BackColor = Color.FromArgb(66, 202, 207);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(723, 48);
            pnlHeader.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 11);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(184, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "USER MANAGEMENT";
            //
            // pnlHost
            //
            pnlHost.BackColor = Color.FromArgb(243, 248, 249);
            pnlHost.Dock = DockStyle.Fill;
            pnlHost.Location = new Point(0, 48);
            pnlHost.Name = "pnlHost";
            pnlHost.Size = new Size(723, 484);
            pnlHost.TabIndex = 1;
            //
            // pnlFooter
            //
            pnlFooter.BackColor = Color.FromArgb(230, 236, 245);
            pnlFooter.Controls.Add(btnClose);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 532);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(723, 58);
            pnlFooter.TabIndex = 2;
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Right;
            btnClose.Font = new Font("Bahnschrift Light", 9F, FontStyle.Bold);
            btnClose.Location = new Point(587, 12);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(112, 34);
            btnClose.TabIndex = 0;
            btnClose.Text = "CLOSE";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            //
            // frmUserManagement
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(723, 590);
            Controls.Add(pnlHost);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmUserManagement";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "User Management";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
    }
}