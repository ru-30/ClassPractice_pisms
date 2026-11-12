# Pharmacy Inventory and Sales Management System (PISMS)

PISMS is a Windows Forms pharmacy inventory and sales application built with C# and .NET Framework 4.7.2. It supports authentication, role-based access, medicines, suppliers, stock reception, sales, customers, prescriptions, alerts, reports, and user management.

## Technologies

- C# Windows Forms
- .NET Framework 4.7.2
- MySQL/MariaDB through XAMPP
- MySqlConnector
- Visual Studio

## User roles

| Role | Username | Password | Access |
|---|---|---|---|
| Administrator | `admin` | `Admin@123` | All modules |
| Pharmacist | `pharmacist` | `Pharmacist@123` | Medicines, suppliers, stock, sales, customers, alerts, reports |
| Cashier | `cashier` | `Cashier@123` | Sales and customers |

Passwords are stored as PBKDF2 hashes and salts by the application. Do not store plaintext passwords in the database.

## XAMPP database setup

1. Start **Apache** and **MySQL** in XAMPP.
2. Open `http://localhost/phpmyadmin`.
3. Create or select a database named `pisms_db`.
4. Open the **SQL** tab.
5. Paste and execute the SQL below.
6. Build and run the project from Visual Studio.

The application connection is configured in `Database.cs`:

```csharp
Server=127.0.0.1;Port=3306;Database=pisms_db;User ID=root;Password=;SslMode=None;
```

If the XAMPP MySQL root account has a password, update the `Password` value in `Database.cs`.

## Database table creation SQL

This is the database schema SQL. It is also suitable for MySQL/MariaDB in XAMPP phpMyAdmin.

```sql
USE pisms_db;

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS sale_items;
DROP TABLE IF EXISTS sales;
DROP TABLE IF EXISTS prescriptions;
DROP TABLE IF EXISTS customers;
DROP TABLE IF EXISTS stock_receptions;
DROP TABLE IF EXISTS suppliers;
DROP TABLE IF EXISTS medicines;
DROP TABLE IF EXISTS insurers;
DROP TABLE IF EXISTS users;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE users (
	user_id INT AUTO_INCREMENT PRIMARY KEY,
	username VARCHAR(50) NOT NULL UNIQUE,
	password_hash VARBINARY(64) NOT NULL,
	password_salt VARBINARY(32) NOT NULL,
	role ENUM('Cashier', 'Pharmacist', 'Administrator') NOT NULL,
	is_active BOOLEAN NOT NULL DEFAULT TRUE,
	created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE medicines (
	medicine_id INT AUTO_INCREMENT PRIMARY KEY,
	medicine_code VARCHAR(20) NOT NULL UNIQUE,
	name VARCHAR(150) NOT NULL,
	category ENUM('Antibiotic', 'Painkiller', 'Vitamin', 'Antimalarial', 'Other') NOT NULL,
	unit_price INT NOT NULL,
	quantity_in_stock INT NOT NULL DEFAULT 0,
	reorder_level INT NOT NULL DEFAULT 0,
	expiry_date DATE NOT NULL,
	prescription_only BOOLEAN NOT NULL DEFAULT FALSE,
	has_sales_history BOOLEAN NOT NULL DEFAULT FALSE,
	created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	UNIQUE KEY unique_medicine_name_expiry (name, expiry_date),
	CHECK (unit_price > 0),
	CHECK (quantity_in_stock >= 0),
	CHECK (reorder_level >= 0)
) ENGINE=InnoDB;

CREATE TABLE suppliers (
	supplier_id INT AUTO_INCREMENT PRIMARY KEY,
	name VARCHAR(150) NOT NULL,
	phone VARCHAR(50) NOT NULL,
	address VARCHAR(255) NOT NULL,
	created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE stock_receptions (
	reception_id INT AUTO_INCREMENT PRIMARY KEY,
	supplier_id INT NOT NULL,
	medicine_id INT NOT NULL,
	quantity INT NOT NULL,
	unit_cost INT NOT NULL,
	expiry_date DATE NOT NULL,
	received_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	FOREIGN KEY (supplier_id) REFERENCES suppliers(supplier_id),
	FOREIGN KEY (medicine_id) REFERENCES medicines(medicine_id),
	CHECK (quantity > 0),
	CHECK (unit_cost > 0)
) ENGINE=InnoDB;

CREATE TABLE customers (
	customer_id INT AUTO_INCREMENT PRIMARY KEY,
	full_name VARCHAR(150) NOT NULL,
	phone VARCHAR(50) NOT NULL,
	insurance_number VARCHAR(100),
	created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE insurers (
	insurer_id INT AUTO_INCREMENT PRIMARY KEY,
	name VARCHAR(150) NOT NULL UNIQUE,
	coverage_percent INT NOT NULL,
	is_active BOOLEAN NOT NULL DEFAULT TRUE,
	CHECK (coverage_percent BETWEEN 0 AND 100)
) ENGINE=InnoDB;

CREATE TABLE sales (
	sale_id INT AUTO_INCREMENT PRIMARY KEY,
	receipt_number VARCHAR(30) NOT NULL UNIQUE,
	sale_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	cashier_id INT NOT NULL,
	customer_id INT NULL,
	insurer_id INT NULL,
	insurance_percent INT NOT NULL DEFAULT 0,
	total INT NOT NULL,
	patient_payable INT NOT NULL,
	amount_paid INT NOT NULL,
	change_amount INT NOT NULL,
	FOREIGN KEY (cashier_id) REFERENCES users(user_id),
	FOREIGN KEY (customer_id) REFERENCES customers(customer_id),
	FOREIGN KEY (insurer_id) REFERENCES insurers(insurer_id),
	CHECK (total >= 0),
	CHECK (patient_payable >= 0),
	CHECK (amount_paid >= patient_payable),
	CHECK (insurance_percent BETWEEN 0 AND 100)
) ENGINE=InnoDB;

CREATE TABLE sale_items (
	sale_item_id INT AUTO_INCREMENT PRIMARY KEY,
	sale_id INT NOT NULL,
	medicine_id INT NOT NULL,
	quantity INT NOT NULL,
	unit_price INT NOT NULL,
	line_total INT NOT NULL,
	prescription_number VARCHAR(100),
	FOREIGN KEY (sale_id) REFERENCES sales(sale_id) ON DELETE CASCADE,
	FOREIGN KEY (medicine_id) REFERENCES medicines(medicine_id),
	CHECK (quantity > 0),
	CHECK (unit_price > 0),
	CHECK (line_total = quantity * unit_price)
) ENGINE=InnoDB;

CREATE TABLE prescriptions (
	prescription_id INT AUTO_INCREMENT PRIMARY KEY,
	customer_id INT NOT NULL,
	doctor_name VARCHAR(150) NOT NULL,
	prescription_date DATE NOT NULL,
	medicines TEXT NOT NULL,
	created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
) ENGINE=InnoDB;

INSERT INTO insurers (name, coverage_percent)
VALUES
	('Community Health Insurance', 85),
	('Private Insurance', 70);
```

## Sample medicine data

After the tables are created, medicines can be entered through the Medicine Management form. Example values:

| Name | Category | Unit price | Stock | Reorder level | Expiry date | Prescription only |
|---|---|---:|---:|---:|---|---|
| Paracetamol | Painkiller | 500 | 100 | 20 | 2026-12-31 | No |
| Amoxicillin | Antibiotic | 1200 | 50 | 10 | 2027-06-30 | Yes |
| Vitamin C | Vitamin | 800 | 30 | 10 | 2027-03-31 | No |
| Artesunate | Antimalarial | 2500 | 15 | 5 | 2027-08-31 | Yes |
| Cough Syrup | Other | 1500 | 25 | 8 | 2026-11-30 | No |

Medicine codes are generated automatically by the application.

## Running the project

1. Clone the repository.
2. Open `ClassPractice.slnx` in Visual Studio.
3. Restore NuGet packages if required.
4. Configure XAMPP and create the `pisms_db` tables.
5. Confirm the MySQL connection in `Database.cs`.
6. Build and run the project.

## Notes

- Money values are stored as whole Rwandan Francs (RWF).
- Quantities are whole numbers.
- The application uses parameterized SQL commands for database operations.
- Passwords are never stored as plaintext.
- The application creates the default demo users in the `users` table when it finds that the table is empty.
