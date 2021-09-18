using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket
{
    public class Registration : Player
    {
        public string username { get; set; }
        public string password { get; set; }
        public Registration() { }
        public Registration(string _username, string psw, string _name, string _surname, string pos, float ht, float wt, int _age, string _city, string? _team)
        {
            username = _username;
            password = psw;
            name = _name;
            surname = _surname;
            position = pos;
            height = ht;
            weight = wt;
            age = _age;
            city = _city;
            team = _team;
        }
    }
}
