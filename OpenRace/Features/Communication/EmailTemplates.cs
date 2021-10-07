using System.Collections.Generic;
using System.Linq;

namespace OpenRace.Features.Communication
{
    public partial class EmailTemplates
    {
        public string GetTemplate1Html(
            string title, string subtitle, string bodyText, string footerText, string siteUrl, string unsubscribeUri)
        {
            return GetTemplate(_template1, new Dictionary<string, string>
            {
                { nameof(title), title },
                { nameof(subtitle), subtitle },
                { nameof(bodyText), bodyText },
                { nameof(footerText), footerText },
                { nameof(siteUrl), siteUrl },
                { nameof(unsubscribeUri), unsubscribeUri },
            });
        }

        private string GetTemplate(string templateHtml, Dictionary<string, string> parameters)
        {
            var result = templateHtml;
            foreach (var (paramName, value) in parameters.OrderByDescending(it => it.Key.Length))
            {
                result = result.Replace("{{" + paramName + "}}", value);
            }

            return result;
        }
    }
}