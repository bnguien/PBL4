namespace Server.Forms
{
    partial class SoftwareInfoForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.grpOs = new System.Windows.Forms.GroupBox();
            this.lblOs = new System.Windows.Forms.Label();
            this.grpRuntime = new System.Windows.Forms.GroupBox();
            this.lblRuntime = new System.Windows.Forms.Label();
            this.grpApps = new System.Windows.Forms.GroupBox();
            this.dgvApps = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1.SuspendLayout();
            this.grpOs.SuspendLayout();
            this.grpRuntime.SuspendLayout();
            this.grpApps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvApps)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.grpOs, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.grpRuntime, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.grpApps, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(900, 560);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // grpOs
            // 
            this.grpOs.Controls.Add(this.lblOs);
            this.grpOs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOs.Location = new System.Drawing.Point(3, 3);
            this.grpOs.Name = "grpOs";
            this.grpOs.Size = new System.Drawing.Size(444, 114);
            this.grpOs.TabIndex = 0;
            this.grpOs.TabStop = false;
            this.grpOs.Text = "Operating System";
            // 
            // lblOs
            // 
            this.lblOs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOs.Location = new System.Drawing.Point(3, 19);
            this.lblOs.Name = "lblOs";
            this.lblOs.Size = new System.Drawing.Size(438, 92);
            this.lblOs.TabIndex = 0;
            this.lblOs.Text = "Name: -\r\nVersion: -\r\nBuild: -\r\nArchitecture: -";
            // 
            // grpRuntime
            // 
            this.grpRuntime.Controls.Add(this.lblRuntime);
            this.grpRuntime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpRuntime.Location = new System.Drawing.Point(453, 3);
            this.grpRuntime.Name = "grpRuntime";
            this.grpRuntime.Size = new System.Drawing.Size(444, 114);
            this.grpRuntime.TabIndex = 1;
            this.grpRuntime.TabStop = false;
            this.grpRuntime.Text = ".NET Runtime";
            // 
            // lblRuntime
            // 
            this.lblRuntime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRuntime.Location = new System.Drawing.Point(3, 19);
            this.lblRuntime.Name = "lblRuntime";
            this.lblRuntime.Size = new System.Drawing.Size(438, 92);
            this.lblRuntime.TabIndex = 0;
            this.lblRuntime.Text = ".NET: -\r\nCLR: -";
            // 
            // grpApps
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.grpApps, 2);
            this.grpApps.Controls.Add(this.dgvApps);
            this.grpApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpApps.Location = new System.Drawing.Point(3, 123);
            this.grpApps.Name = "grpApps";
            this.grpApps.Size = new System.Drawing.Size(894, 434);
            this.grpApps.TabIndex = 2;
            this.grpApps.TabStop = false;
            this.grpApps.Text = "Installed Applications";
            // 
            // dgvApps
            // 
            this.dgvApps.AllowUserToAddRows = false;
            this.dgvApps.AllowUserToDeleteRows = false;
            this.dgvApps.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvApps.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvApps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvApps.Location = new System.Drawing.Point(3, 19);
            this.dgvApps.MultiSelect = false;
            this.dgvApps.Name = "dgvApps";
            this.dgvApps.ReadOnly = true;
            this.dgvApps.RowHeadersVisible = false;
            this.dgvApps.RowTemplate.Height = 25;
            this.dgvApps.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvApps.TabIndex = 0;
            // 
            // SoftwareInfoForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SoftwareInfoForm";
            this.Text = "Software";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.grpOs.ResumeLayout(false);
            this.grpRuntime.ResumeLayout(false);
            this.grpApps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvApps)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpOs;
        private System.Windows.Forms.Label lblOs;
        private System.Windows.Forms.GroupBox grpRuntime;
        private System.Windows.Forms.Label lblRuntime;
        private System.Windows.Forms.GroupBox grpApps;
        private System.Windows.Forms.DataGridView dgvApps;
    }
}