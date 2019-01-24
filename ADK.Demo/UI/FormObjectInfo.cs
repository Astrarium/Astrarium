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
                switch (item)
                {
                    case InfoElementHeader h:
                        sb.AppendLine($"<tr><td class='header' colspan='2'>{HttpUtility.HtmlEncode(h.Text)}</td></tr>");
                        break;
                    case InfoElementProperty p:
                        sb.AppendLine($"<tr><td class='expand'>{HttpUtility.HtmlEncode(p.Caption)}</td>")
                          .AppendLine($"<td class='shrink'>{p.Value}</td></tr>");
                        break;
                    case InfoElementPropertyLink pl:
                        sb.AppendLine($"<tr><td class='expand'>{HttpUtility.HtmlEncode(pl.Caption)}</td>")
                          .AppendLine($"<td class='shrink'><a href='?jd={pl.JulianDay.ToString(CultureInfo.InvariantCulture)}'>{HttpUtility.HtmlEncode(pl.Value)}</td></tr>");
                        break;
                    default:
                        break;
                }
            }

            templateContent.Replace("{0}", "Object type");
            templateContent.Replace("{1}", ObjectInfo.Title);
            templateContent.Replace("{2}", sb.ToString());

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

        }
    }
}
