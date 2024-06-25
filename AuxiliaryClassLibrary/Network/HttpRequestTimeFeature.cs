using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Reference: https://stackoverflow.com/questions/50589179/get-the-time-of-request-in-asp-net-core-2-1
namespace AuxiliaryClassLibrary.Network
{
    public interface IHttpRequestTimeFeature
    {
        DateTime RequestTime { get; set; }
    }

    public class HttpRequestTimeFeature : IHttpRequestTimeFeature
    {
        public DateTime RequestTime { get; set; }

        public HttpRequestTimeFeature()
        {
            RequestTime = DateTime.Now;
        }
    }

}
