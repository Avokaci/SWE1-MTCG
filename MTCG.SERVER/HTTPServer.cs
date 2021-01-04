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

        #region variables
        //db 
        string _connectionString;
        NpgsqlConnection con;
        NpgsqlCommand cmd;
        //logging
        LogWriter logger = new LogWriter();
        //game 
        List<User> users = new List<User>();
        List<package> packages = new List<package>();
        List<TradeDeal> tradeDeals = new List<TradeDeal>();
        int packid = -1;
        string token = "";
        int playercount = 0;
       
        //class variables
        TcpListener server = null;
        RequestContext req = null;
        #endregion

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
                                Password VARCHAR(255), 
                                Name VARCHAR(255), 
                                Bio VARCHAR(255), 
                                Image VARCHAR(255),
                                Token VARCHAR(255),
                                Games INT,
                                Wins INT,
                                Losses INT,
                                Elo INT)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DROP TABLE IF EXISTS packages";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE TABLE packages(Id int , 
                                Card1 VARCHAR(255),
                                Card2 VARCHAR(255), 
                                Card3 VARCHAR(255), 
                                Card4 VARCHAR(255), 
                                Card5 VARCHAR(255))";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DROP TABLE IF EXISTS trade_deals";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE TABLE trade_deals(Id VARCHAR(255) , 
                                Card_To_Trade_ID VARCHAR(255),
                                Card_To_Trade_Name VARCHAR(255),
                                Card_To_Trade_Damage int,
                                Wanted_Card_Type VARCHAR(255), 
                                Wanted_Card_Minimum_Damage int, 
                                Requesting_User VARCHAR(255))";
            cmd.ExecuteNonQuery();



            server = new TcpListener(IPAddress.Any, port);          
        }
        //for multithreaded client purposes
        public void start()
        {
            server.Start();
            Thread.Sleep(2000);
            Thread t = new Thread(handleClients);
            t.Start();
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
                    sr = new StreamReader(client.GetStream());
                    sw = new StreamWriter(client.GetStream());                           
                    readRequest(sr);
                    doHTTPMethod(req, sw);
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                server.Stop();
            }
            finally
            {
                sw.Close();
                sr.Close();
                server.Stop();
            }
        }
        //method to read request and process it
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
        //method to react on the request and send a response 
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
                        bool closeSW = false;
                        string[] arr = req.HeaderLines["Authorization"].Split(" ");
                        token = arr[1];
                        cmd = new NpgsqlCommand("select username from users where token=@Token", con);           
                        cmd.Parameters.AddWithValue("Token", token);
                        string username = cmd.ExecuteScalar().ToString();

                        string[] splittedtoken = token.Split("-");

                        if (splittedPath[2] == username && splittedtoken[0] == splittedPath[2])
                        {
                            foreach (User item in users)
                            {
                                if (item.Username == username && item.Token == token) //validation not 100% correct
                                {
                                    logger.LogToConsole("Username " + item.Username + " Name: " + item.Name + " Bio: " + item.Bio + " Image: " + item.Image);
                                    string responseMsg = ("Username " + item.Username + " Name: " + item.Name + " Bio: " + item.Bio + " Image: " + item.Image);
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                                    closeSW = true;
                                }
                            }
                        }
                        if (closeSW == false)
                        {
                            logger.LogToConsole("Not authorized to show profile!");
                            string responseMsg = ("Not authorized to show profile!");
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                        }

                    }                 
                }
                //show all aquired cards
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
                                string responseMsg = "User " + item.Username + " has the following cards: \r\n";
                                logger.LogToConsole("User " + item.Username + " has the following cards: \r\n");
                                foreach (Card cardd in item.Stack)
                                {
                                    responseMsg += "Card ID: " + cardd.Id + "\r\nCard Name: " + cardd.Name + "\r\nCard Damage: " + cardd.Damage + "\r\n";
                                    logger.LogToConsole("Card ID: " + cardd.Id + "\r\nCard Name: " + cardd.Name + "\r\nCard Damage: " + cardd.Damage + "\r\n");
                                }
                                
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                                closeSW = true;
                            }                          
                        }                     
                    }
                    else
                    {
                        logger.LogToConsole("Invalid Token!");
                        string responseMsg = "Invalid Token!";
                        sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], responseMsg);

                    }

                }
                //show deck
                if (splittedPath[1].Equals("deck"))
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
                            string responseMsg = "User " + item.Username + " has the following cards in his deck: \r\n";
                            logger.LogToConsole("User " + item.Username + " has the following cards in his deck: \r\n");
                            foreach (Card cardd in item.Deck)
                            {
                                responseMsg += "Card ID: " + cardd.Id + "\r\nCard Name: " + cardd.Name + "\r\nCard Damage: " + cardd.Damage + "\r\n";
                                logger.LogToConsole("Card ID: " + cardd.Id + "\r\nCard Name: " + cardd.Name + "\r\nCard Damage: " + cardd.Damage + "\r\n");
                            }
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                        }
                    }
                   
                }
                //show deck in different representation
                if (splittedPath[1].Equals("deck?format=plain"))
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
                            string responseMsg = "User " + item.Username + " has the following cards in his deck (plain view): \r\n";
                            logger.LogToConsole("User " + item.Username + " has the following cards in his deck (plain view): \r\n");
                            foreach (Card cardd in item.Deck)
                            {
                                responseMsg += "Card ID: " + cardd.Id + "\r\n";
                                logger.LogToConsole("Card ID: " + cardd.Id + "\r\n");
                            }
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                        }
                    }

                }
                //show stats for a user
                if (splittedPath[1].Equals("stats"))
                {
                    bool closeSW = false;
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();
                    string[] splittedtoken = token.Split("-");
                 
                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token) 
                        {
                           
                            double wlRatio;
                            if (item.Losses == 0)
                                wlRatio = item.Wins;
                            else
                                wlRatio = item.Wins / item.Losses;

                            string responseMsg = "Stats for user " + item.Username + " : \r\n" +
                                "Games played: " + item.PlayedGames + "\r\n" +
                                "Wins: " + item.Wins + "\r\n" +
                                "Losses: " + item.Losses + "\r\n" +
                                "Elo: " + item.Elo + "\r\n" +
                                "Win/Lose ratio: " + wlRatio;

                            logger.LogToConsole("Stats for user " + item.Username + " : \r\n"  + 
                                "Games played: " + item.PlayedGames + "\r\n"+
                                "Wins: " + item.Wins + "\r\n" +
                                "Losses: " + item.Losses + "\r\n" +
                                "Elo: " + item.Elo + "\r\n" +
                                "Win/Lose ratio: " + wlRatio); //OPTIONAL win lose ratio
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                            closeSW = true;
                        }
                    }
                    
                }
                //scoreboard that shows all users sorted by their elo
                if (splittedPath[1].Equals("score"))
                {
                    bool closeSW = false;
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();

                    users.Sort((x, y) => x.Elo.CompareTo(y.Elo));
                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token)
                        {
                            string response = "Scoreboard: \r\n";
                            logger.LogToConsole("Scoreboard: \r\n");
                            foreach (User itemm in users)
                            {
                                response += "Elo: " + itemm.Elo + " Username: " + itemm.Username +
                                      "Games played: " + itemm.PlayedGames +
                                      "Wins: " + itemm.Wins +
                                      "Losses: " + itemm.Losses + "\r\n";

                                Console.WriteLine("Elo: " + itemm.Elo + " Username: " + itemm.Username +
                                      "Games played: " + itemm.PlayedGames +
                                      "Wins: " + itemm.Wins +
                                      "Losses: " + itemm.Losses);
                                
                            }
                          
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"],response);
                            closeSW = true;
                        }
                    }
                }
                //OPTIONAL: show transaction history from saved txt file
                if (splittedPath[1].Equals("transactions"))
                {
                    using (StreamReader r = File.OpenText("TransactionLog.txt"))
                    {
                        logger.DumpLog(r);
                    }
                }
                //WIP: check trading deals
                if (splittedPath[1].Equals("tradings"))
                {
                    bool closeSW = false;
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();

                   
                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token)
                        {
                            string response;
                            bool isEmpty;
                            isEmpty = checkIfColumnExists("trade_deals",username);
                            if (isEmpty)
                            {
                                response = "User " + username + " currently has no open trade deals!";
                                logger.LogToConsole("User " + username + " currently has no open trade deals!");
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"],response);
                            }
                            else if(!isEmpty)
                            {                             
                                cmd = new NpgsqlCommand("select id from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                string tradeId = cmd.ExecuteScalar().ToString();
                                cmd = new NpgsqlCommand("select card_to_trade_id from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                string cardToTradeId = cmd.ExecuteScalar().ToString();
                                cmd = new NpgsqlCommand("select card_to_trade_name from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                string cardToTradeName = cmd.ExecuteScalar().ToString();
                                cmd = new NpgsqlCommand("select card_to_trade_damage from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                int cardToTradeDamage = Convert.ToInt32( cmd.ExecuteScalar());
                                cmd = new NpgsqlCommand("select wanted_card_type from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                string wantedCartType = cmd.ExecuteScalar().ToString();
                                cmd = new NpgsqlCommand("select wanted_card_minimum_damage from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                int wantedCartMinDamage =Convert.ToInt32( cmd.ExecuteScalar());

                                logger.LogToConsole("User " + username + " has the following deal open: \r\n"
                                    + "Trade Id: " + tradeId + "\r\n"
                                    + "Card to trade ID: " + cardToTradeId + "\r\n"
                                    + "Card to trade Name: " + cardToTradeName + "\r\n"
                                    + "Card to trade Damage: " + cardToTradeDamage + "\r\n"
                                    + "Wanted Card Type: " + wantedCartType + "\r\n"
                                    + "Wanted Card Minimum Damage: " + wantedCartMinDamage + "\r\n");

                                response = "User " + username + " has the following deal open: \r\n"
                                    + "Trade Id: " + tradeId + "\r\n"
                                    + "Card to trade ID: " + cardToTradeId + "\r\n"
                                    + "Card to trade Name: " + cardToTradeName + "\r\n"
                                    + "Card to trade Damage: " + cardToTradeDamage + "\r\n"
                                    + "Wanted Card Type: " + wantedCartType + "\r\n"
                                    + "Wanted Card Minimum Damage: " + wantedCartMinDamage + "\r\n";

                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);
                            }
                            else
                            {
                                response = "User " + username + " currently has no open trade deals!";
                                logger.LogToConsole("User " + username + " currently has no open trade deals!");
                                sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], response);
                            }
                        }

                    }
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
                            string responseMsg = "User " + item.Username + " already exists. Choose another username!";
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                            closeSW = true;
                        }                     
                    }
                    if (closeSW == false)
                    {
                        cmd = new NpgsqlCommand("insert into users(username,password,name,bio,image,token,games,wins,losses,elo) " +
                            "values(@Username,@Password,null,null,null,@Token,@Games,@Wins,@Losses,@Elo)", con);
                        cmd.Parameters.AddWithValue("Username", acc.Username);
                        cmd.Parameters.AddWithValue("Password", acc.Password);
                        cmd.Parameters.AddWithValue("Token", acc.Username + "-mtcgToken");
                        cmd.Parameters.AddWithValue("Games", acc.PlayedGames);
                        cmd.Parameters.AddWithValue("Wins", acc.Wins);
                        cmd.Parameters.AddWithValue("Losses", acc.Losses);
                        cmd.Parameters.AddWithValue("Elo", acc.Elo);
                        cmd.ExecuteNonQuery();      
                        
                        
                        users.Add(acc);
                        logger.LogToConsole("User " + acc.Username + " succesfully created!");
                        string responseMsg = "User " + acc.Username + " succesfully created!";
                        sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                    }
                }
                //user login
                if (splittedPath[1].Equals("sessions"))
                {
                    bool closeSW = false;
                    string response;
                    User acc = JsonSerializer.Deserialize<User>(req.Payload);
                    
                    foreach (User item in users)
                    {
                        if (item.Username == acc.Username && item.Password == acc.Password)
                        {
                            response = "User " + acc.Username + " succesfully logged in!";
                            logger.LogToConsole("User " + acc.Username + " succesfully logged in!");
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);
                            closeSW = true;
                            token = acc.Username + "-mtcgToken";
                        }                      
                    }
                    if (closeSW == false)
                    {
                        response = "User does not exist or credentials are wrong!";
                        logger.LogToConsole("User does not exist or credentials are wrong!");
                        sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], response);
                    }

                }
                //admin creates package
                if (splittedPath[1].Equals("packages"))
                {
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    string tokenn = arr[1];
                    string response;
                    //token = headerlines 
                    if (tokenn == "admin-mtcgToken")
                    {
                        
                        List<Card> cardpackage = JsonSerializer.Deserialize<List<Card>>(req.Payload);
                        package pack = new package(cardpackage[0],cardpackage[1],cardpackage[2], cardpackage[3], cardpackage[4]);
                        packid++;
                        pack.PackageId = packid;
                        

                        cmd = new NpgsqlCommand("insert into packages(id,card1,card2,card3,card4,card5) " +
                            "values(@Id,@Card1,@Card2,@Card3,@Card4,@Card5)", con);
                        cmd.Parameters.AddWithValue("Id", packid);
                        cmd.Parameters.AddWithValue("Card1", cardpackage[0].Id);
                        cmd.Parameters.AddWithValue("Card2", cardpackage[1].Id);
                        cmd.Parameters.AddWithValue("Card3", cardpackage[2].Id);
                        cmd.Parameters.AddWithValue("Card4", cardpackage[3].Id);
                        cmd.Parameters.AddWithValue("Card5", cardpackage[4].Id);   
                        cmd.ExecuteNonQuery();

                        packages.Add(pack);
                        response = "package " + packid + " succesfully created!";
                        logger.LogToConsole("package " + packid + " succesfully created!");
                        sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);
                    }
                    else
                    {
                        response = "Not authorized to create package!";
                        logger.LogToConsole("Not authorized to create package!");
                        sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], response);
                    }

                }
                //user aquires packages
                if (splittedPath[1].Equals("transactions")&&splittedPath[2].Equals("packages"))
                {
                    bool closeSW = false;
                    string response;
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();

                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token) 
                        {
                            if (packages.Count != 0)
                            {
                                if (item.Coins >= 5)
                                {
                                    Random rand = new Random();
                                    int rando = rand.Next(0, packages.Count);

                                    item.Stack.Add(packages[rando].Card1);
                                    item.Stack.Add(packages[rando].Card2);
                                    item.Stack.Add(packages[rando].Card3);
                                    item.Stack.Add(packages[rando].Card4);
                                    item.Stack.Add(packages[rando].Card5);

                                    item.Coins -= 5;

                                    //OPTIONAL FOR TRANSACTION HISTORY --> GET
                                    using (StreamWriter w = File.AppendText("TransactionLog.txt"))
                                    {
                                        logger.LogToFile("User " + item.Username + "  aquired Package with following cards: \r\n"
                                          + packages[rando].Card1.Id + " " + packages[rando].Card1.Name + " " + packages[rando].Card1.Damage + "\r\n"
                                          + packages[rando].Card2.Id + " " + packages[rando].Card2.Name + " " + packages[rando].Card2.Damage + "\r\n"
                                          + packages[rando].Card3.Id + " " + packages[rando].Card3.Name + " " + packages[rando].Card3.Damage + "\r\n"
                                          + packages[rando].Card4.Id + " " + packages[rando].Card4.Name + " " + packages[rando].Card4.Damage + "\r\n"
                                          + packages[rando].Card5.Id + " " + packages[rando].Card5.Name + " " + packages[rando].Card5.Damage + "\r\n", w);
                                    }

                                    cmd = new NpgsqlCommand("delete from packages " +
                                        "where id = @Id", con);
                                    cmd.Parameters.AddWithValue("Id", packages[rando].PackageId);
                                    cmd.ExecuteNonQuery();

                                    packages.RemoveAt(rando);


                                    response = "User " + item.Username + " succesfully aquired Package";
                                    logger.LogToConsole("User " + item.Username + " succesfully aquired Package");
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);
                                    closeSW = true;
                                }
                                else
                                {
                                    response = "User " + item.Username + " doesn't have enough Coins!";
                                    logger.LogToConsole("User " + item.Username + " doesn't have enough Coins!");
                                    sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], response);
                                   

                                }
                            }
                            else
                            {
                                response = "User " + item.Username + " can't aquire packages because there are no packages left!";
                                logger.LogToConsole("User " + item.Username + " can't aquire packages because there are no packages left!");
                                sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], response);
                              

                            }
                        }
                    }
                
                }
                //WIP: battle procedure
                if (splittedPath[1].Equals("battles"))
                {
                    bool closeSW = false;
                    string response ="";
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();

                    //Placeholder approach but couldnt solve it in the end...
                    if (playercount == 2)
                    {
                        
                        foreach (User player1 in users)
                        {
                            foreach (User player2 in users)
                            {

                                if ((player1.Username == username && player1.Token == token) && (player2.Username == username && player2.Token == token))
                                {
                                    
                                    Game game = new Game();
                                    int roundCounter = 0;
                                    while (player1.Deck.Count != 0 || player2.Deck.Count != 0|| roundCounter < 100)
                                    {
                                        Random rand = new Random();
                                        int randoPlayer1 = rand.Next(0, player1.Deck.Count);
                                        rand = new Random();
                                        int randoPlayer2 = rand.Next(0, player2.Deck.Count);

                                        //choose random card from deck
                                        Card player1CurrentCard = player1.Deck[randoPlayer1];
                                        Card player2CurrentCard = player1.Deck[randoPlayer2];

                                        //set card element for both player cards
                                            //player1
                                        if(player1CurrentCard.Name.Contains("Water"))
                                        {
                                            player1CurrentCard.Element = ElementType.water;
                                        }
                                        if (player1CurrentCard.Name.Contains("Fire"))
                                        {
                                            player1CurrentCard.Element = ElementType.fire;
                                        }
                                        if (player1CurrentCard.Name.Contains("Normal"))
                                        {
                                            player1CurrentCard.Element = ElementType.normal;
                                        }
                                        if (player1CurrentCard.Name.Contains("Earth"))
                                        {
                                            player1CurrentCard.Element = ElementType.earth;
                                        }
                                            //player2
                                        if (player2CurrentCard.Name.Contains("Water"))
                                        {
                                            player2CurrentCard.Element = ElementType.water;
                                        }
                                        if (player2CurrentCard.Name.Contains("Fire"))
                                        {
                                            player2CurrentCard.Element = ElementType.fire;
                                        }
                                        if (player2CurrentCard.Name.Contains("Normal"))
                                        {
                                            player2CurrentCard.Element = ElementType.normal;
                                        }
                                        if (player2CurrentCard.Name.Contains("Earth"))
                                        {
                                            player1CurrentCard.Element = ElementType.earth;
                                        }
                                       
                                        //fight and return winner card
                                        Card winnercard = game.battle(player1CurrentCard, player2CurrentCard);
                                        response = "Player 1 card: " + player1CurrentCard.Name + " " + player1CurrentCard.Damage + " " + player1CurrentCard.Element
                                                + " vs " +
                                                "Player 2 card: " + player2CurrentCard.Name + " " + player2CurrentCard.Damage + " " + player2CurrentCard.Element+"\r\n";

                                        logger.LogToConsole(
                                                "Player 1 card: " + player1CurrentCard.Name + " " + player1CurrentCard.Damage + " " + player1CurrentCard.Element
                                                + " vs " +
                                                "Player 2 card: " + player2CurrentCard.Name + " " + player2CurrentCard.Damage + " " + player2CurrentCard.Element);


                                        //player1 round win
                                        if (player1.Deck.Contains(winnercard))
                                        {
                                            player1.Deck.Add(player2CurrentCard);
                                            player2.Deck.Remove(player2CurrentCard);
                                            response += "Player 1 won " + roundCounter + ". round and got enemies card\r\n";
                                            logger.LogToConsole("Player 1 won " + roundCounter + ". round and got enemies card");

                                        }
                                        //player2 round win
                                        else
                                        {
                                            player2.Deck.Add(player1CurrentCard);
                                            player1.Deck.Remove(player1CurrentCard);
                                            response += "Player 2 won " + roundCounter + ". round and got enemies card\r\n";
                                            logger.LogToConsole("Player 2 won " + roundCounter + ". round and got enemies card");
                                        }
                                        //deck card count of both players
                                        response += "Player 1 has: " + player1.Deck.Count + " cards left\r\n + Player 2 has: " + player2.Deck.Count + " cards left";
                                        logger.LogToConsole("Player 1 has: " + player1.Deck.Count +" cards left");
                                        logger.LogToConsole("Player 2 has: " + player2.Deck.Count + " cards left");

                                    }
                                    //player 1 game loose
                                    if(player1.Deck.Count == 0)
                                    {
                                        response += "Player 1 has lost the battle";
                                        logger.LogToConsole("Player 1 has lost the battle");
                                        player1.Elo -= 5;
                                        player2.Elo += 3;
                                    }
                                    //player 2 game loose
                                    else 
                                    {
                                        response += "Player 2 has lost the battle";
                                        logger.LogToConsole("Player 2 has lost the battle");
                                        player2.Elo -= 5;
                                        player1.Elo += 3;
                                    }
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);

                                }
                               

                            }
                        }
                    }
                    else
                    {
                        logger.LogToConsole("Insufficient Players to battle!");
                        response = "Insufficient Players to battle!";
                        sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], response);
                    }

                }
                //create deal
                if (splittedPath[1].Equals("tradings"))
                {
                    string response = "";
                    bool closeSW = false;
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();

                    TradeDeal deal = JsonSerializer.Deserialize<TradeDeal>(req.Payload);
                    

                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token)
                        {
                            //get the trading card from user stack 
                            Card tradingCard = new Card();
                            foreach (Card card in item.Stack)
                            {
                                if(card.Id == deal.CardToTrade)
                                {
                                    tradingCard = card;
                                }
                            }


                            cmd = new NpgsqlCommand("insert into trade_deals(id,card_to_trade_id,card_to_trade_name, card_to_trade_damage," +
                                "wanted_card_type,wanted_card_minimum_damage, requesting_user) " +
                                "values(@Id,@card_to_trade_id, @card_to_trade_name, @card_to_trade_damage, @wanted_card_type," +
                                "@wanted_card_minimum_damage,@RequestingUser)", con);
                            cmd.Parameters.AddWithValue("Id", deal.Id);
                            cmd.Parameters.AddWithValue("card_to_trade_id", tradingCard.Id);
                            cmd.Parameters.AddWithValue("card_to_trade_name", tradingCard.Name);
                            cmd.Parameters.AddWithValue("card_to_trade_damage", tradingCard.Damage);
                            cmd.Parameters.AddWithValue("wanted_card_type", deal.Type);
                            cmd.Parameters.AddWithValue("wanted_card_minimum_damage", deal.MinimumDamage);
                            cmd.Parameters.AddWithValue("RequestingUser", username);
                            cmd.ExecuteNonQuery();

                            tradeDeals.Add(deal);
                            for (int i = 0; i < item.Stack.Count; i++)
                            {
                                if(item.Stack[i].Id == deal.CardToTrade)
                                {
                                    item.Stack.Remove(item.Stack[i]);
                                }
                            }

                            response = "Trading deal " + deal.Id + " succesfully created";
                            logger.LogToConsole("Trading deal " + deal.Id+ " succesfully created");
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);
                        }
                        
                    }
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
                //Change User profile
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
                        string[] splittedtoken = token.Split("-");

                        User acc = JsonSerializer.Deserialize<User>(req.Payload);
                        if (splittedPath[2] == username && splittedtoken[0] == splittedPath[2])
                        {
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
                                    string responseMsg = "User data succesfully changed";
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                                    closeSW = true;
                                }

                            }
                        }
                        if(closeSW == false)
                        {
                            logger.LogToConsole("Not authorized to change profile!");
                            string responseMsg = "Not authorized to change profile!";
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], responseMsg);
                        }
                    }                                                   
                }
                //Change cards in deck
                if (splittedPath[1].Equals("deck"))
                {
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();
                    bool closeSW = false;
                    string response = "";

                    string[] cardIds = JsonSerializer.Deserialize<string[]>(req.Payload);
                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token) 
                        {                   
                            foreach (Card cardd in item.Stack)
                            {
                                if (cardIds.Length == 4)
                                {
                                    if (cardd.Id == cardIds[0] || cardd.Id == cardIds[1] || cardd.Id == cardIds[2] || cardd.Id == cardIds[3])
                                    {
                                        if (item.Deck.Count <= 4)
                                        {
                                            if (!item.Deck.Contains(cardd))
                                            {
                                                //db
                                                item.Deck.Add(cardd);
                                                response = "Card ID: " + cardd.Id + " was succesfully added to " + item.Username + "s deck \r\n";
                                                logger.LogToConsole("Card ID: " + cardd.Id + " was succesfully added to " + item.Username + "s deck \r\n");
                                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);
                                                closeSW = true;

                                            }
                                            else
                                            {
                                                response = "Card ID: " + cardd.Id + " already exists in deck. Can't put same card into deck twice!";
                                                logger.LogToConsole("Card ID: " + cardd.Id + " already exists in deck. Can't put same card into deck twice!");
                                                sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], response);
                                                
                                            }
                                        }
                                        else
                                        {
                                            response = "Deck can't have more than 4 cards!";
                                            logger.LogToConsole("Deck can't have more than 4 cards!");
                                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], response);
                                           
                                        }
                                    }
                                }
                                else if(closeSW == false)
                                {
                                    response = "Deck has only " + cardIds.Length + " cards. Deck must have exactly 4 cards!";
                                    logger.LogToConsole("Deck has only " + cardIds.Length + " cards. Deck must have exactly 4 cards!");
                                    sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"],response);
                                    closeSW = true;
                                }

                            }
                            
                        }
                    }
                }
            }
            if (req.Verb.Equals("DELETE"))
            {
                if (req == null)
                {
                    logger.LogToConsole("request wasn't executed properly. Request not found!");
                    sendResponse(sw, "404 NOT FOUND", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                }
                //delete sessions. Not needed?
                if (splittedPath[1].Equals("sessions"))
                {

                }
                if (splittedPath[1].Equals("tradings"))
                {
                    string response = "";
                    bool closeSW = false;
                    string[] arr = req.HeaderLines["Authorization"].Split(" ");
                    token = arr[1];
                    cmd = new NpgsqlCommand("select username from users where token=@Token", con);
                    cmd.Parameters.AddWithValue("Token", token);
                    string username = cmd.ExecuteScalar().ToString();

                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token)
                        {
                            //retrieve card to get back from db
                            cmd = new NpgsqlCommand("select card_to_trade_id from trade_deals where id='" + splittedPath[2]+"'", con);
                            string cardId = cmd.ExecuteScalar().ToString();
                            cmd = new NpgsqlCommand("select card_to_trade_name from trade_deals where id='" + splittedPath[2] + "'", con);
                            string cardName = cmd.ExecuteScalar().ToString();
                            cmd = new NpgsqlCommand("select card_to_trade_damage from trade_deals where id='" + splittedPath[2] + "'", con);
                            int cardDamage = Convert.ToInt32( cmd.ExecuteScalar());

                            //create card to get back
                            Card getBackCard = new Card();
                            getBackCard.Id = cardId;
                            getBackCard.Name = cardName;
                            getBackCard.Damage = cardDamage;

                            //add card to users stack again
                            item.Stack.Add(getBackCard);

                            //delete trade deal
                            cmd = new NpgsqlCommand("delete from trade_deals where id='" + splittedPath[2] + "'", con);                          
                            cmd.ExecuteNonQuery();

                            response = "Trading deal " + splittedPath[2] + " succesfully deleted";
                            logger.LogToConsole("Trading deal " + splittedPath[2] + " succesfully deleted");
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], response);
                        }
                    }
                }
            }
        }
        //method to send Response to Client --> gets used in doHTTPMethod
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

        private bool checkTableEmpty(string tablename)
        {
            cmd = new NpgsqlCommand("select count(*) from " + tablename, con);          
            int counter = Convert.ToInt32( cmd.ExecuteScalar());           
            if (counter > 0)
                return false;
            return true;
        }
        private bool checkIfColumnExists(string tablename, string username)
        {
            bool checkTableEmptyy = checkTableEmpty(tablename);
            if (!checkTableEmptyy)
            {
                cmd = new NpgsqlCommand("select count(1) from " + tablename + " where requesting_user = " + username, con);
                int counter;
                try
                {
                    counter = Convert.ToInt32(cmd.ExecuteScalar());
                    if (counter > 0)
                        return false;
                }
                catch (Exception)
                {
                    return false;
                }
               
            }
            return true;
        }
    }

}
