using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class SupplierForm : Form
    {
        private readonly TextBox nameTextBox = new TextBox();
        private readonly TextBox phoneTextBox = new TextBox();
        private readonly TextBox addressTextBox = new TextBox();
        private readonly DataGridView supplierGrid = new DataGridView();

        public SupplierForm()
        {
            Text = "PISMS - Suppliers";
            Size = new Size(700, 500);
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(new Label { Text = "Supplier name", Location = new Point(20, 20), AutoSize = true });
            nameTextBox.Location = new Point(20, 43); nameTextBox.Width = 190; Controls.Add(nameTextBox);
            Controls.Add(new Label { Text = "Phone number", Location = new Point(225, 20), AutoSize = true });
            phoneTextBox.Location = new Point(225, 43); phoneTextBox.Width = 150; Controls.Add(phoneTextBox);
            Controls.Add(new Label { Text = "Address", Location = new Point(390, 20), AutoSize = true });
            addressTextBox.Location = new Point(390, 43); addressTextBox.Width = 250; Controls.Add(addressTextBox);
            var addButton = new Button { Text = "Register supplier", Location = new Point(20, 80), Width = 140 };
            addButton.Click += AddButton_Click; Controls.Add(addButton);
            supplierGrid.Location = new Point(20, 125); supplierGrid.Size = new Size(640, 300); supplierGrid.ReadOnly = true; supplierGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(supplierGrid);
            RefreshGrid();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(phoneTextBox.Text) || string.IsNullOrWhiteSpace(addressTextBox.Text))
            {
                MessageBox.Show("Supplier name, phone number, and address are required.", "Suppliers", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            StockStore.AddSupplier(nameTextBox.Text, phoneTextBox.Text, addressTextBox.Text);
            nameTextBox.Clear(); phoneTextBox.Clear(); addressTextBox.Clear(); RefreshGrid();
        }

        private void RefreshGrid()
        {
            supplierGrid.DataSource = StockStore.Suppliers.Select(s => new { s.SupplierId, s.Name, s.Phone, s.Address }).ToList();
        }
    }
}
