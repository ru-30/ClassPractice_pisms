using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class PrescriptionForm : Form
    {
        private readonly Customer customer;
        private readonly TextBox doctorTextBox = new TextBox();
        private readonly DateTimePicker datePicker = new DateTimePicker();
        private readonly TextBox medicinesTextBox = new TextBox();
        private readonly DataGridView grid = new DataGridView();

        public PrescriptionForm(Customer customer)
        {
            this.customer = customer;
            Text = "PISMS - Prescription"; Size = new Size(700, 500); StartPosition = FormStartPosition.CenterParent;
            Controls.Add(new Label { Text = "Customer: " + customer.FullName, Location = new Point(20, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Doctor's name", Location = new Point(20, 60), AutoSize = true }); doctorTextBox.Location = new Point(20, 83); doctorTextBox.Width = 200; Controls.Add(doctorTextBox);
            Controls.Add(new Label { Text = "Date", Location = new Point(235, 60), AutoSize = true }); datePicker.Location = new Point(235, 83); Controls.Add(datePicker);
            Controls.Add(new Label { Text = "Medicines", Location = new Point(390, 60), AutoSize = true }); medicinesTextBox.Location = new Point(390, 83); medicinesTextBox.Width = 250; Controls.Add(medicinesTextBox);
            var saveButton = new Button { Text = "Save prescription", Location = new Point(20, 125), Width = 130 }; saveButton.Click += SaveButton_Click; Controls.Add(saveButton);
            grid.Location = new Point(20, 175); grid.Size = new Size(620, 250); grid.ReadOnly = true; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(grid); RefreshGrid();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(doctorTextBox.Text) || string.IsNullOrWhiteSpace(medicinesTextBox.Text)) { MessageBox.Show("Doctor's name and medicines are required.", "Prescription", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            CustomerStore.AddPrescription(customer, doctorTextBox.Text, datePicker.Value, medicinesTextBox.Text); doctorTextBox.Clear(); medicinesTextBox.Clear(); RefreshGrid();
        }

        private void RefreshGrid()
        {
            grid.DataSource = CustomerStore.Prescriptions.Where(p => p.Customer != null && p.Customer.CustomerId == customer.CustomerId).Select(p => new { p.PrescriptionId, p.DoctorName, p.Date, p.Medicines }).ToList();
        }
    }
}
