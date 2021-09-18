using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket
{
    public class Login
    {
        public string username { get; set; }
        public string password { get; set; }
        public Login() { }
        public Login(string _username, string psw)
        {
            username = _username;
            password = psw;
        }
    }
}
