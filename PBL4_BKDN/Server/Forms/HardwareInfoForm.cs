using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server.Forms
{
    public partial class HardwareInfoForm : Form
    {
        private readonly Common.Models.HardwareInfoModel _hardware;
        public HardwareInfoForm()
        {
            InitializeComponent();
        }

        public HardwareInfoForm(Common.Models.HardwareInfoModel hardware) : this()
        {
            _hardware = hardware;
            LoadData();
        }

        private void LoadData()
        {
            if (_hardware?.Cpu != null)
            {
                lblCpu.Text = $"Name: {_hardware.Cpu.Name}\r\nLogical: {_hardware.Cpu.CoresLogical}\r\nPhysical: {_hardware.Cpu.CoresPhysical}\r\nMax Clock: {_hardware.Cpu.MaxClockMHz} MHz";
            }
            if (_hardware?.Ram != null)
            {
                lblRam.Text = $"Total: {_hardware.Ram.TotalMB} MB\r\nAvailable: {_hardware.Ram.AvailableMB} MB";
            }
            lstGpu.Items.Clear();
            foreach (var g in _hardware?.Gpus ?? new System.Collections.Generic.List<Common.Models.GpuInfoModel>())
            {
                lstGpu.Items.Add($"{g.Name} ({g.MemoryMB} MB)");
            }
            var diskRows = new System.Collections.Generic.List<dynamic>();
            foreach (var d in _hardware?.Disks ?? new System.Collections.Generic.List<Common.Models.DiskInfoModel>())
            {
                double totalGb = d.TotalBytes / (1024.0 * 1024 * 1024);
                double freeGb = d.FreeBytes / (1024.0 * 1024 * 1024);
                diskRows.Add(new
                {
                    d.DriveLetter,
                    d.Type,
                    TotalGB = Math.Round(totalGb, 2),
                    FreeGB = Math.Round(freeGb, 2),
                    Format = d.Format
                });
            }
            dgvDisks.DataSource = diskRows;
        }

        private void grpCpu_Enter(object sender, EventArgs e)
        {

        }
    }
}
