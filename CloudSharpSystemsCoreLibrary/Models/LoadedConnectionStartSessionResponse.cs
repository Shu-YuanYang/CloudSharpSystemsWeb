namespace CloudSharpLimitedCentralWeb.Models
{
    public class LoadedConnectionStartSessionResponse
    {
        public string? client_ip { get; set; }
		public string? thread_id { get; set; }
		public string? client_location { get; set; }
		public DateTime client_requested_time { get; set; }
		public string? server_host_ip { get; set; }
		public string? server_location { get; set; }
		public LoadedConnectionStartSessionConnectionData? connection_data { get; set; }
		public double processing_delay { get; set; }
		public string? message { get; set; }

	}
}
