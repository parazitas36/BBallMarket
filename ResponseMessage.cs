using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BBallMarket
{
    public class ResponseMessage
    {
        public string type { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public ResponseMessage(string _type, string _code, string _msg)
        {
            type = _type;
            code = _code;
            message = _msg;
        }
    }
}
