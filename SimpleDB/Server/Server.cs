using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Data.SqlClient;

namespace Server
{
    class Server
    {
        //список всех подключений к серверу
        public static List<Client> Clients = new List<Client>();

        //добавляем новое подключение
        public static void NewClient(Socket handle)
        {
            try
            {
                Client newClient = new Client(handle);
                Clients.Add(newClient);
                Console.WriteLine("New client connected: {0}", handle.RemoteEndPoint);
                //Server.updateAllDbViews();
            }
            catch (Exception exp) { Console.WriteLine("Error with addNewClient: {0}.", exp.Message); }
        }

        //удаляем подключение
        public static void EndClient(Client client)
        {
            try
            {
                Console.WriteLine("User {0} has been disconnected.", client.remoteEndPoint);
                client.End();
                Clients.Remove(client);
            }
            catch (Exception exp) { Console.WriteLine("Error with endClient: {0}.", exp.Message); }
        }


        
        //

        public static void updateAllDbViews()
        {
            Console.WriteLine("Попытка обновления страниц");

            using (SqlCommand command = new SqlCommand("SELECT * FROM [RESERVATIONS];", DbController.sqlConnection))
            using (SqlDataReader sqlReader = command.ExecuteReader())
            {
                string data = "#refreshview|";

                bool tryagain = true;
                while (tryagain)
                {
                    try
                    {
                        while (sqlReader.Read())
                        {
                            string[] oneRow = { sqlReader[0].ToString(), sqlReader[1].ToString(), sqlReader[2].ToString(),
                    sqlReader[3].ToString(), sqlReader[4].ToString(), sqlReader[5].ToString(), sqlReader[6].ToString()};
                            data = data + string.Join("&", oneRow) + "!";
                        }
                        tryagain = false;

                    }

                    catch (Exception exception)
                    {
                        Console.WriteLine("Error while selecting...");
                        Console.WriteLine(exception.Message.ToString(), exception.Source.ToString());
                    }
                    finally
                    {
                        if (sqlReader != null)
                            sqlReader.Close();
                    }
                }

                tryagain = true;
                while (tryagain)
                {
                    try
                    {
                        foreach (Client c in Clients)
                        {
                            c.updateDbView(data);
                            Console.WriteLine("Клиент {0} обновлен информацией {1} !", c.remoteEndPoint, data);
                            tryagain = false;
                        }
                    }
                    catch (Exception exp) { Console.WriteLine("Error with updateAllDbViews: {0}.", exp.Message); }
                }
            }
        }
    }
}
