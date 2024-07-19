using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Types.Utils
{
    public static class Downloader
    {
        public static void Download(Uri uri, string localPath)
        {
            Download(uri, localPath, null, null);
        }

        public static void Download(Uri uri, string localPath, CancellationTokenSource cancelTokenSource)
        {
            Download(uri, localPath, cancelTokenSource, null);
        }

        public static void Download(Uri uri, string localPath, Progress<double> progress)
        {
            Download(uri, localPath, null, progress);
        }

        public static void Download(Uri uri, string localPath, CancellationTokenSource cancelTokenSource, Progress<double> progress)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            WebRequest request = WebRequest.Create(uri);

            if (request is FtpWebRequest ftpRequest)
            {
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpRequest.Credentials = new NetworkCredential();
            }

            request.Timeout = 10000;
            WebResponse response = request.GetResponse();
            using (Stream responseStream = response.GetResponseStream())
            using (Stream fileStream = new FileStream(localPath, FileMode.OpenOrCreate))
            using (BinaryWriter streamWriter = new BinaryWriter(fileStream))
            {
                responseStream.ReadTimeout = 10000;

                long fileSize = response.ContentLength;
                long totalRead = 0;

                byte[] buffer = new byte[1024 * 10];
                int bytesRead = 0;
                StringBuilder remainder = new StringBuilder();

                do
                {
                    if (cancelTokenSource?.IsCancellationRequested == true)
                    {
                        break;
                    };

                    bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                    totalRead += bytesRead;

                    if (progress != null)
                    {
                        (progress as IProgress<double>).Report(totalRead / (double)fileSize * 100);
                    }

                    streamWriter.Write(buffer, 0, bytesRead);
                }
                while (bytesRead > 0);
            }
        }

        //public static void DownloadFtp(Uri uri, string localPath, CancellationTokenSource cancelTokenSource, Progress<double> progress)
        //{
        //    FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(uri);
        //    downloadRequest.Timeout = 10000;
        //    downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
        //    downloadRequest.Credentials = new NetworkCredential();

        //    using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
        //    using (Stream responseStream = downloadResponse.GetResponseStream())
        //    using (Stream fileStream = File.Create(localPath))
        //    {
        //        responseStream.ReadTimeout = 10000;

        //        byte[] buffer = new byte[1024 * 10];
        //        int read;
        //        while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            fileStream.Write(buffer, 0, read);
        //        }
        //    }
        //}
    }
}
