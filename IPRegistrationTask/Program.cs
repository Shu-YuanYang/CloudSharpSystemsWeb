// See https://aka.ms/new-console-template for more information
using APIConnector.IPHelper;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


// Get configuration for DB connection string:
using IHost host = Host.CreateDefaultBuilder(args).Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
string connectionString = config["ConnectionStrings:DatabaseAppMainConnectionString"]!;


// Test IP and host name:
string IP = LocalIP.GetMyIP()!.Result!;
Console.WriteLine(IP);
Console.WriteLine(Environment.MachineName);


// Connect to DB:
var DBOptions = new DbContextOptionsBuilder<AppDBMainContext>()
    .UseSqlServer(connectionString)
    .Options;

using (var DBContext = new AppDBMainContext(DBOptions))
{
    // update IP
    NetworkWebsiteHostContext.UpdateComputerIP(DBContext, Environment.MachineName, IP).Wait();
}





