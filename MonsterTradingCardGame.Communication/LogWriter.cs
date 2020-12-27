using System;
using System.IO;

namespace MonsterTradingCardGame.Communication
{
    public class LogWriter
    {

       

        public void LogToConsole(string logMessage)
        {
            Console.Write("\r\n" + $"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}"  + $"  : {logMessage}");

        }
        public  void LogToFile(string logMessage, TextWriter w)
        {
            w.Write("\r\n"+ $"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}" + $"  : {logMessage}");
       
        }

        public void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }
    }
}
