using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Model
{
    public class LoginResultModel
    {
        public bool HasSucceeded { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
    }
}