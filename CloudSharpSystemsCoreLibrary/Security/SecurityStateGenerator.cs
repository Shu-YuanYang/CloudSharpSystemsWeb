using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloudSharpSystemsCoreLibrary.Security
{
    public class SecurityStateGenerator
    {
        public enum Salt { 
            Test = 0, 
            Byte32Base64 = 32,
            Byte64Base64 = 64
        }


        public static string GenerateRandomState(Salt salt) {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                var random_number = new byte[(int)salt];
                if (salt == Salt.Test) return "STATEGENERATETESTSHORTVERSION";

                rng.GetBytes(random_number);
                string state = Convert.ToBase64String(random_number);
                return state;
            }
        }


        public static string GenerateHashFromString(string text) {
            if (string.IsNullOrEmpty(text)) return String.Empty;

            byte[] byte_arr = System.Text.Encoding.UTF8.GetBytes(text);

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(byte_arr);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
            
        }

    }


}
