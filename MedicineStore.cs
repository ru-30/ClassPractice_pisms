using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlConnector;

namespace ClassPractice
{
    public enum MedicineCategory { Antibiotic, Painkiller, Vitamin, Antimalarial, Other }

    public sealed class Medicine
    {
        public int MedicineId { get; set; }
        public string MedicineCode { get; set; }
        public string Name { get; set; }
        public MedicineCategory Category { get; set; }
        public int UnitPrice { get; set; }
        public int QuantityInStock { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool PrescriptionOnly { get; set; }
        public bool HasSalesHistory { get; set; }
    }

    public static class MedicineStore
    {
        public static IReadOnlyList<Medicine> GetAll()
        {
            DataTable table = DatabaseCommands.Query("SELECT medicine_id, medicine_code, name, category, unit_price, quantity_in_stock, reorder_level, expiry_date, prescription_only, has_sales_history FROM medicines ORDER BY name");
            return table.Rows.Cast<DataRow>().Select(Map).ToList().AsReadOnly();
        }

        public static Medicine Find(string code)
        {
            return GetAll().FirstOrDefault(item => item.MedicineCode == code);
        }

        public static bool Exists(string name, DateTime expiryDate, string codeToIgnore)
        {
            object result = DatabaseCommands.ExecuteScalar("SELECT COUNT(*) FROM medicines WHERE LOWER(name) = LOWER(@name) AND expiry_date = @expiry AND (@code IS NULL OR medicine_code <> @code)", DatabaseCommands.Parameter("@name", name.Trim()), DatabaseCommands.Parameter("@expiry", expiryDate.Date), DatabaseCommands.Parameter("@code", codeToIgnore));
            return Convert.ToInt32(result) > 0;
        }

        public static Medicine Add(string name, MedicineCategory category, int unitPrice, int quantity, int reorderLevel, DateTime expiryDate, bool prescriptionOnly)
        {
            string code = "MED" + Convert.ToInt32(DatabaseCommands.ExecuteScalar("SELECT COALESCE(MAX(medicine_id), 0) + 1 FROM medicines")).ToString("D4");
            DatabaseCommands.ExecuteNonQuery("INSERT INTO medicines (medicine_code, name, category, unit_price, quantity_in_stock, reorder_level, expiry_date, prescription_only) VALUES (@code, @name, @category, @price, @quantity, @reorder, @expiry, @prescription)", DatabaseCommands.Parameter("@code", code), DatabaseCommands.Parameter("@name", name.Trim()), DatabaseCommands.Parameter("@category", category.ToString()), DatabaseCommands.Parameter("@price", unitPrice), DatabaseCommands.Parameter("@quantity", quantity), DatabaseCommands.Parameter("@reorder", reorderLevel), DatabaseCommands.Parameter("@expiry", expiryDate.Date), DatabaseCommands.Parameter("@prescription", prescriptionOnly));
            return Find(code);
        }

        public static void Update(Medicine medicine)
        {
            DatabaseCommands.ExecuteNonQuery("UPDATE medicines SET name = @name, category = @category, unit_price = @price, quantity_in_stock = @quantity, reorder_level = @reorder, expiry_date = @expiry, prescription_only = @prescription WHERE medicine_id = @id", DatabaseCommands.Parameter("@name", medicine.Name.Trim()), DatabaseCommands.Parameter("@category", medicine.Category.ToString()), DatabaseCommands.Parameter("@price", medicine.UnitPrice), DatabaseCommands.Parameter("@quantity", medicine.QuantityInStock), DatabaseCommands.Parameter("@reorder", medicine.ReorderLevel), DatabaseCommands.Parameter("@expiry", medicine.ExpiryDate.Date), DatabaseCommands.Parameter("@prescription", medicine.PrescriptionOnly), DatabaseCommands.Parameter("@id", medicine.MedicineId));
        }

        public static void Remove(Medicine medicine)
        {
            DatabaseCommands.ExecuteNonQuery("DELETE FROM medicines WHERE medicine_id = @id AND quantity_in_stock = 0 AND has_sales_history = 0", DatabaseCommands.Parameter("@id", medicine.MedicineId));
        }

        private static Medicine Map(DataRow row)
        {
            return new Medicine { MedicineId = Convert.ToInt32(row["medicine_id"]), MedicineCode = Convert.ToString(row["medicine_code"]), Name = Convert.ToString(row["name"]), Category = (MedicineCategory)Enum.Parse(typeof(MedicineCategory), Convert.ToString(row["category"])), UnitPrice = Convert.ToInt32(row["unit_price"]), QuantityInStock = Convert.ToInt32(row["quantity_in_stock"]), ReorderLevel = Convert.ToInt32(row["reorder_level"]), ExpiryDate = Convert.ToDateTime(row["expiry_date"]), PrescriptionOnly = Convert.ToBoolean(row["prescription_only"]), HasSalesHistory = Convert.ToBoolean(row["has_sales_history"]) };
        }
    }
}
