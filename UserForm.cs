using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class UserForm : Form
    {
        private readonly TextBox usernameTextBox = new TextBox();
        private readonly TextBox passwordTextBox = new TextBox();
        private readonly ComboBox roleComboBox = new ComboBox();
        private readonly CheckBox activeCheckBox = new CheckBox();
        private readonly DataGridView userGrid = new DataGridView();
        private UserAccount selectedUser;

        public UserForm()
        {
            Text = "PISMS - User Management"; Size = new Size(800, 570); StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(new Label { Text = "Username", Location = new Point(20, 20), AutoSize = true }); usernameTextBox.Location = new Point(20, 43); usernameTextBox.Width = 180; Controls.Add(usernameTextBox);
            Controls.Add(new Label { Text = "Password (new users/reset)", Location = new Point(215, 20), AutoSize = true }); passwordTextBox.Location = new Point(215, 43); passwordTextBox.Width = 180; passwordTextBox.PasswordChar = '*'; Controls.Add(passwordTextBox);
            Controls.Add(new Label { Text = "Role", Location = new Point(410, 20), AutoSize = true }); roleComboBox.Location = new Point(410, 43); roleComboBox.Width = 150; roleComboBox.DropDownStyle = ComboBoxStyle.DropDownList; roleComboBox.DataSource = Enum.GetValues(typeof(UserRole)); Controls.Add(roleComboBox);
            activeCheckBox.Text = "Active"; activeCheckBox.Checked = true; activeCheckBox.Location = new Point(580, 43); activeCheckBox.AutoSize = true; Controls.Add(activeCheckBox);
            var createButton = new Button { Text = "Create", Location = new Point(20, 85), Width = 90 }; createButton.Click += CreateButton_Click; Controls.Add(createButton);
            var updateButton = new Button { Text = "Update", Location = new Point(120, 85), Width = 90 }; updateButton.Click += UpdateButton_Click; Controls.Add(updateButton);
            var resetButton = new Button { Text = "Reset password", Location = new Point(220, 85), Width = 120 }; resetButton.Click += ResetButton_Click; Controls.Add(resetButton);
            var deactivateButton = new Button { Text = "Deactivate", Location = new Point(350, 85), Width = 100 }; deactivateButton.Click += DeactivateButton_Click; Controls.Add(deactivateButton);
            var deleteButton = new Button { Text = "Delete", Location = new Point(460, 85), Width = 90 }; deleteButton.Click += DeleteButton_Click; Controls.Add(deleteButton);
            userGrid.Location = new Point(20, 135); userGrid.Size = new Size(730, 350); userGrid.ReadOnly = true; userGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; userGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; userGrid.SelectionChanged += UserGrid_SelectionChanged; Controls.Add(userGrid); RefreshGrid();
        }

        private bool ValidateCommon(bool requirePassword)
        {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || (requirePassword && string.IsNullOrWhiteSpace(passwordTextBox.Text))) { MessageBox.Show("Username and password are required.", "Users", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!requirePassword && string.IsNullOrWhiteSpace(usernameTextBox.Text)) { MessageBox.Show("Username is required.", "Users", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (!ValidateCommon(true)) return;
            if (AuthService.CreateUser(usernameTextBox.Text, passwordTextBox.Text, (UserRole)roleComboBox.SelectedItem) == null) { MessageBox.Show("That username already exists.", "Users", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            ClearForm(); RefreshGrid();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (selectedUser == null || !ValidateCommon(false)) return;
            AuthService.UpdateUser(selectedUser, usernameTextBox.Text, (UserRole)roleComboBox.SelectedItem, activeCheckBox.Checked); RefreshGrid();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            if (selectedUser == null || string.IsNullOrWhiteSpace(passwordTextBox.Text)) { MessageBox.Show("Select a user and enter a new password.", "Users", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            AuthService.ResetPassword(selectedUser, passwordTextBox.Text); passwordTextBox.Clear(); MessageBox.Show("Password reset successfully.", "Users", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeactivateButton_Click(object sender, EventArgs e) { if (selectedUser == null) return; selectedUser.IsActive = false; activeCheckBox.Checked = false; RefreshGrid(); }
        private void DeleteButton_Click(object sender, EventArgs e) { if (selectedUser == null) return; AuthService.DeleteUser(selectedUser); ClearForm(); RefreshGrid(); }

        private void RefreshGrid() { userGrid.DataSource = AuthService.GetAccounts().Select(a => new { a.Username, Role = a.Role.ToString(), Status = a.IsActive ? "Active" : "Inactive" }).ToList(); }

        private void UserGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (userGrid.CurrentRow == null) return;
            string username = Convert.ToString(userGrid.CurrentRow.Cells["Username"].Value);
            selectedUser = AuthService.GetAccounts().FirstOrDefault(a => a.Username == username);
            if (selectedUser == null) return;
            usernameTextBox.Text = selectedUser.Username; roleComboBox.SelectedItem = selectedUser.Role; activeCheckBox.Checked = selectedUser.IsActive; passwordTextBox.Clear();
        }

        private void ClearForm() { selectedUser = null; usernameTextBox.Clear(); passwordTextBox.Clear(); activeCheckBox.Checked = true; }
    }
}
