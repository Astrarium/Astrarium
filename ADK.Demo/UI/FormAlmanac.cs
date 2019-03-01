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
    public partial class FormAlmanac : Form
    {
        public FormAlmanac(ICollection<AstroEvent> events, double utcOffset)
        {
            InitializeComponent();
            LoadPage(events, utcOffset);
        }

        /// <summary>
        /// Gets Julian Day selected by user in the almanac window.
        /// </summary>
        public double JulianDay { get; private set; }

        private void LoadPage(ICollection<AstroEvent> events, double utcOffset)
        {
            var days = events.GroupBy(e => Formatters.DateOnly.Format(new Date(e.JulianDay, utcOffset)));

            StringBuilder templateContent = null;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ADK.Demo.UI.AlmanacTemplate.html"))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                templateContent = new StringBuilder(reader.ReadToEnd());
            }

            StringBuilder sb = new StringBuilder();
            foreach (var day in days)
            {
                sb.Append($"<b>{day.Key}</b>");
                sb.Append($"<ul>");
                foreach (var e in day)
                {
                    sb.Append($"<li>");
                    if (!e.NoExactTime)
                    {
                        sb.Append($"<a href='?jd={e.JulianDay.ToString(CultureInfo.InvariantCulture)}'>{Formatters.Time.Format(new Date(e.JulianDay, utcOffset).Time)}</a> ");
                    }
                    sb.Append($"{e.Text}");
                    sb.Append($"</li>");
                }
                sb.Append($"</ul>");
            }

            templateContent.Replace("{almanac}", sb.ToString());

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
