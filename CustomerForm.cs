using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class CustomerForm : Form
    {
        private readonly TextBox nameTextBox = new TextBox();
        private readonly TextBox phoneTextBox = new TextBox();
        private readonly TextBox insuranceTextBox = new TextBox();
        private readonly TextBox searchTextBox = new TextBox();
        private readonly DataGridView customerGrid = new DataGridView();
        private readonly DataGridView historyGrid = new DataGridView();
        private Customer selectedCustomer;

        public CustomerForm()
        {
            Text = "PISMS - Customers"; Size = new Size(950, 620); StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(new Label { Text = "Full name", Location = new Point(20, 20), AutoSize = true }); nameTextBox.Location = new Point(20, 43); nameTextBox.Width = 210; Controls.Add(nameTextBox);
            Controls.Add(new Label { Text = "Phone number", Location = new Point(245, 20), AutoSize = true }); phoneTextBox.Location = new Point(245, 43); phoneTextBox.Width = 150; Controls.Add(phoneTextBox);
            Controls.Add(new Label { Text = "Insurance number (optional)", Location = new Point(410, 20), AutoSize = true }); insuranceTextBox.Location = new Point(410, 43); insuranceTextBox.Width = 190; Controls.Add(insuranceTextBox);
            var registerButton = new Button { Text = "Register customer", Location = new Point(620, 40), Width = 140 }; registerButton.Click += RegisterButton_Click; Controls.Add(registerButton);
            var prescriptionButton = new Button { Text = "Record prescription", Location = new Point(770, 40), Width = 140 }; prescriptionButton.Click += PrescriptionButton_Click; Controls.Add(prescriptionButton);
            Controls.Add(new Label { Text = "Search customers", Location = new Point(20, 90), AutoSize = true }); searchTextBox.Location = new Point(140, 87); searchTextBox.Width = 300; searchTextBox.TextChanged += (sender, args) => RefreshCustomers(); Controls.Add(searchTextBox);
            customerGrid.Location = new Point(20, 125); customerGrid.Size = new Size(890, 190); customerGrid.ReadOnly = true; customerGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; customerGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; customerGrid.SelectionChanged += CustomerGrid_SelectionChanged; Controls.Add(customerGrid);
            Controls.Add(new Label { Text = "Purchase history", Location = new Point(20, 340), AutoSize = true }); historyGrid.Location = new Point(20, 370); historyGrid.Size = new Size(890, 190); historyGrid.ReadOnly = true; historyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(historyGrid);
            RefreshCustomers();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(phoneTextBox.Text)) { MessageBox.Show("Full name and phone number are required.", "Customers", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            CustomerStore.AddCustomer(nameTextBox.Text, phoneTextBox.Text, insuranceTextBox.Text); nameTextBox.Clear(); phoneTextBox.Clear(); insuranceTextBox.Clear(); RefreshCustomers();
        }

        private void RefreshCustomers()
        {
            string term = searchTextBox.Text.Trim(); customerGrid.DataSource = CustomerStore.Customers.Where(c => term.Length == 0 || c.FullName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || c.Phone.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || c.InsuranceNumber.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).Select(c => new { c.CustomerId, c.FullName, c.Phone, c.InsuranceNumber }).ToList();
        }

        private void CustomerGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (customerGrid.CurrentRow == null) return;
            var row = customerGrid.CurrentRow.DataBoundItem;
            if (row == null) return;
            int id = (int)row.GetType().GetProperty("CustomerId").GetValue(row, null);
            var customer = CustomerStore.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null) return;
            selectedCustomer = customer;
            var records = SalesStore.Sales.Where(s => s.Customer != null && s.Customer.CustomerId == customer.CustomerId).Select(s => new { s.SaleId, s.Date, s.Total, s.PatientPayable, s.CashierName }).ToList();
            historyGrid.DataSource = records;
        }

        private void PrescriptionButton_Click(object sender, EventArgs e)
        {
            if (selectedCustomer == null) { MessageBox.Show("Select a customer first.", "Prescription", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using (var form = new PrescriptionForm(selectedCustomer)) form.ShowDialog(this);
        }
    }
}
