private class Record
        {
            public string catalogue { get; set; }
            public string catNumber { get; set; }
            public string catNumberExt { get; set; }
            public string catNumberComp { get; set; }
            // skip: dreyer
            public string status { get; set; }
            // skip: precision
            // skip: constell
            public string rh { get; set; }
            public string rm { get; set; }
            public string rs { get; set; }
            public string v { get; set; }
            public string dg { get; set; }
            public string dm { get; set; }
            public string ds { get; set; }
            // skip: photographic mag
            public string mag  { get; set; }
            // skip: Difference of visual and blue magnitude
            // skip: Surface brightness (mag/arcmin2)
            public string X { get; set; }
            public string Y { get; set; }
            public string PA { get; set; }
            public string type { get; set; }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/NI2018.csv");

            
            StringBuilder sr = new StringBuilder();
            List<Record> records = new List<Record>();

            using (var reader = new StreamReader(file))
            {
                using (var writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/NGCIC.dat")))
                {
                    while (!reader.EndOfStream)
                    {
                        Record r = new Record();

                        var line = reader.ReadLine();
                        var values = line.Split(';');

                       

                        r.catalogue = values[0];                       
                        r.catNumber = values[1];                       
                        r.catNumberExt = values[2];                    
                        r.catNumberComp = values[3];                   
                        // skip: dreyer
                        r.status = values[5];                          
                        // skip: precision
                        // skip: constell
                        r.rh = values[8];                              
                        r.rm = values[9];                              
                        r.rs = values[10];                             
                        r.v = values[11];                              
                        r.dg = values[12];                             
                        r.dm = values[13];                             
                        r.ds = values[14];                             
                        // skip: photographic mag
                        r.mag = values[16];                            
                        // skip: Difference of visual and blue magnitude
                        // skip: Surface brightness (mag/arcmin2)
                        r.X = values[19];                              
                        r.Y = values[20];                              
                        r.PA = values[21];                             
                        r.type = values[22];                           


                        records.Add(r);


                        
                    }


                    int magLen = records.Select(r => r.mag.Length).Max();
                    int xLen = records.Select(r => r.X.Length).Max();
                    int yLen = records.Select(r => r.Y.Length).Max();
                    int paLen = records.Select(r => r.PA.Length).Max();
                    int typeLen = records.Select(r => r.type.Length).Max();

                    foreach (var r in records)
                    {
                        sr.Clear();

                        sr.Append(r.catalogue.PadRight(1)).Append("|");
                        sr.Append((r.catNumber + " " + r.catNumberExt + r.catNumberComp).PadRight(7)).Append("|");
                        sr.Append((int.Parse(r.status) % 10).ToString()).Append("|");
                        sr.Append(int.Parse(r.rh).ToString("D2"));
                        sr.Append(int.Parse(r.rm).ToString("D2"));
                        sr.Append(float.Parse(r.rs.Replace(',', '.'), CultureInfo.InvariantCulture).ToString("00.0", CultureInfo.InvariantCulture)).Append("|");
                        sr.Append(r.v.PadRight(1));
                        sr.Append(int.Parse(r.dg).ToString("D2"));
                        sr.Append(int.Parse(r.dm).ToString("D2"));
                        sr.Append(int.Parse(r.ds).ToString("D2")).Append("|");
                        sr.Append((r.mag ?? "").Replace(',', '.').PadRight(magLen)).Append("|");
                        sr.Append((r.X ?? "").Replace(',', '.').PadRight(xLen)).Append("|");
                        sr.Append((r.Y ?? "").Replace(',', '.').PadRight(yLen)).Append("|");
                        sr.Append((r.PA ?? "").PadRight(paLen)).Append("|");
                        sr.Append(r.type.PadRight(typeLen)).Append("|");

                        writer.WriteLine(sr.ToString());
                    }

                }
            }




        }
    }