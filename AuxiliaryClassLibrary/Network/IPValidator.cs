using System.Net.NetworkInformation;

namespace CloudSharpSystemsWeb.Network
{
    public class IPValidator
    {
        // Reference: https://zetcode.com/csharp/ping/
        public static IPStatus Ping_IPV4(string IPV4_address) {
            string url = IPV4_address;

            using var ping = new Ping();
            PingReply res;
            try {
                res = ping.Send(url);
            } catch {
                return IPStatus.Unknown;
            }

            return res.Status;
        }

        public static void ValidateForwardedNetworkRequestIP(string forwarded_message_IP, string hostIP) {
            if (!forwarded_message_IP.Equals(hostIP))
            {
                throw new Exception($"Request not allowed! Target IP of the request <{forwarded_message_IP}> does not match the host IP. The sender request was intercepted and blocked.");
            }
        }

    }
}
