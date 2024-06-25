using APIConnector.Model;
using Microsoft.Extensions.Configuration;
using ConsoleTables;
using APIConnector;
using AuxiliaryClassLibrary.DateTimeHelper;
using AuxiliaryClassLibrary.IO;
using System.Text.Json;
using IPMonitoringTask.Models;

namespace IPMonitoringTask
{
    internal class MonitorTestRunner
    {
        public async static Task RunMonitor(IConfiguration config) 
        {
            string website = config["website"]!;
            string alert_email = config["alert"]!;
            double poll_interval = double.Parse(config["poll-interval"]!);
            double email_interval = double.Parse(config["email-interval"]!);
            string log_file_name = config["log-file-name"]!;
            NetworkAssignment3DelaySeedObj? request_body = config.GetSection("request-body").Get<NetworkAssignment3DelaySeedObj>()!; //config["request-body"];
            //string test_body = JsonSerializer.Serialize(request_body);

            // Create email agent instance:
            CourierEmailDemo.EmailSender email_sender = new CourierEmailDemo.EmailSender();
            //var response = await email_sender.sendEmail("zantgoron@gmail.com", website, "160", "get failure - Forbidden");
            //string result = await response.Content.ReadAsStringAsync();
            var input_obj = new[] { new KeyValuePair<string, string>("truncid", request_body.truncid) };
            // Configure Web API input:
            var cts = new CancellationTokenSource();
            RESTAPIInputModel GetWebsiteAPIInput = new RESTAPIInputModel
            {
                URL = website,
                ContentType = "application/x-www-form-urlencoded",
                APIType = (request_body == null)? RESTAPIType.GET : RESTAPIType.POST,
                InputObject = input_obj
            };



            // Run Monitoring Task:
            var ConsoleTable = new ConsoleTable("Website", "Time", "Note", "Latency (ms)");
            int poll_count = 0;
            DateTime last_email_time = DateTime.MinValue;
            int email_state = 0;
            await IPMonitoringTask.Monitor(async () => {

                // Send get request, compute latency:
                DateTime __request_start_time = DateTime.Now;
                var web_response = await RESTAPIConnector.CallRestAPIResponseAsync(GetWebsiteAPIInput);
                DateTime __request_response_time = DateTime.Now;
                double reqeust_latency = (__request_response_time - __request_start_time).TotalMilliseconds;

                string result = await web_response.Content.ReadAsStringAsync();
                if (!result.Contains("GOOD")) throw new Exception("Bad response!");
                string note = web_response.IsSuccessStatusCode ? $"get success - {web_response.StatusCode}" : $"get failure - {web_response.StatusCode}";
                
                if (web_response.IsSuccessStatusCode)
                {
                    IPMonitoringTask.UpdateEmailState(web_response.IsSuccessStatusCode, ref email_state, ref last_email_time);
                }
                ConsoleTable.AddRow(website, TimestampHelper.ToPreciseFormatString(__request_start_time), note, reqeust_latency);
                if (!String.IsNullOrEmpty(log_file_name)) CSVFileHelper.AddToFile(Path.Combine(new string[2] { "Log", log_file_name }), new string[1] { string.Format("{0},{1},{2},{3}", website, TimestampHelper.ToPreciseFormatString(__request_start_time), note, reqeust_latency.ToString()) });
                

                // If get request failed and last email sent was a long time ago, send another email reminder:
                DateTime __email_time = DateTime.Now;
                bool is_email_timeout_reached = (__email_time - last_email_time).TotalHours > (email_interval / 3600);
                string email_time_str = TimestampHelper.ToPreciseFormatString(__email_time);
                if (!web_response.IsSuccessStatusCode && is_email_timeout_reached)
                {
                    var email_response = await email_sender.sendEmail(alert_email, website, email_time_str, note);
                    if (email_response.IsSuccessStatusCode)
                    {
                        IPMonitoringTask.UpdateEmailState(web_response.IsSuccessStatusCode, ref email_state, ref last_email_time);
                        ConsoleTable.AddRow(website, email_time_str, $"sent email alert to {alert_email}", "");
                    }
                    else
                    {
                        ConsoleTable.AddRow(website, email_time_str, $"email alert to {alert_email} failed", "");
                    }
                    //string result = await response.Content.ReadAsStringAsync();

                }

                ++poll_count;

                ConsoleTable.Write();
                ConsoleTable.Rows.Clear();
                if (poll_count > 100)
                {
                    Console.Clear();
                    poll_count = 0;
                }


            }, poll_interval, cts.Token);
        } 
           
    }
}
