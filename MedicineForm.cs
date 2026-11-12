using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class MedicineForm : Form
    {
        private readonly TextBox nameTextBox = new TextBox();
        private readonly ComboBox categoryComboBox = new ComboBox();
        private readonly TextBox priceTextBox = new TextBox();
        private readonly TextBox quantityTextBox = new TextBox();
        private readonly TextBox reorderTextBox = new TextBox();
        private readonly DateTimePicker expiryPicker = new DateTimePicker();
        private readonly CheckBox prescriptionCheckBox = new CheckBox();
        private readonly TextBox searchTextBox = new TextBox();
        private readonly DataGridView medicineGrid = new DataGridView();
        private Medicine selectedMedicine;

        public MedicineForm()
        {
            Text = "PISMS - Medicine Management";
            Size = new Size(1050, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BuildControls();
            RefreshGrid();
        }

        private void BuildControls()
        {
            Controls.Add(new Label { Text = "Medicine name", Location = new Point(20, 20), AutoSize = true });
            nameTextBox.Location = new Point(20, 42); nameTextBox.Width = 180; Controls.Add(nameTextBox);
            Controls.Add(new Label { Text = "Category", Location = new Point(215, 20), AutoSize = true });
            categoryComboBox.Location = new Point(215, 42); categoryComboBox.Width = 145; categoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList; categoryComboBox.DataSource = Enum.GetValues(typeof(MedicineCategory)); Controls.Add(categoryComboBox);
            Controls.Add(new Label { Text = "Unit price (RWF)", Location = new Point(375, 20), AutoSize = true });
            priceTextBox.Location = new Point(375, 42); priceTextBox.Width = 110; Controls.Add(priceTextBox);
            Controls.Add(new Label { Text = "Stock", Location = new Point(500, 20), AutoSize = true });
            quantityTextBox.Location = new Point(500, 42); quantityTextBox.Width = 80; Controls.Add(quantityTextBox);
            Controls.Add(new Label { Text = "Reorder level", Location = new Point(595, 20), AutoSize = true });
            reorderTextBox.Location = new Point(595, 42); reorderTextBox.Width = 90; Controls.Add(reorderTextBox);
            Controls.Add(new Label { Text = "Expiry date", Location = new Point(700, 20), AutoSize = true });
            expiryPicker.Location = new Point(700, 42); expiryPicker.Width = 145; expiryPicker.MinDate = DateTime.Today; Controls.Add(expiryPicker);
            prescriptionCheckBox.Text = "Prescription only"; prescriptionCheckBox.Location = new Point(20, 78); prescriptionCheckBox.AutoSize = true; Controls.Add(prescriptionCheckBox);

            var addButton = new Button { Text = "Add medicine", Location = new Point(215, 75), Width = 110 };
            var updateButton = new Button { Text = "Update", Location = new Point(335, 75), Width = 85 };
            var deleteButton = new Button { Text = "Delete", Location = new Point(430, 75), Width = 85 };
            var clearButton = new Button { Text = "Clear", Location = new Point(525, 75), Width = 85 };
            addButton.Click += AddButton_Click; updateButton.Click += UpdateButton_Click; deleteButton.Click += DeleteButton_Click; clearButton.Click += ClearButton_Click;
            Controls.AddRange(new Control[] { addButton, updateButton, deleteButton, clearButton });

            Controls.Add(new Label { Text = "Search by name, code, or category", Location = new Point(20, 120), AutoSize = true });
            searchTextBox.Location = new Point(220, 117); searchTextBox.Width = 300; searchTextBox.TextChanged += (sender, args) => RefreshGrid(); Controls.Add(searchTextBox);
            medicineGrid.Location = new Point(20, 155); medicineGrid.Size = new Size(990, 390); medicineGrid.ReadOnly = true; medicineGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; medicineGrid.AutoGenerateColumns = true; medicineGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; medicineGrid.SelectionChanged += MedicineGrid_SelectionChanged; Controls.Add(medicineGrid);
        }

        private bool TryReadInput(out string name, out MedicineCategory category, out int price, out int quantity, out int reorder, out DateTime expiry, out bool prescriptionOnly)
        {
            name = nameTextBox.Text.Trim(); category = (MedicineCategory)categoryComboBox.SelectedItem; price = 0; quantity = 0; reorder = 0; expiry = expiryPicker.Value.Date; prescriptionOnly = prescriptionCheckBox.Checked;
            if (name.Length == 0) return Invalid("Medicine name is required.");
            if (!int.TryParse(priceTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out price) || price <= 0) return Invalid("Unit price must be a positive whole number.");
            if (!int.TryParse(quantityTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out quantity) || quantity < 0) return Invalid("Quantity must be a whole number of 0 or more.");
            if (!int.TryParse(reorderTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out reorder) || reorder < 0) return Invalid("Reorder level must be a whole number of 0 or more.");
            if (expiry.Date < DateTime.Today) return Invalid("Expiry date cannot be earlier than today.");
            if (MedicineStore.Exists(name, expiry, selectedMedicine == null ? null : selectedMedicine.MedicineCode)) return Invalid("A medicine with the same name and expiry date already exists.");
            return true;
        }

        private bool Invalid(string message) { MessageBox.Show(message, "Medicine", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
        private void AddButton_Click(object sender, EventArgs e) { string n; MedicineCategory c; int p, q, r; DateTime d; bool pr; if (!TryReadInput(out n, out c, out p, out q, out r, out d, out pr)) return; MedicineStore.Add(n, c, p, q, r, d, pr); RefreshGrid(); ClearForm(); }
        private void UpdateButton_Click(object sender, EventArgs e) { if (selectedMedicine == null) { Invalid("Select a medicine to update."); return; } string n; MedicineCategory c; int p, q, r; DateTime d; bool pr; if (!TryReadInput(out n, out c, out p, out q, out r, out d, out pr)) return; selectedMedicine.Name = n; selectedMedicine.Category = c; selectedMedicine.UnitPrice = p; selectedMedicine.QuantityInStock = q; selectedMedicine.ReorderLevel = r; selectedMedicine.ExpiryDate = d; selectedMedicine.PrescriptionOnly = pr; MedicineStore.Update(selectedMedicine); RefreshGrid(); }
        private void DeleteButton_Click(object sender, EventArgs e) { if (selectedMedicine == null) { Invalid("Select a medicine to delete."); return; } if (selectedMedicine.QuantityInStock > 0) { Invalid("A medicine can only be removed when its stock is zero and it has no sales history."); return; } MedicineStore.Remove(selectedMedicine); ClearForm(); RefreshGrid(); }
        private void ClearButton_Click(object sender, EventArgs e) { ClearForm(); }
        private void ClearForm() { selectedMedicine = null; nameTextBox.Clear(); priceTextBox.Clear(); quantityTextBox.Clear(); reorderTextBox.Clear(); prescriptionCheckBox.Checked = false; medicineGrid.ClearSelection(); }
        private void RefreshGrid() { string term = searchTextBox.Text.Trim(); medicineGrid.DataSource = MedicineStore.GetAll().Where(m => term.Length == 0 || m.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || m.MedicineCode.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || m.Category.ToString().IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).ToList(); }
        private void MedicineGrid_SelectionChanged(object sender, EventArgs e) { if (medicineGrid.CurrentRow == null) return; selectedMedicine = medicineGrid.CurrentRow.DataBoundItem as Medicine; if (selectedMedicine == null) return; nameTextBox.Text = selectedMedicine.Name; categoryComboBox.SelectedItem = selectedMedicine.Category; priceTextBox.Text = selectedMedicine.UnitPrice.ToString(); quantityTextBox.Text = selectedMedicine.QuantityInStock.ToString(); reorderTextBox.Text = selectedMedicine.ReorderLevel.ToString(); expiryPicker.Value = selectedMedicine.ExpiryDate; prescriptionCheckBox.Checked = selectedMedicine.PrescriptionOnly; }
    }
}
