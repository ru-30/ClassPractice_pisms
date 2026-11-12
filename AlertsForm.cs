using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class AlertsForm : Form
    {
        private readonly DataGridView lowStockGrid = new DataGridView();
        private readonly DataGridView expiryGrid = new DataGridView();

        public AlertsForm()
        {
            Text = "PISMS - Alerts";
            Size = new Size(950, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(new Label { Text = "Low-stock medicines", Location = new Point(20, 20), AutoSize = true, Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold) });
            lowStockGrid.Location = new Point(20, 50); lowStockGrid.Size = new Size(890, 220); ConfigureGrid(lowStockGrid); Controls.Add(lowStockGrid);
            Controls.Add(new Label { Text = "Expired or expiring within 90 days", Location = new Point(20, 300), AutoSize = true, Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold) });
            expiryGrid.Location = new Point(20, 330); expiryGrid.Size = new Size(890, 220); ConfigureGrid(expiryGrid); Controls.Add(expiryGrid);
            RefreshAlerts();
        }

        private void ConfigureGrid(DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void RefreshAlerts()
        {
            lowStockGrid.DataSource = MedicineStore.GetAll().Where(m => m.QuantityInStock <= m.ReorderLevel).Select(m => new { m.MedicineCode, m.Name, m.QuantityInStock, m.ReorderLevel, m.ExpiryDate }).ToList();
            expiryGrid.DataSource = MedicineStore.GetAll().Where(AlertService.IsExpiredOrNearExpiry).Select(m => new { m.MedicineCode, m.Name, m.QuantityInStock, m.ExpiryDate, Status = m.ExpiryDate.Date < DateTime.Today ? "Expired" : "Expiring soon" }).ToList();
        }
    }
}
