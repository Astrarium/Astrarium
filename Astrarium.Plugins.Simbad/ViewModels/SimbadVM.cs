using Astrarium.Types;
using Astrarium.Types.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Simbad.ViewModels
{
    public class SimbadVM : ViewModelBase
    {
        private CelestialObject body;
        private readonly ISky sky;

        
        public SimbadVM(ISky sky)
        {
            this.sky = sky;
        }

        public SimbadVM ForBody(CelestialObject body)
        {
            this.body = body;
            MakeRequest();
            return this;
        }

        public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsLoading
        {
            get => GetValue<bool>(nameof(IsLoading));
            private set
            {
                SetValue(nameof(IsLoading), value);
                NotifyPropertyChanged(nameof(HasData));
            }
        }

        public bool HasData => !IsLoading && !HasErrors;

        public string ErrorMessage
        {
            get => GetValue<string>(nameof(ErrorMessage));
            private set 
            { 
                SetValue(nameof(ErrorMessage), value);
                NotifyPropertyChanged(nameof(HasErrors), nameof(IsLoading));
            }
        }

        public List<InfoElement> Table
        {
            get => GetValue<List<InfoElement>>(nameof(Table));
            private set => SetValue(nameof(Table), value);
        }

        private async void MakeRequest()
        {
            IsLoading = true;
            VoTable table = null;

            try
            {
                table = await Task.Run(() =>
                {
                    string tempFile = Path.GetTempFileName();

                    try
                    {
                        // query by id
                        string url = $"https://simbad.cds.unistra.fr/simbad/sim-id?Ident={body.CommonName}&output.format=votable&NbIdent=1&Radius=1&Radius.unit=arcsec&output.params=main_id,otype(v),coordinates,propermotions,parallax,fluxdata(B),fluxdata(V),velocity,sptype,morphtype,dimensions,ids";

                        // download to temp dir
                        Downloader.Download(new Uri(url), tempFile);

                        // make the VoTable object
                        return new VoTable(tempFile);
                    }
                    finally
                    {
                        FileSystem.DeleteFile(tempFile);
                    }
                });

                ErrorMessage = table?.ErrorMessage;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            if (table != null && !HasErrors)
            {
                var infoElements = new List<InfoElement>();

                if (!table.HasErrors)
                {
                    var row = table.Rows.First();
                    string currentGroup = "";
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        object obj = row.GetCellValue(i);
                        var col = table.Columns[i];

                        string group = VoTableFields.FieldToGroupMap[col.Id];

                        if (obj != null && obj != (object)"")
                        {
                            if (group != currentGroup)
                            {
                                infoElements.Add(new InfoElementHeader() { Text = $"$Simbad.FieldGroup.{group}" });
                                currentGroup = group;
                            }

                            object value = col.ParseValue(obj);
                            var formatter = VoTableFields.FieldFormatters[col.Id];
                            var caption = $"$Simbad.VoTable.Field.{col.Id}";

                            if (formatter is ExcludedFormatter)
                            {
                                // Skip
                            }
                            else if (formatter is BibcodeFormatter)
                            {
                                infoElements.Add(new InfoElementLink()
                                {
                                    Caption = caption,
                                    UriText = value.ToString(),
                                    Uri = new Uri($"http://simbad.cds.unistra.fr/simbad/sim-ref?bibcode={Uri.EscapeDataString(value.ToString())}")
                                });
                            }
                            else if (formatter is IdsFormatter)
                            {
                                infoElements.Add(new InfoElementMultilineProperty()
                                {
                                    Caption = caption,
                                    Formatter = formatter,
                                    Value = value,
                                });
                            }
                            else
                            {
                                infoElements.Add(new InfoElementProperty()
                                {
                                    Caption = caption,
                                    Formatter = formatter,
                                    Value = value,
                                });
                            }
                        }
                    }

                    Table = infoElements;
                }
            }

            IsLoading = false;
        }
    }
}
