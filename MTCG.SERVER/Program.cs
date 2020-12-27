using System;

namespace RESTHTTPWebservice
{
    class Program
    {
        static void Main(string[] args)
        {
            HTTPServer server = new HTTPServer(10001);
            Console.WriteLine("Server running");
            Console.WriteLine("Waiting for a connection... ");
            server.start();
        }
    }
}
