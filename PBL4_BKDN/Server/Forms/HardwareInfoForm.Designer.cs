namespace Server.Forms
{
    partial class HardwareInfoForm
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
			tableLayoutPanel1 = new TableLayoutPanel();
			grpCpu = new GroupBox();
			lblCpu = new Label();
			grpRam = new GroupBox();
			lblRam = new Label();
			grpGpu = new GroupBox();
			lstGpu = new ListBox();
			grpDisks = new GroupBox();
			dgvDisks = new DataGridView();
			tableLayoutPanel1.SuspendLayout();
			grpCpu.SuspendLayout();
			grpRam.SuspendLayout();
			grpGpu.SuspendLayout();
			grpDisks.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dgvDisks).BeginInit();
			SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.ColumnCount = 2;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
			tableLayoutPanel1.Controls.Add(grpCpu, 0, 0);
			tableLayoutPanel1.Controls.Add(grpRam, 1, 0);
			tableLayoutPanel1.Controls.Add(grpGpu, 0, 1);
			tableLayoutPanel1.Controls.Add(grpDisks, 0, 2);
			tableLayoutPanel1.Dock = DockStyle.Fill;
			tableLayoutPanel1.Location = new Point(0, 0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 3;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 135F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel1.Size = new Size(1100, 700);
			tableLayoutPanel1.TabIndex = 0;
			// 
			// grpCpu
			// 
			grpCpu.Controls.Add(lblCpu);
			grpCpu.Dock = DockStyle.Fill;
			grpCpu.Location = new Point(3, 3);
			grpCpu.Name = "grpCpu";
			grpCpu.Size = new Size(654, 110);
			grpCpu.TabIndex = 0;
			grpCpu.TabStop = false;
			grpCpu.Text = "CPU";
			grpCpu.Enter += grpCpu_Enter;
			// 
			// lblCpu
			// 
			lblCpu.Dock = DockStyle.Fill;
			lblCpu.Location = new Point(3, 23);
			lblCpu.Name = "lblCpu";
			lblCpu.Size = new Size(648, 84);
			lblCpu.TabIndex = 0;
			lblCpu.Text = "Name: -\r\nLogical: -\r\nPhysical: -\r\nMax Clock: -";
			// 
			// grpRam
			// 
			grpRam.Controls.Add(lblRam);
			grpRam.Dock = DockStyle.Fill;
			grpRam.Location = new Point(663, 3);
			grpRam.Name = "grpRam";
			grpRam.Size = new Size(434, 110);
			grpRam.TabIndex = 1;
			grpRam.TabStop = false;
			grpRam.Text = "RAM";
			// 
			// lblRam
			// 
			lblRam.Dock = DockStyle.Fill;
			lblRam.Location = new Point(3, 23);
			lblRam.Name = "lblRam";
			lblRam.Size = new Size(428, 84);
			lblRam.TabIndex = 0;
			lblRam.Text = "Total: - MB\r\nAvailable: - MB";
			// 
			// grpGpu
			// 
			tableLayoutPanel1.SetColumnSpan(grpGpu, 2);
			grpGpu.Controls.Add(lstGpu);
			grpGpu.Dock = DockStyle.Fill;
			grpGpu.Location = new Point(3, 119);
			grpGpu.Name = "grpGpu";
			grpGpu.Size = new Size(1094, 129);
			grpGpu.TabIndex = 2;
			grpGpu.TabStop = false;
			grpGpu.Text = "GPU";
			// 
			// lstGpu
			// 
			lstGpu.Dock = DockStyle.Fill;
			lstGpu.FormattingEnabled = true;
			lstGpu.Location = new Point(3, 23);
			lstGpu.Name = "lstGpu";
			lstGpu.Size = new Size(1088, 103);
			lstGpu.TabIndex = 0;
			// 
			// grpDisks
			// 
			tableLayoutPanel1.SetColumnSpan(grpDisks, 2);
			grpDisks.Controls.Add(dgvDisks);
			grpDisks.Dock = DockStyle.Fill;
			grpDisks.Location = new Point(3, 254);
			grpDisks.Name = "grpDisks";
			grpDisks.Size = new Size(1094, 443);
			grpDisks.TabIndex = 3;
			grpDisks.TabStop = false;
			grpDisks.Text = "Disks";
			// 
			// dgvDisks
			// 
			dgvDisks.AllowUserToAddRows = false;
			dgvDisks.AllowUserToDeleteRows = false;
			dgvDisks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgvDisks.BackgroundColor = SystemColors.Window;
			dgvDisks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dgvDisks.Dock = DockStyle.Fill;
			dgvDisks.Location = new Point(3, 23);
			dgvDisks.MultiSelect = false;
			dgvDisks.Name = "dgvDisks";
			dgvDisks.ReadOnly = true;
			dgvDisks.RowHeadersVisible = false;
			dgvDisks.RowHeadersWidth = 51;
			dgvDisks.RowTemplate.Height = 25;
			dgvDisks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dgvDisks.Size = new Size(1088, 417);
			dgvDisks.TabIndex = 0;
			// 
			// HardwareInfoForm
			// 
			AutoScaleDimensions = new SizeF(120F, 120F);
			AutoScaleMode = AutoScaleMode.Dpi;
			ClientSize = new Size(1100, 700);
			Controls.Add(tableLayoutPanel1);
			MinimumSize = new Size(900, 600);
			Name = "HardwareInfoForm";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Hardware";
			tableLayoutPanel1.ResumeLayout(false);
			grpCpu.ResumeLayout(false);
			grpRam.ResumeLayout(false);
			grpGpu.ResumeLayout(false);
			grpDisks.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)dgvDisks).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpCpu;
        private System.Windows.Forms.Label lblCpu;
        private System.Windows.Forms.GroupBox grpRam;
        private System.Windows.Forms.Label lblRam;
        private System.Windows.Forms.GroupBox grpGpu;
        private System.Windows.Forms.ListBox lstGpu;
        private System.Windows.Forms.GroupBox grpDisks;
        private System.Windows.Forms.DataGridView dgvDisks;
    }
}