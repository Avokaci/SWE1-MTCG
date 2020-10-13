using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MonsterTradingCardGame.Lib
{
    public class Card
    {
        //instances
        protected string name;
        protected ElementType _element;

        //constructors
        public Card(ElementType element, string name)
        {
            this.Name = name;
            this._element = element;
        }

        //access modifiers
        public ElementType Element { get => _element; set => _element = value; }
        public string Name { get => name; set => name = value; }


        //methods



    }
}
