using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Model
{
    public class RegisterResponseModel
    {
        public bool HasSucceeded { get; set; }

        public string Message { get; set; }

        public List<IdentityError> IdentityErrors { get; set; }
    }
}