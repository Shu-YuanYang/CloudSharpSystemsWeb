using CloudSharpSystemsWeb.Models;
using CloudSharpSystemsWeb.Network;
using Microsoft.AspNetCore.Mvc;

namespace CloudSharpSystemsWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<TemplateController> _logger;

        public WeatherForecastController(ILogger<TemplateController> logger, IConfiguration config)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        
        [HttpGet("GetPing")]
        public string GetPing()
        {

            
            return IPValidator.Ping_IPV4("3.18.199.243").ToString();
            
        }
        
    }
}