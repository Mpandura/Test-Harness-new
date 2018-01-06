/////////////////////////////////////////////////////////////////////
// MessageTest.cs - defines specialized communication messages     //
// ver 1.0                                                         //
// author : Manjunath Panduranga                                    //
// source: Jim Fawcett,                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Messages provides helper code for building and parsing XML messages.

 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace TestHarness
{
    public class TestElement
    {
        public string testName { get; set; }
        public string testDriver { get; set; }
        public List<string> testCodes { get; set; } = new List<string>();

        public TestElement() { }
        public TestElement(string name)
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
            string te = "\ntestName:\t" + testName;
            te += "\ntestDriver:\t" + testDriver;
            foreach (string code in testCodes)
            {
                te += "\ntestCode:\t" + code;
            }
            return te += "\n";
        }
    }
    public class TestRequest
    {
        public TestRequest() { }
        public string author { get; set; }
        public List<TestElement> tests { get; set; } = new List<TestElement>();

        public override string ToString()
        {
            string tr = "\nAuthor:\t" + author + "\n";
            foreach (TestElement te in tests)
            {
                tr += te.ToString();
            }
            return tr;
        }
    }

    public class MessageTest
    {
        public static string makeTestRequest()
        {

            testElement te1 = new testElement("test1");
            testElement te2 = new testElement("test2");
            testElement te3 = new testElement("test3");
            testElement tlg = new testElement("loggerTest");
            te2.addDriver("td1.dll");
            te2.addCode("tc1.dll");
            te1.addDriver("testdriver.dll");
            te1.addCode("testedcode.dll");
            te3.addDriver("anothertestdriver.dll");
            te3.addCode("anothertestedcode.dll");
            tlg.addDriver("logger.dll");
            testRequest tr = new testRequest();
            tr.author = "Jim Fawcett";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            tr.tests.Add(te3);
            tr.tests.Add(tlg);
            return tr.ToString();
        }
      
        static void Main(string[] args)
        {
            Message msg = new Message();
            msg.to = "http://localhost:8080/ICommunicator";
            msg.from = "http://localhost:8081/ICommunicator";
            msg.author = "Manjunath";

            testElement te1 = new testElement("test1");
            testElement te2 = new testElement("test2");
            testElement te3 = new testElement("test3");
            testElement tlg = new testElement("loggerTest");
            te2.addDriver("td1.dll");
            te2.addCode("tc1.dll");
            te1.addDriver("testdriver.dll");
            te1.addCode("testedcode.dll");
            te3.addDriver("anothertestdriver.dll");
            te3.addCode("anothertestedcode.dll");
            tlg.addDriver("logger.dll");
            testRequest tr = new testRequest();
            tr.author = "Manjunath";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            tr.tests.Add(te3);
            tr.tests.Add(tlg);
            msg.body = tr.ToString();

            Console.Write("\n  Serialized TestRequest:");
            Console.Write("\n -------------------------\n");
            Console.Write(msg.body.shift());

            Console.Write("\n  TestRequest Message:");
            Console.Write("\n ----------------------");
            msg.showMsg();

            Console.Write("\n  Testing Deserialized TestRequest");
            Console.Write("\n ----------------------------------\n");
            TestRequest trDS = msg.body.FromXml<TestRequest>();
            Console.Write(trDS.showThis());
        }
    }
}
