namespace Server.Forms
{
    partial class NetworkInfoForm
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
            this.grpPrimary = new System.Windows.Forms.GroupBox();
            this.lblPrimary = new System.Windows.Forms.Label();
            this.grpAdapters = new System.Windows.Forms.GroupBox();
            this.dgvAdapters = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1.SuspendLayout();
            this.grpPrimary.SuspendLayout();
            this.grpAdapters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdapters)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.grpPrimary, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.grpAdapters, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(900, 560);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // grpPrimary
            // 
            this.grpPrimary.Controls.Add(this.lblPrimary);
            this.grpPrimary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPrimary.Location = new System.Drawing.Point(3, 3);
            this.grpPrimary.Name = "grpPrimary";
            this.grpPrimary.Size = new System.Drawing.Size(894, 94);
            this.grpPrimary.TabIndex = 0;
            this.grpPrimary.TabStop = false;
            this.grpPrimary.Text = "Primary";
            // 
            // lblPrimary
            // 
            this.lblPrimary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPrimary.Location = new System.Drawing.Point(3, 19);
            this.lblPrimary.Name = "lblPrimary";
            this.lblPrimary.Size = new System.Drawing.Size(888, 72);
            this.lblPrimary.TabIndex = 0;
            this.lblPrimary.Text = "Primary IPv4: -\r\nPrimary MAC: -";
            // 
            // grpAdapters
            // 
            this.grpAdapters.Controls.Add(this.dgvAdapters);
            this.grpAdapters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAdapters.Location = new System.Drawing.Point(3, 103);
            this.grpAdapters.Name = "grpAdapters";
            this.grpAdapters.Size = new System.Drawing.Size(894, 454);
            this.grpAdapters.TabIndex = 1;
            this.grpAdapters.TabStop = false;
            this.grpAdapters.Text = "Adapters";
            // 
            // dgvAdapters
            // 
            this.dgvAdapters.AllowUserToAddRows = false;
            this.dgvAdapters.AllowUserToDeleteRows = false;
            this.dgvAdapters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAdapters.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvAdapters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAdapters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAdapters.Location = new System.Drawing.Point(3, 19);
            this.dgvAdapters.MultiSelect = false;
            this.dgvAdapters.Name = "dgvAdapters";
            this.dgvAdapters.ReadOnly = true;
            this.dgvAdapters.RowHeadersVisible = false;
            this.dgvAdapters.RowTemplate.Height = 25;
            this.dgvAdapters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAdapters.TabIndex = 0;
            // 
            // NetworkInfoForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "NetworkInfoForm";
            this.Text = "Network";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.grpPrimary.ResumeLayout(false);
            this.grpAdapters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAdapters)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox grpPrimary;
        private System.Windows.Forms.Label lblPrimary;
        private System.Windows.Forms.GroupBox grpAdapters;
        private System.Windows.Forms.DataGridView dgvAdapters;
    }
}