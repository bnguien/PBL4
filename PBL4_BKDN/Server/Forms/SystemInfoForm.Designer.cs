namespace Server.Forms
{
    partial class SystemInfoForm
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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabOverview = new System.Windows.Forms.TabPage();
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnHardware = new System.Windows.Forms.Button();
            this.btnSoftware = new System.Windows.Forms.Button();
            this.btnNetwork = new System.Windows.Forms.Button();
            this.tabHardware = new System.Windows.Forms.TabPage();
            this.tabSoftware = new System.Windows.Forms.TabPage();
            this.tabNetwork = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabOverview.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabOverview);
            this.tabControl1.Controls.Add(this.tabHardware);
            this.tabControl1.Controls.Add(this.tabSoftware);
            this.tabControl1.Controls.Add(this.tabNetwork);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(900, 560);
            this.tabControl1.TabIndex = 0;
            // 
            // tabOverview
            // 
            this.tabOverview.Controls.Add(this.btnNetwork);
            this.tabOverview.Controls.Add(this.btnSoftware);
            this.tabOverview.Controls.Add(this.btnHardware);
            this.tabOverview.Controls.Add(this.lblSummary);
            this.tabOverview.Location = new System.Drawing.Point(4, 24);
            this.tabOverview.Name = "tabOverview";
            this.tabOverview.Padding = new System.Windows.Forms.Padding(8);
            this.tabOverview.Size = new System.Drawing.Size(892, 532);
            this.tabOverview.TabIndex = 0;
            this.tabOverview.Text = "Overview";
            this.tabOverview.UseVisualStyleBackColor = true;
            // 
            // lblSummary
            // 
            this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummary.Location = new System.Drawing.Point(16, 16);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(860, 440);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.Text = "CPU: -\r\nRAM: -\r\nOS: -\r\nPrimary IP: -\r\nInstalled apps: -";
            // 
            // btnHardware
            // 
            this.btnHardware.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHardware.Location = new System.Drawing.Point(16, 472);
            this.btnHardware.Name = "btnHardware";
            this.btnHardware.Size = new System.Drawing.Size(96, 32);
            this.btnHardware.TabIndex = 1;
            this.btnHardware.Text = "Hardware";
            this.btnHardware.UseVisualStyleBackColor = true;
            // 
            // btnSoftware
            // 
            this.btnSoftware.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSoftware.Location = new System.Drawing.Point(128, 472);
            this.btnSoftware.Name = "btnSoftware";
            this.btnSoftware.Size = new System.Drawing.Size(96, 32);
            this.btnSoftware.TabIndex = 2;
            this.btnSoftware.Text = "Software";
            this.btnSoftware.UseVisualStyleBackColor = true;
            // 
            // btnNetwork
            // 
            this.btnNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNetwork.Location = new System.Drawing.Point(240, 472);
            this.btnNetwork.Name = "btnNetwork";
            this.btnNetwork.Size = new System.Drawing.Size(96, 32);
            this.btnNetwork.TabIndex = 3;
            this.btnNetwork.Text = "Network";
            this.btnNetwork.UseVisualStyleBackColor = true;
            // 
            // tabHardware
            // 
            this.tabHardware.Location = new System.Drawing.Point(4, 24);
            this.tabHardware.Name = "tabHardware";
            this.tabHardware.Padding = new System.Windows.Forms.Padding(3);
            this.tabHardware.Size = new System.Drawing.Size(892, 532);
            this.tabHardware.TabIndex = 1;
            this.tabHardware.Text = "Hardware";
            this.tabHardware.UseVisualStyleBackColor = true;
            // 
            // tabSoftware
            // 
            this.tabSoftware.Location = new System.Drawing.Point(4, 24);
            this.tabSoftware.Name = "tabSoftware";
            this.tabSoftware.Padding = new System.Windows.Forms.Padding(3);
            this.tabSoftware.Size = new System.Drawing.Size(892, 532);
            this.tabSoftware.TabIndex = 2;
            this.tabSoftware.Text = "Software";
            this.tabSoftware.UseVisualStyleBackColor = true;
            // 
            // tabNetwork
            // 
            this.tabNetwork.Location = new System.Drawing.Point(4, 24);
            this.tabNetwork.Name = "tabNetwork";
            this.tabNetwork.Padding = new System.Windows.Forms.Padding(3);
            this.tabNetwork.Size = new System.Drawing.Size(892, 532);
            this.tabNetwork.TabIndex = 3;
            this.tabNetwork.Text = "Network";
            this.tabNetwork.UseVisualStyleBackColor = true;
            // 
            // SystemInfoForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tabControl1);
            this.Name = "SystemInfoForm";
            this.Text = "System Information";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.tabControl1.ResumeLayout(false);
            this.tabOverview.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabOverview;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Button btnNetwork;
        private System.Windows.Forms.Button btnSoftware;
        private System.Windows.Forms.Button btnHardware;
        private System.Windows.Forms.TabPage tabHardware;
        private System.Windows.Forms.TabPage tabSoftware;
        private System.Windows.Forms.TabPage tabNetwork;
    }
}