using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Linq;
using System.IO;

namespace ConsoleServer_TePMulticlientProgect
{
    class Program
    {
        class Data_Pack
        {
            string name;
            string surname;
            string fiscal_Code;
            string uni_Code;
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            public string Surname
            {
                get { return surname; }
                set { surname = value; }
            }
            public string Fiscal_code
            {
                get { return fiscal_Code; }
                set { fiscal_Code = value; }
            }
            public string Uni_code
            {
                get { return uni_Code; }
                set { uni_Code = value; }
            }
     
            public void aggiungiData(string t)
            {
                string nomeFile = "C:/Users/GMP/source/repos/ConsoleServer_TePMulticlientProgect/ConsoleServer_TePMulticlientProgect/database.txt";
                StreamWriter sw = File.AppendText(nomeFile);
                try
                {
                    sw.WriteLine(t);
                    Console.WriteLine(t);
                    sw.Close();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e);
                }
                sw = null;
            }
            public bool check_data(string t){
                string nomeFile= "C:/Users/GMP/source/repos/ConsoleServer_TePMulticlientProgect/ConsoleServer_TePMulticlientProgect/database.txt";
                StreamReader File;
                try
                {
                    File = new StreamReader(nomeFile,true);
                    while (!File.EndOfStream)
                    {
                        string record = File.ReadLine();
                        if(t == record){
                            return true;
                        }
                    }
                    File.Close();
                   
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e);
                    
                }
                return false;
                
            }

        }

        

        public class ClientListener
        {
            public static string data = null;
            Socket s;

            public ClientListener(Socket ss)
            {
                s = ss;
            }

            public void SalvaVoto(string t)
            {
                string nomeFile = "C:/Users/GMP/source/repos/ConsoleServer_TePMulticlientProgect/ConsoleServer_TePMulticlientProgect/risultati.txt";
                StreamWriter sw = File.AppendText(nomeFile);
                try
                {
                    sw.WriteLine(t);
                    Console.WriteLine(t);
                    sw.Close();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e);
                }
                sw = null;
            }
            public void listening()
            {
                Console.WriteLine("sono entrati in listening");
                Data_Pack pack = new Data_Pack();
                byte[] bytes = new byte[1024];
                try
                {

                    while (true)
                    {
                        

                        data = "";
                        int i = 0;
                        while (i == 0)
                        {
                            Console.WriteLine("prima di ciclo lettura");
                            while (data.IndexOf("$$") == -1)
                            {
                                int byterec = s.Receive(bytes);
                                data += Encoding.ASCII.GetString(bytes, 0, byterec);
                                Console.WriteLine("stringa ricevuta: "+ Encoding.ASCII.GetString(bytes, 0, byterec));
                            }
                            Console.WriteLine("messaggio ricevuto : {0}", data);

                            
                            string[] dataSplit = data.Split('&');
                            switch (dataSplit[0])
                            {
                                case "<333>":

                                    byte[] msg = Encoding.ASCII.GetBytes("<333>&now&you&can&send&data&$$");
                                    s.Send(msg);
                                    break;
                                case "<400>":
                                    msg = Encoding.ASCII.GetBytes("<300>&data&arrived&successful&$$");
                                    s.Send(msg);
                                    break;
                                case "<414>":
                                    SalvaVoto(dataSplit[1]);
                                    msg = Encoding.ASCII.GetBytes("<300>&data&arrived&successful&$$");
                                    s.Send(msg);
                                    break;
                                case "<444>":

                                    pack.Name = dataSplit[1];
                                    pack.Surname = dataSplit[2];
                                    pack.Fiscal_code = dataSplit[3];
                                    pack.Uni_code = dataSplit[4];
                                    Console.WriteLine(pack.Name);
                                    msg = Encoding.ASCII.GetBytes("<300>&data&arrived&successful&$$");
                                    s.Send(msg);
                                    break;
                                case "<500>":
                                    Console.WriteLine(pack.Name + ";" + pack.Surname + ";" + pack.Fiscal_code);
                                    if (pack.check_data(pack.Name + ";" + pack.Surname + ";" + pack.Fiscal_code))
                                    {
                                        msg = Encoding.ASCII.GetBytes("<500>&end&connection&bye&0&$$");
                                        s.Send(msg);
                                    }
                                    else
                                    {
                                        pack.aggiungiData(pack.Name+";"+pack.Surname+";"+pack.Fiscal_code);
                                        msg = Encoding.ASCII.GetBytes("<500>&end&connection&bye&1&$$");
                                        s.Send(msg);
                                    }
                                    i = 1;
                                    break;
                                case "<501>":
                                    msg = Encoding.ASCII.GetBytes("<500>&end&connection&bye&$$");
                                    s.Send(msg);
                                    break;
                            }
                            data = "";
                        }
                        s.Shutdown(SocketShutdown.Both);
                        s.Close();
                    }

                }
                
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine("connessione chiusa dal client");
                }
                catch (SocketException e)
                {
                    Console.WriteLine("connessione chiusa dal client");
                }
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public class ServerListener
        {
            public static string data = null;
            public static void StartListenig()
            {
                byte[] bytes = new byte[1024];
                //string hostname = Dns.GetHostName();
                //string myIP = Dns.GetHostByName(hostname).AddressList[0].ToString();
                
                IPAddress ipadress = System.Net.IPAddress.Parse(GetLocalIPAddress());
                
                Console.WriteLine("IP: " + ipadress.ToString());
                IPEndPoint localendpoint = new IPEndPoint(ipadress, 1000);
                Console.WriteLine("Port: "+localendpoint);

                Socket listener = new Socket(ipadress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                Console.WriteLine("Timeout : {0}", listener.ReceiveTimeout);

                try
                {
                    listener.Bind(localendpoint);
                    listener.Listen(1000);

                    while (true)
                    {
                        Console.WriteLine("Waiting for a connection...");

                        Socket handler = listener.Accept();
                        data = "";

                        while (data.IndexOf("$$") == -1)
                        {
                            int byterec = handler.Receive(bytes);
                            data += Encoding.ASCII.GetString(bytes, 0, byterec);
                        }
                        Console.WriteLine("messaggio ricevuto : {0}", data);

                        Data_Pack pack = new Data_Pack();
                        string[] dataSplit = data.Split('&');
                        if (dataSplit[0] == "<222>")
                        {
                            Console.WriteLine("creo il thread");
                            ClientListener client = new ClientListener(handler);
                            byte[] msg = Encoding.ASCII.GetBytes("<230>&connection&succesful&$$");
                            handler.Send(msg);
                            Console.WriteLine("sto per lanciarlo");

                            Thread t = new Thread(new ThreadStart(client.listening));
                            t.Start();

                        }
                        else
                        {
                            byte[] msg = Encoding.ASCII.GetBytes("<231>&connection&denied&$$");
                            handler.Send(msg);
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        static void Main(string[] args)
        {
            Thread server = new Thread(new ThreadStart(ServerListener.StartListenig));
            server.Start();
        }
    }
}


    