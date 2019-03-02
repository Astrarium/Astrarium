using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    public partial class FormEphemeris : Form
    {
        public FormEphemeris(List<List<Ephemeris>> ephem, double from, double to, double step, double utcOffset)
        {
            InitializeComponent();
            LoadPage(ephem, from, to, step, utcOffset);
        }

        /// <summary>
        /// Gets Julian Day selected by user in the almanac window.
        /// </summary>
        public double JulianDay { get; private set; }

        private void LoadPage(List<List<Ephemeris>> ephem, double from, double to, double step, double utcOffset)
        {
            StringBuilder templateContent = null;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ADK.Demo.UI.EphemerisTemplate.html"))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                templateContent = new StringBuilder(reader.ReadToEnd());
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"<tr>");
            sb.Append($"<th>Date</th>");
            foreach (var e in ephem[0])
            {
                sb.Append($"<th>{e.Key}</th>");
            }
            sb.Append($"</tr>");

            int i = 0;
            for (double jd = from; jd < to; jd += step)
            {
                sb.Append($"<tr>");
                sb.Append($"<td>{Formatters.DateTime.Format(new Date(jd, utcOffset))}</td>");                
                foreach (var e in ephem[i])
                {
                    sb.Append($"<td>{e.Formatter.Format(e.Value)}</td>");
                }
                sb.Append($"</tr>");
                i++;
            }

            templateContent.Replace("{ephemeris}", sb.ToString());

            wbInfo.DocumentText = "";
            wbInfo.Document.Write(templateContent.ToString());

            foreach (HtmlElement link in wbInfo.Document.Links)
            {
                link.Click += new HtmlElementEventHandler(HandleLinkClicked);
            }
        }

        private void HandleLinkClicked(object sender, HtmlElementEventArgs e)
        {
            string href = (sender as HtmlElement).GetAttribute("href");
            var url = new Uri(href);
            var query = HttpUtility.ParseQueryString(url.Query);
            if (query.AllKeys.Contains("jd"))
            {
                JulianDay = Convert.ToDouble(query["jd"], CultureInfo.InvariantCulture);
            }
            DialogResult = DialogResult.OK;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
