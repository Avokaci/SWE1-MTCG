using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib
{
    public class Player
    {
        private string _userName;
        private string _password;
        private int _elo;
        private int _gamesPlayed;
        private int _coins;

        public string UserName { get => _userName; set => _userName = value; }
        public string Password { get => _password; set => _password = value; }
        public int Elo { get => _elo; set => _elo = value; }
        public int GamesPlayed { get => _gamesPlayed; set => _gamesPlayed = value; }
        public int Coins { get => _coins; set => _coins = value; }

        public void register(string username, string password)
        {
            _userName = username;
            _password = password;
            _elo = 100;
            _gamesPlayed = 0;
            _coins = 20;
        }
        public void login(string username, string password)
        {

        }
       
    }
}
