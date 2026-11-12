using System;
using System.Linq;

namespace ClassPractice
{
    public static class AlertService
    {
        public static int LowStockCount
        {
            get { return MedicineStore.GetAll().Count(m => m.QuantityInStock <= m.ReorderLevel); }
        }

        public static int ExpiryCount
        {
            get { return MedicineStore.GetAll().Count(IsExpiredOrNearExpiry); }
        }

        public static bool IsExpiredOrNearExpiry(Medicine medicine)
        {
            return medicine.ExpiryDate.Date <= DateTime.Today.AddDays(90);
        }
    }
}
