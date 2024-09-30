namespace NimbusProto2
{
    partial class LoggingIn
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Label label1;
            Label label2;
            Label label3;
            btnCancel = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label1.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(498, 63);
            label1.TabIndex = 0;
            label1.Text = "В открывшемся окне браузера разрешите приложению доступ к Вашей учётной записи Яндекс.";
            label1.TextAlign = ContentAlignment.TopCenter;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label2.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label2.Location = new Point(12, 72);
            label2.Name = "label2";
            label2.Size = new Size(498, 37);
            label2.TabIndex = 0;
            label2.Text = "(может потребоваться вход в аккаунт)";
            label2.TextAlign = ContentAlignment.TopCenter;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label3.Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label3.Location = new Point(12, 109);
            label3.Name = "label3";
            label3.Size = new Size(498, 37);
            label3.TabIndex = 0;
            label3.Text = "Это окно закроется после успешного входа.";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnCancel.Location = new Point(192, 148);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(138, 53);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Отмена";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // LoggingIn
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(522, 213);
            ControlBox = false;
            Controls.Add(btnCancel);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "LoggingIn";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Вход в аккаунт";
            Load += LoggingIn_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button btnCancel;
    }
}