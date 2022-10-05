using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AEMWebApplication.Models
{
    public class Login
    {
        public Login(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public string username { get; set; }
        public string password { get; set; }
    }
}
