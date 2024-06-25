using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMonitoringTask
{
    internal class IPMonitoringTask
    {
        public static async Task Monitor(Action action, double seconds, CancellationToken token)
        {
            if (action == null)
                return;

            await Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                }
            }, token);
        }


        public static void UpdateEmailState(bool request_successful, ref int email_state, ref DateTime last_email_time) 
        {
            // Reset email state if request is successful:
            if (request_successful)
            {
                last_email_time = DateTime.MinValue;
                email_state = 0;
            }
            // Increment email state if request fails:
            else 
            {
                ++email_state;
                if (email_state == 2)
                {
                    last_email_time = DateTime.Now;
                    email_state = 0;
                }
            }
            
        }

    }
}
