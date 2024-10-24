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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            lblLogin = new Label();
            picAvatar = new PictureBox();
            btnLogInOut = new Button();
            statusStrip1 = new StatusStrip();
            Upload = new ToolStripStatusLabel();
            toolStripProgressBar1 = new ToolStripProgressBar();
            split1 = new SplitContainer();
            lvDirView = new ListView();
            pnlPath = new FlowLayoutPanel();
            btnProgressCancel = new ToolStripDropDownButton();
            panel1 = new Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            statusStrip1.SuspendLayout();
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
            statusStrip1.Items.AddRange(new ToolStripItem[] { Upload, toolStripProgressBar1, btnProgressCancel });
            statusStrip1.Location = new Point(0, 695);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.RenderMode = ToolStripRenderMode.Professional;
            statusStrip1.Size = new Size(1006, 26);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // Upload
            // 
            Upload.Name = "Upload";
            Upload.Size = new Size(94, 20);
            Upload.Text = "Загружаем...";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(256, 18);
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
            split1.Size = new Size(1006, 631);
            split1.SplitterDistance = 688;
            split1.TabIndex = 2;
            split1.TabStop = false;
            // 
            // lvDirView
            // 
            lvDirView.AllowDrop = true;
            lvDirView.BorderStyle = BorderStyle.None;
            lvDirView.Dock = DockStyle.Fill;
            lvDirView.FullRowSelect = true;
            lvDirView.Location = new Point(0, 37);
            lvDirView.Name = "lvDirView";
            lvDirView.Size = new Size(688, 594);
            lvDirView.TabIndex = 1;
            lvDirView.UseCompatibleStateImageBehavior = false;
            lvDirView.ItemActivate += lvDirView_ItemActivate;
            lvDirView.ItemDrag += lvDirView_ItemDrag;
            lvDirView.DragDrop += lvDirView_DragDrop;
            lvDirView.DragEnter += lvDirView_DragEnter;
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
            // btnProgressCancel
            // 
            btnProgressCancel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnProgressCancel.Image = (Image)resources.GetObject("btnProgressCancel.Image");
            btnProgressCancel.ImageTransparentColor = Color.Magenta;
            btnProgressCancel.Name = "btnProgressCancel";
            btnProgressCancel.Size = new Size(34, 24);
            btnProgressCancel.Text = "toolStripDropDownButton1";
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
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
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
        private ToolStripStatusLabel Upload;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripDropDownButton btnProgressCancel;
    }
}
