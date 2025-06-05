using Microsoft.AspNetCore.Html;

namespace CloudSharpSystemsCoreLibrary.Messaging
{
	public class HTMLGenerator
	{
		public static string GetHTMLFromTemplate(string template, object[]? variables) {
			var builder = new HtmlContentBuilder();
			if (variables is null) builder.AppendFormat(template);
			else builder.AppendFormat(template, variables);
            StringWriter html_str_writer = new StringWriter();
			builder.WriteTo(html_str_writer, System.Text.Encodings.Web.HtmlEncoder.Default);
			return html_str_writer.ToString();
		}

	}
}
