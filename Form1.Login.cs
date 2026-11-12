using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPractice
{
    public partial class Form1
    {
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private int failedAttempts;
        private DateTime lockedUntil;

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = "PISMS - Login";
            ClientSize = new Size(430, 235);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var titleLabel = new Label { AutoSize = true, Text = "PISMS Login", Location = new Point(130, 25) };
            var usernameLabel = new Label { AutoSize = true, Text = "Username:", Location = new Point(45, 80) };
            var passwordLabel = new Label { AutoSize = true, Text = "Password:", Location = new Point(45, 120) };
            usernameTextBox = new TextBox { Location = new Point(140, 77), Size = new Size(220, 26) };
            passwordTextBox = new TextBox { Location = new Point(140, 117), Size = new Size(220, 26), PasswordChar = '*' };
            var loginButton = new Button { Text = "Login", Location = new Point(140, 165), Size = new Size(105, 35) };
            var exitButton = new Button { Text = "Exit", Location = new Point(255, 165), Size = new Size(105, 35) };
            loginButton.Click += LoginButton_Click;
            exitButton.Click += ExitButton_Click;
            AcceptButton = loginButton;
            Controls.AddRange(new Control[] { titleLabel, usernameLabel, passwordLabel, usernameTextBox, passwordTextBox, loginButton, exitButton });
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (DateTime.Now < lockedUntil)
            {
                MessageBox.Show("Login is temporarily locked. Please try again later.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                MessageBox.Show("Please enter your username and password", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            UserAccount account = AuthService.Authenticate(usernameTextBox.Text, passwordTextBox.Text);
            if (account == null)
            {
                failedAttempts++;
                passwordTextBox.Clear();
                usernameTextBox.Focus();
                if (failedAttempts >= 3)
                {
                    lockedUntil = DateTime.Now.AddMinutes(1);
                    failedAttempts = 0;
                    MessageBox.Show("Too many failed attempts. Login is locked for 1 minute.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            failedAttempts = 0;
            MessageBox.Show("Login successful!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Hide();
            using (var menu = new MenuForm(account))
            {
                menu.ShowDialog(this);
            }
            usernameTextBox.Clear();
            passwordTextBox.Clear();
            Show();
            usernameTextBox.Focus();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
