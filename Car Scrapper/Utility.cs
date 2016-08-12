using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Car_Scrapper
{
    public static class Utility
    {
        public static string ScrapeWebpage(string url, DateTime? updateDate)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream responseStream = null;
            StreamReader reader = null;
            string html = null;

            try
            {
                //create request (which supports http compression)
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Pipelined = true;
                request.KeepAlive = true;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                if (updateDate != null)
                    request.IfModifiedSince = updateDate.Value;

                //get response.
                response = (HttpWebResponse)request.GetResponse();
                responseStream = response.GetResponseStream();
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

                //read html.
                reader = new StreamReader(responseStream, Encoding.Default);
                html = reader.ReadToEnd();
            }
            catch
            {
                throw;
            }
            finally
            {//dispose of objects.
                request = null;
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseStream.Dispose();
                }
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return html;
        }

        public static string RemoveQueryStringByKey(string url, string key)
        {
            var uri = new Uri(url);

            // this gets all the query string key value pairs as a collection
            var newQueryString = System.Web.HttpUtility.ParseQueryString(uri.Query);

            // this removes the key if exists
            newQueryString.Remove(key);

            // this gets the page path from root without QueryString
            string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }

    }
}
