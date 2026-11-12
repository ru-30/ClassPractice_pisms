using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlConnector;

namespace ClassPractice
{
    public sealed class Supplier { public int SupplierId { get; set; } public string Name { get; set; } public string Phone { get; set; } public string Address { get; set; } }
    public sealed class StockReception { public int ReceptionId { get; set; } public DateTime Date { get; set; } public Supplier Supplier { get; set; } public Medicine Medicine { get; set; } public int Quantity { get; set; } public int UnitCost { get; set; } public DateTime ExpiryDate { get; set; } }

    public static class StockStore
    {
        public static IReadOnlyList<Supplier> Suppliers { get { return LoadSuppliers().AsReadOnly(); } }
        public static IReadOnlyList<StockReception> Receptions { get { return LoadReceptions().AsReadOnly(); } }

        public static Supplier AddSupplier(string name, string phone, string address)
        {
            object id = DatabaseCommands.ExecuteScalar("INSERT INTO suppliers (name, phone, address) VALUES (@name, @phone, @address); SELECT LAST_INSERT_ID();", DatabaseCommands.Parameter("@name", name.Trim()), DatabaseCommands.Parameter("@phone", phone.Trim()), DatabaseCommands.Parameter("@address", address.Trim()));
            return new Supplier { SupplierId = Convert.ToInt32(id), Name = name.Trim(), Phone = phone.Trim(), Address = address.Trim() };
        }

        public static void ReceiveStock(Supplier supplier, Medicine medicine, int quantity, int unitCost, DateTime expiryDate)
        {
            using (var connection = Database.CreateConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = new MySqlCommand("UPDATE medicines SET quantity_in_stock = quantity_in_stock + @quantity, expiry_date = @expiry WHERE medicine_id = @medicine; INSERT INTO stock_receptions (supplier_id, medicine_id, quantity, unit_cost, expiry_date) VALUES (@supplier, @medicine, @quantity, @cost, @expiry);", connection, transaction))
                    {
                        command.Parameters.Add(DatabaseCommands.Parameter("@quantity", quantity)); command.Parameters.Add(DatabaseCommands.Parameter("@expiry", expiryDate.Date)); command.Parameters.Add(DatabaseCommands.Parameter("@medicine", medicine.MedicineId)); command.Parameters.Add(DatabaseCommands.Parameter("@supplier", supplier.SupplierId)); command.Parameters.Add(DatabaseCommands.Parameter("@cost", unitCost)); command.ExecuteNonQuery();
                    }
                    transaction.Commit(); medicine.QuantityInStock += quantity; medicine.ExpiryDate = expiryDate.Date;
                }
                catch { transaction.Rollback(); throw; }
            }
        }

        private static List<Supplier> LoadSuppliers()
        {
            DataTable table = DatabaseCommands.Query("SELECT supplier_id, name, phone, address FROM suppliers ORDER BY name");
            return table.Rows.Cast<DataRow>().Select(row => new Supplier { SupplierId = Convert.ToInt32(row["supplier_id"]), Name = Convert.ToString(row["name"]), Phone = Convert.ToString(row["phone"]), Address = Convert.ToString(row["address"]) }).ToList();
        }

        private static List<StockReception> LoadReceptions()
        {
            DataTable table = DatabaseCommands.Query("SELECT r.reception_id, r.received_at, r.quantity, r.unit_cost, r.expiry_date, s.supplier_id, s.name AS supplier_name, s.phone, s.address, m.medicine_id, m.medicine_code, m.name AS medicine_name, m.category, m.unit_price, m.quantity_in_stock, m.reorder_level, m.expiry_date AS medicine_expiry, m.prescription_only, m.has_sales_history FROM stock_receptions r JOIN suppliers s ON s.supplier_id = r.supplier_id JOIN medicines m ON m.medicine_id = r.medicine_id ORDER BY r.received_at DESC");
            return table.Rows.Cast<DataRow>().Select(row => new StockReception { ReceptionId = Convert.ToInt32(row["reception_id"]), Date = Convert.ToDateTime(row["received_at"]), Quantity = Convert.ToInt32(row["quantity"]), UnitCost = Convert.ToInt32(row["unit_cost"]), ExpiryDate = Convert.ToDateTime(row["expiry_date"]), Supplier = new Supplier { SupplierId = Convert.ToInt32(row["supplier_id"]), Name = Convert.ToString(row["supplier_name"]), Phone = Convert.ToString(row["phone"]), Address = Convert.ToString(row["address"]) }, Medicine = new Medicine { MedicineId = Convert.ToInt32(row["medicine_id"]), MedicineCode = Convert.ToString(row["medicine_code"]), Name = Convert.ToString(row["medicine_name"]), Category = (MedicineCategory)Enum.Parse(typeof(MedicineCategory), Convert.ToString(row["category"])), UnitPrice = Convert.ToInt32(row["unit_price"]), QuantityInStock = Convert.ToInt32(row["quantity_in_stock"]), ReorderLevel = Convert.ToInt32(row["reorder_level"]), ExpiryDate = Convert.ToDateTime(row["medicine_expiry"]), PrescriptionOnly = Convert.ToBoolean(row["prescription_only"]), HasSalesHistory = Convert.ToBoolean(row["has_sales_history"]) } }).ToList();
        }
    }
}
