namespace NimbusProto2
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
            Panel panel1;
            lblLogin = new Label();
            picAvatar = new PictureBox();
            btnLogInOut = new Button();
            panel1 = new Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.LightSteelBlue;
            panel1.Controls.Add(lblLogin);
            panel1.Controls.Add(picAvatar);
            panel1.Controls.Add(btnLogInOut);
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1006, 64);
            panel1.TabIndex = 0;
            // 
            // lblLogin
            // 
            lblLogin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblLogin.Location = new Point(825, 9);
            lblLogin.Name = "lblLogin";
            lblLogin.Size = new Size(170, 25);
            lblLogin.TabIndex = 2;
            lblLogin.Text = "Вход не выполнен";
            // 
            // picAvatar
            // 
            picAvatar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            picAvatar.BackColor = Color.AliceBlue;
            picAvatar.Location = new Point(760, 0);
            picAvatar.Margin = new Padding(2, 0, 2, 0);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new Size(64, 64);
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picAvatar.TabIndex = 1;
            picAvatar.TabStop = false;
            // 
            // btnLogInOut
            // 
            btnLogInOut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogInOut.Location = new Point(825, 32);
            btnLogInOut.Name = "btnLogInOut";
            btnLogInOut.Size = new Size(94, 29);
            btnLogInOut.TabIndex = 0;
            btnLogInOut.Text = "Войти...";
            btnLogInOut.UseVisualStyleBackColor = true;
            btnLogInOut.Click += btnLogInOut_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1006, 721);
            Controls.Add(panel1);
            Name = "MainWindow";
            Text = "NimbusKeeper";
            Load += MainWindow_Load;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox picAvatar;
        private Button btnLogInOut;
        private Label lblLogin;
    }
}
