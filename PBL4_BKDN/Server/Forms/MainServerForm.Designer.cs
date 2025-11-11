using Common.Enums;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Server.Forms
{
    partial class MainServerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
        /// This method is a merged, safe version combining both branches.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            topPanel = new Panel();
            btnChangePort = new Button();
            btnStartStop = new Button();
            txtPort = new TextBox();
            lblStatus = new Label();
            lblPort = new Label();
            splitContainer = new SplitContainer();
            dgvClients = new DataGridView();
            ctxClients = new ContextMenuStrip(components);

            // system info group
            systemInfoToolStripMenuItem = new ToolStripMenuItem();
            hardwareInfoToolStripMenuItem = new ToolStripMenuItem();
            softwareInfoToolStripMenuItem = new ToolStripMenuItem();
            networkInfoToolStripMenuItem = new ToolStripMenuItem();

            // remote / message / file / task / keylogger / user management
            remoteShellToolStripMenuItem = new ToolStripMenuItem();
            messageBoxToolStripMenuItem = new ToolStripMenuItem();
            fileManagerToolStripMenuItem = new ToolStripMenuItem();
            taskManagerToolStripMenuItem = new ToolStripMenuItem();
            screenControlToolStripMenuItem = new ToolStripMenuItem();
            keyLoggerToolStripMenuItem = new ToolStripMenuItem();
            keyLoggerStartParallelToolStripMenuItem = new ToolStripMenuItem();
            keyLoggerStartContinuousToolStripMenuItem = new ToolStripMenuItem();

            shutdownActionToolStripMenuItem = new ToolStripMenuItem();
            ShutdownToolStripMenuItem = new ToolStripMenuItem();
            RestartToolStripMenuItem = new ToolStripMenuItem();
            StandbyToolStripMenuItem = new ToolStripMenuItem();

            userManagementToolStripMenuItem = new ToolStripMenuItem();
            elevateToolStripMenuItem = new ToolStripMenuItem();
            disconnectToolStripMenuItem = new ToolStripMenuItem();

            rightPanel = new Panel();
            txtLog = new TextBox();
            cboLogFilter = new ComboBox();
            lblLogFilter = new Label();

            // Begin layout
            topPanel.SuspendLayout();
            ((ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((ISupportInitialize)dgvClients).BeginInit();
            ctxClients.SuspendLayout();
            rightPanel.SuspendLayout();
            SuspendLayout();

            // topPanel
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

            // btnChangePort
            btnChangePort.Location = new Point(220, 12);
            btnChangePort.Name = "btnChangePort";
            btnChangePort.Size = new Size(96, 30);
            btnChangePort.TabIndex = 4;
            btnChangePort.Text = "Change Port";
            btnChangePort.UseVisualStyleBackColor = true;
            btnChangePort.Click += btnChangePort_Click;

            // btnStartStop
            btnStartStop.Location = new Point(322, 12);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(96, 30);
            btnStartStop.TabIndex = 3;
            btnStartStop.Text = "Start Server";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;

            // txtPort
            txtPort.Location = new Point(108, 13);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(106, 27);
            txtPort.TabIndex = 2;
            txtPort.Text = "5000";

            // lblStatus
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(440, 12);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(548, 23);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Server stopped";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            // lblPort
            lblPort.AutoSize = true;
            lblPort.Location = new Point(12, 16);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(96, 20);
            lblPort.TabIndex = 0;
            lblPort.Text = "Port (default)";

            // splitContainer
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 48);
            splitContainer.Name = "splitContainer";

            // Panel1: clients grid
            splitContainer.Panel1.Controls.Add(dgvClients);

            // Panel2: right panel with log
            splitContainer.Panel2.Controls.Add(rightPanel);

            splitContainer.Size = new Size(1000, 552);
            splitContainer.SplitterDistance = 555;
            splitContainer.TabIndex = 1;

            // dgvClients
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
            dgvClients.Size = new Size(555, 552);
            dgvClients.TabIndex = 0;

            // ctxClients: combine menu items from both versions
            ctxClients.ImageScalingSize = new Size(20, 20);
            ctxClients.Items.AddRange(new ToolStripItem[]
            {
                systemInfoToolStripMenuItem,
                remoteShellToolStripMenuItem,
                messageBoxToolStripMenuItem,
                fileManagerToolStripMenuItem,
                taskManagerToolStripMenuItem,
                screenControlToolStripMenuItem,
                keyLoggerToolStripMenuItem,
                shutdownActionToolStripMenuItem,
                userManagementToolStripMenuItem
            });
            ctxClients.Name = "ctxClients";
            ctxClients.Size = new Size(220, 200);

            // systemInfoToolStripMenuItem
            systemInfoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                hardwareInfoToolStripMenuItem,
                softwareInfoToolStripMenuItem,
                networkInfoToolStripMenuItem
            });
            systemInfoToolStripMenuItem.Name = "systemInfoToolStripMenuItem";
            systemInfoToolStripMenuItem.Size = new Size(180, 24);
            systemInfoToolStripMenuItem.Text = "System Info";

            hardwareInfoToolStripMenuItem.Name = "hardwareInfoToolStripMenuItem";
            hardwareInfoToolStripMenuItem.Size = new Size(187, 26);
            hardwareInfoToolStripMenuItem.Text = "Hardware Info";
            hardwareInfoToolStripMenuItem.Click += hardwareInfoToolStripMenuItem_Click;

            softwareInfoToolStripMenuItem.Name = "softwareInfoToolStripMenuItem";
            softwareInfoToolStripMenuItem.Size = new Size(187, 26);
            softwareInfoToolStripMenuItem.Text = "Software Info";
            softwareInfoToolStripMenuItem.Click += softwareInfoToolStripMenuItem_Click;

            networkInfoToolStripMenuItem.Name = "networkInfoToolStripMenuItem";
            networkInfoToolStripMenuItem.Size = new Size(187, 26);
            networkInfoToolStripMenuItem.Text = "Network Info";
            networkInfoToolStripMenuItem.Click += networkInfoToolStripMenuItem_Click;

            // remoteShellToolStripMenuItem
            remoteShellToolStripMenuItem.Name = "remoteShellToolStripMenuItem";
            remoteShellToolStripMenuItem.Size = new Size(180, 24);
            remoteShellToolStripMenuItem.Text = "Remote Shell";
            remoteShellToolStripMenuItem.Click += remoteShellToolStripMenuItem_Click;

            // messageBoxToolStripMenuItem (from main)
            messageBoxToolStripMenuItem.Name = "messageBoxToolStripMenuItem";
            messageBoxToolStripMenuItem.Size = new Size(180, 24);
            messageBoxToolStripMenuItem.Text = "Message Box";
            messageBoxToolStripMenuItem.Click += messageBoxToolStripMenuItem_Click;

            // fileManagerToolStripMenuItem
            fileManagerToolStripMenuItem.Name = "fileManagerToolStripMenuItem";
            fileManagerToolStripMenuItem.Size = new Size(180, 24);
            fileManagerToolStripMenuItem.Text = "File Manager";
            fileManagerToolStripMenuItem.Click += fileManagerToolStripMenuItem_Click;

            // taskManagerToolStripMenuItem (from feature branch)
            taskManagerToolStripMenuItem.Name = "taskManagerToolStripMenuItem";
            taskManagerToolStripMenuItem.Size = new Size(200, 24);
            taskManagerToolStripMenuItem.Text = "Task Manager";
            taskManagerToolStripMenuItem.Click += taskManagerToolStripMenuItem_Click;

            // screenControlToolStripMenuItem
            screenControlToolStripMenuItem.Name = "screenControlToolStripMenuItem";
            screenControlToolStripMenuItem.Size = new Size(200, 24);
            screenControlToolStripMenuItem.Text = "Screen Control";
            screenControlToolStripMenuItem.Click += screenControlToolStripMenuItem_Click;

            // keyLogger items (from main)
            keyLoggerToolStripMenuItem.Name = "keyLoggerToolStripMenuItem";
            keyLoggerToolStripMenuItem.Size = new Size(180, 24);
            keyLoggerToolStripMenuItem.Text = "Key Logger";
            keyLoggerToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                keyLoggerStartParallelToolStripMenuItem,
                keyLoggerStartContinuousToolStripMenuItem
            });

            keyLoggerStartParallelToolStripMenuItem.Name = "keyLoggerStartParallelToolStripMenuItem";
            keyLoggerStartParallelToolStripMenuItem.Size = new Size(220, 26);
            keyLoggerStartParallelToolStripMenuItem.Text = "Start (Parallel)";
            keyLoggerStartParallelToolStripMenuItem.Click += keyLoggerStartParallelToolStripMenuItem_Click;

            keyLoggerStartContinuousToolStripMenuItem.Name = "keyLoggerStartContinuousToolStripMenuItem";
            keyLoggerStartContinuousToolStripMenuItem.Size = new Size(220, 26);
            keyLoggerStartContinuousToolStripMenuItem.Text = "Start (Continuous)";
            keyLoggerStartContinuousToolStripMenuItem.Click += keyLoggerStartContinuousToolStripMenuItem_Click;

            // shutdownActionToolStripMenuItem (group)
            shutdownActionToolStripMenuItem.Name = "shutdownActionToolStripMenuItem";
            shutdownActionToolStripMenuItem.Size = new Size(180, 24);
            shutdownActionToolStripMenuItem.Text = "Action";
            shutdownActionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                ShutdownToolStripMenuItem,
                RestartToolStripMenuItem,
                StandbyToolStripMenuItem
            });

            ShutdownToolStripMenuItem.Name = "ShutdownToolStripMenuItem";
            ShutdownToolStripMenuItem.Size = new Size(220, 26);
            ShutdownToolStripMenuItem.Text = "Shutdown";
            ShutdownToolStripMenuItem.Click += ShutdownToolStripMenuItem_Click;

            RestartToolStripMenuItem.Name = "RestartToolStripMenuItem";
            RestartToolStripMenuItem.Size = new Size(220, 26);
            RestartToolStripMenuItem.Text = "Restart";
            RestartToolStripMenuItem.Click += RestartToolStripMenuItem_Click;

            StandbyToolStripMenuItem.Name = "StandbyToolStripMenuItem";
            StandbyToolStripMenuItem.Size = new Size(220, 26);
            StandbyToolStripMenuItem.Text = "Standby";
            StandbyToolStripMenuItem.Click += StandbyToolStripMenuItem_Click;

            // userManagementToolStripMenuItem
            userManagementToolStripMenuItem.Name = "userManagementToolStripMenuItem";
            userManagementToolStripMenuItem.Size = new Size(200, 24);
            userManagementToolStripMenuItem.Text = "User Management";
            userManagementToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                elevateToolStripMenuItem,
                disconnectToolStripMenuItem
            });

            elevateToolStripMenuItem.Name = "elevateToolStripMenuItem";
            elevateToolStripMenuItem.Size = new Size(200, 26);
            elevateToolStripMenuItem.Text = "Elevate (Run as Admin)";
            elevateToolStripMenuItem.Click += ElevateToolStripMenuItem_Click;

            disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            disconnectToolStripMenuItem.Size = new Size(200, 26);
            disconnectToolStripMenuItem.Text = "Disconnect";
            disconnectToolStripMenuItem.Click += DisconnectToolStripMenuItem_Click;

            // rightPanel
            rightPanel.Controls.Add(txtLog);
            rightPanel.Controls.Add(cboLogFilter);
            rightPanel.Controls.Add(lblLogFilter);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Padding = new Padding(8);
            rightPanel.Size = new Size(441, 552);
            rightPanel.TabIndex = 0;

            // txtLog
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Location = new Point(16, 46);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(413, 488);
            txtLog.TabIndex = 2;

            // cboLogFilter
            cboLogFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLogFilter.FormattingEnabled = true;
            cboLogFilter.Items.AddRange(new object[] { "All", "Info", "Warning", "Error", "Network" });
            cboLogFilter.Location = new Point(64, 12);
            cboLogFilter.Name = "cboLogFilter";
            cboLogFilter.Size = new Size(160, 28);
            cboLogFilter.TabIndex = 1;

            // lblLogFilter
            lblLogFilter.AutoSize = true;
            lblLogFilter.Location = new Point(16, 16);
            lblLogFilter.Name = "lblLogFilter";
            lblLogFilter.Size = new Size(42, 20);
            lblLogFilter.TabIndex = 0;
            lblLogFilter.Text = "Filter";

            // MainServerForm
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 600);
            Controls.Add(splitContainer);
            Controls.Add(topPanel);
            Name = "MainServerForm";
            Text = "Server";

            // Resume layout
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((ISupportInitialize)dgvClients).EndInit();
            ctxClients.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            rightPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // --- Fields (merged) ---
        private Panel topPanel;
        private Button btnChangePort;
        private Button btnStartStop;
        private TextBox txtPort;
        private Label lblStatus;
        private Label lblPort;
        private SplitContainer splitContainer;
        private DataGridView dgvClients;
        private ContextMenuStrip ctxClients;
        private ToolStripMenuItem systemInfoToolStripMenuItem;
        private ToolStripMenuItem hardwareInfoToolStripMenuItem;
        private ToolStripMenuItem softwareInfoToolStripMenuItem;
        private ToolStripMenuItem networkInfoToolStripMenuItem;
        private ToolStripMenuItem remoteShellToolStripMenuItem;
        private ToolStripMenuItem messageBoxToolStripMenuItem;
        private ToolStripMenuItem fileManagerToolStripMenuItem;
        private ToolStripMenuItem taskManagerToolStripMenuItem;
        private ToolStripMenuItem screenControlToolStripMenuItem;
        private ToolStripMenuItem keyLoggerToolStripMenuItem;
        private ToolStripMenuItem keyLoggerStartParallelToolStripMenuItem;
        private ToolStripMenuItem keyLoggerStartContinuousToolStripMenuItem;
        private ToolStripMenuItem shutdownActionToolStripMenuItem;
        private ToolStripMenuItem ShutdownToolStripMenuItem;
        private ToolStripMenuItem RestartToolStripMenuItem;
        private ToolStripMenuItem StandbyToolStripMenuItem;
        private ToolStripMenuItem userManagementToolStripMenuItem;
        private ToolStripMenuItem elevateToolStripMenuItem;
        private ToolStripMenuItem disconnectToolStripMenuItem;
        private Panel rightPanel;
        private ComboBox cboLogFilter;
        private Label lblLogFilter;
        private TextBox txtLog;
    }
}
