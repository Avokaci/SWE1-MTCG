using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Core
{
    public class Account
    {
        string _username;
        string _password;

        public Account()
        {
            
        }

        public string Username { get => _username; set => _username = value; }
        public string Password { get => _password; set => _password = value; }
    }
}
