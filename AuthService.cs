using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using MySqlConnector;

namespace ClassPractice
{
    public enum UserRole { Cashier, Pharmacist, Administrator }

    public sealed class UserAccount
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
    }

    public static class AuthService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static IReadOnlyList<UserAccount> GetAccounts()
        {
            EnsureDefaultAccounts();
            DataTable table = DatabaseCommands.Query("SELECT user_id, username, password_hash, password_salt, role, is_active FROM users ORDER BY username");
            return table.Rows.Cast<DataRow>().Select(MapAccount).ToList().AsReadOnly();
        }

        public static UserAccount Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || password == null) return null;
            EnsureDefaultAccounts();
            DataTable table = DatabaseCommands.Query("SELECT user_id, username, password_hash, password_salt, role, is_active FROM users WHERE username = @username LIMIT 1", DatabaseCommands.Parameter("@username", username.Trim()));
            if (table.Rows.Count == 0) return null;
            UserAccount account = MapAccount(table.Rows[0]);
            return account.IsActive && VerifyPassword(password, account.PasswordHash, account.PasswordSalt) ? account : null;
        }

        public static UserAccount CreateUser(string username, string password, UserRole role)
        {
            EnsureDefaultAccounts();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return null;
            if (GetAccounts().Any(item => string.Equals(item.Username, username.Trim(), StringComparison.OrdinalIgnoreCase))) return null;
            byte[] salt; byte[] hash; CreatePassword(password, out hash, out salt);
            DatabaseCommands.ExecuteNonQuery("INSERT INTO users (username, password_hash, password_salt, role, is_active) VALUES (@username, @hash, @salt, @role, 1)", DatabaseCommands.Parameter("@username", username.Trim()), DatabaseCommands.Parameter("@hash", hash), DatabaseCommands.Parameter("@salt", salt), DatabaseCommands.Parameter("@role", role.ToString()));
            return GetAccounts().First(item => string.Equals(item.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static bool ChangePassword(UserAccount account, string currentPassword, string newPassword)
        {
            if (account == null || !VerifyPassword(currentPassword, account.PasswordHash, account.PasswordSalt)) return false;
            SetPassword(account, newPassword);
            return true;
        }

        public static bool ResetPassword(UserAccount account, string newPassword)
        {
            if (account == null || string.IsNullOrWhiteSpace(newPassword)) return false;
            SetPassword(account, newPassword);
            return true;
        }

        public static void UpdateUser(UserAccount account, string username, UserRole role, bool isActive)
        {
            DatabaseCommands.ExecuteNonQuery("UPDATE users SET username = @username, role = @role, is_active = @active WHERE user_id = @id", DatabaseCommands.Parameter("@username", username.Trim()), DatabaseCommands.Parameter("@role", role.ToString()), DatabaseCommands.Parameter("@active", isActive), DatabaseCommands.Parameter("@id", account.UserId));
            account.Username = username.Trim(); account.Role = role; account.IsActive = isActive;
        }

        public static void DeleteUser(UserAccount account)
        {
            DatabaseCommands.ExecuteNonQuery("DELETE FROM users WHERE user_id = @id", DatabaseCommands.Parameter("@id", account.UserId));
        }

        private static void EnsureDefaultAccounts()
        {
            object count = DatabaseCommands.ExecuteScalar("SELECT COUNT(*) FROM users");
            if (Convert.ToInt32(count) != 0) return;
            CreateUserWithoutEnsure("admin", "Admin@123", UserRole.Administrator);
            CreateUserWithoutEnsure("pharmacist", "Pharmacist@123", UserRole.Pharmacist);
            CreateUserWithoutEnsure("cashier", "Cashier@123", UserRole.Cashier);
        }

        private static void CreateUserWithoutEnsure(string username, string password, UserRole role)
        {
            byte[] salt; byte[] hash; CreatePassword(password, out hash, out salt);
            DatabaseCommands.ExecuteNonQuery("INSERT INTO users (username, password_hash, password_salt, role, is_active) VALUES (@username, @hash, @salt, @role, 1)", DatabaseCommands.Parameter("@username", username), DatabaseCommands.Parameter("@hash", hash), DatabaseCommands.Parameter("@salt", salt), DatabaseCommands.Parameter("@role", role.ToString()));
        }

        private static void SetPassword(UserAccount account, string password)
        {
            byte[] salt; byte[] hash; CreatePassword(password, out hash, out salt);
            DatabaseCommands.ExecuteNonQuery("UPDATE users SET password_hash = @hash, password_salt = @salt WHERE user_id = @id", DatabaseCommands.Parameter("@hash", hash), DatabaseCommands.Parameter("@salt", salt), DatabaseCommands.Parameter("@id", account.UserId));
            account.PasswordHash = hash; account.PasswordSalt = salt;
        }

        private static UserAccount MapAccount(DataRow row)
        {
            return new UserAccount { UserId = Convert.ToInt32(row["user_id"]), Username = Convert.ToString(row["username"]), PasswordHash = (byte[])row["password_hash"], PasswordSalt = (byte[])row["password_salt"], Role = (UserRole)Enum.Parse(typeof(UserRole), Convert.ToString(row["role"])), IsActive = Convert.ToBoolean(row["is_active"]) };
        }

        private static void CreatePassword(string password, out byte[] hash, out byte[] salt)
        {
            salt = new byte[SaltSize]; using (var generator = RandomNumberGenerator.Create()) generator.GetBytes(salt); hash = HashPassword(password, salt);
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations)) return deriveBytes.GetBytes(HashSize);
        }

        private static bool VerifyPassword(string password, byte[] expectedHash, byte[] salt)
        {
            byte[] actualHash = HashPassword(password, salt); if (actualHash.Length != expectedHash.Length) return false;
            int difference = 0; for (int index = 0; index < actualHash.Length; index++) difference |= actualHash[index] ^ expectedHash[index]; return difference == 0;
        }
    }
}
