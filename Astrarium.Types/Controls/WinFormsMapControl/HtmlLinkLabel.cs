using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Windows.Forms
{
    /// <summary>
    /// Extended <see cref="LinkLabel"/> control with auto-parsing of HTML links in the <see cref="Text"/> property.
    /// </summary>
    [DesignerCategory("Code")]
    internal class HtmlLinkLabel : LinkLabel
    {
        public override string Text 
        { 
            get => base.Text; 
            set => SetText(value);
        }

        private void SetText(string value)
        {
            Links.Clear();

            if (!string.IsNullOrEmpty(value))
            {
                StringBuilder text = new StringBuilder();
                int lastIndex = 0;

                MatchCollection m1 = Regex.Matches(value, @"(<a.*?>.*?</a>)", RegexOptions.Singleline);

                foreach (Match m2 in m1)
                {
                    Link link = new Link();

                    text.Append(value.Substring(lastIndex, m2.Index - lastIndex));

                    string fullLinkWithTags = m2.Groups[1].Value;

                    Match m3 = Regex.Match(fullLinkWithTags, @"href=[\""'](.*?)[\""']", RegexOptions.Singleline);
                    if (m3.Success)
                    {
                        link.LinkData = m3.Groups[1].Value;
                        link.Start = text.Length;
                    }

                    string innerText = Regex.Replace(fullLinkWithTags, @"\s*<.*?>\s*", "", RegexOptions.Singleline);

                    text.Append(innerText);
                    link.Length = innerText.Length;
                    lastIndex = m2.Index + m2.Length;
                    Links.Add(link);
                }

                if (lastIndex <= value.Length - 1)
                {
                    text.Append(value.Substring(lastIndex));
                }

                base.Text = text.ToString();
            }
            else
            {
                base.Text = value;
            }            
        }
    }
}
