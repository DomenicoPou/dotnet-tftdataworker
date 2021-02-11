using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TFTDataWorker.Handlers
{
    public static class WebCrawlerHandler
    {
        public static HtmlDocument ObtainHtml(string url)
        {
            HttpClient client = new HttpClient();
            string htmlString = client.GetStringAsync(url).Result;
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlString);
            return html;
        }
    }
}
