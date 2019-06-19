using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Server
{
    class DbController
    {
        public static SqlConnection sqlConnection;
        private static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Екатерина\source\repos\SimpleDB\Server\Database1.mdf;Integrated Security=True";

        public static void initializeDb()
        {
            sqlConnection = new SqlConnection(connectionString);
            bool tryAgain = true;
            while (tryAgain)
            {
                try
                {
                    sqlConnection.Open();
                    tryAgain = false;
                    Console.WriteLine("База данных успешно подключена");
                }
                catch (Exception exp) { Console.WriteLine("Error with connecting to DB: {0}", exp.Message);}

            }

        }

        public static void closeDbConnection()
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
        }

        public static void addRow(string data)
        {
            string[] fields = data.Split('&');

            SqlCommand command = new SqlCommand("INSERT INTO [RESERVATIONS] (NAME, CHECKIN_DATE, CHECKOUT_DATE, TOTAL_AMOUNT, APARTMENT, PHONE_NUMBER)" +
                "VALUES(@NAME, @CHECKIN, @CHECKOUT, @PRICE, @APARTMENT, @PHONENUMBER)", sqlConnection);
            command.Parameters.AddWithValue("NAME", fields[0]);
            command.Parameters.AddWithValue("CHECKIN", Convert.ToDateTime(fields[1]));
            command.Parameters.AddWithValue("CHECKOUT", Convert.ToDateTime(fields[2]));
            command.Parameters.AddWithValue("PRICE", fields[3]);
            command.Parameters.AddWithValue("APARTMENT", fields[4]);
            command.Parameters.AddWithValue("PHONENUMBER", fields[5]);

           
            command.ExecuteNonQuery();

            Console.WriteLine("Строка {0} добавлена в базу", data);
        }

        public static void deleteRow(string data)
        {
            SqlCommand command = new SqlCommand("DELETE FROM [RESERVATIONS] WHERE ID = @ID", sqlConnection);
            command.Parameters.AddWithValue("ID", data);
            command.ExecuteNonQuery();
            Console.WriteLine("Строка {0} удалена из базы", data);
        }

        public static void updateRow(string data)
        {
            string[] fields = data.Split('&');


            SqlCommand command = new SqlCommand("UPDATE [RESERVATIONS] SET  NAME = @NAME, CHECKIN_DATE = @CHECKIN, " +
                    "CHECKOUT_DATE = @CHECKOUT, TOTAL_AMOUNT = @PRICE, " +
                    "APARTMENT = @APARTMENT, PHONE_NUMBER = @PHONENUMBER WHERE ID = @ID", sqlConnection);

                command.Parameters.AddWithValue("ID", fields[0]);
                command.Parameters.AddWithValue("NAME", fields[1]);
                command.Parameters.AddWithValue("CHECKIN", Convert.ToDateTime(fields[2]));
                command.Parameters.AddWithValue("CHECKOUT", Convert.ToDateTime(fields[3]));
                command.Parameters.AddWithValue("PRICE", fields[6]);
                command.Parameters.AddWithValue("APARTMENT", fields[4]);
                command.Parameters.AddWithValue("PHONENUMBER", fields[5]);

                command.ExecuteNonQuery ();

            Console.WriteLine("Строка {0} изменена", data);
        }

        




    }
}
