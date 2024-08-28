using APIConnector.Model;
using APIConnector;
using AuxiliaryClassLibrary.DateTimeHelper;
using Microsoft.Extensions.Configuration;
using ConsoleTables;
using DBConnectionLibrary;
using Microsoft.EntityFrameworkCore;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using CloudSharpSystemsCoreLibrary.Models;
using CloudSharpSystemsWeb.Network;
using DBConnectionLibrary.Models.Mongo;
using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver;
using DBConnectionLibrary.DBObjectContexts.Mongo;

namespace IPMonitoringTask
{
    internal class MonitorRunner
    {
        private static string __program_name = "";
        public static string PROGRAM_NAME { get => __program_name; }


        private static Dictionary<string, List<SystemHealthTraceRecord>> __system_health_record_dict = new Dictionary<string, List<SystemHealthTraceRecord>>();



        public async static Task RunMonitor(IConfiguration config)
        {
            __program_name = config["program-name"]!;
            double poll_interval = double.Parse(config["poll-interval"]!);
            List<string> MonitoredSiteList = config.GetSection("SiteConfig").Get<List<string>>()!;
            string monitored_route = config["monitor-route"]!;
            string dbmain_connectionString = config["ConnectionStrings:DatabaseAppMainConnectionString"]!;
            string dbmain_instance_id = config["ConnectionStrings:DatabaseAppMainInstanceID"]!;
            string dbmongo_connectionString = config["ConnectionStrings:DatabaseAppMongoConnectionString"]!;
            string dbmongo_instance_id = config["ConnectionStrings:DatabaseAppMongoInstanceID"]!;


            // Connect to DB:
            var DBOptions = new DbContextOptionsBuilder<AppDBMainContext>()
                .UseSqlServer(dbmain_connectionString)
                .Options;

            

            // Get website list to monitor:
            List<TB_WEBSITE_HOST> hosts = new List<TB_WEBSITE_HOST>();
            TB_WEBSITE_HOST DBMain_host, DBMongo_host;
            using (var DBMainContext = new AppDBMainContext(DBOptions, null))
            {
                foreach (string site_ID in MonitoredSiteList) 
                {
                    var sub_lst = await NetworkWebsiteHostContext.GetWebsiteHostsBySiteID(DBMainContext, site_ID);
                    hosts.AddRange(sub_lst);
                }
                DBMain_host = await NetworkWebsiteHostContext.GetWebsiteHostDetailByIP(DBMainContext, "", dbmain_instance_id);
                DBMongo_host = await NetworkWebsiteHostContext.GetWebsiteHostDetailByIP(DBMainContext, "", dbmongo_instance_id);
            }

            
            

            // Monitor the app database:
            //var dbmain_task = RunTasksForDBMain(DBOptions, DBMain_host, poll_interval * 2); // Shu-Yuan Yang 20240827 temporarily disabled
            var dbmongo_task = RunTasksForDBMongo(dbmongo_connectionString, DBMongo_host, poll_interval * 2);

            // Monitor each website host:
            List<Task> running_tasks = new List<Task>();
            foreach (TB_WEBSITE_HOST host in hosts) 
            {
                var task = RunTasksForHost(host, monitored_route, poll_interval);
                running_tasks.Add(task);
            }

            await RunDBUpdateTask(DBOptions, poll_interval / 2); 

        }


        private async static Task RunTasksForDBMain(DbContextOptions<AppDBMainContext> DBOptions, TB_WEBSITE_HOST host_record, double poll_interval) {
            var cts = new CancellationTokenSource();
            await IPMonitoringTask.Monitor(async () => {

                double latency = -1;
                bool is_query_successful = false;
                string error_message = "";
                TB_USER_SESSION? recent_session = null;
                try {
                    using (var DBContext = new AppDBMainContext(DBOptions, null))
                    {
                        DateTime start_time = DateTime.Now;
                        recent_session = await NetworkUserSessionContext.GetMostRecentUserSession(DBContext);
                        DateTime end_time = DateTime.Now;
                        latency = (end_time - start_time).TotalMilliseconds;
                        is_query_successful = true;
                    }
                }
                catch (Exception ex) {
                    is_query_successful = false;
                    error_message = ex.Message.Substring(0, Math.Min(ex.Message.Length, 450));
                }
                

                if (!__system_health_record_dict.ContainsKey(host_record.HOST_IP!)) __system_health_record_dict[host_record.HOST_IP!] = new List<SystemHealthTraceRecord>();
                __system_health_record_dict[host_record.HOST_IP!].Add(
                    new SystemHealthTraceRecord
                    {
                        host_IP = host_record.HOST_IP,
                        port = host_record.PORT,
                        system_status = is_query_successful? "NORMAL" : "ERROR",
                        trace_ID = PROGRAM_NAME + "_" + host_record.HOST_IP + "_" + TimestampHelper.ToTimeStampIDFormatString(DateTime.Now),
                        message = is_query_successful? $"query success - {recent_session?.SESSION_ID}" : $"query failure - {error_message}",
                        recorded_by = PROGRAM_NAME,
                        latency = latency
                    }
                );

            }, poll_interval, cts.Token);
        }


        private async static Task RunTasksForDBMongo(string db_connection_string, TB_WEBSITE_HOST host_record, double poll_interval)
        {
            var cts = new CancellationTokenSource();
            await IPMonitoringTask.Monitor(async () => {

                double latency = -1;
                bool is_query_successful = false;
                string error_message = "";
                try
                {
                    AppDBMongoContext DBContext = new AppDBMongoContext(db_connection_string);
                    //DateTime start_time = DateTime.Now; // Shu-Yuan Yang 20240827 temporarily disabled pinging.
                    //var result_doc = DBContext.PingDatabases();
                    //DateTime end_time = DateTime.Now;
                    await TeamNoteContext.ClearOldTeamNotes(DBContext, 24, PROGRAM_NAME);
                    //latency = (end_time - start_time).TotalMilliseconds;
                    is_query_successful = true;
                }
                catch (Exception ex)
                {
                    is_query_successful = false;
                    error_message = ex.Message.Substring(0, Math.Min(ex.Message.Length, 450));
                }


                if (!__system_health_record_dict.ContainsKey(host_record.HOST_IP!)) __system_health_record_dict[host_record.HOST_IP!] = new List<SystemHealthTraceRecord>();
                __system_health_record_dict[host_record.HOST_IP!].Add(
                    new SystemHealthTraceRecord
                    {
                        host_IP = host_record.HOST_IP,
                        port = host_record.PORT,
                        system_status = is_query_successful ? "NORMAL" : "ERROR",
                        trace_ID = PROGRAM_NAME + "_" + host_record.HOST_IP + "_" + TimestampHelper.ToTimeStampIDFormatString(DateTime.Now),
                        message = is_query_successful ? $"Note clearing success" : $"Note clearing failure - {error_message}",
                        recorded_by = PROGRAM_NAME,
                        latency = latency
                    }
                );

            }, poll_interval, cts.Token);
        }

        private async static Task RunTasksForHost(TB_WEBSITE_HOST host_record, string monitored_route, double poll_interval) 
        {

            // Configure Web API input:
            var cts = new CancellationTokenSource();
            string url = RESTAPIConnector.ConstructRESTBaseURL(host_record.HOST_IP!) + monitored_route;
            RESTAPIInputModel GetWebsiteAPIInput = new RESTAPIInputModel
            {
                URL = url,
                ContentType = "application/json",
                APIType = RESTAPIType.GET
            };
            
            // Run Monitoring Task:
            await IPMonitoringTask.Monitor(async () => {

                // If ping fails, then the system is unresponsive:
                DateTime start_time = DateTime.Now;
                var ping_result = IPValidator.Ping_IPV4(host_record.HOST_IP!);
                DateTime end_time = DateTime.Now;
                double latency = (end_time - start_time).TotalMilliseconds;
                if (ping_result != System.Net.NetworkInformation.IPStatus.Success) 
                {
                    if (!__system_health_record_dict.ContainsKey(host_record.HOST_IP!)) __system_health_record_dict[host_record.HOST_IP!] = new List<SystemHealthTraceRecord>();
                    __system_health_record_dict[host_record.HOST_IP!].Add(
                        new SystemHealthTraceRecord
                        {
                            host_IP = host_record.HOST_IP,
                            port = host_record.PORT,
                            system_status = "UNRESPONSIVE",
                            trace_ID = PROGRAM_NAME + "_" + host_record.HOST_IP + "_" + TimestampHelper.ToTimeStampIDFormatString(DateTime.Now),
                            message = "Host is unreachable.",
                            recorded_by = PROGRAM_NAME,
                            latency = latency
                        }
                    );
                    return;
                }


                // Send get request
                start_time = DateTime.Now;
                var web_response = await RESTAPIConnector.CallRestAPIResponseAsync(GetWebsiteAPIInput);
                end_time = DateTime.Now;
                latency = (end_time - start_time).TotalMilliseconds;
                if (!__system_health_record_dict.ContainsKey(host_record.HOST_IP!)) __system_health_record_dict[host_record.HOST_IP!] = new List<SystemHealthTraceRecord>();
                __system_health_record_dict[host_record.HOST_IP!].Add(
                    new SystemHealthTraceRecord
                    {
                        host_IP = host_record.HOST_IP,
                        port = host_record.PORT,
                        system_status = web_response.IsSuccessStatusCode ? "NORMAL" : "ERROR",
                        trace_ID = PROGRAM_NAME + "_" + host_record.HOST_IP + "_" + TimestampHelper.ToTimeStampIDFormatString(DateTime.Now),
                        message = web_response.IsSuccessStatusCode ? $"get success - {web_response.StatusCode}" : $"get failure - {web_response.StatusCode}",
                        recorded_by = PROGRAM_NAME,
                        latency = latency
                    }
                );
            }, poll_interval, cts.Token);

        }


        private async static Task RunDBUpdateTask(DbContextOptions<AppDBMainContext> DBOptions, double poll_interval)
        {
            var ConsoleTable = new ConsoleTable("Host IP", "Port", "Status", "Trace ID", "Latency (ms)", "Message");
            int poll_count = 0;

            // Run Monitoring Task:
            var cts = new CancellationTokenSource();
            await Task.Delay(TimeSpan.FromSeconds(poll_interval));
            await IPMonitoringTask.Monitor(async () => {

                ++poll_count;

                using (var DBContext = new AppDBMainContext(DBOptions, null))
                {
                    var copied_dict = __system_health_record_dict.Select(IP_record_pair => new KeyValuePair<string, List<SystemHealthTraceRecord> >(IP_record_pair.Key, IP_record_pair.Value));
                    foreach (var IP_record_pair in copied_dict)
                    {
                        foreach (SystemHealthTraceRecord record in IP_record_pair.Value)
                        {
                            ConsoleTable.AddRow(record.host_IP!, record.port!, record.system_status!, record.trace_ID!, record.latency, record.message!);
                            //Console.WriteLine($"{record.host_IP!}\t\t{record.port!}\t\t{record.system_status!}\t\t{record.trace_ID!}\t\t{record.message!}\t\t{record.recorded_by!}");
                            // await NetworkWebsiteHostContext.UpdateHostStatus(DBContext, record.host_IP!, record.port!, record.system_status!, record.trace_ID!, record.message!, record.recorded_by!, record.latency); // Shu-Yuan Yang 20240827 temporarily disabled
                        }
                        //Console.WriteLine();
                        __system_health_record_dict.Remove(IP_record_pair.Key);
                    }
                }

                if (ConsoleTable.Rows.Any())
                {
                    ConsoleTable.Write();
                    ConsoleTable.Rows.Clear();
                }
                if (poll_count > 200)
                {
                    Console.Clear();
                    poll_count = 0;
                }

            }, poll_interval, cts.Token);

        }



    }
}
