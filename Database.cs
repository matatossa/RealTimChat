namespace ChatApp
{
    using MySql.Data.MySqlClient;

    using System;

    public class Database
    {
        private string connectionString;
        public Database(string server, string database1, string username, string password)
        {
            connectionString = $"Server={server};Database={database1};Uid={username};Pwd={password};"; // string with the information li khssna bch ntconnectaw l database
        }
        public bool adduser(User user)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();// bhal php , hel connection w der requete dialk
                    string query = "insert into  users (username,haspassword) values ( @username,@haspassword)"; //@username , l @ bhal place holder
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", user.username);
                        cmd.Parameters.AddWithValue("@haspassword", user.haspassword);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error adding user" + ex.Message);
                    return false;

                }
            }
        }
        public bool authentificateUser(string username, string password)

        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT haspassword FROM users WHERE username = @username";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString("haspassword");
                                User tempuser = new User(username, password);
                                return tempuser.verifyPass(password);
                            }

                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("error authentification" + ex.Message); }
                return false;

            }
        }
        // TestConnection method to check if the database connection works
        public void TestConnection()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Try opening the connection to test
                    conn.Close(); // Close it once the test is successful
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error testing connection: " + ex.Message);
                }
            }
        }


    }
}