using System;
using System.Linq;
using System.Windows.Forms;
using Goosetuv.Snow.NET.Classes.Platform;
using Goosetuv.Snow.NET.Methods;

namespace Snow.NET.WindowsForms_Examples
{
    public partial class frmLogin : Form
    {
        Authenticate auth;

        public int CustomerID { get; set; }

        public frmLogin()
        {
            InitializeComponent();
        }

        private void btnAuthenticate_Click(object sender, EventArgs e)
        {
            bool isError = false;

            if (string.IsNullOrEmpty(txtUsername.Text) && !isError)
            {
                MessageBox.Show("Please enter an API username.", "Authentication", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isError = true;
            }

            if (string.IsNullOrEmpty(txtPassword.Text) && !isError)
            {
                MessageBox.Show("Please enter an API password.", "Authentication", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isError = true;
            }

            if (string.IsNullOrEmpty(txtBaseURL.Text) && !isError)
            {
                MessageBox.Show("Please enter a platform url.", "Authentication", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isError = true;
            }

            try
            {
                if (!isError)
                {
                    auth = new Authenticate(txtBaseURL.Text, txtUsername.Text, txtPassword.Text);

                    if (auth.Client != null)
                    {
                        PlatformData platform = new PlatformData(auth.Client);

                        Customers customers = platform.Customers();

                        for (int i = 0; i < customers.Body.Count; i++)
                        {
                            cbCustomerList.Items.Add($"{customers.Body[i].Body.Name} - {customers.Body[i].Body.Id}");
                        }

                        btnLogin.Enabled = true;
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Value cannot be null"))
                {
                    MessageBox.Show("Json value is null.  Are your credentials and baseURL correct and platform online?");
                }
                else if (ex.Message.Contains("'<' is an invalid start of a value."))
                {
                    MessageBox.Show("Invalid start of value.  Ensure that your baseURL has /api/ at the end.");
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool isError = false;

            if (cbCustomerList.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a customer.", "Authentication", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!isError)
            {
                string customerDetails = cbCustomerList.Text;
                MessageBox.Show($"Login successful for {cbCustomerList.Text}");

                CustomerID = Convert.ToInt32(customerDetails.Split('-').Last());

                frmComputers frmComputers = new frmComputers()
                {
                    CustomerID = CustomerID,
                    auth = auth,
                };

                frmComputers.Show();
                Hide();
            }
        }
    }
}
