// See https://aka.ms/new-console-template for more information
// Powershell execution references: https://code-maze.com/csharp-run-powershell-script/
using System.Diagnostics;
using System.Net;

try
{
    Console.WriteLine("Hello?");
    var processStartInfo = new ProcessStartInfo();
    processStartInfo.FileName = "powershell.exe";
    //processStartInfo.Arguments = $"-Command \"ls\"";
    string command = "Get-DnsServerResourceRecord -ZoneName \"cloudsharp.com\"";
    processStartInfo.Arguments = $"-Command \"{command}\"";
    processStartInfo.UseShellExecute = false;
    processStartInfo.RedirectStandardOutput = true;

    using var process = new Process();
    process.StartInfo = processStartInfo;
    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    Console.WriteLine(output);
    var name = Console.ReadLine();
}
catch (Exception ex) 
{
    Console.WriteLine(ex.Message);
    var name = Console.ReadLine();
}