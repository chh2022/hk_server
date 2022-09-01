using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Form1 : Form
    {
        public delegate void MyInvoke(string str);
        TcpClient tmpTcpClient;
        TcpListener tcpListener;
        public Form1()
        {
            InitializeComponent();
        }

        
        public void Updatetext(string param1)
        {
            this.textBox1.Text = param1 ;
        }
        public void Updatetext2(string param1)
        {
            this.textBox2.Text = param1;
        }
        void RecvPorocess()
        {
            try
            {
                while (true)
                {

                    string receiveMsg = ReceiveMsg(tmpTcpClient);
                    MyInvoke mi = new MyInvoke(Updatetext);
                    MyInvoke mi2 = new MyInvoke(Updatetext2);
                    this.BeginInvoke(mi, new Object[] { receiveMsg });

                    string strFilePath = System.Windows.Forms.Application.StartupPath + "\\" + receiveMsg;

                    bool fileExist = File.Exists(strFilePath);
                    if (fileExist)
                    {

                        this.BeginInvoke(mi, new Object[] { receiveMsg });
                        SendMsg("@file found", tmpTcpClient);
                        string fileNameExt = strFilePath.Substring(strFilePath.LastIndexOf("\\") + 1);
                        FileInfo fileinfo = new FileInfo(strFilePath);
                        System.IO.FileStream fsr = new FileStream(strFilePath, FileMode.Open);
                        byte[] fbt = new byte[fileinfo.Length];
                        fsr.Read(fbt, 0, (int)fileinfo.Length);
                        Sendbyte(fbt, tmpTcpClient);
                        fsr.Close();

                    }
                    else
                    {
                        SendMsg("@file is not exist", tmpTcpClient);
                        this.BeginInvoke(mi, new Object[] { "File does not exist." });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           

        }
        private void button2_Click(object sender, EventArgs e)
        {
            string str = System.Windows.Forms.Application.StartupPath;
            string strFilePath = @"D:\a.txt";
            string fileNameExt = strFilePath.Substring(strFilePath.LastIndexOf("\\") + 1);
            //MessageBox.Show(fileNameExt + "\\3");
            FileInfo fileinfo = new FileInfo(strFilePath);
            //MessageBox.Show(fileinfo.Length.ToString());
            System.IO.FileStream fsr =new FileStream(strFilePath, FileMode.Open);
            byte[] fbt= new byte[fileinfo.Length];
            fsr.Read(fbt, 0, (int)fileinfo.Length);
        
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "*.*|*.*";
            //saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                byte[] bt = new byte[] { 65,66,67,68,69};
                //MessageBox.Show(saveFileDialog1.FileName);
                //System.IO.File.WriteAllBytes(saveFileDialog1.FileName, bt);
                fs.Write(fbt, 0, fbt.Length);
                fs.Close();
            }

        }
        public string ReceiveMsg(TcpClient tmpTcpClient)
        {
            string receiveMsg = string.Empty;
            byte[] receiveBytes = new byte[tmpTcpClient.ReceiveBufferSize];
            int numberOfBytesRead = 0;
            NetworkStream ns = tmpTcpClient.GetStream();

            if (ns.CanRead)
            {
                do
                {
                    numberOfBytesRead = ns.Read(receiveBytes, 0, tmpTcpClient.ReceiveBufferSize);
                    receiveMsg = Encoding.Default.GetString(receiveBytes, 0, numberOfBytesRead);
                }
                while (ns.DataAvailable);
            }
            return receiveMsg;
        }
        public void SendMsg(string msg, TcpClient tmpTcpClient)
        {
            NetworkStream ns = tmpTcpClient.GetStream();
            if (ns.CanWrite)
            {
                byte[] msgByte = Encoding.Default.GetBytes(msg);
                ns.Write(msgByte, 0, msgByte.Length);
            }
        }
        public void Sendbyte(byte[] msg, TcpClient tmpTcpClient)
        {
            NetworkStream ns = tmpTcpClient.GetStream();
            if (ns.CanWrite)
            {
                //byte[] msgByte = Encoding.Default.GetBytes(msg);
                ns.Write(msg, 0, msg.Length);
            }
        }
        private void listening()
        {
            try
            {
                //取得本機名稱
                string hostName = Dns.GetHostName();
                Console.WriteLine("本機名稱=" + hostName);

                //取得本機IP
                //IPAddress[] ipa = Dns.GetHostAddresses(hostName);
                //Console.WriteLine("本機IP=" + ipa[0].ToString());
                //textBox1.Text = ipa[0].ToString();
                //建立本機端的IPEndPoint物件
                IPAddress ipa = IPAddress.Parse("127.0.0.1");
                //IPEndPoint ipe = new IPEndPoint(ipa[0], 1234);
                IPEndPoint ipe = new IPEndPoint(ipa, 1234);

                //建立TcpListener物件

                //TcpListener tcpListener = new TcpListener(ipe);
                tcpListener = new TcpListener(ipe);
                //開始監聽port
                tcpListener.Start();
                //TcpClient tmpTcpClient;
                tmpTcpClient = tcpListener.AcceptTcpClient();
                if (tmpTcpClient.Connected)
                {
                    SslStream sslStream = new SslStream(tmpTcpClient.GetStream(), true);

                    // 取得連接者 IP 與 Port
                    IPEndPoint point = tmpTcpClient.Client.RemoteEndPoint as IPEndPoint;
                    string ip = point.Address.ToString(); // result: 127.0.0.1
                    string port = point.Port.ToString(); // result: 55236
                    //System.Threading.Thread myThread = new Thread(new ThreadStart(RecvPorocess));
                    textBox3.Text = ip;
                    textBox4.Text = port;

                    //ClassRecv handleClient = new ClassRecv(tmpTcpClient);
                    //Thread myThread = new Thread(new ThreadStart(handleClient.Communicate));
                    Thread myThread = new Thread(new ThreadStart(RecvPorocess));
                    myThread.Start();

                }
            }
            catch (Exception ex)
            {
                tcpListener.Stop();
                //tmpTcpClient.Close();
            }

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            Thread myThread = new Thread(new ThreadStart(listening));
            myThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                tcpListener.Stop();
                //tmpTcpClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
