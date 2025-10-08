namespace Server.Forms
{
    partial class MessageBoxForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblCaption;
        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.Label lblButtons;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.TextBox txtCaption;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.ComboBox cmbButtons;
        private System.Windows.Forms.ComboBox cmbIcon;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblCaption = new Label();
            lblContent = new Label();
            lblButtons = new Label();
            lblIcon = new Label();
            txtCaption = new TextBox();
            txtContent = new TextBox();
            cmbButtons = new ComboBox();
            cmbIcon = new ComboBox();
            btnPreview = new Button();
            btnSend = new Button();
            txtLog = new TextBox();
            SuspendLayout();
            // 
            // lblCaption
            // 
            lblCaption.AutoSize = true;
            lblCaption.Location = new Point(20, 20);
            lblCaption.Name = "lblCaption";
            lblCaption.Size = new Size(64, 20);
            lblCaption.TabIndex = 1;
            lblCaption.Text = "Caption:";
            // 
            // lblContent
            // 
            lblContent.AutoSize = true;
            lblContent.Location = new Point(20, 60);
            lblContent.Name = "lblContent";
            lblContent.Size = new Size(64, 20);
            lblContent.TabIndex = 3;
            lblContent.Text = "Content:";
            // 
            // lblButtons
            // 
            lblButtons.AutoSize = true;
            lblButtons.Location = new Point(20, 160);
            lblButtons.Name = "lblButtons";
            lblButtons.Size = new Size(62, 20);
            lblButtons.TabIndex = 5;
            lblButtons.Text = "Buttons:";
            // 
            // lblIcon
            // 
            lblIcon.AutoSize = true;
            lblIcon.Location = new Point(20, 200);
            lblIcon.Name = "lblIcon";
            lblIcon.Size = new Size(40, 20);
            lblIcon.TabIndex = 7;
            lblIcon.Text = "Icon:";
            // 
            // txtCaption
            // 
            txtCaption.Location = new Point(100, 20);
            txtCaption.Name = "txtCaption";
            txtCaption.Size = new Size(250, 27);
            txtCaption.TabIndex = 2;
            // 
            // txtContent
            // 
            txtContent.Location = new Point(100, 60);
            txtContent.Multiline = true;
            txtContent.Name = "txtContent";
            txtContent.Size = new Size(250, 80);
            txtContent.TabIndex = 4;
            // 
            // cmbButtons
            // 
            cmbButtons.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbButtons.Location = new Point(100, 160);
            cmbButtons.Name = "cmbButtons";
            cmbButtons.Size = new Size(150, 28);
            cmbButtons.TabIndex = 6;
            // 
            // cmbIcon
            // 
            cmbIcon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbIcon.Location = new Point(100, 200);
            cmbIcon.Name = "cmbIcon";
            cmbIcon.Size = new Size(150, 28);
            cmbIcon.TabIndex = 8;
            // 
            // btnPreview
            // 
            btnPreview.Location = new Point(60, 250);
            btnPreview.Name = "btnPreview";
            btnPreview.Size = new Size(100, 30);
            btnPreview.TabIndex = 9;
            btnPreview.Text = "Preview";
            btnPreview.Click += btnPreview_Click;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(200, 250);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(100, 30);
            btnSend.TabIndex = 10;
            btnSend.Text = "Send";
            btnSend.Click += btnSend_Click;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(20, 300);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(330, 80);
            txtLog.TabIndex = 0;
            // 
            // MessageBoxForm
            // 
            ClientSize = new Size(380, 420);
            Controls.Add(txtLog);
            Controls.Add(lblCaption);
            Controls.Add(txtCaption);
            Controls.Add(lblContent);
            Controls.Add(txtContent);
            Controls.Add(lblButtons);
            Controls.Add(cmbButtons);
            Controls.Add(lblIcon);
            Controls.Add(cmbIcon);
            Controls.Add(btnPreview);
            Controls.Add(btnSend);
            Name = "MessageBoxForm";
            Text = "Send MessageBox to Client";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
