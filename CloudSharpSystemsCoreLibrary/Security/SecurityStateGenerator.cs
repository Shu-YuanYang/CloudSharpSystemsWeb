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


        public static string GenerateRandomCode(int code_length) {
			Random random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var codes = new char[code_length];
            for (int i = 0; i < code_length; ++i) codes[i] = chars[random.Next(chars.Length)];
			return new string(codes);
		}

        public static string GenerateRandomState(Salt salt) {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                var random_number = new byte[(int)salt];
                if (salt == Salt.Test) return "STATEGENERATETESTSHORTVERSION";

                rng.GetBytes(random_number);
                string state = Base64UrlEncodeCompact(random_number); //Convert.ToBase64String(random_number);
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


		private static string Base64UrlEncodeCompact(byte[] data) =>
		Convert.ToBase64String(data)
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		public static string GenerateAuthorizationCode(int size) {
			using var rng = RandomNumberGenerator.Create();
			var randomBytes = new byte[size];
			rng.GetBytes(randomBytes);
			var verifier = Base64UrlEncodeCompact(randomBytes);

			var buffer = Encoding.UTF8.GetBytes(verifier);
			var hash = SHA256.Create().ComputeHash(buffer);
			var code = Base64UrlEncodeCompact(hash);

            return code;
		}

        public static string GenerateAuthenticationToken() {
			byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
			byte[] key = Guid.NewGuid().ToByteArray();
			string authentication_token = Convert.ToBase64String(time.Concat(key).ToArray());
            return authentication_token;
        }


	}


}
