using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace Complaint_Hostel_system.Helper
{
    public static class TokenHelper
    {
        public static string GenerateToken(int size)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[size];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes)
                              .Replace("+", "")
                              .Replace("/", "")
                              .TrimEnd('=');
            }
        }
    }
}

