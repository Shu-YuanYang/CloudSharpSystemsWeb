// See https://aka.ms/new-console-template for more information
using APIConnector;
using APIConnector.Model;
using AuxiliaryClassLibrary.DateTimeHelper;
using AuxiliaryClassLibrary.IO;
using IPMonitoringTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;


// Get configuration for DB connection string:
using IHost host = Host.CreateDefaultBuilder(args).Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
await MonitorRunner.RunMonitor(config);
/*
try
{
    await MonitorRunner.RunMonitor(config);
}
catch 
{
    Environment.Exit(-1);
}
*/
