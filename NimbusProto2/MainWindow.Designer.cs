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
            statusStrip1 = new StatusStrip();
            split1 = new SplitContainer();
            lvDirView = new ListView();
            pnlPath = new FlowLayoutPanel();
            panel1 = new Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)split1).BeginInit();
            split1.Panel1.SuspendLayout();
            split1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.LightSteelBlue;
            panel1.Controls.Add(lblLogin);
            panel1.Controls.Add(picAvatar);
            panel1.Controls.Add(btnLogInOut);
            panel1.Dock = DockStyle.Top;
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
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Location = new Point(0, 699);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.RenderMode = ToolStripRenderMode.Professional;
            statusStrip1.Size = new Size(1006, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // split1
            // 
            split1.Dock = DockStyle.Fill;
            split1.Location = new Point(0, 64);
            split1.Margin = new Padding(0);
            split1.Name = "split1";
            // 
            // split1.Panel1
            // 
            split1.Panel1.Controls.Add(lvDirView);
            split1.Panel1.Controls.Add(pnlPath);
            // 
            // split1.Panel2
            // 
            split1.Panel2.BackColor = SystemColors.Control;
            split1.Size = new Size(1006, 635);
            split1.SplitterDistance = 688;
            split1.TabIndex = 2;
            split1.TabStop = false;
            // 
            // lvDirView
            // 
            lvDirView.BorderStyle = BorderStyle.None;
            lvDirView.Dock = DockStyle.Fill;
            lvDirView.FullRowSelect = true;
            lvDirView.Location = new Point(0, 37);
            lvDirView.Name = "lvDirView";
            lvDirView.Size = new Size(688, 598);
            lvDirView.TabIndex = 1;
            lvDirView.UseCompatibleStateImageBehavior = false;
            lvDirView.ItemActivate += lvDirView_ItemActivate;
            lvDirView.ItemDrag += lvDirView_ItemDrag;
            lvDirView.KeyDown += lvDirView_KeyDown;
            // 
            // pnlPath
            // 
            pnlPath.BackColor = Color.LightBlue;
            pnlPath.Dock = DockStyle.Top;
            pnlPath.Location = new Point(0, 0);
            pnlPath.Name = "pnlPath";
            pnlPath.Size = new Size(688, 37);
            pnlPath.TabIndex = 0;
            pnlPath.WrapContents = false;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1006, 721);
            Controls.Add(split1);
            Controls.Add(statusStrip1);
            Controls.Add(panel1);
            Name = "MainWindow";
            Text = "NimbusKeeper";
            Load += MainWindow_Load;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            split1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)split1).EndInit();
            split1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picAvatar;
        private Button btnLogInOut;
        private Label lblLogin;
        private StatusStrip statusStrip1;
        private SplitContainer split1;
        private FlowLayoutPanel pnlPath;
        private ListView lvDirView;
    }
}
