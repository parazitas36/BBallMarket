using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket.Data.Entities
{
    public class Account
    {
        public int id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string? email { get; set; }
        public Account() { }
        public Account(int _id, string _username, string _password, string? _email)
        {
            id = _id;
            username = _username;
            password = _password;
            email = _email;
        }
    }
}
