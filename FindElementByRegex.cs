using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace TelegramBot
{
    public class FindElementsByRegex
    {
        public string x { get; set; }
        public FindElementsByRegex(string soz)
        {
            x = soz;
        }
        public static List<string> NatijalarListi { get; set; }

        public static async void MalumotIzla(string x)
        {
            string Natijalar;
            NatijalarListi = new List<string>();
            string str = "[";
            for (int i = 0; i < x.Length; i++)
            {
                if (!str.Contains(x[i].ToString())) str = str + x[i].ToString();
            }
            str += "]";

            using (HttpClient client = new HttpClient())
            {

                string BaseUrl = "https://api.publicapis.org/entries";
                HttpResponseMessage proces = await client.GetAsync(BaseUrl);
                string JsonResult = await proces.Content.ReadAsStringAsync();
                CountAndApi APIss = JsonConvert.DeserializeObject<CountAndApi>(JsonResult);

                foreach (var item in APIss.entries)
                {
                    if (Regex.IsMatch(item.API, str))
                    {
                        NatijalarListi.Add(item.API);
                    }
                }
            }
        }

        public static List<string> TopibOlinganlar()
        {

            return NatijalarListi;
        }
    }
}
