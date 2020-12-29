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
using MonsterTradingCardGame.Communication;
using MonsterTradingCardGame.Core;
using System.Text.Json;
using MonsterTradingCardGame.Lib;

namespace RESTHTTPWebservice
{
    public class HTTPServer
    {
        LogWriter logger = new LogWriter();
        StreamWriter w = File.AppendText("log.txt");
        //StreamReader r = File.OpenText("log.txt");
        List<User> users = new List<User>();
        List<Card> package;
        
        string token = "";

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
                    //logger.LogToConsole("Client connected");
                    sr = new StreamReader(client.GetStream());
                    sw = new StreamWriter(client.GetStream());
                   
                    readRequest(sr);
                    doHTTPMethod(req, sw);

                    client.Close();
                    //logger.LogToConsole("Client disconnected");



                }
            }
            catch (SocketException ex)
            {
                logger.LogToConsole("SocketException: " + ex);
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
            string[] splittedPath = req.Path.Split("/");
            

            if (req.Verb.Equals("GET"))
            {
                //request is faulty/empty
                if (req == null)
                {
                    logger.LogToConsole("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                //list user informaton
                if (splittedPath[1].Equals("users"))
                {
                    if (splittedPath[2] != null)
                    {
                        foreach (User item in users)
                        {
                            if(item.Username == splittedPath[2])
                            {
                                logger.LogToConsole("Username " + item.Username + " Name: " + item.Name + " Bio: " + item.Bio + " Image: " +item.Image  );
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                            }
                        }
                    }                 
                }
                if (splittedPath[1].Equals("cards"))
                {

                }
                if (splittedPath[1].Equals("deck"))
                {

                }
                if (splittedPath[1].Equals("stats"))
                {

                }
                if (splittedPath[1].Equals("score"))
                {

                }
                if (splittedPath[1].Equals("tradings"))
                {

                }


               
            }
            if (req.Verb.Equals("POST"))
            {
                //request is faulty/empty
                if (req == null)
                {
                    logger.LogToConsole("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                //create new user
                if(splittedPath[1].Equals("users"))
                {
                    bool closeSW = false;
                    User acc = JsonSerializer.Deserialize<User>(req.Payload);
                    foreach (User item in users)
                    {
                        if(item.Username == acc.Username)
                        {
                            logger.LogToConsole("User " + item.Username + " already exists. Choose another username!");
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                            closeSW = true;
                        }                     
                    }
                    if (closeSW == false)
                    {
                        users.Add(acc);
                        logger.LogToConsole("User " + acc.Username + " succesfully created!");
                        sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                    }
                }
                //user login
                if (splittedPath[1].Equals("sessions"))
                {
                    bool closeSW = false;
                    User acc = JsonSerializer.Deserialize<User>(req.Payload);
                    foreach (User item in users)
                    {
                        if (item.Username == acc.Username && item.Password == acc.Password)
                        {
                            logger.LogToConsole("User " + acc.Username + " succesfully logged in!");
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                            closeSW = true;
                            token = acc.Username + "-mtcgToken";
                        }                      
                    }
                    if (closeSW == false)
                    {
                        logger.LogToConsole("User does not exist or credentials are wrong!");
                        sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                    }

                }
                //admin creates package
                if (splittedPath[1].Equals("package"))
                {
                    if (token == "admin-mtcgToken")
                    {
                       
                        package = JsonSerializer.Deserialize<List<Card>>(req.Payload);
                        logger.LogToConsole("package succesfully created!");
                        sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                    }
                    else
                    {
                        logger.LogToConsole("Not authorized to create package!");
                        sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                    }

                }
                if (splittedPath[1].Equals("transactions/package"))
                {

                }
                if (splittedPath[1].Equals("battles"))
                {

                }
                if (splittedPath[1].Equals("tradings"))
                {

                }

                

            }
            if (req.Verb.Equals("PUT"))
            {
                //request is faulty/empty
                if (req == null)
                {
                    logger.LogToConsole("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                //Request: Change User profile
                if (splittedPath[1].Equals("users"))
                {
                    if (splittedPath[2] != null)
                    {                   
                        User acc = JsonSerializer.Deserialize<User>(req.Payload);
                        foreach (User item in users)
                        {
                            if (item.Username == acc.Username)
                            {
                                item.Name = acc.Name;
                                item.Bio = acc.Bio;
                                item.Image = acc.Image;
                                logger.LogToConsole("User data succesfully changed");
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                            }
                        }
                    }                                                   
                }
                if (splittedPath[1].Equals("deck"))
                {

                }
                

            }
            if (req.Verb.Equals("DELETE"))
            {
                if (req == null)
                {
                    logger.LogToConsole("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                if (splittedPath[1].Equals("sessions"))
                {

                }

            }
        }

        private void sendResponse(StreamWriter sw, string statusCode, string version, string host, string payload)
        {
            sw.Write(version + " " + statusCode + "\r\n");
            sw.Write("host: " + host + "\r\n");
            sw.Write("Content-Type: " + "application/json\r\n");
            sw.Write("Content-Length: " + payload.Length + "\r\n");
            sw.Write("\r\n");
            sw.Write(payload);
            sw.Write("\r\n\r\n");
            sw.Close();
            sw.Dispose();
        }


    }

}
