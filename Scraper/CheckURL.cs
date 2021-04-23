using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scraper
{
    public class CheckUrlStatus
    {
        public bool checkStatus()
        {
            string responseData = "";

            try
            {
                string url = @"https://ares.sncoapps.us/?clearFilters=True";
                string postData = "";
                var cookies = new CookieContainer();
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Accept = "text/html";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                request.CookieContainer = cookies;
                request.Timeout = Timeout.Infinite;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                var response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                var encoding = new System.Text.UTF8Encoding();
                var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                responseData = responseReader.ReadToEnd();
                response.Close();
                responseReader.Close();
            }
            catch (Exception)
            {
                responseData = "";
            }

            //Check if website is working
            if (responseData.Contains("<a title=\"\" href=\"http://www.snco.us/Ap/beta\">Real Estate Search</a>"))
                return true;
            else
                return false;
        }

    }
}
