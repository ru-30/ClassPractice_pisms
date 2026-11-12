using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ClassPractice
{
    public sealed class Customer { public int CustomerId { get; set; } public string FullName { get; set; } public string Phone { get; set; } public string InsuranceNumber { get; set; } }
    public sealed class Prescription { public int PrescriptionId { get; set; } public Customer Customer { get; set; } public string DoctorName { get; set; } public DateTime Date { get; set; } public string Medicines { get; set; } }

    public static class CustomerStore
    {
        public static IReadOnlyList<Customer> Customers { get { return LoadCustomers().AsReadOnly(); } }
        public static IReadOnlyList<Prescription> Prescriptions { get { return LoadPrescriptions().AsReadOnly(); } }

        public static Customer AddCustomer(string fullName, string phone, string insuranceNumber)
        {
            object id = DatabaseCommands.ExecuteScalar("INSERT INTO customers (full_name, phone, insurance_number) VALUES (@name, @phone, @insurance); SELECT LAST_INSERT_ID();", DatabaseCommands.Parameter("@name", fullName.Trim()), DatabaseCommands.Parameter("@phone", phone.Trim()), DatabaseCommands.Parameter("@insurance", insuranceNumber.Trim()));
            return new Customer { CustomerId = Convert.ToInt32(id), FullName = fullName.Trim(), Phone = phone.Trim(), InsuranceNumber = insuranceNumber.Trim() };
        }

        public static Prescription AddPrescription(Customer customer, string doctorName, DateTime date, string medicines)
        {
            object id = DatabaseCommands.ExecuteScalar("INSERT INTO prescriptions (customer_id, doctor_name, prescription_date, medicines) VALUES (@customer, @doctor, @date, @medicines); SELECT LAST_INSERT_ID();", DatabaseCommands.Parameter("@customer", customer.CustomerId), DatabaseCommands.Parameter("@doctor", doctorName.Trim()), DatabaseCommands.Parameter("@date", date.Date), DatabaseCommands.Parameter("@medicines", medicines.Trim()));
            return new Prescription { PrescriptionId = Convert.ToInt32(id), Customer = customer, DoctorName = doctorName.Trim(), Date = date.Date, Medicines = medicines.Trim() };
        }

        private static List<Customer> LoadCustomers()
        {
            DataTable table = DatabaseCommands.Query("SELECT customer_id, full_name, phone, insurance_number FROM customers ORDER BY full_name");
            return table.Rows.Cast<DataRow>().Select(row => new Customer { CustomerId = Convert.ToInt32(row["customer_id"]), FullName = Convert.ToString(row["full_name"]), Phone = Convert.ToString(row["phone"]), InsuranceNumber = row["insurance_number"] == DBNull.Value ? string.Empty : Convert.ToString(row["insurance_number"]) }).ToList();
        }

        private static List<Prescription> LoadPrescriptions()
        {
            DataTable table = DatabaseCommands.Query("SELECT p.prescription_id, p.customer_id, p.doctor_name, p.prescription_date, p.medicines, c.full_name, c.phone, c.insurance_number FROM prescriptions p JOIN customers c ON c.customer_id = p.customer_id ORDER BY p.prescription_date DESC");
            return table.Rows.Cast<DataRow>().Select(row => new Prescription { PrescriptionId = Convert.ToInt32(row["prescription_id"]), Date = Convert.ToDateTime(row["prescription_date"]), DoctorName = Convert.ToString(row["doctor_name"]), Medicines = Convert.ToString(row["medicines"]), Customer = new Customer { CustomerId = Convert.ToInt32(row["customer_id"]), FullName = Convert.ToString(row["full_name"]), Phone = Convert.ToString(row["phone"]), InsuranceNumber = row["insurance_number"] == DBNull.Value ? string.Empty : Convert.ToString(row["insurance_number"]) } }).ToList();
        }
    }
}
