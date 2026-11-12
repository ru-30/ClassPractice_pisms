using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlConnector;

namespace ClassPractice
{
    public sealed class SaleLine { public Medicine Medicine { get; set; } public int Quantity { get; set; } public int LineTotal { get { return Medicine.UnitPrice * Quantity; } } }
    public sealed class SaleRecord { public int SaleId { get; set; } public DateTime Date { get; set; } public string CashierName { get; set; } public Customer Customer { get; set; } public int Total { get; set; } public int InsurancePercent { get; set; } public int PatientPayable { get; set; } public List<SaleLine> Lines { get; set; } }

    public static class SalesStore
    {
        public static IReadOnlyList<SaleRecord> Sales { get { return LoadSales().AsReadOnly(); } }

        public static SaleRecord Save(string cashierName, List<SaleLine> lines, int insurancePercent, Customer customer = null, int amountPaid = 0)
        {
            int total = GetTotal(lines); int patientPayable = GetPatientPayable(lines, insurancePercent);
            using (var connection = Database.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    int cashierId = GetId(connection, transaction, "SELECT user_id FROM users WHERE username = @value LIMIT 1", cashierName);
                    int? insurerId = GetNullableId(connection, transaction, "SELECT insurer_id FROM insurers WHERE coverage_percent = @value AND is_active = 1 LIMIT 1", insurancePercent);
                    int? customerId = customer == null ? (int?)null : customer.CustomerId;
                    string receipt = "R" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    long saleId;
                    using (var command = new MySqlCommand("INSERT INTO sales (receipt_number, cashier_id, customer_id, insurer_id, insurance_percent, total, patient_payable, amount_paid, change_amount) VALUES (@receipt, @cashier, @customer, @insurer, @percent, @total, @payable, 0, 0); SELECT LAST_INSERT_ID();", connection, transaction))
                    {
                        Add(command, "@percent", insurancePercent); Add(command, "@total", total); Add(command, "@payable", patientPayable); command.CommandText = command.CommandText.Replace("0, 0); SELECT", "@paid, @change); SELECT"); Add(command, "@paid", amountPaid); Add(command, "@change", amountPaid - patientPayable); saleId = Convert.ToInt64(command.ExecuteScalar());
                    }
                    foreach (SaleLine line in lines)
                    {
                        using (var stockCommand = new MySqlCommand("UPDATE medicines SET quantity_in_stock = quantity_in_stock - @quantity, has_sales_history = 1 WHERE medicine_id = @medicine AND quantity_in_stock >= @quantity", connection, transaction))
                        { Add(stockCommand, "@quantity", line.Quantity); Add(stockCommand, "@medicine", line.Medicine.MedicineId); if (stockCommand.ExecuteNonQuery() != 1) throw new InvalidOperationException("Insufficient stock for " + line.Medicine.Name + "."); }
                        using (var itemCommand = new MySqlCommand("INSERT INTO sale_items (sale_id, medicine_id, quantity, unit_price, line_total) VALUES (@sale, @medicine, @quantity, @price, @lineTotal)", connection, transaction))
                        { Add(itemCommand, "@sale", saleId); Add(itemCommand, "@medicine", line.Medicine.MedicineId); Add(itemCommand, "@quantity", line.Quantity); Add(itemCommand, "@price", line.Medicine.UnitPrice); Add(itemCommand, "@lineTotal", line.LineTotal); itemCommand.ExecuteNonQuery(); }
                    }
                    transaction.Commit();
                    foreach (SaleLine line in lines) line.Medicine.QuantityInStock -= line.Quantity;
                    return new SaleRecord { SaleId = Convert.ToInt32(saleId), Date = DateTime.Now, CashierName = cashierName, Customer = customer, Total = total, InsurancePercent = insurancePercent, PatientPayable = patientPayable, Lines = new List<SaleLine>(lines) };
                }
                catch { transaction.Rollback(); throw; }
            }
        }

        public static int GetTotal(IEnumerable<SaleLine> lines) { return lines.Sum(line => line.LineTotal); }
        public static int GetPatientPayable(IEnumerable<SaleLine> lines, int insurancePercent) { return (int)Math.Round(GetTotal(lines) * (100 - insurancePercent) / 100.0, MidpointRounding.AwayFromZero); }

        private static List<SaleRecord> LoadSales()
        {
            DataTable table = DatabaseCommands.Query("SELECT s.sale_id, s.sale_date, s.cashier_id, u.username, s.customer_id, c.full_name, c.phone, c.insurance_number, s.total, s.insurance_percent, s.patient_payable FROM sales s JOIN users u ON u.user_id = s.cashier_id LEFT JOIN customers c ON c.customer_id = s.customer_id ORDER BY s.sale_date DESC");
            var sales = table.Rows.Cast<DataRow>().Select(row => new SaleRecord { SaleId = Convert.ToInt32(row["sale_id"]), Date = Convert.ToDateTime(row["sale_date"]), CashierName = Convert.ToString(row["username"]), Customer = row["customer_id"] == DBNull.Value ? null : new Customer { CustomerId = Convert.ToInt32(row["customer_id"]), FullName = Convert.ToString(row["full_name"]), Phone = Convert.ToString(row["phone"]), InsuranceNumber = row["insurance_number"] == DBNull.Value ? string.Empty : Convert.ToString(row["insurance_number"]) }, Total = Convert.ToInt32(row["total"]), InsurancePercent = Convert.ToInt32(row["insurance_percent"]), PatientPayable = Convert.ToInt32(row["patient_payable"]), Lines = new List<SaleLine>() }).ToList();
            foreach (SaleRecord sale in sales)
            {
                DataTable items = DatabaseCommands.Query("SELECT m.medicine_id, m.medicine_code, m.name, m.category, m.unit_price, m.quantity_in_stock, m.reorder_level, m.expiry_date, m.prescription_only, m.has_sales_history, i.quantity FROM sale_items i JOIN medicines m ON m.medicine_id = i.medicine_id WHERE i.sale_id = @sale", DatabaseCommands.Parameter("@sale", sale.SaleId));
                sale.Lines = items.Rows.Cast<DataRow>().Select(row => new SaleLine { Quantity = Convert.ToInt32(row["quantity"]), Medicine = new Medicine { MedicineId = Convert.ToInt32(row["medicine_id"]), MedicineCode = Convert.ToString(row["medicine_code"]), Name = Convert.ToString(row["name"]), Category = (MedicineCategory)Enum.Parse(typeof(MedicineCategory), Convert.ToString(row["category"])), UnitPrice = Convert.ToInt32(row["unit_price"]), QuantityInStock = Convert.ToInt32(row["quantity_in_stock"]), ReorderLevel = Convert.ToInt32(row["reorder_level"]), ExpiryDate = Convert.ToDateTime(row["expiry_date"]), PrescriptionOnly = Convert.ToBoolean(row["prescription_only"]), HasSalesHistory = Convert.ToBoolean(row["has_sales_history"]) } }).ToList();
            }
            return sales;
        }

        private static int GetId(MySqlConnection connection, MySqlTransaction transaction, string sql, string value) { using (var command = new MySqlCommand(sql, connection, transaction)) { Add(command, "@value", value); object result = command.ExecuteScalar(); if (result == null) throw new InvalidOperationException("The cashier account was not found."); return Convert.ToInt32(result); } }
        private static int? GetNullableId(MySqlConnection connection, MySqlTransaction transaction, string sql, int value) { using (var command = new MySqlCommand(sql, connection, transaction)) { Add(command, "@value", value); object result = command.ExecuteScalar(); return result == null ? (int?)null : Convert.ToInt32(result); } }
        private static void Add(MySqlCommand command, string name, object value) { command.Parameters.Add(new MySqlParameter(name, value ?? DBNull.Value)); }
    }
}
