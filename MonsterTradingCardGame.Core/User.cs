﻿using MonsterTradingCardGame.Lib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Core
{
    public class User
    {
        //credentials and profile information
        string _username;
        string _password;
        string _name;
        string _bio;
        string _image;
        string _token;
        int _playedGames;
        int _wins;
        int _losses;
        int _elo;

        int _coins;
        List<Card> _stack;
        List<Card> _deck;
        public User()
        {
            Coins = 20;
            _playedGames = 0;
            _wins = 0;
            _losses = 0;
            _elo = 100;
            Stack = new List<Card>();
            Deck = new List<Card>();
        }

        public string Username { get => _username; set => _username = value; }
        public string Password { get => _password; set => _password = value; }
        public string Name { get => _name; set => _name = value; }
        public string Bio { get => _bio; set => _bio = value; }
        public string Image { get => _image; set => _image = value; }
        public List<Card> Stack { get => _stack; set => _stack = value; }
        public List<Card> Deck { get => _deck; set => _deck = value; }
        public int Coins { get => _coins; set => _coins = value; }
        public string Token { get => _token; set => _token = value; }
        public int PlayedGames { get => _playedGames; set => _playedGames = value; }
        public int Wins { get => _wins; set => _wins = value; }
        public int Losses { get => _losses; set => _losses = value; }
        public int Elo { get => _elo; set => _elo = value; }
    }
}
