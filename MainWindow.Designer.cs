namespace Yandex.Music.UnofficialClient
{
    partial class MainWindow
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
            LoginPanel = new Panel();
            linkLabel1 = new LinkLabel();
            LoginStep2Group = new GroupBox();
            TwoFactorSendButton = new Button();
            TwoFactorBox = new TextBox();
            LoginStep1Group = new GroupBox();
            LoginButton = new Button();
            LoginBox = new TextBox();
            PasswordBox = new MaskedTextBox();
            ShowPasswordBox = new CheckBox();
            LoadingPanel = new Panel();
            LoadingBox2 = new PictureBox();
            LoadingBox1 = new PictureBox();
            LoginPanel.SuspendLayout();
            LoginStep2Group.SuspendLayout();
            LoginStep1Group.SuspendLayout();
            LoadingPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LoadingBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)LoadingBox1).BeginInit();
            SuspendLayout();
            // 
            // LoginPanel
            // 
            LoginPanel.Controls.Add(linkLabel1);
            LoginPanel.Controls.Add(LoginStep2Group);
            LoginPanel.Controls.Add(LoginStep1Group);
            LoginPanel.Dock = DockStyle.Fill;
            LoginPanel.Location = new Point(0, 0);
            LoginPanel.Name = "LoginPanel";
            LoginPanel.Size = new Size(634, 450);
            LoginPanel.TabIndex = 0;
            LoginPanel.Visible = false;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(174, 21);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(188, 15);
            linkLabel1.TabIndex = 5;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "I'm have problems with logging in";
            // 
            // LoginStep2Group
            // 
            LoginStep2Group.Controls.Add(TwoFactorSendButton);
            LoginStep2Group.Controls.Add(TwoFactorBox);
            LoginStep2Group.Enabled = false;
            LoginStep2Group.Location = new Point(174, 154);
            LoginStep2Group.Name = "LoginStep2Group";
            LoginStep2Group.Size = new Size(267, 81);
            LoginStep2Group.TabIndex = 4;
            LoginStep2Group.TabStop = false;
            LoginStep2Group.Text = "Step 2 - Two-factor authorization";
            // 
            // TwoFactorSendButton
            // 
            TwoFactorSendButton.Location = new Point(184, 51);
            TwoFactorSendButton.Name = "TwoFactorSendButton";
            TwoFactorSendButton.Size = new Size(75, 23);
            TwoFactorSendButton.TabIndex = 1;
            TwoFactorSendButton.Text = "Send";
            TwoFactorSendButton.UseVisualStyleBackColor = true;
            // 
            // TwoFactorBox
            // 
            TwoFactorBox.Location = new Point(8, 22);
            TwoFactorBox.Name = "TwoFactorBox";
            TwoFactorBox.PlaceholderText = "Code";
            TwoFactorBox.Size = new Size(251, 23);
            TwoFactorBox.TabIndex = 0;
            // 
            // LoginStep1Group
            // 
            LoginStep1Group.Controls.Add(LoginButton);
            LoginStep1Group.Controls.Add(LoginBox);
            LoginStep1Group.Controls.Add(PasswordBox);
            LoginStep1Group.Controls.Add(ShowPasswordBox);
            LoginStep1Group.Location = new Point(174, 39);
            LoginStep1Group.Name = "LoginStep1Group";
            LoginStep1Group.Size = new Size(267, 109);
            LoginStep1Group.TabIndex = 3;
            LoginStep1Group.TabStop = false;
            LoginStep1Group.Text = "Step 1 - Login and password";
            // 
            // LoginButton
            // 
            LoginButton.Location = new Point(186, 79);
            LoginButton.Name = "LoginButton";
            LoginButton.Size = new Size(75, 23);
            LoginButton.TabIndex = 3;
            LoginButton.Text = "Login";
            LoginButton.UseVisualStyleBackColor = true;
            // 
            // LoginBox
            // 
            LoginBox.Location = new Point(116, 22);
            LoginBox.Name = "LoginBox";
            LoginBox.PlaceholderText = "Login";
            LoginBox.Size = new Size(151, 23);
            LoginBox.TabIndex = 0;
            // 
            // PasswordBox
            // 
            PasswordBox.Culture = new System.Globalization.CultureInfo("en-US");
            PasswordBox.Location = new Point(116, 50);
            PasswordBox.Name = "PasswordBox";
            PasswordBox.Size = new Size(151, 23);
            PasswordBox.TabIndex = 2;
            PasswordBox.UseSystemPasswordChar = true;
            // 
            // ShowPasswordBox
            // 
            ShowPasswordBox.AutoSize = true;
            ShowPasswordBox.Location = new Point(2, 52);
            ShowPasswordBox.Name = "ShowPasswordBox";
            ShowPasswordBox.Size = new Size(108, 19);
            ShowPasswordBox.TabIndex = 1;
            ShowPasswordBox.Text = "Show password";
            ShowPasswordBox.UseVisualStyleBackColor = true;
            ShowPasswordBox.CheckedChanged += ShowPasswordBox_CheckedChanged;
            // 
            // LoadingPanel
            // 
            LoadingPanel.Controls.Add(LoadingBox2);
            LoadingPanel.Controls.Add(LoadingBox1);
            LoadingPanel.Dock = DockStyle.Fill;
            LoadingPanel.Location = new Point(0, 0);
            LoadingPanel.Name = "LoadingPanel";
            LoadingPanel.Size = new Size(634, 450);
            LoadingPanel.TabIndex = 1;
            // 
            // LoadingBox2
            // 
            LoadingBox2.Image = Properties.Resources.Loading;
            LoadingBox2.Location = new Point(269, 301);
            LoadingBox2.Name = "LoadingBox2";
            LoadingBox2.Size = new Size(64, 64);
            LoadingBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            LoadingBox2.TabIndex = 1;
            LoadingBox2.TabStop = false;
            // 
            // LoadingBox1
            // 
            LoadingBox1.Image = Properties.Resources.Icon;
            LoadingBox1.Location = new Point(174, 39);
            LoadingBox1.Name = "LoadingBox1";
            LoadingBox1.Size = new Size(256, 256);
            LoadingBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            LoadingBox1.TabIndex = 0;
            LoadingBox1.TabStop = false;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(634, 450);
            Controls.Add(LoadingPanel);
            Controls.Add(LoginPanel);
            Name = "MainWindow";
            Text = "Yandex Music";
            LoginPanel.ResumeLayout(false);
            LoginPanel.PerformLayout();
            LoginStep2Group.ResumeLayout(false);
            LoginStep2Group.PerformLayout();
            LoginStep1Group.ResumeLayout(false);
            LoginStep1Group.PerformLayout();
            LoadingPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)LoadingBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)LoadingBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel LoginPanel;
        private TextBox LoginBox;
        private MaskedTextBox PasswordBox;
        private CheckBox ShowPasswordBox;
        private GroupBox LoginStep1Group;
        private Button LoginButton;
        private GroupBox LoginStep2Group;
        private Button TwoFactorSendButton;
        private TextBox TwoFactorBox;
        private LinkLabel linkLabel1;
        private Panel LoadingPanel;
        private PictureBox LoadingBox1;
        private PictureBox LoadingBox2;
    }
}
