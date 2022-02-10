using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public class OrbitalElementsDownloader
    {
        private const int BUFFER_SIZE = 1024;

        public int Download(string url, string filePath, int maxCount, Func<string, bool> matcher, CancellationTokenSource tokenSource, IProgress<double> progress = null)
        {
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Ssl3;

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            using (Stream responseStream = response.GetResponseStream())
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead = 0;
                int totalRecords = 0;
                StringBuilder remainder = new StringBuilder();

                do
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        return 0;
                    }

                    bytesRead = responseStream.Read(buffer, 0, BUFFER_SIZE);
                    remainder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    string[] records = remainder.ToString().Split('\n');

                    foreach (string record in records)
                    {
                        if (matcher(record))
                        {
                            streamWriter.WriteLine(record);
                            totalRecords++;
                            progress?.Report((int)(totalRecords / (double)maxCount * 100));
                            if (totalRecords >= maxCount)
                            {
                                return totalRecords;
                            }
                            if (record == records.Last())
                            {
                                remainder.Clear();
                            }
                        }
                        else if (record == records.Last())
                        {
                            remainder.Clear();
                            remainder.Append(record);
                        }
                    }
                }
                while (bytesRead > 0 && totalRecords < maxCount);

                return totalRecords;
            }
        }
    }
}
