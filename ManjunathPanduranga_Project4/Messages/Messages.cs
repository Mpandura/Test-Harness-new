﻿/////////////////////////////////////////////////////////////////////
// Messages.cs - defines communication messages                    //
// ver 1.0                                                         //
// author : Manjunath Panduranga                                    //
// source: Jim Fawcett,                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Messages provides helper code for building and parsing XML messages.
 *
 */

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
//using Utilities;


namespace TestHarness
{
    [Serializable]
    public class Message
    {

        public string type { get; set; } = "default";
        public string to { get; set; }
        public string from { get; set; }
        public string author { get; set; } = "";
        public DateTime time { get; set; } = DateTime.Now;
        public string body { get; set; } = "";

        public Message(string bodyStr = "")
        {
            body = bodyStr;
        }

        public Message copy_()
        {
            Message temp = new Message();
            temp.to = to;
            temp.from = from;
            temp.author = author;
            temp.time = DateTime.Now;
            temp.body = body;
            return temp;
        }
        public Message fromString(string msgStr)
        {
            Message msg = new Message();
            try
            {
                string[] parts = msgStr.Split(',');
                for (int i = 0; i < parts.Count(); ++i)
                    parts[i] = parts[i].Trim();

                msg.to = parts[0].Substring(4);
                msg.from = parts[1].Substring(6);
                msg.author = parts[2].Substring(8);
                msg.time = DateTime.Parse(parts[3].Substring(6));
                msg.body = parts[4].Substring(6);
            }
            catch
            {
                Console.Write("\n  string parsing failed in Message.fromString(string)");
                return null;
            }
            //XDocument doc = XDocument.Parse(body);
            return msg;
        }
        public override string ToString()
        {
            string temp = "to: " + to;
            temp += ", from: " + from;
            if (author != "")
                temp += ", author: " + author;
            temp += ", time: " + time;
            temp += ", body:\n" + body;
            return temp;
        }
        public Message copy(Message msg)
        {
            Message temp = new Message();
            temp.type = type;
            temp.to = msg.to;
            temp.from = msg.from;
            temp.author = msg.author;
            temp.time = DateTime.Now;
            temp.body = msg.body;
            return temp;
        }
    
    }
    public class testElement
    {
        public string testName { get; set; }
        public string testDriver { get; set; }
        public List<string> testCodes { get; set; } = new List<string>();
        public testElement(string name)
        {
            testName = name;
        }
        public void addDriver(string name)
        {
            testDriver = name;
        }
        public void addCode(string name)
        {
            testCodes.Add(name);
        }
        public override string ToString()
        {
            string temp = "<test name=\"" + testName + "\">";
            temp += "<testDriver>" + testDriver + "</testDriver>";
            foreach (string code in testCodes)
                temp += "<library>" + code + "</library>";
            temp += "</test>";
            return temp;
        }
    }
    public class testRequest
    {
        public string author { get; set; }
        public List<testElement> tests { get; set; } = new List<testElement>();
        public override string ToString()
        {
            string temp = "<testRequest>";
            foreach (testElement te in tests)
                temp += te.ToString();
            temp += "</testRequest>";
            temp = "\n" + temp.formatXml(4);
            return temp;
        }
    }

    public static class extMethods
    {
        public static void show(this Message msg)
        {
            Console.Write("\n  formatted message:");
            string[] lines = msg.ToString().Split(',');
            foreach (string line in lines)
                Console.Write("\n    {0}", line.Trim());
            Console.WriteLine();
        }
        public static string shift(this string str, int n = 2)
        {
            string insertString = new string(' ', n);
            string[] lines = str.Split('\n');
            for (int i = 0; i < lines.Count(); ++i)
            {
                lines[i] = insertString + lines[i];
            }
            string temp = "";
            foreach (string line in lines)
                temp += line + "\n";
            return temp;
        }
        public static string showMsg(this Message msg)
        {
            string str = "";
            str+="\n  formatted message:";
            string[] lines = msg.ToString().Split(new char[] { ',' });
            foreach (string line in lines)
            {
                str+="\n    "+ line.Trim();
            }
            return str;
        }
        public static string showThis(this object msg)
        {
            string showStr = "\n  formatted message:";
            string[] lines = msg.ToString().Split('\n');
            foreach (string line in lines)
                showStr += "\n    " + line.Trim();
            showStr += "\n";
            return showStr;
        }

     
        public static string formatXml(this string xml, int n = 2)
        {
            XDocument doc = XDocument.Parse(xml);
            return doc.ToString().shift(n);
        }
    }


    class TestMessages
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Message Class");
            Console.Write("\n =======================\n");

            Message msg = new Message();
            msg.to = "http://localhost:8080/ICommunicator";
            msg.from = "http://localhost:8081/ICommunicator";
            msg.to = "TH";
            msg.from = "CL";
            msg.author = "Manjunath";
            msg.type = "TestRequest";

            Console.Write("\n  base message:\n    {0}", msg.ToString());
            msg.show();
            Console.WriteLine();

            Console.Write("\n  Testing testRequest");
            Console.Write("\n ---------------------");
            testElement te1 = new testElement("test1");
            te1.addDriver("td1.dll");
            te1.addCode("tc1.dll");
            te1.addCode("tc2.dll");
            testElement te2 = new testElement("test2");
            te2.addDriver("td2.dll");
            te2.addCode("tc3.dll");
            te2.addCode("tc4.dll");
            testRequest tr = new testRequest();
            tr.author = "Manjunath";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            msg.body = tr.ToString();
            Console.Write("\n  TestRequest:");
            msg.show();
            Console.WriteLine();
            Console.Write(msg.body.formatXml());

            Console.Write("\n  Testing Message.fromString(string)");
            Console.Write("\n ------------------------------------");
            Message parsed = msg.fromString(msg.ToString());
            parsed.show();
        }
        //#endif
    }
}
