using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SimpleDB
{
    public partial class Form1 : Form
    {
        private delegate void printer(string data);
        private delegate void cleaner();
        private delegate void createMsgBox(string msg);
        printer Printer;
        cleaner Cleaner;
        createMsgBox CreateMsgBox;

        private Socket _serverSocket;
        private Thread _clientThread;

        private const string _serverHost = "localhost";
        private const int _serverPort = 8080;

        


        public Form1()
        {
            InitializeComponent();
            
            Printer = new printer(print);
            Cleaner = new cleaner(clearDbView);
            CreateMsgBox = new createMsgBox(msgBox);

            connect();
            _clientThread = new Thread(listner);
            _clientThread.IsBackground = true;
            _clientThread.Start();

            send("#refreshview");
        }

        //соединение с сервером 
        private void connect()
        {
            bool tryAgain = true;
            while (tryAgain)
            {
                try
                {
                    IPHostEntry ipHost = Dns.GetHostEntry(_serverHost);
                    IPAddress ipAddress = ipHost.AddressList[0];
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, _serverPort);
                    _serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    _serverSocket.Connect(ipEndPoint);
                    tryAgain = false;
                }
                catch {  }
            }
        }

        //получаем данные от сервера
        //здесь будем слушать, что таблицу надо обновить
        private void listner()
        {
            while (_serverSocket.Connected)
            {
                byte[] buffer = new byte[8196];
                int bytesRec = _serverSocket.Receive(buffer);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                if (data.Contains("#refreshview"))
                {
                    clearDbView();
                    UpdateDbView(data.Substring(data.IndexOf("|")+1));
                    continue;
                }
            }
        }

        private void clearDbView()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Cleaner);
                return;
            }
            dataGridView1.Rows.Clear(); 
        }

        private void msgBox(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(CreateMsgBox, msg);
                return;
            }
            MessageBox.Show(msg, "Ошибка", MessageBoxButtons.OK);
        }

        private void print(string row)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(Printer, row);
                return;
            }
            else
            {
                if (row != "") {
                    string[] s = row.Split('&');
                    dataGridView1.Rows.Add(s[0], s[1], s[2], s[3], s[4], s[6], s[5]);
                }
            }
        }


        private void UpdateDbView(string data)
        {
            clearDbView();
            string[] rows = data.Split('!');

            foreach (string r in rows)
            {
                string[] cells = r.Split('&');
                print(r);
            }
        }

        private void send(string data)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int bytesSent = _serverSocket.Send(buffer);
            }
            catch { msgBox("Связь с сервером прервалась... Пожалуйста, перезапустите приложение"); }
        }


        private void updateRow(DataGridViewRow row)
        {
            
            if (!string.IsNullOrEmpty(row.Cells[1].Value.ToString()) && !string.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) &&
                !string.IsNullOrEmpty(row.Cells[2].Value.ToString()) && !string.IsNullOrWhiteSpace(row.Cells[2].Value.ToString()) &&
                !string.IsNullOrEmpty(row.Cells[3].Value.ToString()) && !string.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) &&
                !string.IsNullOrEmpty(row.Cells[4].Value.ToString()) && !string.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) &&
                !string.IsNullOrEmpty(row.Cells[5].Value.ToString()) && !string.IsNullOrWhiteSpace(row.Cells[1].Value.ToString()) &&
                !string.IsNullOrEmpty(row.Cells[6].Value.ToString()) && !string.IsNullOrWhiteSpace(row.Cells[1].Value.ToString())
                )
            {
                string[] fields = { row.Cells[0].Value.ToString(), row.Cells[1].Value.ToString(), row.Cells[2].Value.ToString(),
                row.Cells[3].Value.ToString(), row.Cells[4].Value.ToString(), row.Cells[5].Value.ToString(), row.Cells[6].Value.ToString()};

                string data = string.Join("&", fields);
                data = "#updaterow|" + data; 
                send(data);
                infoListBox.Items.Add("Бронирование " + row.Cells["ID"].Value + " успешно изменено");
            }
            else
            {
                msgBox("упс! не хватает данных. все ячейки должны быть заполнены");
            }
        }

        private void deleteRow(DataGridViewRow row)
        {
            string data = "#deleterow|" + row.Cells[0].Value;
            send(data);
            infoListBox.Items.Add( "Бронирование " + row.Cells["ID"].Value + " успешно удалено"); 

        }

        

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            send("#close");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            send("#close");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 8)
            {
                updateRow(dataGridView1.CurrentRow);
            }
            else
            if (e.ColumnIndex == 7)
            {
                deleteRow(dataGridView1.CurrentRow);
            }

        }
        

        private void createReservation_click(object sender, EventArgs e)
        {
           
            //прописать проверку строки 
            if (!string.IsNullOrEmpty(nameInsert.Text) && !string.IsNullOrWhiteSpace(nameInsert.Text) &&
                !string.IsNullOrEmpty(dateTimePicker1.Value.ToString()) && !string.IsNullOrWhiteSpace(dateTimePicker1.Value.ToString()) &&
                !string.IsNullOrEmpty(dateTimePicker2.Value.ToString()) && !string.IsNullOrWhiteSpace(dateTimePicker2.Value.ToString()) &&
                !string.IsNullOrEmpty(priceInsert.Text) && !string.IsNullOrWhiteSpace(priceInsert.Text) &&
                !string.IsNullOrEmpty(apartmentInsert.Text) && !string.IsNullOrWhiteSpace(apartmentInsert.Text) &&
                !string.IsNullOrEmpty(phoneInsert.Text) && !string.IsNullOrWhiteSpace(phoneInsert.Text)
                )
            {
                string[] fields = {nameInsert.Text, dateTimePicker1.Value.ToString(), dateTimePicker2.Value.ToString(), priceInsert.Text ,
                apartmentInsert.Text, phoneInsert.Text};
                string data = string.Join("&", fields);
                data = "#addrow|" + data;
                send(data);

                nameInsert.Text = "";
                checkinInsert.Text = "";
                checkoutInsert.Text = "";
                priceInsert.Text = "";
                apartmentInsert.Text = "";
                phoneInsert.Text = "";
            }
            else
            {
                //сделать нормальное всплывающее окно
                msgBox("упс! не хватает данных. все ячейки должны быть заполнены");
            }

            
            
        }
    }
}
