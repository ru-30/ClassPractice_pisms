using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPractice
{
    public partial class MenuForm : Form
    {
        private readonly UserAccount account;

        public MenuForm(UserAccount account)
        {
            this.account = account;
            InitializeComponent();
            userLabel.Text = "Logged in user: " + account.Username + " (" + account.Role + ")";
            var changePasswordButton = new Button { Text = "Change Password", Location = new Point(35, 255), Size = new Size(135, 40) };
            changePasswordButton.Click += (sender, args) => { using (var form = new ChangePasswordForm(account)) form.ShowDialog(this); };
            Controls.Add(changePasswordButton);
            ConfigureRoleAccess();
        }

        private void ConfigureRoleAccess()
        {
            usersButton.Visible = account.Role == UserRole.Administrator;
            medicinesButton.Visible = account.Role != UserRole.Cashier;
            suppliersButton.Visible = account.Role != UserRole.Cashier;
            receiveStockButton.Visible = account.Role != UserRole.Cashier;
            alertsButton.Visible = account.Role != UserRole.Cashier;
            reportsButton.Visible = account.Role != UserRole.Cashier;
            alertsButton.Text = "Alerts (" + (AlertService.LowStockCount + AlertService.ExpiryCount) + ")";
        }

        private void ModuleButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == medicinesButton)
            {
                using (var medicineForm = new MedicineForm())
                {
                    medicineForm.ShowDialog(this);
                }
                return;
            }
            if (button == usersButton)
            {
                using (var userForm = new UserForm())
                {
                    userForm.ShowDialog(this);
                }
                return;
            }
            if (button == reportsButton)
            {
                using (var reportsForm = new ReportsForm(account.Role == UserRole.Administrator))
                {
                    reportsForm.ShowDialog(this);
                }
                return;
            }
            if (button == alertsButton)
            {
                using (var alertsForm = new AlertsForm())
                {
                    alertsForm.ShowDialog(this);
                }
                return;
            }
            if (button == customersButton)
            {
                using (var customerForm = new CustomerForm())
                {
                    customerForm.ShowDialog(this);
                }
                return;
            }
            if (button == suppliersButton)
            {
                using (var supplierForm = new SupplierForm())
                {
                    supplierForm.ShowDialog(this);
                }
                return;
            }
            if (button == receiveStockButton)
            {
                using (var receiveStockForm = new ReceiveStockForm())
                {
                    receiveStockForm.ShowDialog(this);
                }
                return;
            }
            if (button == salesButton)
            {
                using (var salesForm = new SalesForm(account))
                {
                    salesForm.ShowDialog(this);
                }
                return;
            }
            MessageBox.Show(button.Text + " will be available in the next implementation step.", "PISMS", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
