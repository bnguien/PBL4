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
    public partial class SystemInfoForm: Form
    {
        private readonly Common.Models.SystemInfoModel _systemInfo;
        public SystemInfoForm()
        {
            InitializeComponent();
        }

        public SystemInfoForm(Common.Models.SystemInfoModel model): this()
        {
            _systemInfo = model;
            LoadData();
        }

        private void LoadData()
        {
            var cpuName = _systemInfo.Hardware?.Cpu?.Name ?? "-";
            var ramTotal = _systemInfo.Hardware?.Ram?.TotalMB.ToString() ?? "-";
            var osName = _systemInfo.Software?.OS?.Name ?? "-";
            var primaryIp = _systemInfo.Network?.PrimaryIPv4 ?? "-";
            var apps = _systemInfo.Software?.InstalledApps?.Count ?? 0;
            lblSummary.Text = $"CPU: {cpuName}\r\nRAM: {ramTotal} MB\r\nOS: {osName}\r\nPrimary IP: {primaryIp}\r\nInstalled apps: {apps}";
        }
    }
}
