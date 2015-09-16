using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Net;
using ShortURLService.Infrastructure;

namespace ShortURLService.Models
{
    public class URL
    {
        [Key]
        public int UrlId { get; set; }
        [Required]
        [DisplayName("Long URL")]
        public string LongUrl { get; set; }
        [Required]
        [DisplayName("Short URL")]
        public string ShortUrl { get; set; }
        [Required]
        [DisplayName("Clicks")]
        public int Hits { get; set; }
        [Required]
        [DisplayName("Created")]
        public DateTime GeneratedDate { get; set; }
        public string UserId { get; set; }

        public virtual List<UrlStat> UrlStats { get; set; }

        public static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        public static readonly int Base = Alphabet.Length;

        public static string Encode(int i)
        {
            if (i == 0) return Alphabet[0].ToString();

            var s = string.Empty;

            while (i > 0)
            {
                s += Alphabet[i % Base];
                i = i / Base;
            }

            return string.Join(string.Empty, s.Reverse());
        }

        public static int Decode(string s)
        {
            var i = 0;

            foreach (var c in s)
            {
                i = (i * Base) + Alphabet.IndexOf(c);
            }

            return i;
        }

        public void AssignShortUrl(string shortUrl)
        {
            ShortUrl = shortUrl;
        }

        /// <summary>
        /// Check if provided URL is valid HTTP url
        /// </summary>
        /// <param name="url">URL (Uniform resource locator)</param>
        /// <returns>true or false depends on the URL contains HTTP</returns>
        public bool HasHTTPProtocol(string url)
        {
            url = url.ToLower();
            if (url.Length > 5)
            {
                if (url.StartsWith(Constants.Protocol.HTTP) || url.StartsWith(Constants.Protocol.HTTPS))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Check whether provided URL exists by doing request to it and waiting for response.
        /// </summary>
        /// <returns>true or false depends on the availability of the provided URL</returns>
        public bool CheckLongUrlExists()
        {
            int linkLength = LongUrl.Length;
            if(!HasHTTPProtocol(LongUrl))
                LongUrl = Constants.Protocol.HTTP + LongUrl;

            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(LongUrl) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                var wex = GetNestedException<WebException>(ex);

                // If there is no nested WebException
                if (wex == null) { return false; }

                // Get the response object.
                var response = wex.Response as HttpWebResponse;

                // If it's not an HTTP response or is not error 403, re-throw.
                if (response == null || response.StatusCode != HttpStatusCode.Forbidden)
                {
                    return false;
                }

                return true;
            }
        }

        private T GetNestedException<T>(Exception ex) where T : Exception
        {
            if (ex == null) { return null; }

            var tEx = ex as T;
            if (tEx != null) { return tEx; }

            return GetNestedException<T>(ex.InnerException);
        }
    }
}