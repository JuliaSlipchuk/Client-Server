using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Sockets;

// файл Request1.xml (<root><subKey></subKey><subKeyOrParam></subKeyOrParam><what></what></root>)
// файл Request2.xml (<root><answer></answer></root>)
namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string Answer;

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null && textBox1.Text != "" && textBox2.Text != null && textBox2.Text != "")
            {
                WriteRequest1();
                string reply = Speaking(@"\Request1.xml");
                MessageBox.Show(reply);
                WriteReply1(reply);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(@"\Reply1.xml");
                XmlNode answer1 = xmlDoc.SelectSingleNode("root/answer");
                if (answer1.InnerText == "Yes")
                {
                    DialogResult digRes = MessageBox.Show("Do you want create a new subKey/param?", "Question", MessageBoxButtons.YesNo);
                    if (digRes == DialogResult.Yes)
                    {
                        WriteRequest2("Yes");
                    }
                    else
                    {
                        WriteRequest2("No");
                    }
                    string reply2 = Speaking(@"\Request2.xml");
                    MessageBox.Show(reply2);
                    WriteReply2(reply2);
                }
                else
                {
                    MessageBox.Show(answer1.InnerText);
                }
            }
            else
            {
                MessageBox.Show("somewhere empty space");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string reply = Speaking(@"\Request2.xml");
            MessageBox.Show(reply);
            WriteReply2(reply);
        }

        public string Speaking(string pathToFile)
        {
            string xmlFromFile = File.ReadAllText(pathToFile, Encoding.UTF8);
            byte[] massByteReq = Encoding.UTF8.GetBytes(xmlFromFile);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("10.0.41.152"), 53000);
            byte[] massByteRep = new byte[10000000];
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(ipEndPoint);
                socket.Send(massByteReq);
                int lenghtRepByteMass = socket.Receive(massByteRep);
                Answer = Encoding.UTF8.GetString(massByteRep, 0, lenghtRepByteMass);
                socket.Shutdown(SocketShutdown.Both);
                return Answer;
            }
        }

        public void WriteRequest1()
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (StreamWriter strWr = new StreamWriter(@"\Request1.xml", false))
            {
                strWr.Write("<root></root>");
            }
            xmlDoc.Load(@"\Request1.xml");
            XmlElement xRoot = xmlDoc.DocumentElement;
            XmlElement subKey = xmlDoc.CreateElement("subKey");
            XmlText subKeyText = xmlDoc.CreateTextNode(textBox1.Text);
            subKey.AppendChild(subKeyText);
            XmlElement subKeyOrParam = xmlDoc.CreateElement("subKeyOrParam");
            XmlText subKeyOrParamText = xmlDoc.CreateTextNode(textBox2.Text);
            subKeyOrParam.AppendChild(subKeyOrParamText);
            xRoot.AppendChild(subKey);
            xRoot.AppendChild(subKeyOrParam);
            XmlElement what = xmlDoc.CreateElement("what");
            XmlText whatText = null;
            if (checkBox1.Checked)
            {
                whatText = xmlDoc.CreateTextNode("subKey");
            }
            else
            {
                whatText = xmlDoc.CreateTextNode("param");
            }
            what.AppendChild(whatText);
            xRoot.AppendChild(what);
            xmlDoc.Save(@"\Request1.xml");
        }

        public void WriteReply1(string reply)
        {
            using (StreamWriter strWr = new StreamWriter(@"\Reply1.xml", false))
            {
                strWr.Write(reply);
            }
        }

        public void WriteRequest2(string answ)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (StreamWriter strWr = new StreamWriter(@"\Request2.xml", false))
            {
                strWr.Write("<root></root>");
            }
            xmlDoc.Load(@"\Request2.xml");
            XmlElement xRoot = xmlDoc.DocumentElement;
            XmlElement answer = xmlDoc.CreateElement("answer");
            XmlText answerText = xmlDoc.CreateTextNode(answ);
            answer.AppendChild(answerText);
            XmlElement subKey = xmlDoc.CreateElement("subKey");
            XmlElement subKeyOrParam = xmlDoc.CreateElement("subKeyOrParam");
            XmlElement what = xmlDoc.CreateElement("what");
            XmlText subKeyT = xmlDoc.CreateTextNode(textBox1.Text);
            XmlText subKeyOrParamT = xmlDoc.CreateTextNode(textBox2.Text);
            XmlText whatT = null;
            if (checkBox1.Checked)
            {
                whatT = xmlDoc.CreateTextNode("subKey");
            }
            else
            {
                whatT = xmlDoc.CreateTextNode("param");
            }
            subKey.AppendChild(subKeyT);
            subKeyOrParam.AppendChild(subKeyOrParamT);
            what.AppendChild(whatT);
            xRoot.AppendChild(answer);
            xRoot.AppendChild(subKey);
            xRoot.AppendChild(subKeyOrParam);
            xRoot.AppendChild(what);
            xmlDoc.Save(@"\Request2.xml");
        }

        public void WriteReply2(string reply)
        {
            using (StreamWriter strWr = new StreamWriter(@"\Reply2.xml", false))
            {
                strWr.Write(reply);
            }
        }
    }
}
