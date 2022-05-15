using System;
using System.Windows.Forms;
using Goosetuv.Snow.NET.Classes.Computer;
using Goosetuv.Snow.NET.Methods;

namespace Snow.NET.WindowsForms_Examples
{
    public partial class frmComputers : Form
    {

        public int CustomerID { get; set; }
        public Authenticate auth { get; set; }

        private int loadedComputers = 0;

        public frmComputers()
        {
            InitializeComponent();
        }

        private void frmComputers_Load(object sender, EventArgs e)
        {
            Text = $"Snow License Manager Web API, Viewing CID: {CustomerID}";
        }

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgvComputers.Rows.Clear();

            LoadComputers();
        }

        private void LoadComputers(int _skipCount = 0)
        {
            ComputerData cd = new ComputerData(auth.Client);
            Computers computers = cd.Computers(cid: 1, skipCount: _skipCount);

            for (int i = 0; i < computers.Body.Count; i++)
            {
                Computer computer = cd.Computer(cid: 1, computers.Body[i].Body.Id);

                dgvComputers.Rows.Add(
                    computer.Body.Id,
                    computer.Body.Name,
                    computer.Body.Manufacturer,
                    computer.Body.OperatingSystem,
                    computer.Body.ClientVersion,
                    computer.Body.LastScanDate
                );

                loadedComputers++;
            }

            if (loadedComputers < computers.Body.Count)
            {
                LoadComputers(_skipCount: loadedComputers);
            }
        }
    }
}
