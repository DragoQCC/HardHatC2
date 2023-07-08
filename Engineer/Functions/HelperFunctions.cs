using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Functions
{
    public class HelperFunctions
    {

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rand = new Random();

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rand.Next(s.Length)])
                .ToArray());
        }

        public static bool NT_SUCCESS(int status)
        {
            return (status >= 0);
        }
    }
}
