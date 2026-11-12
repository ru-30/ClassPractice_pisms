using System.Drawing;
using System.Windows.Forms;

namespace ClassPractice
{
    public sealed class ChangePasswordForm : Form
    {
        private readonly UserAccount account;
        private readonly TextBox currentTextBox = new TextBox();
        private readonly TextBox newTextBox = new TextBox();
        private readonly TextBox confirmTextBox = new TextBox();

        public ChangePasswordForm(UserAccount account)
        {
            this.account = account;
            Text = "PISMS - Change Password"; Size = new Size(420, 250); StartPosition = FormStartPosition.CenterParent;
            AddLabel("Current password", 20, 20); currentTextBox.Location = new Point(180, 17); currentTextBox.Width = 190; currentTextBox.PasswordChar = '*'; Controls.Add(currentTextBox);
            AddLabel("New password", 20, 60); newTextBox.Location = new Point(180, 57); newTextBox.Width = 190; newTextBox.PasswordChar = '*'; Controls.Add(newTextBox);
            AddLabel("Confirm password", 20, 100); confirmTextBox.Location = new Point(180, 97); confirmTextBox.Width = 190; confirmTextBox.PasswordChar = '*'; Controls.Add(confirmTextBox);
            var saveButton = new Button { Text = "Change password", Location = new Point(180, 145), Width = 130 }; saveButton.Click += SaveButton_Click; Controls.Add(saveButton);
        }

        private void AddLabel(string text, int x, int y) { Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true }); }

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newTextBox.Text) || newTextBox.Text != confirmTextBox.Text) { MessageBox.Show("New passwords must match and cannot be empty.", "Password", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!AuthService.ChangePassword(account, currentTextBox.Text, newTextBox.Text)) { MessageBox.Show("The current password is incorrect.", "Password", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            MessageBox.Show("Password changed successfully.", "Password", MessageBoxButtons.OK, MessageBoxIcon.Information); Close();
        }
    }
}
