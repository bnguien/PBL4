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
    public partial class NetworkInfoForm: Form
    {
        private readonly Common.Models.NetworkInfoModel? _network;
        public NetworkInfoForm()
        {
            InitializeComponent();
        }

        public NetworkInfoForm(Common.Models.NetworkInfoModel network): this()
        {
            _network = network;
            LoadData();
        }

        private void LoadData()
        {
            lblPrimary.Text = $"Primary IPv4: {_network?.PrimaryIPv4 ?? "N/A"}\r\nPrimary MAC: {_network?.PrimaryMac ?? "N/A"}";
            var adapterRows = new System.Collections.Generic.List<dynamic>();
            foreach (var a in _network?.Adapters ?? new List<Common.Models.NetworkAdapterModel>())
            {
                adapterRows.Add(new
                {
                    Name = a.Name,
                    Mac = a.Mac,
                    IsUp = a.IsUp,
                    SpeedMbps = a.SpeedMbps,
                    IPv4 = string.Join(", ", a.IPv4s),
                    IPv6 = string.Join(", ", a.IPv6s)
                });
            }
            dgvAdapters.DataSource = adapterRows;
        }
    }
}
