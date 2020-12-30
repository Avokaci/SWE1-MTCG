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
using Npgsql;
using MTCG.Lib;

namespace RESTHTTPWebservice
{
    public class HTTPServer
    {

        //db 
        string _connectionString;
        NpgsqlConnection con;
        NpgsqlCommand cmd;
        //logging
        LogWriter logger = new LogWriter();
        StreamWriter w = File.AppendText("log.txt");
        //StreamReader r = File.OpenText("log.txt");
        //game 
        List<User> users = new List<User>();
        List<package> packages = new List<package>();
        List<Card> cardpackage;

        string token = "";

        //class variables
        TcpListener server = null;
        RequestContext req = null;
      

        //Constructor
        public HTTPServer(int port)
        {
            _connectionString = "Host=localhost;Username=postgres;Password=s$cret;Database=mtcg";
            con = new NpgsqlConnection(_connectionString);
            con.Open();
            string sql = "SELECT version()";
            cmd = new NpgsqlCommand(sql, con);
            var version = cmd.ExecuteScalar().ToString();
            Console.WriteLine($"PostgreSQL version: {version}");
            //Create tables
            cmd.CommandText = "DROP TABLE IF EXISTS users";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE TABLE users(Id serial , 
                                Username VARCHAR(255) unique,
                                Password VARCHAR(255), Name VARCHAR(255), 
                                Bio VARCHAR(255), Image VARCHAR(255),
                                Token VARCHAR(255))";
            cmd.ExecuteNonQuery();
           

            server = new TcpListener(IPAddress.Any, port);          
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
            string[] splitted;
            Dictionary<string, string> headerLines = new Dictionary<string, string>();
            string payload = null;
            //weitere header einlesen
            string headerLinesAndPayload = "";
            
            while (sr.Peek() != -1)
            {
                headerLinesAndPayload += (char)sr.Read();
            }
            splitted = headerLinesAndPayload.Split("\r\n");
            payload = splitted[splitted.Length - 1];
            string[] headerliness;
            for (int i = 0; i < splitted.Length-2; i++)
            {
                headerliness = splitted[i].Split(": ");
                headerLines.Add(headerliness[0], headerliness[1]);
            }

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
                        
                        string[] arr = req.HeaderLines["Authorization"].Split(" ");
                        token = arr[1];
                        cmd = new NpgsqlCommand("select username from users where token=@Token", con);           
                        cmd.Parameters.AddWithValue("Token", token);
                        string username = cmd.ExecuteScalar().ToString();

                        foreach (User item in users)
                        {
                            if(item.Username == username && item.Token == token) //validation not 100% correct
                            {
                                logger.LogToConsole("Username " + item.Username + " Name: " + item.Name + " Bio: " + item.Bio + " Image: " +item.Image  );
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                            }
                        }

                    }                 
                }
                if (splittedPath[1].Equals("cards"))
                {
                    bool closeSW = false;
                    if (req.HeaderLines.ContainsKey("Authorization"))
                    {
                        string[] arr = req.HeaderLines["Authorization"].Split(" ");
                        token = arr[1];
                        cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                        cmd.Parameters.AddWithValue("Token", token);
                        string username = cmd.ExecuteScalar().ToString();

                        foreach (User item in users)
                        {
                            if (item.Username == username && item.Token == token) //validation not 100% correct
                            {
                                logger.LogToConsole("User " + item.Username + " has the following cards: \r\n");
                                foreach (Card cardd in item.Stack)
                                {
                                    logger.LogToConsole("Card ID: " + cardd.Id + "\r\nCard Name: " + cardd.Name + "\r\nCard Damage: " + cardd.Damage + "\r\n");
                                }
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                closeSW = true;
                            }                          
                        }                     
                    }
                    else
                    {
                        logger.LogToConsole("Invalid Token!");
                        sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);

                    }

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
                    acc.Token = acc.Username + "-mtcgToken";
                    cmd = new NpgsqlCommand();
                    foreach (User item in users)
                    {
                        if(item.Username == acc.Username )
                        {
                            logger.LogToConsole("User " + item.Username + " already exists. Choose another username!");
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                            closeSW = true;
                        }                     
                    }
                    if (closeSW == false)
                    {
                        cmd = new NpgsqlCommand("insert into users(username,password,name,bio,image,token) " +
                            "values(@Username,@Password,null,null,null,@Token)", con);
                        cmd.Parameters.AddWithValue("Username", acc.Username);
                        cmd.Parameters.AddWithValue("Password", acc.Password);
                        cmd.Parameters.AddWithValue("Token", acc.Username + "-mtcgToken");
                        cmd.ExecuteNonQuery();      
                        
                        
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
                if (splittedPath[1].Equals("packages"))
                {
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    string tokenn = arr[1];
                   
                    //token = headerlines 
                    if (tokenn == "admin-mtcgToken")
                    {
                       
                        List<Card> cardpackage = JsonSerializer.Deserialize<List<Card>>(req.Payload);
                        package pack = new package(cardpackage[0],cardpackage[1],cardpackage[2], cardpackage[3], cardpackage[4]);                        
                        //insert packages into db
                        packages.Add(pack);
                        logger.LogToConsole("package succesfully created!");
                        sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                    }
                    else
                    {
                        logger.LogToConsole("Not authorized to create package!");
                        sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                    }

                }
                //user aquires packages
                if (splittedPath[1].Equals("transactions")&&splittedPath[2].Equals("packages"))
                {
                    bool closeSW = false;
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();

                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token) //validation not 100% correct
                        {
                            if (packages.Count != 0)
                            {
                                if (item.Coins >= 5)
                                {
                                    Random rand = new Random();
                                    int rando = rand.Next(0, packages.Count - 1);

                                    item.Stack.Add(packages[rando].Card1);
                                    item.Stack.Add(packages[rando].Card2);
                                    item.Stack.Add(packages[rando].Card3);
                                    item.Stack.Add(packages[rando].Card4);
                                    item.Stack.Add(packages[rando].Card5);

                                    item.Coins -= 5;
                                    packages.RemoveAt(rando);

                                    logger.LogToConsole("User " + item.Username + " succesfully aquired Package");
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                    closeSW = true;
                                }
                                else
                                {
                                    logger.LogToConsole("User " + item.Username + " doesn't have enough Coins!");
                                    sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                   

                                }
                            }
                            else
                            {
                                logger.LogToConsole("User " + item.Username + " can't aquire packages because there are no packages left!");
                                sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                              

                            }
                        }
                    }
                
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
                        string[] arr = req.HeaderLines["Authorization"].Split(" ");
                        token = arr[1];
                        cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                        cmd.Parameters.AddWithValue("Token", token);
                        string username = cmd.ExecuteScalar().ToString();
                        bool closeSW = false;

                        User acc = JsonSerializer.Deserialize<User>(req.Payload);
                        foreach (User item in users)
                        {
                            if (item.Username == username && item.Token == token) //validation not 100% correct
                            {
                                item.Name = acc.Name;
                                item.Bio = acc.Bio;
                                item.Image = acc.Image;

                                cmd = new NpgsqlCommand("update users set Name = @Name, Bio = @Bio, Image = @Image where username = @UserName", con);
                                cmd.Parameters.AddWithValue("Name", item.Name);
                                cmd.Parameters.AddWithValue("Bio", item.Bio);
                                cmd.Parameters.AddWithValue("Image", item.Image);
                                cmd.Parameters.AddWithValue("Username", item.Username);
                                cmd.ExecuteNonQuery();


                                logger.LogToConsole("User data succesfully changed");
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                closeSW = true;
                            }
                          
                        }
                        if(closeSW == false)
                        {
                            logger.LogToConsole("Not authorized to change profile!");
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
            //sw.Close();
            //sw.Dispose();
        }


    }

}
