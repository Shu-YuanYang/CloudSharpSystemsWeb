// See https://aka.ms/new-console-template for more information
using APIConnector;
using APIConnector.IPHelper;
// AuxiliaryClassLibrary.DateTimeHelper;
using System.Threading;

Console.WriteLine("Hello, World!");

Console.WriteLine(System.Text.Encoding.Unicode.GetBytes("Hello, World!").Length);

string jsonstr = System.Text.Json.JsonSerializer.Serialize(new
{
    Encoding = "Seven",
    Token = "aAEGE32isehgoiseWasdihgaoheo#OGHOHOGHSEO",
    BitMap = new char[] { 'e', 'f', 's', 'L', 'c', 'c', 'a' },
    Data = new
    {
        Field1 = 5,
        Field2 = "description",
        Field3 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
    }
});
Console.WriteLine(jsonstr);
Console.WriteLine(System.Text.Encoding.Unicode.GetBytes(jsonstr).Length);

//DateTime current_time = DateTime.Now;
//string current_time_str = TimestampHelper.ToPreciseFormatString(current_time);
//DateTime time = TimestampHelper.ToPreciseFormatDateTime(current_time_str);



//string myIP = LocalIP.GetMyIP().Result!;
//Console.WriteLine(myIP);


//Console.Write(IPValidator.Ping_IPV4("10.18.1306.63"));

/*
RESTAPIConnector.TryCallRestAPI(new APIConnector.Model.RESTAPIInputModel
{
    URL = "https://6dvsvj47eujpghwa6xxxksnzi40oqxzl.lambda-url.us-east-2.on.aws/SupplyChain/get_test_data",
    ContentType = "application/json",
    Parameters = ""
}, true);
*/


var IPObj = await LocalIP.GetIPGeoLocation("44.228.224.21");
Console.WriteLine(IPObj.status);
Console.WriteLine(IPObj.lon);
Console.WriteLine(IPObj.lat);
Console.WriteLine(IPObj.region);
Console.WriteLine(IPObj.city);
Console.WriteLine(IPObj.AS);

