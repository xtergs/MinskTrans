
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MetroLog;
using MinskTrans.Utilites.Base.Net;

namespace MinskTrans.Context.Geopositioning
{
    public class WebResult
    {
        public Location Location { get; set; }
        public string Address { get; set; }
    }

    public class WebSeacher
    {
        public ILogger Log { get; set; }
        public WebSeacher(InternetHelperBase helper, ILogManager logManager = null)
        {
            if (helper == null)
                throw new ArgumentNullException(nameof(helper));
            internetHelper = helper;
            this.Log = logManager?.GetLogger<WebSeacher>();
        }
        private InternetHelperBase internetHelper;
        public async Task<WebResult[]> QueryToPosition(string query)
        {
            string request =
                $"https://geocode-maps.yandex.ru/1.x/?geocode=страна Беларусь, город Минск, {query}&lang=ru-RU";
            var result = await internetHelper.Download(request);
            var document = XDocument.Parse(result);
            var xx = document.Root.Elements().Elements();
            var res = document.Root.Elements().Elements()
                        .Elements()
                        .Skip(1).Select((element =>
            {
                var loc =
                    element
                        .Elements("{http://www.opengis.net/gml}Point")
                        .First()
                        .Value.Split(' ').Select(x=> x.Replace('.', ',')).ToArray();
                return new WebResult()
                {
                    Address = element.Elements("{http://www.opengis.net/gml}name").First().Value,
                    Location = new Location(double.Parse(loc[1]), double.Parse(loc[0]))
                };
            })).ToArray();
            for (int i = 0; i < res.Length; i++)
                Log?.Debug($"Response {i}: {res[i].Address}, {res[i].Location}");
            return res;
        }
    }
}
