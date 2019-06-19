using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        private const string _serverHost = "localhost";
        private const int _serverPort = 8080; //порт приема входящих запросов
        private static Thread _serverThread;

        static void Main(string[] args)
        {
            //создаем фоновый поток, который будет обрабатывать собития на клиентах
            _serverThread = new Thread(startServer);
            _serverThread.IsBackground = true;
            _serverThread.Start();

            //основной поток 
            while (true)
                Console.ReadLine();
        }

        private static void startServer()
        {
            DbController.initializeDb();
            IPHostEntry ipHost = Dns.GetHostEntry(_serverHost);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, _serverPort);
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //связываем сокет с точкой, по которой будем принимать данные
            socket.Bind(ipEndPoint);
            //слушаем до 1000 подключений
            socket.Listen(1000);
            Console.WriteLine("Server has been started on IP: {0}.", ipEndPoint);
            while (true)
            {
                try
                {
                    //получаем входящее подключение
                    Socket user = socket.Accept();
                    //создаем обработчик для этого подключения
                    Server.NewClient(user);
                }
                catch (Exception exp) { Console.WriteLine("Error: {0}", exp.Message); }
            }
        }
    }
}
