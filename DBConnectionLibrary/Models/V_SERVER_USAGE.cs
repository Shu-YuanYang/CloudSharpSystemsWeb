using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class V_SERVER_USAGE
    {
        /*
         S.SERIAL_NO, S.[STATUS] AS SERVER_STATUS, S.NET_LOAD_CAPACITY, S.SERVER_SPEC, S.CPU, S.RAM, S.STORAGE, S.REGISTRATION_DATE, S.LAST_SERVICE_DATE, S.OWNED_BY AS [SERVER_OWNED_BY], S.LOCATION_CODE,
		W.SITE_ID, W.APP_ID, W.DOMAIN_NAME, W.SITE_NAME, W.LOAD_BALANCING_ALGORITHM, W.IS_ENABLED AS [IS_SITE_ENABLED], W.OWNED_BY AS [SITE_OWNED_BY],
		H.HOST_IP, H.[PORT], H.[STATUS] AS HOST_STATUS, H.ERROR_MEASUREMENT_ALGORITHM, H.ERROR_RATE, H.MEASURED_TIME
         */
        public string? SERIAL_NO { get; set; }
        public string? SERVER_STATUS { get; set; }
        public int? NET_LOAD_CAPACITY { get; set; }
        public string? SERVER_SPEC { get; set; }
        public string? CPU { get; set; }
        public string? RAM { get; set; }
        public string? STORAGE { get; set; }
        public DateTime? REGISTRATION_DATE { get; set; }
        public DateTime? LAST_SERVICE_DATE { get; set; }
        public string? SERVER_OWNED_BY { get; set; }
        public string? LOCATION_CODE { get; set; }
        public string? SITE_ID { get; set; }
        public string? APP_ID { get; set; }
        public string? DOMAIN_NAME { get; set; }
        public string? SITE_NAME { get; set; }
        public string? LOAD_BALANCING_ALGORITHM { get; set; }
        public char? IS_SITE_ENABLED { get; set; }
        public string? SITE_OWNED_BY { get; set; }
        public string? HOST_IP { get; set; }
        public string? PORT { get; set; }
        public string? HOST_STATUS { get; set; }
        public string? ERROR_MEASUREMENT_ALGORITHM { get; set; }
        public double? ERROR_RATE { get; set; }
        public DateTime? MEASURED_TIME { get; set; }
    }
}
