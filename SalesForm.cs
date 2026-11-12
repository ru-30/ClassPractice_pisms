using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class SalesForm : Form
    {
        private readonly UserAccount cashier;
        private readonly ComboBox medicineComboBox = new ComboBox();
        private readonly ComboBox customerComboBox = new ComboBox();
        private readonly TextBox quantityTextBox = new TextBox();
        private readonly TextBox prescriptionTextBox = new TextBox();
        private readonly CheckBox insuredCheckBox = new CheckBox();
        private readonly ComboBox insurerComboBox = new ComboBox();
        private readonly TextBox paidTextBox = new TextBox();
        private readonly Label totalLabel = new Label();
        private readonly Label payableLabel = new Label();
        private readonly DataGridView cartGrid = new DataGridView();
        private readonly List<SaleLine> cart = new List<SaleLine>();
        private readonly Dictionary<string, int> insurers = new Dictionary<string, int>
        {
            { "Community Health Insurance", 85 }, { "Private Insurance", 70 }
        };

        public SalesForm(UserAccount cashier)
        {
            this.cashier = cashier;
            Text = "PISMS - Sales and Billing";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BuildControls();
            RefreshMedicineList();
            RefreshCustomerList();
            RefreshCart();
        }

        private void BuildControls()
        {
            Controls.Add(new Label { Text = "Medicine", Location = new Point(20, 20), AutoSize = true });
            medicineComboBox.Location = new Point(20, 43); medicineComboBox.Width = 220; medicineComboBox.DropDownStyle = ComboBoxStyle.DropDownList; Controls.Add(medicineComboBox);
            Controls.Add(new Label { Text = "Quantity", Location = new Point(255, 20), AutoSize = true });
            quantityTextBox.Location = new Point(255, 43); quantityTextBox.Width = 80; Controls.Add(quantityTextBox);
            Controls.Add(new Label { Text = "Prescription number", Location = new Point(350, 20), AutoSize = true });
            prescriptionTextBox.Location = new Point(350, 43); prescriptionTextBox.Width = 145; Controls.Add(prescriptionTextBox);
            var addButton = new Button { Text = "Add to cart", Location = new Point(510, 40), Width = 110 }; addButton.Click += AddButton_Click; Controls.Add(addButton);
            var removeButton = new Button { Text = "Remove item", Location = new Point(630, 40), Width = 110 }; removeButton.Click += RemoveButton_Click; Controls.Add(removeButton);

            Controls.Add(new Label { Text = "Customer (optional)", Location = new Point(380, 85), AutoSize = true });
            customerComboBox.Location = new Point(500, 82); customerComboBox.Width = 220; customerComboBox.DropDownStyle = ComboBoxStyle.DropDownList; Controls.Add(customerComboBox);
            insuredCheckBox.Text = "Insured sale"; insuredCheckBox.Location = new Point(20, 85); insuredCheckBox.AutoSize = true; insuredCheckBox.CheckedChanged += (sender, args) => UpdateTotals(); Controls.Add(insuredCheckBox);
            insurerComboBox.Location = new Point(140, 82); insurerComboBox.Width = 220; insurerComboBox.DropDownStyle = ComboBoxStyle.DropDownList; insurerComboBox.DataSource = insurers.Keys.ToList(); insurerComboBox.Enabled = false; insurerComboBox.SelectedIndexChanged += (sender, args) => UpdateTotals(); Controls.Add(insurerComboBox);
            insuredCheckBox.CheckedChanged += (sender, args) => insurerComboBox.Enabled = insuredCheckBox.Checked;

            cartGrid.Location = new Point(20, 125); cartGrid.Size = new Size(720, 300); cartGrid.ReadOnly = true; cartGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; cartGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(cartGrid);
            totalLabel.Location = new Point(20, 450); totalLabel.AutoSize = true; totalLabel.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold); Controls.Add(totalLabel);
            payableLabel.Location = new Point(20, 480); payableLabel.AutoSize = true; payableLabel.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold); Controls.Add(payableLabel);
            Controls.Add(new Label { Text = "Amount paid (RWF)", Location = new Point(500, 450), AutoSize = true });
            paidTextBox.Location = new Point(650, 447); paidTextBox.Width = 120; Controls.Add(paidTextBox);
            var confirmButton = new Button { Text = "Confirm sale", Location = new Point(780, 445), Width = 130 }; confirmButton.Click += ConfirmButton_Click; Controls.Add(confirmButton);
        }

        private void RefreshCustomerList()
        {
            var customers = CustomerStore.Customers.ToList();
            customers.Insert(0, new Customer { FullName = "No customer" });
            customerComboBox.DataSource = customers;
            customerComboBox.DisplayMember = "FullName";
        }

        private void RefreshMedicineList()
        {
            medicineComboBox.DataSource = MedicineStore.GetAll().Where(m => m.QuantityInStock > 0).ToList();
            medicineComboBox.DisplayMember = "Name";
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var medicine = medicineComboBox.SelectedItem as Medicine;
            int quantity;
            if (medicine == null) { ShowError("Select a medicine."); return; }
            if (!int.TryParse(quantityTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out quantity) || quantity <= 0) { ShowError("Quantity must be a whole number greater than 0."); return; }
            if (medicine.ExpiryDate.Date < DateTime.Today) { ShowError("This medicine has expired and cannot be sold."); return; }
            int alreadyInCart = cart.Where(line => line.Medicine == medicine).Sum(line => line.Quantity);
            if (quantity + alreadyInCart > medicine.QuantityInStock) { ShowError("Insufficient stock. Stock left: " + (medicine.QuantityInStock - alreadyInCart)); return; }
            if (medicine.PrescriptionOnly && string.IsNullOrWhiteSpace(prescriptionTextBox.Text)) { ShowError("A prescription number is required for this medicine."); return; }
            SaleLine existing = cart.FirstOrDefault(line => line.Medicine == medicine);
            if (existing == null) cart.Add(new SaleLine { Medicine = medicine, Quantity = quantity }); else existing.Quantity += quantity;
            quantityTextBox.Clear(); prescriptionTextBox.Clear(); RefreshCart();
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (cartGrid.CurrentRow == null) { ShowError("Select an item to remove."); return; }
            SaleLine line = cartGrid.CurrentRow.DataBoundItem as SaleLine;
            if (line != null) cart.Remove(line);
            RefreshCart();
        }

        private void RefreshCart()
        {
            cartGrid.DataSource = null; cartGrid.DataSource = cart.ToList(); UpdateTotals();
        }

        private void UpdateTotals()
        {
            int total = SalesStore.GetTotal(cart);
            int coverage = insuredCheckBox.Checked && insurerComboBox.SelectedItem != null ? insurers[insurerComboBox.SelectedItem.ToString()] : 0;
            int payable = SalesStore.GetPatientPayable(cart, coverage);
            totalLabel.Text = "Total: " + total.ToString("N0") + " RWF";
            payableLabel.Text = "Patient pays: " + payable.ToString("N0") + " RWF";
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0) { ShowError("Add at least one medicine to the sale."); return; }
            int coverage = insuredCheckBox.Checked && insurerComboBox.SelectedItem != null ? insurers[insurerComboBox.SelectedItem.ToString()] : 0;
            int payable = SalesStore.GetPatientPayable(cart, coverage);
            int paid;
            if (!int.TryParse(paidTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out paid) || paid < payable) { ShowError("Amount paid must be at least " + payable.ToString("N0") + " RWF."); return; }
            Customer customer = customerComboBox.SelectedItem as Customer;
            if (customer != null && customer.CustomerId == 0) customer = null;
            SaleRecord sale = SalesStore.Save(cashier.Username, cart, coverage, customer, paid);
            int change = paid - sale.PatientPayable;
            ShowReceipt(sale, paid, change);
            cart.Clear(); paidTextBox.Clear(); RefreshMedicineList(); RefreshCart();
        }

        private void ShowReceipt(SaleRecord sale, int paid, int change)
        {
            var receipt = new StringBuilder();
            receipt.AppendLine("PISMS RECEIPT"); receipt.AppendLine("Receipt No: " + sale.SaleId); receipt.AppendLine("Date: " + sale.Date); receipt.AppendLine("Cashier: " + sale.CashierName); receipt.AppendLine(new string('-', 35));
            foreach (SaleLine line in sale.Lines) receipt.AppendLine(line.Quantity + " x " + line.Medicine.Name + " = " + line.LineTotal.ToString("N0") + " RWF");
            receipt.AppendLine(new string('-', 35)); receipt.AppendLine("Total: " + sale.Total.ToString("N0") + " RWF"); receipt.AppendLine("Patient pays: " + sale.PatientPayable.ToString("N0") + " RWF"); receipt.AppendLine("Paid: " + paid.ToString("N0") + " RWF"); receipt.AppendLine("Change: " + change.ToString("N0") + " RWF");
            MessageBox.Show(receipt.ToString(), "Printable Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowError(string message) { MessageBox.Show(message, "Sales", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}
