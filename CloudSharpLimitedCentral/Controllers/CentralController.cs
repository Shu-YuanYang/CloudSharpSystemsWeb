using APIConnector;
using APIConnector.GoogleCloud;
using APIConnector.Model;
using AuxiliaryClassLibrary.DateTimeHelper;
using AuxiliaryClassLibrary.Network;
using Azure;
using Azure.Core;
using CloudSharpLimitedCentralWeb.Models;
using CloudSharpSystemsCoreLibrary.Models;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace CloudSharpLimitedCentral.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CentralController : ControllerBase
    {
        public const string SITE_CONFIG_KEY = "SiteConfig";


        protected readonly ILogger<TestController> _logger;
        protected readonly AppDBMainContext _app_db_main_context;
        protected readonly IConfiguration _configuration;

        private readonly string _SITE_ID;

        public CentralController(ILogger<TestController> logger, IConfiguration config, AppDBMainContext appDBMainContext)
        {
            _logger = logger;
            _app_db_main_context = appDBMainContext;
            _configuration = config;

            _SITE_ID = config[TestController.SITE_CONFIG_KEY + ":" + this.GetType().Name]!;
        }





        [HttpPost("update_task_statuses")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> UpdateTaskStatuses(string app_id, string program_name/*, [FromBody] TestSessionResourceResetConfig resource_configuration*/) {

            string severity = "GOOD";
            string program_type = "TASK";

            await DBTransactionContext.DBTransact(this._app_db_main_context, async (context, transaction) =>
            {
                var log = await AppDataContext.WriteSystemLog(context, new TB_CENTRAL_SYSTEM_LOG { 
                    APP_ID = app_id,
                    SYSTEM_NAME = this._configuration["HostConfig"]!,
                    TRACE_ID = "(UNAVAILABLE)",
                    RECORD_TYPE = severity,
                    RECORD_KEY = program_type,
                    RECORD_VALUE1 = $"program: {program_name}",
                    RECORD_VALUE2 = $"logName: projects/{app_id}/logs/{program_name}",
                    RECORD_VALUE3 = "",
                    RECORD_VALUE4 = $"severity: {severity}",
                    RECORD_VALUE5 = "details: see Google Cloud Logging (find latest log by logName in column RECORD_VALUE2)",
                    RECORD_MESSAGE = "message: see Google Cloud Logging (find latest log by logName in column RECORD_VALUE2)",
                    RECORD_NOTE = "Cloud Scheduler job successfully triggered.",
                    EDIT_BY = program_name
                });

                var status_record = await AppDataContext.UpdateProgramStatus(context, new TB_PROGRAM_STATUS {
                    PROGRAM_ID = program_name,
                    APP_ID = app_id,
                    LAST_TRACE_ID = "(UNAVAILABLE)",
                    PROGRAM_STATUS = severity,
                    LAST_LOG_TIME = log.EDIT_TIME,
                    NOTES = log.RECORD_NOTE,
                    EDIT_BY = program_name
                });

                await AppDataContext.UpdateTaskStatuses(context, app_id, program_type, program_name);
            });

            return new GeneralAPIResponse { Status = "Success", Message = "Task statuses updated!" }; ;
        }

    }
}
