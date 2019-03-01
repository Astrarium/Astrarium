using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    /// <summary>
    /// Provides detailed info about selected celestial object
    /// </summary>
    public partial class FormObjectInfo : Form
    {
        /// <summary>
        /// Gets Julian Day to be set when user clicks on a date/time link in the "Object Info" window.
        /// This property is set only if <see cref="Form.DialogResult"/> property is "OK", otherwise it's undefined (i.e. "NaN").
        /// </summary>
        public double JulianDay { get; private set; } = double.NaN;

        /// <summary>
        /// Gets object info passed to the constructor of the form.
        /// </summary>
        public CelestialObjectInfo ObjectInfo { get; private set; }

        /// <summary>
        /// Creates new instance of the form.
        /// </summary>
        /// <param name="info">Object info to be displyed in the form.</param>
        public FormObjectInfo(CelestialObjectInfo info)
        {
            InitializeComponent();
            ObjectInfo = info;
            LoadInfoPage();
        }

        private void LoadInfoPage()
        {
            StringBuilder templateContent = null;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ADK.Demo.UI.ObjectInfoTemplate.html"))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                templateContent = new StringBuilder(reader.ReadToEnd());
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in ObjectInfo.InfoElements)
            {
                IEphemFormatter formatter;
                switch (item)
                {
                    case InfoElementHeader h:
                        sb.AppendLine($"<tr><td class='header' colspan='2'>{HttpUtility.HtmlEncode(h.Text)}</td></tr>");
                        break;
                    case InfoElementPropertyLink p when !double.IsNaN(p.JulianDay):
                        formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);
                        sb.AppendLine($"<tr><td class='expand'>{HttpUtility.HtmlEncode(p.Caption)}</td>")
                          .AppendLine($"<td class='shrink'><a href='?jd={p.JulianDay.ToString(CultureInfo.InvariantCulture)}'>{HttpUtility.HtmlEncode(formatter.Format(p.Value))}</td></tr>");
                        break;
                    case InfoElementProperty p:
                        formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);
                        sb.AppendLine($"<tr><td class='expand'>{HttpUtility.HtmlEncode(p.Caption)}</td>")
                          .AppendLine($"<td class='shrink'>{formatter.Format(p.Value)}</td></tr>");
                        break;
                    default:
                        break;
                }
            }

            templateContent.Replace("{subtitle}", ObjectInfo.Subtitle);
            templateContent.Replace("{title}", ObjectInfo.Title);
            templateContent.Replace("{info}", sb.ToString());

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

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in ObjectInfo.InfoElements)
            {
                IEphemFormatter formatter;
                switch (item)
                {
                    case InfoElementHeader h:
                        sb.AppendLine().AppendLine(h.Text);
                        break;
                    case InfoElementProperty p:
                        formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);
                        sb.AppendLine($"{p.Caption}: {formatter.Format(p.Value)}");
                        break;
                    default:
                        break;
                }
            }

            string text = Regex.Replace(sb.ToString(), "<.*?>", string.Empty);

            Clipboard.SetText(text, TextDataFormat.UnicodeText);
        }
    }
}
