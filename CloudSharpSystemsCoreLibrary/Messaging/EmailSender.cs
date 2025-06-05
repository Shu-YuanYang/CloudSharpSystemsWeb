
// References: 1. https://app.courier.com/
//             2. https://www.courier.com/guides/csharp-send-email/
using DBConnectionLibrary.Models;
using System.Text;
using System.Text.Json;
using Azure.Communication.Email;

namespace CloudSharpSystemsCoreLibrary.Messaging
{
    public class EmailSender
    {
		// create an HTTP Clientstatic HttpClient client = new HttpClient();
		// Courier Send Email To Recipient Endpointstatic string apiEndpoint = "https://api.courier.com/send";
		// Your courier authentication token (Bearer Token)static string token = "Bearer " + "AUTH-TOKEN";

		//private readonly HttpClient client;
		//private readonly string apiEndpoint;
		//private readonly string token;
		private EmailCommService _email_comm_service;
		public class EmailStatusTracker
		{
			public string? Recipient { get; set; }
			public string? Message { get; set; }
			public string? ResponseStatus { get; set; }
		}

		public EmailSender(EmailCommService email_comm_service) 
        {
			this._email_comm_service = email_comm_service;
            /*
            this.client = new HttpClient();
            this.apiEndpoint = "https://api.courier.com/send";
            this.token = "Bearer " + "pk_prod_NZ4GC2Z22042VFJWRCCVZFBMGP9S";

            // attach the Auth Token
            client.DefaultRequestHeaders.Add("Authorization", token);
            */
        }

		/*
        public async Task<HttpResponseMessage> sendEmail(string recipient_email, string website_address, string time_step, string message_note)
        {
            // construct the JSON Payload to send to the API
            var content_obj = new { 
                message = new {
                    data = new { 
                        website = website_address,
                        time = time_step,
                        note = message_note
                    },
                    to = new { 
                        preferences = new { },
                        email = recipient_email
                    },
                    template = "G4F25Y7ZM2MTB5QQBRRDV973BHM5"
                }
            };
            string payload = JsonSerializer.Serialize(content_obj);


            // construct the JSON Post Object
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");

            // Send the email
            var resp = await client.PostAsync(new Uri(apiEndpoint), content);
            //Console.WriteLine(resp);
            return resp;
        }
        */

		public EmailStatusTracker sendEmail(CL_TEMPLATED_EMAIL data)
		{
			try
			{
				var param_values = data.message!.param_values?.First().Value;
				var html = HTMLGenerator.GetHTMLFromTemplate(data.message!.body_template!, param_values);

				var operation = this._email_comm_service.SendBulkEmails(data.recipients!, data.message!.subject!, html);
				if (operation.Value.Status != EmailSendStatus.Succeeded)
				{
					return new EmailStatusTracker
					{
						Recipient = String.Join(",", data.recipients!),
						Message = "Failed to send email!",
						ResponseStatus = operation.Value.Status.ToString()
					};
				}

			}
			catch (Exception ex)
			{
				return new EmailStatusTracker
				{
					Recipient = String.Join(",", data.recipients!),
					Message = ex.Message,
					ResponseStatus = EmailSendStatus.Failed.ToString()
				};
			}
			
			return new EmailStatusTracker
			{
				Recipient = String.Join(",", data.recipients!),
				Message = "",
				ResponseStatus = EmailSendStatus.Succeeded.ToString()
			};
		}

	}
}
