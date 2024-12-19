using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatApp
{
    class ChatServeur
    {
        static Database db = new Database("localhost", "chat", "root", "");
        static List<TcpClient> clients = new List<TcpClient>(); // List to store all connected clients
        static readonly object clientLock = new object(); // For thread-safe access to the client list

        static void Main(string[] args)
        {
            Console.WriteLine("Server Start");

            if (!TestDatabaseConnection())
            {
                Console.WriteLine("Database connection failed. Exiting...");
                return;
            }

            Console.WriteLine("Database connection successful.");
            TcpListener server = new TcpListener(IPAddress.Any, 8000);
            server.Start();
            Console.WriteLine("Server started. Waiting for clients...");

            while (true)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("New client connected!");

                    // Add client to the list
                    lock (clientLock)
                    {
                        clients.Add(client);
                    }

                    // Start a new thread to handle the client
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        static bool TestDatabaseConnection()
        {
            try
            {
                db.TestConnection();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
        }

        static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            string username = null;

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Client disconnected

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Handle authentication
                    if (message.StartsWith("AUTHENTIFICATE"))
                    {
                        string[] parts = message.Split(':');
                        if (parts.Length == 3)
                        {
                            string tempUsername = parts[1];
                            string password = parts[2];

                            if (db.authentificateUser(tempUsername, password))
                            {
                                username = tempUsername;
                                SendMessageToClient(client, "Authentication successful");
                            }
                            else
                            {
                                SendMessageToClient(client, "Authentication failed");
                            }
                        }
                    }
                    // Broadcast messages if user is authenticated
                    else if (username != null)
                    {
                        string formattedMessage = $"{username}: {message}";
                        Console.WriteLine(formattedMessage);
                        BroadcastMessage(formattedMessage, client);
                    }
                    else
                    {
                        SendMessageToClient(client, "Please authenticate first.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                // Remove the client from the list
                lock (clientLock)
                {
                    clients.Remove(client);
                }
                stream.Close();
                client.Close();
                Console.WriteLine("Client disconnected.");
            }
        }

        // Send a message to a single client
        static void SendMessageToClient(TcpClient client, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                client.GetStream().Write(data, 0, data.Length);
            }
            catch
            {
                // Handle errors silently (client might have disconnected)
            }
        }

        // Broadcast a message to all clients except the sender
        static void BroadcastMessage(string message, TcpClient sender)
        {
            lock (clientLock)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                foreach (TcpClient client in clients)
                {
                    if (client != sender && client.Connected)
                    {
                        try
                        {
                            client.GetStream().Write(data, 0, data.Length);
                        }
                        catch
                        {
                            // If a client fails, remove it (optional cleanup)
                            clients.Remove(client);
                        }
                    }
                }
            }
        }
    }
}
