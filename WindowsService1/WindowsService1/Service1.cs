using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Xml;
using System.IO;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        Thread thread;

        protected override void OnStart(string[] args)
        {
            Debugger.Launch();
            thread = new Thread(WorkerThread);
            thread.Start();
        }

        protected override void OnStop()
        {
            if (thread != null || thread.IsAlive)
                thread.Abort();
        }

        public static void WorkerThread()
        {
            IPAddress ipAddr = IPAddress.Parse("10.0.41.152");
            IPEndPoint ipEndPoint1 = new IPEndPoint(ipAddr, 53000);
            Socket socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket1.Bind(ipEndPoint1);
            socket1.Listen(10);
            while (true)
            {
                using (Socket socket2 = socket1.Accept())
                {
                    IPEndPoint ipEndPoint2 = new IPEndPoint(ipAddr, 0);
                    EndPoint endPoint = (EndPoint)ipEndPoint2;
                    byte[] byteMassGet = new byte[100000];
                    int lengthRec = socket2.ReceiveFrom(byteMassGet, ref endPoint);
                    string request = Encoding.UTF8.GetString(byteMassGet, 0, lengthRec);
                    WriteLog(request);
                    string reply = "";
                    Debugger.Launch();
                    if (!request.Contains("answer"))
                    {
                        using (StreamWriter strWr = new StreamWriter(@"\Request1.xml", false))
                        {
                            strWr.Write(request);
                        }
                        reply = Method1();
                    }
                    else
                    {
                        Debugger.Launch();
                        using (StreamWriter strWr = new StreamWriter(@"\Request2.xml", false))
                        {
                            strWr.Write(request);
                        }
                        reply = Method2(request);
                    }
                    WriteLog(reply);
                    byte[] massByteRep = Encoding.UTF8.GetBytes(reply);
                    socket2.Send(massByteRep);
                    socket2.Shutdown(SocketShutdown.Both);
                }
            }
        }

        public static string Method1()
        {
            string reply = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"\Request1.xml");
            XmlNode subKeyX = xmlDoc.SelectSingleNode("root/subKey");
            XmlNode subKeyOrParamX = xmlDoc.SelectSingleNode("root/subKeyOrParam");
            XmlNode whatX = xmlDoc.SelectSingleNode("root/what");
            string subKey = subKeyX.InnerText;
            string subKeyOrParam = subKeyOrParamX.InnerText;
            string what = whatX.InnerText;
            bool check = false;
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\"))
            {
                string[] regKeys = regKey.GetSubKeyNames();
                for (int i = 0; i < regKeys.Length; i++)
                {
                    if (regKeys[i] == subKey)
                    {
                        check = true;
                        if (what == "subKey")
                        {
                            reply = Method(what, subKey, subKeyOrParam);
                        }
                        else
                        {
                            reply = Method(what, subKey, subKeyOrParam);
                        }
                        break;
                    }
                }
                if (!check)
                {
                    reply = "<root> <answer> No </answer> </root>";
                }
            }
            return reply;
        }

        public static string Method(string what, string subKey, string subKeyOrParam)
        {
            string reply = "Yes";
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\" + subKey))
            {
                if (what == "subKey")
                {
                    string[] massSubKeys = regKey.GetSubKeyNames();
                    for (int i = 0; i < massSubKeys.Length; i++)
                    {
                        if (massSubKeys[i] == subKeyOrParam)
                        {
                            reply = "No";
                            break;
                        }
                    }
                }
                else
                {
                    string[] massValues = regKey.GetValueNames();
                    for (int i = 0; i < massValues.Length; i++)
                    {
                        if (massValues[i] == subKeyOrParam)
                        {
                            reply = "No";
                            break;
                        }
                    }
                }
            }
            return "<root> <answer>" + reply + "</answer> </root>";
        }

        public static string Method2(string request2)
        {
            string reply = "";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"\Request2.xml");
            XmlNode answer = xmlDoc.SelectSingleNode("root/answer");
            if (answer.InnerText == "Yes")
            {
                XmlNode subKey = xmlDoc.SelectSingleNode("root/subKey");
                XmlNode subKeyOrParam = xmlDoc.SelectSingleNode("root/subKeyOrParam");
                XmlNode what = xmlDoc.SelectSingleNode("root/what");
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\" + subKey.InnerText, true))
                {
                    if (what.InnerText == "subKey")
                    {
                        regKey.CreateSubKey(subKeyOrParam.InnerText);
                    }
                    else
                    {
                        regKey.SetValue(subKeyOrParam.InnerText, "");
                    }
                }
                reply = String.Format($"<answer> SubKey {subKey.InnerText} and subKeyOrParam {subKeyOrParam.InnerText} successfully created </answer>");
            }
            else
            {
                reply = "<answer> You have canceled the creation of a subset/parameter, not created </answer>";
            }
            return reply;
        }

        public static void WriteReply(string reply)
        {
            using (StreamWriter strWr = new StreamWriter(@"\Reply.xml", false))
            {
                strWr.Write(reply);
            }
        }

        public static void WriteLog(string str)
        {
            using (StreamWriter strWr = new StreamWriter(@"C:\Windows\Logs\RequestsOrExept.log", true))
            {
                strWr.Write(str);
            }
        }
    }
}
