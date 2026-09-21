namespace DentalClinicSystem
{
    partial class frmLogin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            txtUsername = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtPassword = new TextBox();
            btnLogin = new Button();
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            label3 = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(175, 129);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(166, 31);
            txtUsername.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Cooper Black", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(29, 135);
            label1.Name = "label1";
            label1.Size = new Size(131, 21);
            label1.TabIndex = 1;
            label1.Text = "USERNAME: ";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Cooper Black", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.Location = new Point(28, 225);
            label2.Name = "label2";
            label2.Size = new Size(132, 21);
            label2.TabIndex = 3;
            label2.Text = "PASSWORD: ";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(175, 219);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(166, 31);
            txtPassword.TabIndex = 2;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.LightBlue;
            btnLogin.Font = new Font("Cooper Black", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = SystemColors.ActiveCaptionText;
            btnLogin.Location = new Point(128, 307);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(148, 47);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "LOGIN";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Window;
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(btnLogin);
            panel1.Controls.Add(txtUsername);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(txtPassword);
            panel1.Location = new Point(72, 85);
            panel1.Name = "panel1";
            panel1.Size = new Size(417, 447);
            panel1.TabIndex = 5;
            panel1.Paint += panel1_Paint;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.Window;
            pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.ErrorImage = null;
            pictureBox1.Location = new Point(482, 85);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(451, 447);
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = SystemColors.Window;
            label3.Font = new Font("Cooper Black", 11F, FontStyle.Italic, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.ActiveCaptionText;
            label3.Location = new Point(47, 60);
            label3.Name = "label3";
            label3.Size = new Size(314, 25);
            label3.TabIndex = 7;
            label3.Text = "LOG IN TO DENTAL CARE.";
            // 
            // frmLogin
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = SystemColors.ActiveCaption;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1031, 600);
            Controls.Add(pictureBox1);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "frmLogin";
            Text = "Dental Clinic Management System - Login";
            Load += frmLogin_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtUsername;
        private Label label1;
        private Label label2;
        private TextBox txtPassword;
        private Button btnLogin;
        private Panel panel1;
        private PictureBox pictureBox1;
        private Label label3;
    }
}
