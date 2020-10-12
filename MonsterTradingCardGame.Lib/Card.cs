using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterTradingCardGame.Lib
{
    public class Card
    {
        //instances
        protected string _name;
        protected ElementType _element;

        //constructors
        public Card(ElementType element, string name)
        {
            this._name = name;
            this._element = element;
        }

        //access modifiers
        public ElementType Element { get => _element; set => _element = value; }

        //methods


    }
}
