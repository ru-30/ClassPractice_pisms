using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class ReportsForm : Form
    {
        private readonly DateTimePicker fromPicker = new DateTimePicker();
        private readonly DateTimePicker toPicker = new DateTimePicker();
        private readonly Label totalLabel = new Label();
        private readonly DataGridView bestSellerGrid = new DataGridView();
        private readonly DataGridView cashierGrid = new DataGridView();
        private readonly Button exportButton = new Button();
        private List<SaleRecord> filteredSales = new List<SaleRecord>();

        public ReportsForm(bool canExport)
        {
            Text = "PISMS - Reports"; Size = new Size(950, 650); StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(new Label { Text = "From", Location = new Point(20, 20), AutoSize = true }); fromPicker.Location = new Point(20, 43); fromPicker.Width = 150; Controls.Add(fromPicker);
            Controls.Add(new Label { Text = "To", Location = new Point(190, 20), AutoSize = true }); toPicker.Location = new Point(190, 43); toPicker.Width = 150; Controls.Add(toPicker);
            var runButton = new Button { Text = "Generate report", Location = new Point(360, 42), Width = 130 }; runButton.Click += (sender, args) => GenerateReport(); Controls.Add(runButton);
            exportButton.Text = "Export CSV"; exportButton.Location = new Point(505, 42); exportButton.Width = 110; exportButton.Enabled = canExport; exportButton.Click += ExportButton_Click; Controls.Add(exportButton);
            totalLabel.Location = new Point(20, 90); totalLabel.AutoSize = true; totalLabel.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold); Controls.Add(totalLabel);
            Controls.Add(new Label { Text = "Best-selling medicines", Location = new Point(20, 130), AutoSize = true, Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold) }); ConfigureGrid(bestSellerGrid, new Point(20, 160));
            Controls.Add(new Label { Text = "Sales per cashier", Location = new Point(20, 405), AutoSize = true, Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold) }); ConfigureGrid(cashierGrid, new Point(20, 435));
            fromPicker.Value = DateTime.Today; toPicker.Value = DateTime.Today; GenerateReport();
        }

        private void ConfigureGrid(DataGridView grid, Point location)
        {
            grid.Location = location; grid.Size = new Size(890, 210); grid.ReadOnly = true; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; Controls.Add(grid);
        }

        private void GenerateReport()
        {
            if (fromPicker.Value.Date > toPicker.Value.Date) { MessageBox.Show("The start date cannot be after the end date.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            DateTime from = fromPicker.Value.Date; DateTime to = toPicker.Value.Date.AddDays(1);
            filteredSales = SalesStore.Sales.Where(s => s.Date >= from && s.Date < to).ToList();
            totalLabel.Text = "Total sales: " + filteredSales.Sum(s => s.Total).ToString("N0") + " RWF | Transactions: " + filteredSales.Count;
            bestSellerGrid.DataSource = filteredSales.SelectMany(s => s.Lines).GroupBy(l => l.Medicine.Name).Select(g => new { Medicine = g.Key, QuantitySold = g.Sum(l => l.Quantity), SalesAmount = g.Sum(l => l.LineTotal) }).OrderByDescending(x => x.QuantitySold).ToList();
            cashierGrid.DataSource = filteredSales.GroupBy(s => s.CashierName).Select(g => new { Cashier = g.Key, Transactions = g.Count(), SalesAmount = g.Sum(s => s.Total) }).OrderByDescending(x => x.SalesAmount).ToList();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = "PISMS-Sales-Report.csv" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                var csv = new StringBuilder(); csv.AppendLine("SaleId,Date,Cashier,Total,InsurancePercent,PatientPayable");
                foreach (SaleRecord sale in filteredSales) csv.AppendLine(string.Join(",", sale.SaleId, sale.Date.ToString("s"), Escape(sale.CashierName), sale.Total, sale.InsurancePercent, sale.PatientPayable));
                File.WriteAllText(dialog.FileName, csv.ToString());
                MessageBox.Show("Report exported successfully.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string Escape(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }
    }
}
