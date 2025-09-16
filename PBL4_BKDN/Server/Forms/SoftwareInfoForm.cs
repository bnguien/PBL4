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
    public partial class SoftwareInfoForm: Form
    {
        private readonly Common.Models.SoftwareInfoModel _software;
        public SoftwareInfoForm()
        {
            InitializeComponent();
        }

        public SoftwareInfoForm(Common.Models.SoftwareInfoModel software): this()
        {
            _software = software;
            LoadData();
        }

        private void LoadData()
        {
            if (_software?.OS != null)
            {
                lblOs.Text = $"Name: {_software.OS.Name}\r\nVersion: {_software.OS.Version}\r\nBuild: {_software.OS.Build}\r\nArchitecture: {_software.OS.Architecture}";
            }
            if (_software?.Runtime != null)
            {
                lblRuntime.Text = $".NET: {_software.Runtime.DotnetVersion}\r\nCLR: {_software.Runtime.RuntimeDescription}";
            }
            dgvApps.DataSource = (_software?.InstalledApps ?? new System.Collections.Generic.List<Common.Models.InstalledAppModel>());
        }
    }
}
