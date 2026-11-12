using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class ReceiveStockForm : Form
    {
        private readonly ComboBox supplierComboBox = new ComboBox();
        private readonly ComboBox medicineComboBox = new ComboBox();
        private readonly TextBox quantityTextBox = new TextBox();
        private readonly TextBox costTextBox = new TextBox();
        private readonly DateTimePicker expiryPicker = new DateTimePicker();
        private readonly DataGridView receptionGrid = new DataGridView();

        public ReceiveStockForm()
        {
            Text = "PISMS - Receive Stock";
            Size = new Size(900, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BuildControls();
            RefreshLists();
            RefreshHistory();
        }

        private void BuildControls()
        {
            Controls.Add(new Label { Text = "Supplier", Location = new Point(20, 20), AutoSize = true });
            supplierComboBox.Location = new Point(20, 43); supplierComboBox.Width = 190; supplierComboBox.DropDownStyle = ComboBoxStyle.DropDownList; Controls.Add(supplierComboBox);
            Controls.Add(new Label { Text = "Medicine", Location = new Point(225, 20), AutoSize = true });
            medicineComboBox.Location = new Point(225, 43); medicineComboBox.Width = 190; medicineComboBox.DropDownStyle = ComboBoxStyle.DropDownList; Controls.Add(medicineComboBox);
            Controls.Add(new Label { Text = "Quantity", Location = new Point(430, 20), AutoSize = true });
            quantityTextBox.Location = new Point(430, 43); quantityTextBox.Width = 90; Controls.Add(quantityTextBox);
            Controls.Add(new Label { Text = "Unit cost (RWF)", Location = new Point(535, 20), AutoSize = true });
            costTextBox.Location = new Point(535, 43); costTextBox.Width = 110; Controls.Add(costTextBox);
            Controls.Add(new Label { Text = "Expiry date", Location = new Point(660, 20), AutoSize = true });
            expiryPicker.Location = new Point(660, 43); expiryPicker.Width = 150; expiryPicker.MinDate = DateTime.Today; Controls.Add(expiryPicker);
            var receiveButton = new Button { Text = "Confirm reception", Location = new Point(20, 82), Width = 140 };
            receiveButton.Click += ReceiveButton_Click; Controls.Add(receiveButton);
            receptionGrid.Location = new Point(20, 130); receptionGrid.Size = new Size(820, 350); receptionGrid.ReadOnly = true; receptionGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(receptionGrid);
        }

        private void RefreshLists()
        {
            supplierComboBox.DataSource = StockStore.Suppliers.ToList(); supplierComboBox.DisplayMember = "Name";
            medicineComboBox.DataSource = MedicineStore.GetAll().ToList(); medicineComboBox.DisplayMember = "Name";
        }

        private void ReceiveButton_Click(object sender, EventArgs e)
        {
            var supplier = supplierComboBox.SelectedItem as Supplier;
            var medicine = medicineComboBox.SelectedItem as Medicine;
            int quantity; int cost;
            if (supplier == null || medicine == null) { MessageBox.Show("Register a supplier and add a medicine before receiving stock.", "Receive Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!int.TryParse(quantityTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out quantity) || quantity <= 0) { MessageBox.Show("Quantity must be a positive whole number.", "Receive Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!int.TryParse(costTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out cost) || cost <= 0) { MessageBox.Show("Unit cost must be a positive whole number.", "Receive Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (expiryPicker.Value.Date < DateTime.Today) { MessageBox.Show("Expiry date cannot be earlier than today.", "Receive Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            StockStore.ReceiveStock(supplier, medicine, quantity, cost, expiryPicker.Value.Date);
            MessageBox.Show("Stock reception confirmed.", "Receive Stock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            quantityTextBox.Clear(); costTextBox.Clear(); RefreshHistory();
        }

        private void RefreshHistory()
        {
            receptionGrid.DataSource = StockStore.Receptions.Select(r => new { r.ReceptionId, r.Date, Supplier = r.Supplier.Name, Medicine = r.Medicine.Name, r.Quantity, r.UnitCost, r.ExpiryDate }).ToList();
        }
    }
}
