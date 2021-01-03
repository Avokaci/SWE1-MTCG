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
                                Card_To_Trade VARCHAR(255),
                                Type VARCHAR(255), 
                                Minimum_Damage int, 
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
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                    closeSW = true;
                                }
                            }
                        }
                        if (closeSW == false)
                        {
                            logger.LogToConsole("Not authorized to show profile!");
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                            logger.LogToConsole("User " + item.Username + " has the following cards in his deck: \r\n");
                            foreach (Card cardd in item.Deck)
                            {
                                logger.LogToConsole("Card ID: " + cardd.Id + "\r\nCard Name: " + cardd.Name + "\r\nCard Damage: " + cardd.Damage + "\r\n");
                            }
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                            logger.LogToConsole("User " + item.Username + " has the following cards in his deck (plain view): \r\n");
                            foreach (Card cardd in item.Deck)
                            {
                                logger.LogToConsole("Card ID: " + cardd.Id + "\r\n");
                            }
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                            
                            logger.LogToConsole("Stats for user " + item.Username + " : \r\n"  + 
                                "Games played: " + item.PlayedGames + "\r\n"+
                                "Wins: " + item.Wins + "\r\n" +
                                "Losses: " + item.Losses + "\r\n" +
                                "Elo: " + item.Elo + "\r\n" +
                                "Win/Lose ratio: " + wlRatio); //OPTIONAL win lose ratio
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                            logger.LogToConsole("Scoreboard: \r\n");
                            foreach (User itemm in users)
                            {
                                Console.WriteLine("Elo: " + itemm.Elo + " Username: " + itemm.Username +
                                      "Games played: " + itemm.PlayedGames +
                                      "Wins: " + itemm.Wins +
                                      "Losses: " + itemm.Losses);
                            }
                          
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                            bool isEmpty;
                            isEmpty = checkTableEmpty("trade_deals");
                            if (isEmpty)
                            {
                                logger.LogToConsole("User " + username + " currently has no open trade deals!");
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                            }
                            else
                            {
                              
                                cmd = new NpgsqlCommand("select id from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                string id = cmd.ExecuteScalar().ToString();
                                cmd = new NpgsqlCommand("select card_to_trade from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                string cardToTrade = cmd.ExecuteScalar().ToString();
                                cmd = new NpgsqlCommand("select Type from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                string type = cmd.ExecuteScalar().ToString();
                                cmd = new NpgsqlCommand("select minimum_damage from trade_deals where requesting_user=@RequestingUser", con);
                                cmd.Parameters.AddWithValue("RequestingUser", username);
                                int minimumDamage = (int)cmd.ExecuteScalar();
                                cmd.ExecuteNonQuery();

                                logger.LogToConsole("User " + username + " has the following deal open: \r\n"
                                    + "ID: " + id + "\r\n"
                                    + "Card to trade: " + cardToTrade + "\r\n"
                                    + "Type: " + type + "\r\n"
                                    + "Minimum damage: " + minimumDamage + "\r\n");
                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                        logger.LogToConsole("package " + packid + " succesfully created!");
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
                //WIP: battle procedure
                if (splittedPath[1].Equals("battles"))
                {
                    bool closeSW = false;
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

                                        logger.LogToConsole(
                                                "Player 1 card: " + player1CurrentCard.Name + " " + player1CurrentCard.Damage + " " + player1CurrentCard.Element
                                                + " vs " +
                                                "Player 2 card: " + player2CurrentCard.Name + " " + player2CurrentCard.Damage + " " + player2CurrentCard.Element);


                                        //player1 round win
                                        if (player1.Deck.Contains(winnercard))
                                        {
                                            player1.Deck.Add(player2CurrentCard);
                                            player2.Deck.Remove(player2CurrentCard);
                                            logger.LogToConsole("Player 1 won " + roundCounter + ". round and got enemies card");

                                        }
                                        //player2 round win
                                        else
                                        {
                                            player2.Deck.Add(player1CurrentCard);
                                            player1.Deck.Remove(player1CurrentCard);
                                            logger.LogToConsole("Player 2 won " + roundCounter + ". round and got enemies card");
                                        }
                                        //deck card count of both players
                                        logger.LogToConsole("Player 1 has: " + player1.Deck.Count +" cards left");
                                        logger.LogToConsole("Player 2 has: " + player2.Deck.Count + " cards left");

                                    }
                                    //player 1 game loose
                                    if(player1.Deck.Count == 0)
                                    {
                                        logger.LogToConsole("Player 1 has lost the battle");
                                        player1.Elo -= 5;
                                        player2.Elo += 3;
                                    }
                                    //player 2 game loose
                                    if (player2.Deck.Count == 0)
                                    {
                                        logger.LogToConsole("Player 2 has lost the battle");
                                        player2.Elo -= 5;
                                        player1.Elo += 3;
                                    }
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);

                                }

                            }
                        }
                    }
                    else
                    {
                        logger.LogToConsole("Insufficient Players to battle!");
                        sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                    }

                }

                //WIP: initiate trading process
                if (splittedPath[1].Equals("tradings"))
                {
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
                            cmd = new NpgsqlCommand("insert into trade_deals(id,card_to_trade,type,minimum_damage, requesting_user) " +
                                "values(@Id,@CardToTrade,@Type,@MinimumDamage,@RequestingUser)", con);
                            cmd.Parameters.AddWithValue("Id", deal.Id);
                            cmd.Parameters.AddWithValue("CardToTrade", deal.CardToTrade);
                            cmd.Parameters.AddWithValue("Type", deal.Type);
                            cmd.Parameters.AddWithValue("MinimumDamage", deal.MinimumDamage);
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
                           
                            logger.LogToConsole("Trading deal " + deal.Id+ " succesfully created");
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                                    sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                    closeSW = true;
                                }

                            }
                        }
                        if(closeSW == false)
                        {
                            logger.LogToConsole("Not authorized to change profile!");
                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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

                    string[] cardIds = JsonSerializer.Deserialize<string[]>(req.Payload);
                    foreach (User item in users)
                    {
                        if (item.Username == username && item.Token == token) //validation not 100% correct
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
                                                logger.LogToConsole("Card ID: " + cardd.Id + " was succesfully added to " + item.Username + "s deck \r\n");
                                                sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                                closeSW = true;

                                            }
                                            else
                                            {
                                                logger.LogToConsole("Card ID: " + cardd.Id + " already exists in deck. Can't put same card into deck twice!");
                                                sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                                
                                            }
                                        }
                                        else
                                        {
                                            logger.LogToConsole("Deck can't have more than 4 cards!");
                                            sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
                                           
                                        }
                                    }
                                }
                                else if(closeSW == false)
                                {
                                    logger.LogToConsole("Deck has only " + cardIds.Length + " cards. Deck must have exactly 4 cards!");
                                    sendResponse(sw, "405 METHOD NOT ALLOWED", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
                            //user gets his card back
                            //cmd = new NpgsqlCommand("select cart_to_trade from trade_deals where id=" + splittedPath[2], con);
                            //string cardId = cmd.ExecuteScalar().ToString();
                            //foreach (TradeDeal trade in tradeDeals)
                            //{
                            //    if(trade.Id == splittedPath[2])
                            //    {
                            //        item.Stack.Add(trade.CardToTrade);
                            //    }
                            //}

                            cmd = new NpgsqlCommand("delete from trade_deals where id="+splittedPath[2], con);                          
                            cmd.ExecuteNonQuery();

                            logger.LogToConsole("Trading deal " + splittedPath[2] + " succesfully deleted");
                            sendResponse(sw, "201 Created", req.HttpVersion, req.HeaderLines["Host"], req.Payload);
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
            //sw.Close();
            //sw.Dispose();
        }

        private bool checkTableEmpty(string tablename)
        {
            cmd = new NpgsqlCommand("select count(*) from " + tablename, con);          
            int counter = Convert.ToInt32( cmd.ExecuteScalar());           
            if (counter > 0)
                return false;
            return true;
        }
        private bool checkTableEmptyForUser(string tablename, string username)
        {
            bool checkTableEmptyy = checkTableEmpty(tablename);
            if (!checkTableEmptyy)
            {
                cmd = new NpgsqlCommand("select count(*) from " + tablename + " where requesting_user = " + username, con);
                int counter = Convert.ToInt32(cmd.ExecuteScalar());
                if (counter > 0)
                    return false;
            }
            return true;
        }
    }

}
