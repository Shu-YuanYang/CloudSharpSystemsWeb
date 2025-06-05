using System.Text;

namespace AuxiliaryClassLibrary.Functions
{
	public class EncodingHelper
	{
		public static string UTF8toHex(string text)
		{
			// Convert key (recipient) to hex
			byte[] ba = Encoding.Default.GetBytes(text);
			var hexString = BitConverter.ToString(ba);
			hexString = hexString.Replace("-", "").ToLower();
			return hexString;
		}

	}
}
