using Azure;
using Azure.Communication.Email;

namespace CloudSharpSystemsCoreLibrary.Messaging
{

    public enum EmailServiceProvider { 
        AZURE
    }

    public class EmailCommService
    {

        private EmailServiceProvider _service_provider { get; set; }
        private string _server_endpoint { get; set; }
        private string _sender_email { get; set; }

        //private Dictionary<string, EmailClient> _email_clients;
        private EmailClient _email_client;

        public EmailCommService(EmailServiceProvider sep, string server_endpoint, string sender_email) {
            this._service_provider = sep;
            this._server_endpoint = server_endpoint;
            this._sender_email = sender_email;
            //this._email_clients = new Dictionary<string, EmailClient>();
            this._email_client = new EmailClient(this._server_endpoint);
		}


        // For powerful machines, use async:
        public EmailSendOperation SendEmail(string recipient_email, string subject, string body) {

            var emailMessage = new EmailMessage(
                this._sender_email, 
                recipient_email, 
                new EmailContent(subject)
                {
                    //PlainText = "" // plaintext not implemented
                    Html = body
                }
                //recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress("zantgoron@gmail.com") })
            );

            EmailSendOperation emailSendOperation = this._email_client.Send(WaitUntil.Completed, emailMessage);
            return emailSendOperation;
        }

        public EmailSendOperation SendBulkEmails(List<string> recipients, string subject, string body) {
            // Create the BCC list
            var recipient_addresses = recipients.Select(r => new EmailAddress(r));

			EmailRecipients all_recipients = new EmailRecipients(recipient_addresses);

            // Create the EmailMessage
            var emailMessage = new EmailMessage(
                this._sender_email,
				all_recipients,
				new EmailContent(subject)
				{
					//PlainText = "" // plaintext not implemented
					Html = body
				}
			);

			EmailSendOperation emailSendOperation = this._email_client.Send(WaitUntil.Completed, emailMessage);
            return emailSendOperation;

		}


        /*
		public static async Task<string> ParseMessageHTMLFromTemplate(string template_file_path, object? view_model) {
			var html = await RazorTemplateEngine.RenderAsync(template_file_path, view_model);
            return html;
		}
        */
    }
}
