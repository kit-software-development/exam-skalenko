using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Client
    {
        private Socket _handler;
        private Thread _userThread;

        //конструктор 
        public Client(Socket socket)
        {
            //создаем поток для обработки подключенного клиента
            _handler = socket;
            _userThread = new Thread(listner);
            _userThread.IsBackground = true;
            _userThread.Start();
        }

        public string remoteEndPoint
        {
            get { return Convert.ToString(_handler.RemoteEndPoint); }
        }

        //отключение клиента
        public void End()
        {
            try
            {
                //закрываем сокет и прекращаем поток 
                _handler.Close();
                try
                {
                    _userThread.Abort();
                }
                catch { }
            }
            catch (Exception exp) { Console.WriteLine("Error with end: {0}.", exp.Message); }
        }

        //получаем команду с сервера и передаем ее обработчику события
        private void listner()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRec = _handler.Receive(buffer);
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                    handleCommand(data);
                }
                catch { Server.EndClient(this); return; }
            }
        }

        private void handleCommand(string data)
        {
            //здесь надо описать команды, которые форма будет кидать серверу
            //обновить, удалить, добавить, закрыть БД
            
            if (data.Contains("#updaterow"))
            {
                DbController.updateRow(data.Substring(data.IndexOf("|")+1));
                
                return;
            }
            if (data.Contains("#deleterow"))
            {
                //для deleteRow отправлять только ID!
                DbController.deleteRow(data.Substring(data.IndexOf("|")+1));
                Server.updateAllDbViews();
                return;
            }
            if (data.Contains("#addrow"))
            {
                DbController.addRow(data.Substring(data.IndexOf("|")+1));
                Server.updateAllDbViews();
                return;
            }
            if (data.Contains("#close"))
            {
                End();
                return;
            }
            if (data.Contains("#refreshview"))
            {
                Server.updateAllDbViews();
                return;
            }
        }

        public void updateDbView(string data)
        {
            //отправляем что-то клиенту чтобы он обновил вьюшку
            //отправляем строку строк базы данных
            Send(data);
        }


        public void Send(string command)
        {
            try
            {
                int bytesSent = _handler.Send(Encoding.UTF8.GetBytes(command));
                if (bytesSent > 0) Console.WriteLine("Success");
            }
            catch (Exception exp) { Console.WriteLine("Error with send command: {0}.", exp.Message); Server.EndClient(this); }
        }


    }
}
