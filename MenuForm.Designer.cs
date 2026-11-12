namespace ClassPractice
{
    partial class MenuForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.titleLabel = new System.Windows.Forms.Label();
            this.userLabel = new System.Windows.Forms.Label();
            this.medicinesButton = CreateButton("Medicines", 35, 90);
            this.suppliersButton = CreateButton("Suppliers", 190, 90);
            this.receiveStockButton = CreateButton("Receive Stock", 345, 90);
            this.salesButton = CreateButton("Sales", 35, 145);
            this.customersButton = CreateButton("Customers", 190, 145);
            this.alertsButton = CreateButton("Alerts", 345, 145);
            this.reportsButton = CreateButton("Reports", 35, 200);
            this.usersButton = CreateButton("Users", 190, 200);
            this.logoutButton = CreateButton("Logout", 345, 200);
            this.SuspendLayout();
            //
            // titleLabel
            //
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(35, 25);
            this.titleLabel.Text = "Pharmacy Inventory and Sales Management System";
            //
            // userLabel
            //
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(37, 58);
            this.userLabel.Text = "Logged in user:";
            //
            // buttons
            //
            this.salesButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.customersButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.medicinesButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.suppliersButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.receiveStockButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.alertsButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.reportsButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.usersButton.Click += new System.EventHandler(this.ModuleButton_Click);
            this.logoutButton.Click += new System.EventHandler(this.LogoutButton_Click);
            //
            // MenuForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 275);
            this.Controls.Add(this.logoutButton);
            this.Controls.Add(this.usersButton);
            this.Controls.Add(this.reportsButton);
            this.Controls.Add(this.alertsButton);
            this.Controls.Add(this.customersButton);
            this.Controls.Add(this.salesButton);
            this.Controls.Add(this.receiveStockButton);
            this.Controls.Add(this.suppliersButton);
            this.Controls.Add(this.medicinesButton);
            this.Controls.Add(this.userLabel);
            this.Controls.Add(this.titleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MenuForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PISMS - Main Menu";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button CreateButton(string text, int x, int y)
        {
            return new System.Windows.Forms.Button
            {
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(135, 40),
                Text = text,
                UseVisualStyleBackColor = true
            };
        }

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.Button medicinesButton;
        private System.Windows.Forms.Button suppliersButton;
        private System.Windows.Forms.Button receiveStockButton;
        private System.Windows.Forms.Button salesButton;
        private System.Windows.Forms.Button customersButton;
        private System.Windows.Forms.Button alertsButton;
        private System.Windows.Forms.Button reportsButton;
        private System.Windows.Forms.Button usersButton;
        private System.Windows.Forms.Button logoutButton;
    }
}
