using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassPractice
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string databaseError;
            if (!Database.CanConnect(out databaseError))
            {
                MessageBox.Show("Cannot connect to MySQL database pisms_db. Start MySQL in XAMPP and import pisms_xampp.sql.\n\n" + databaseError, "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Application.Run(new Form1());
        }
    }
}
