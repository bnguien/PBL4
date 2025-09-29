namespace Server.Forms
{
    partial class MainServerForm
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
            components = new System.ComponentModel.Container();
            topPanel = new Panel();
            btnChangePort = new Button();
            btnStartStop = new Button();
            txtPort = new TextBox();
            lblStatus = new Label();
            lblPort = new Label();
            splitContainer = new SplitContainer();
            dgvClients = new DataGridView();
            ctxClients = new ContextMenuStrip(components);
            systemInfoToolStripMenuItem = new ToolStripMenuItem();
            hardwareInfoToolStripMenuItem = new ToolStripMenuItem();
            softwareInfoToolStripMenuItem = new ToolStripMenuItem();
            networkInfoToolStripMenuItem = new ToolStripMenuItem();
            remoteShellToolStripMenuItem = new ToolStripMenuItem();
            fileManagerToolStripMenuItem = new ToolStripMenuItem();
            rightPanel = new Panel();
            txtLog = new TextBox();
            cboLogFilter = new ComboBox();
            lblLogFilter = new Label();
            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClients).BeginInit();
            ctxClients.SuspendLayout();
            rightPanel.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.Controls.Add(btnChangePort);
            topPanel.Controls.Add(btnStartStop);
            topPanel.Controls.Add(txtPort);
            topPanel.Controls.Add(lblStatus);
            topPanel.Controls.Add(lblPort);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(1000, 48);
            topPanel.TabIndex = 0;
            // 
            // btnChangePort
            // 
            btnChangePort.Location = new Point(220, 12);
            btnChangePort.Name = "btnChangePort";
            btnChangePort.Size = new Size(96, 30);
            btnChangePort.TabIndex = 4;
            btnChangePort.Text = "Change Port";
            btnChangePort.UseVisualStyleBackColor = true;
            btnChangePort.Click += btnChangePort_Click;
            // 
            // btnStartStop
            // 
            btnStartStop.Location = new Point(322, 12);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(96, 30);
            btnStartStop.TabIndex = 3;
            btnStartStop.Text = "Start Server";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(96, 13);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(118, 27);
            txtPort.TabIndex = 2;
            txtPort.Text = "5000";
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(440, 12);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(548, 23);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Server stopped";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(12, 16);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(96, 20);
            lblPort.TabIndex = 0;
            lblPort.Text = "Port (default)";
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 48);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(dgvClients);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(rightPanel);
            splitContainer.Size = new Size(1000, 552);
            splitContainer.SplitterDistance = 431;
            splitContainer.TabIndex = 1;
            // 
            // dgvClients
            // 
            dgvClients.AllowUserToAddRows = false;
            dgvClients.AllowUserToDeleteRows = false;
            dgvClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvClients.BackgroundColor = SystemColors.Window;
            dgvClients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClients.ContextMenuStrip = ctxClients;
            dgvClients.Dock = DockStyle.Fill;
            dgvClients.Location = new Point(0, 0);
            dgvClients.MultiSelect = false;
            dgvClients.Name = "dgvClients";
            dgvClients.ReadOnly = true;
            dgvClients.RowHeadersVisible = false;
            dgvClients.RowHeadersWidth = 51;
            dgvClients.RowTemplate.Height = 25;
            dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClients.Size = new Size(431, 552);
            dgvClients.TabIndex = 0;
            // 
            // ctxClients
            // 
            ctxClients.ImageScalingSize = new Size(20, 20);
            ctxClients.Items.AddRange(new ToolStripItem[] { systemInfoToolStripMenuItem, remoteShellToolStripMenuItem, fileManagerToolStripMenuItem });
            ctxClients.Name = "ctxClients";
            ctxClients.Size = new Size(156, 28);
            // 
            // systemInfoToolStripMenuItem
            // 
            systemInfoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { hardwareInfoToolStripMenuItem, softwareInfoToolStripMenuItem, networkInfoToolStripMenuItem });
            systemInfoToolStripMenuItem.Name = "systemInfoToolStripMenuItem";
            systemInfoToolStripMenuItem.Size = new Size(155, 24);
            systemInfoToolStripMenuItem.Text = "System Info";
            // 
            // hardwareInfoToolStripMenuItem
            // 
            hardwareInfoToolStripMenuItem.Name = "hardwareInfoToolStripMenuItem";
            hardwareInfoToolStripMenuItem.Size = new Size(187, 26);
            hardwareInfoToolStripMenuItem.Text = "Hardware Info";
            hardwareInfoToolStripMenuItem.Click += hardwareInfoToolStripMenuItem_Click;
            // 
            // softwareInfoToolStripMenuItem
            // 
            softwareInfoToolStripMenuItem.Name = "softwareInfoToolStripMenuItem";
            softwareInfoToolStripMenuItem.Size = new Size(187, 26);
            softwareInfoToolStripMenuItem.Text = "Software Info";
            softwareInfoToolStripMenuItem.Click += softwareInfoToolStripMenuItem_Click;
            // 
            // networkInfoToolStripMenuItem
            // 
            networkInfoToolStripMenuItem.Name = "networkInfoToolStripMenuItem";
            networkInfoToolStripMenuItem.Size = new Size(187, 26);
            networkInfoToolStripMenuItem.Text = "Network Info";
            networkInfoToolStripMenuItem.Click += networkInfoToolStripMenuItem_Click;
            // 
            // remoteShellToolStripMenuItem
            // 
            remoteShellToolStripMenuItem.Name = "remoteShellToolStripMenuItem";
            remoteShellToolStripMenuItem.Size = new Size(155, 24);
            remoteShellToolStripMenuItem.Text = "Remote Shell";
            remoteShellToolStripMenuItem.Click += remoteShellToolStripMenuItem_Click;
            // 
            // fileManagerToolStripMenuItem
            // 
            fileManagerToolStripMenuItem.Name = "fileManagerToolStripMenuItem";
            fileManagerToolStripMenuItem.Size = new Size(155, 24);
            fileManagerToolStripMenuItem.Text = "File Manager";
            fileManagerToolStripMenuItem.Click += fileManagerToolStripMenuItem_Click;
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(txtLog);
            rightPanel.Controls.Add(cboLogFilter);
            rightPanel.Controls.Add(lblLogFilter);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Padding = new Padding(8);
            rightPanel.Size = new Size(565, 552);
            rightPanel.TabIndex = 0;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(16, 48);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(438, 488);
            txtLog.TabIndex = 2;
            // 
            // cboLogFilter
            // 
            cboLogFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLogFilter.FormattingEnabled = true;
            cboLogFilter.Items.AddRange(new object[] { "All", "Info", "Warning", "Error", "Network" });
            cboLogFilter.Location = new Point(64, 12);
            cboLogFilter.Name = "cboLogFilter";
            cboLogFilter.Size = new Size(160, 28);
            cboLogFilter.TabIndex = 1;
            // 
            // lblLogFilter
            // 
            lblLogFilter.AutoSize = true;
            lblLogFilter.Location = new Point(16, 16);
            lblLogFilter.Name = "lblLogFilter";
            lblLogFilter.Size = new Size(42, 20);
            lblLogFilter.TabIndex = 0;
            lblLogFilter.Text = "Filter";
            // 
            // MainServerForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 600);
            Controls.Add(splitContainer);
            Controls.Add(topPanel);
            Name = "MainServerForm";
            Text = "Server";
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvClients).EndInit();
            ctxClients.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            rightPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button btnChangePort;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.DataGridView dgvClients;
        private System.Windows.Forms.ContextMenuStrip ctxClients;
        private System.Windows.Forms.ToolStripMenuItem systemInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hardwareInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem softwareInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem networkInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remoteShellToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileManagerToolStripMenuItem;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.ComboBox cboLogFilter;
        private System.Windows.Forms.Label lblLogFilter;
        private System.Windows.Forms.TextBox txtLog;
    }
}