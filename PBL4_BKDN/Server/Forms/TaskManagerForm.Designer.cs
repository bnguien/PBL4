namespace Server.Forms
{
	partial class TaskManagerForm

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
			lblDetails = new Label();
			btnEndtask = new Button();
			txtSearch = new TextBox();
			btnSearch = new Button();
			btnRefresh = new Button();
			dataGridView1 = new DataGridView();
			colName = new DataGridViewTextBoxColumn();
			colPID = new DataGridViewTextBoxColumn();
			colStatus = new DataGridViewTextBoxColumn();
			colUserName = new DataGridViewTextBoxColumn();
			colCPU = new DataGridViewTextBoxColumn();
			colWorkingSet = new DataGridViewTextBoxColumn();
			colPlatform = new DataGridViewTextBoxColumn();
			colUACVirtualization = new DataGridViewTextBoxColumn();
			tableLayoutPanel1 = new TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			tableLayoutPanel1.SuspendLayout();
			SuspendLayout();
			// 
			// lblDetails
			// 
			lblDetails.AutoSize = true;
			lblDetails.Dock = DockStyle.Fill;
			lblDetails.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
			lblDetails.Location = new Point(3, 0);
			lblDetails.Name = "lblDetails";
			lblDetails.Size = new Size(120, 32);
			lblDetails.TabIndex = 1;
			lblDetails.Text = "Details";
			lblDetails.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// btnEndtask
			// 
			btnEndtask.Cursor = Cursors.Hand;
			btnEndtask.Dock = DockStyle.Fill;
			btnEndtask.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			btnEndtask.Location = new Point(1107, 3);
			btnEndtask.Margin = new Padding(3, 3, 0, 3);
			btnEndtask.Name = "btnEndtask";
			btnEndtask.Size = new Size(87, 26);
			btnEndtask.TabIndex = 4;
			btnEndtask.Text = "End Task";
			btnEndtask.UseVisualStyleBackColor = true;
			// 
			// txtSearch
			// 
			txtSearch.Dock = DockStyle.Fill;
			txtSearch.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			txtSearch.Location = new Point(129, 3);
			txtSearch.Margin = new Padding(3, 3, 10, 3);
			txtSearch.Name = "txtSearch";
			txtSearch.Size = new Size(785, 27);
			txtSearch.TabIndex = 1;
			txtSearch.Text = "Search process...";
			// 
			// btnSearch
			// 
			btnSearch.Cursor = Cursors.Hand;
			btnSearch.Dock = DockStyle.Fill;
			btnSearch.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			btnSearch.Location = new Point(924, 3);
			btnSearch.Margin = new Padding(0, 3, 10, 3);
			btnSearch.Name = "btnSearch";
			btnSearch.Size = new Size(80, 26);
			btnSearch.TabIndex = 2;
			btnSearch.Text = "Search";
			btnSearch.UseVisualStyleBackColor = true;
			// 
			// btnRefresh
			// 
			btnRefresh.Cursor = Cursors.Hand;
			btnRefresh.Dock = DockStyle.Fill;
			btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			btnRefresh.Location = new Point(1014, 3);
			btnRefresh.Margin = new Padding(0, 3, 10, 3);
			btnRefresh.Name = "btnRefresh";
			btnRefresh.Size = new Size(80, 26);
			btnRefresh.TabIndex = 3;
			btnRefresh.Text = "Refresh";
			btnRefresh.UseVisualStyleBackColor = true;
			// 
			// dataGridView1
			// 
			dataGridView1.AllowUserToOrderColumns = true;
			dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colName, colPID, colStatus, colUserName, colCPU, colWorkingSet, colPlatform, colUACVirtualization });
			dataGridView1.Location = new Point(12, 50);
			dataGridView1.Margin = new Padding(3, 3, 3, 10);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.RowHeadersWidth = 51;
			dataGridView1.Size = new Size(1204, 568);
			dataGridView1.TabIndex = 6;
			// 
			// colName
			// 
			colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colName.FillWeight = 173.762F;
			colName.HeaderText = "Name";
			colName.MinimumWidth = 6;
			colName.Name = "colName";
			colName.ReadOnly = true;
			colName.Resizable = DataGridViewTriState.True;
			// 
			// colPID
			// 
			colPID.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			colPID.FillWeight = 50.67853F;
			colPID.HeaderText = "PID";
			colPID.MinimumWidth = 6;
			colPID.Name = "colPID";
			colPID.ReadOnly = true;
			colPID.Width = 61;
			// 
			// colStatus
			// 
			colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			colStatus.FillWeight = 101.827728F;
			colStatus.HeaderText = "Status";
			colStatus.MinimumWidth = 6;
			colStatus.Name = "colStatus";
			colStatus.ReadOnly = true;
			colStatus.Width = 78;
			// 
			// colUserName
			// 
			colUserName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colUserName.FillWeight = 101.967369F;
			colUserName.HeaderText = "User Name";
			colUserName.MinimumWidth = 6;
			colUserName.Name = "colUserName";
			colUserName.ReadOnly = true;
			// 
			// colCPU
			// 
			colCPU.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			colCPU.FillWeight = 54.4473724F;
			colCPU.HeaderText = "CPU";
			colCPU.MinimumWidth = 6;
			colCPU.Name = "colCPU";
			colCPU.ReadOnly = true;
			colCPU.Width = 65;
			// 
			// colWorkingSet
			// 
			colWorkingSet.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			colWorkingSet.FillWeight = 108.40313F;
			colWorkingSet.HeaderText = "Working Set";
			colWorkingSet.MinimumWidth = 6;
			colWorkingSet.Name = "colWorkingSet";
			colWorkingSet.ReadOnly = true;
			colWorkingSet.Width = 118;
			// 
			// colPlatform
			// 
			colPlatform.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			colPlatform.FillWeight = 57.43248F;
			colPlatform.HeaderText = "Platform";
			colPlatform.MinimumWidth = 6;
			colPlatform.Name = "colPlatform";
			colPlatform.ReadOnly = true;
			colPlatform.Width = 95;
			// 
			// colUACVirtualization
			// 
			colUACVirtualization.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colUACVirtualization.FillWeight = 151.481567F;
			colUACVirtualization.HeaderText = "UAC Virtualization";
			colUACVirtualization.MinimumWidth = 6;
			colUACVirtualization.Name = "colUACVirtualization";
			colUACVirtualization.ReadOnly = true;
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel1.ColumnCount = 6;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10F));
			tableLayoutPanel1.Controls.Add(lblDetails, 0, 0);
			tableLayoutPanel1.Controls.Add(txtSearch, 1, 0);
			tableLayoutPanel1.Controls.Add(btnSearch, 2, 0);
			tableLayoutPanel1.Controls.Add(btnRefresh, 3, 0);
			tableLayoutPanel1.Controls.Add(btnEndtask, 4, 0);
			tableLayoutPanel1.Location = new Point(12, 12);
			tableLayoutPanel1.Margin = new Padding(3, 3, 3, 10);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 1;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
			tableLayoutPanel1.Size = new Size(1204, 32);
			tableLayoutPanel1.TabIndex = 7;
			// 
			// TaskManagerForm
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1228, 630);
			Controls.Add(tableLayoutPanel1);
			Controls.Add(dataGridView1);
			MinimumSize = new Size(800, 500);
			Name = "TaskManagerForm";
			Text = "Task Manager";
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel1.PerformLayout();
			ResumeLayout(false);
		}

		#endregion
		private Label lblDetails;
		private Button btnEndtask;
		private TextBox txtSearch;
		private Button btnSearch;
		private Button btnRefresh;
		private DataGridView dataGridView1;

		private TableLayoutPanel tableLayoutPanel1;
		private DataGridViewTextBoxColumn colName;
		private DataGridViewTextBoxColumn colPID;
		private DataGridViewTextBoxColumn colStatus;
		private DataGridViewTextBoxColumn colUserName;
		private DataGridViewTextBoxColumn colCPU;
		private DataGridViewTextBoxColumn colWorkingSet;
		private DataGridViewTextBoxColumn colPlatform;
		private DataGridViewTextBoxColumn colUACVirtualization;
	}
}