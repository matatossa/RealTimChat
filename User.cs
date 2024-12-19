namespace ChatApp
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class User
    {
        // Public properties for username and haspassword
        public string username { get; private set; }
        public string haspassword { get; private set; }

        public User(string username, string password)
        {
            this.username = username;
            this.haspassword = HashPassword(password); // Hash and store the password
        }

        // Method to hash the password
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Method to verify the password
        public bool verifyPass(string password)
        {
            string hashedInput = HashPassword(password); // Hash the input password
            return haspassword == hashedInput; // Compare with stored hashed password
        }
    }
}
