using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;
using System.Threading;
using System.Text.RegularExpressions;

namespace RESTHTTPWebservice
{
    public class HTTPServer
    {
        //class variables
        TcpListener server = null;
        RequestContext req = null;
        List<RequestContext> requests = null;

        //Constructor
        public HTTPServer(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            requests = new List<RequestContext>();
        }

        public void start()
        {
            server.Start();
            Thread.Sleep(2000);
            handleClients();

        }
        //handler for incoming connections and requests & responses
        private void handleClients()
        {
            StreamReader sr = null;
            StreamWriter sw = null;

            try
            {
                while (true)
                {

                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("\n\nClient connected");
                    sr = new StreamReader(client.GetStream());
                    sw = new StreamWriter(client.GetStream());

                    readRequest(sr);
                    doHTTPMethod(req, sw);

                    client.Close();

                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: " + ex);
            }
            finally
            {
                sw.Close();
                sr.Close();
                server.Stop();
            }
        }

        private void readRequest(StreamReader sr)
        {

            //header einlesen (verb, path, version)
            string request = sr.ReadLine();
            if (request == null)
                return;
            string[] header = request.Split(" ");
            string verb = header[0];
            string path = header[1];
            string httpVersion = header[2];

            Dictionary<string, string> headerLines = new Dictionary<string, string>();
            string payload = null;
            string[] splitted;

            //weitere headerLines einlesen
            for (int i = 0; i < 4; i++)
            {
                request = sr.ReadLine();
                splitted = request.Split(": ");
                headerLines.Add(splitted[0], splitted[1]);
            }
            #region Failures
            //Fehlversuch #1
            //while ((request = sr.ReadLine()) != "\r\n\r\n")
            //{
            //    splitted = request.Split(": ");
            //    headerLines.Add(splitted[0], splitted[1]);
            //}

            //Fehlversuch #2
            //while(sr.Peek() != -1)
            //{
            //    msg += (char)sr.Read();
            //}
            #endregion

            //leerzeile einlesen vor dem payload
            request = sr.ReadLine();

            //payload einlesen
            while (sr.Peek() != -1)
            {
                payload += (char)sr.Read();
            }
            //Problem solver 101
            if (payload == null)
            {
                payload = "";
            }
            //request object erstellen
            req = new RequestContext(verb, path, httpVersion, headerLines, payload);
        }
        private void doHTTPMethod(RequestContext req, StreamWriter sw)
        {
            if (req.Verb.Equals("GET"))
            {
                if (req == null)
                {
                    Console.WriteLine("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                else if (req.Path.Equals("/messages"))
                {
                    foreach (RequestContext item in requests)
                    {
                        Console.WriteLine("\n\nrequest: \n" + item.ToString());
                    }
                    sendResponse(sw, "200 OK", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                else
                {
                    foreach (RequestContext item in requests)
                    {
                        if (item.Path.Equals(req.Path))
                        {
                            Console.WriteLine("\n\nrequest: \n" + item.ToString());
                        }
                    }
                    sendResponse(sw, "200 OK", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
            }
            if (req.Verb.Equals("POST"))
            {
                if (req == null)
                {
                    Console.WriteLine("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                if (req.Path == "/messages")
                {
                    Console.WriteLine("request wasn't executed properly. Can't add messagelist itself!");
                    sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                else if (req.Payload == "")
                {
                    requests.Add(req);
                    Console.WriteLine("No content in the Payload");
                    sendResponse(sw, "204 NO CONTENT", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                else
                {
                    requests.Add(req);
                    Console.WriteLine("Request succesfully added");
                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }


            }
            if (req.Verb.Equals("PUT"))
            {
                if (req == null)
                {
                    Console.WriteLine("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                if (req.Path == "/messages")
                {
                    Console.WriteLine("request wasn't executed properly. Can't change the messagelist itself!");
                    sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                else
                {
                    for (int i = 0; i < requests.Count; i++)
                    {
                        if (requests[i].Path.Equals(req.Path))
                        {
                            string[] splitted = req.Path.Split('/');
                            int msgId = Int32.Parse(splitted[2]);
                            requests[msgId - 1].Payload = req.Payload;

                            requests[msgId - 1].HeaderLines["Content-Length"] = req.Payload.Length.ToString();
                        }
                    }
                    Console.WriteLine("request succesfully changed");
                    sendResponse(sw, "200 OK", req.HttpVersion, req.HeaderLines["Host"], req.Payload);

                    #region Failures
                    //foreach is read only!!!
                    //foreach (RequestContext item in requests)
                    //{                   
                    //    if (item.Path.Equals(req.Path))
                    //    {
                    //        string[] splitted = req.Path.Split('/');
                    //        int msgId = Int32.Parse(splitted[2]);
                    //        requests[msgId-1].Payload = item.Payload;

                    //    }
                    //}   
                    #endregion
                }

            }
            if (req.Verb.Equals("DELETE"))
            {
                if (req == null)
                {
                    Console.WriteLine("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                if (req.Path == "/messages")
                {
                    Console.WriteLine("request wasn't executed properly. Can't delete the messagelist itself!");
                    sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }

                else
                {
                    for (int i = 0; i < requests.Count; i++)
                    {
                        if (requests[i].Path.Equals(req.Path))
                        {
                            string[] splitted = req.Path.Split('/');
                            int msgId = Int32.Parse(splitted[2]);
                            requests.RemoveAt(msgId - 1);
                        }
                    }
                    Console.WriteLine("request succesfully deleted");
                    sendResponse(sw, "200 OK", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }

            }
        }

        private void sendResponse(StreamWriter sw, string statusCode, string version, string host, string payload)
        {
            sw.Write(version + " " + statusCode + "\r\n");
            sw.Write("host: " + host + "\r\n");
            sw.Write("Content-Type: " + "text/plain\r\n");
            sw.Write("Content-Length: " + payload.Length + "\r\n");
            sw.Write("\r\n");
            sw.Write(payload);
            sw.Write("\r\n\r\n");
            sw.Close();
            sw.Dispose();
        }


    }

}
