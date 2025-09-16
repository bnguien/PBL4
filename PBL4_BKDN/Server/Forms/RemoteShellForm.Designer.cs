using System;
using System.Windows.Forms;

namespace Server.Forms
{
    partial class RemoteShellForm
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

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Remote Shell";

            // Tạo control + bố cục trước khi gắn event
            CreateControls();
            SetupLayout();
        }

        #endregion

        // ===== Control declarations (Designer quản lý) =====
        private System.Windows.Forms.RichTextBox txtConsoleOutput;
        private System.Windows.Forms.TextBox txtConsoleInput;
        private System.Windows.Forms.Panel pnlInputArea;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel connectionStatusLabel;
    }
}
