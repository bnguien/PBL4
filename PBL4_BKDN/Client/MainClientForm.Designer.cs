﻿namespace Client
{
    partial class MainClientForm
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
			topPanel = new Panel();
			btnConnect = new Button();
			txtPort = new TextBox();
			txtServerIp = new TextBox();
			lblStatus = new Label();
			lblPort = new Label();
			lblServerIp = new Label();
			txtLog = new TextBox();
			optionsPanel = new Panel();
			chkVerbose = new CheckBox();
			chkAutoReconnect = new CheckBox();
			topPanel.SuspendLayout();
			optionsPanel.SuspendLayout();
			SuspendLayout();
			// 
			// topPanel
			// 
			topPanel.Controls.Add(btnConnect);
			topPanel.Controls.Add(txtPort);
			topPanel.Controls.Add(txtServerIp);
			topPanel.Controls.Add(lblStatus);
			topPanel.Controls.Add(lblPort);
			topPanel.Controls.Add(lblServerIp);
			topPanel.Dock = DockStyle.Top;
			topPanel.Location = new Point(0, 0);
			topPanel.Name = "topPanel";
			topPanel.Size = new Size(900, 48);
			topPanel.TabIndex = 0;
			// 
			// btnConnect
			// 
			btnConnect.Location = new Point(410, 12);
			btnConnect.Name = "btnConnect";
			btnConnect.Size = new Size(96, 27);
			btnConnect.TabIndex = 5;
			btnConnect.Text = "Connect";
			btnConnect.UseVisualStyleBackColor = true;
			btnConnect.Click += btnConnect_Click;
			// 
			// txtPort
			// 
			txtPort.Location = new Point(306, 12);
			txtPort.Name = "txtPort";
			txtPort.Size = new Size(88, 27);
			txtPort.TabIndex = 4;
			txtPort.Text = "5000";
			// 
			// txtServerIp
			// 
			txtServerIp.Location = new Point(80, 12);
			txtServerIp.Name = "txtServerIp";
			txtServerIp.Size = new Size(160, 27);
			txtServerIp.TabIndex = 3;
			txtServerIp.Text = "127.0.0.1";
			// 
			// lblStatus
			// 
			lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lblStatus.ForeColor = Color.Red;
			lblStatus.Location = new Point(520, 12);
			lblStatus.Name = "lblStatus";
			lblStatus.Size = new Size(360, 23);
			lblStatus.TabIndex = 2;
			lblStatus.Text = "Disconnected";
			lblStatus.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// lblPort
			// 
			lblPort.AutoSize = true;
			lblPort.Location = new Point(256, 16);
			lblPort.Name = "lblPort";
			lblPort.Size = new Size(35, 20);
			lblPort.TabIndex = 1;
			lblPort.Text = "Port";
			// 
			// lblServerIp
			// 
			lblServerIp.AutoSize = true;
			lblServerIp.Location = new Point(12, 16);
			lblServerIp.Name = "lblServerIp";
			lblServerIp.Size = new Size(66, 20);
			lblServerIp.TabIndex = 0;
			lblServerIp.Text = "Server IP";
			// 
			// txtLog
			// 
			txtLog.Dock = DockStyle.Fill;
			txtLog.Location = new Point(0, 48);
			txtLog.Multiline = true;
			txtLog.Name = "txtLog";
			txtLog.ReadOnly = true;
			txtLog.ScrollBars = ScrollBars.Vertical;
			txtLog.Size = new Size(900, 482);
			txtLog.TabIndex = 1;
			// 
			// optionsPanel
			// 
			optionsPanel.Controls.Add(chkVerbose);
			optionsPanel.Controls.Add(chkAutoReconnect);
			optionsPanel.Dock = DockStyle.Bottom;
			optionsPanel.Location = new Point(0, 530);
			optionsPanel.Name = "optionsPanel";
			optionsPanel.Padding = new Padding(8);
			optionsPanel.Size = new Size(900, 40);
			optionsPanel.TabIndex = 2;
			// 
			// chkVerbose
			// 
			chkVerbose.AutoSize = true;
			chkVerbose.Location = new Point(160, 10);
			chkVerbose.Name = "chkVerbose";
			chkVerbose.Size = new Size(140, 24);
			chkVerbose.TabIndex = 1;
			chkVerbose.Text = "Verbose logging";
			chkVerbose.UseVisualStyleBackColor = true;
			// 
			// chkAutoReconnect
			// 
			chkAutoReconnect.AutoSize = true;
			chkAutoReconnect.Location = new Point(16, 10);
			chkAutoReconnect.Name = "chkAutoReconnect";
			chkAutoReconnect.Size = new Size(132, 24);
			chkAutoReconnect.TabIndex = 0;
			chkAutoReconnect.Text = "Auto reconnect";
			chkAutoReconnect.UseVisualStyleBackColor = true;
			// 
			// MainClientForm
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(900, 570);
			Controls.Add(txtLog);
			Controls.Add(optionsPanel);
			Controls.Add(topPanel);
			Name = "MainClientForm";
			Text = "Client";
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			optionsPanel.ResumeLayout(false);
			optionsPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtServerIp;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblServerIp;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Panel optionsPanel;
        private System.Windows.Forms.CheckBox chkVerbose;
        private System.Windows.Forms.CheckBox chkAutoReconnect;
    }
}