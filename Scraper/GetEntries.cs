using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Parser;
using static Models.EntryModel;
using static System.Environment;
using Models;

namespace Scraper
{
    public class ScrapeEntries
    {
        public readonly SynchronizationContext sContext;
        EntryParser ep = new EntryParser();

        public List<AllInfoHTMLPage> GetAllPropertyInfoPages(List<PropertyList> pl, IProgress<AsyncProgress> progress)
        {
            var pages = new List<AllInfoHTMLPage>();
            var ap = new AsyncProgress();
            
            string currKeyword = "";

            //Get the value of progress for each property
            int x = 0;

            foreach (var _pl in pl)
            {
                //Get the property's webpages (property info, residential/commercial building data, tax info and property record card)
                pages.Add(GetEntryInfo(_pl.PropertyID));

                //Set Async Progress and Logging
                x++;
                ap.ProgressText = $"Processing. . . .{ x }/{ pl.Count }";
                ap.ProgressValue = (x * 100) / pl.Count;
                ap.LogText = $"R{ _pl.PropertyID }. . . .Done{ NewLine }";
                if (currKeyword != _pl.Keyword) //Only display the message at start of every keyword result (applicable only for multiple keywords)
                    ap.LogText = $"{ NewLine }Fetching webpages for keyword: { _pl.Keyword }{ NewLine }{ ap.LogText }";
                progress.Report(ap);

                currKeyword = _pl.Keyword;
            }

            return pages;
        }
        
        public List<Entry> GetAllEntries(ref TextBox txtLogs, List<AllInfoHTMLPage> pages)
        {
            txtLogs.AppendText($"{ NewLine }Extract all Property Info from each webpage. . . .");

            var e = new List<Entry>();

            foreach (var page in pages)
            {
                //Parse all property info from the webpage
                e.Add(ep.ParseEntries(page));
            }

            txtLogs.AppendText($"Done{ NewLine }");

            return e;
        }

        public List<BuildingComponents> GetAllBuildingComponents(ref TextBox txtLogs, List<AllInfoHTMLPage> pages)
        {
            txtLogs.AppendText($"Extract data from Building Components table. . . .");

            var bc = new List<BuildingComponents>();

            foreach (var page in pages)
            {
                if (!page.BuildingData.Contains("<strong>Error</strong> Data is Null."))
                    bc.AddRange(ep.ParseBuildingComponents(page.BuildingData)); //Parse all building components data from the webpage
            }

            txtLogs.AppendText($"Done{ NewLine }");

            return bc;
        }

        public List<OtherImprovements> GetAllOtherImprovements(ref TextBox txtLogs, List<AllInfoHTMLPage> pages)
        {
            txtLogs.AppendText($"Extract data from Other Improvements table. . . .");

            var oi = new List<OtherImprovements>();

            foreach (var page in pages)
            {
                if (!page.BuildingData.Contains("<strong>Error</strong> Data is Null."))
                    oi.AddRange(ep.ParseOtherImprovements(page.BuildingData)); //Parse other improvements table from webpage
            }

            txtLogs.AppendText($"Done{ NewLine }{ NewLine }");

            return oi;
        }

        public List<ComparableData> GetAllComparables(List<AllInfoHTMLPage> pages)
        {
            var cd = new List<ComparableData>();

            foreach(var page in pages)
            {
                if (!page.Comparables.Contains("<strong>Error</strong> Please search and select a property."))
                    cd.AddRange(ep.ParseComparables(page.Comparables, page.BuildingData));
            }

            return cd;
        }

        private AllInfoHTMLPage GetEntryInfo(string propertyID)
        {
            AllInfoHTMLPage pages = new AllInfoHTMLPage();

            CookieContainer cookies;
            HttpWebRequest request;
            HttpWebResponse response;

            string strUrl;
            string postData;
                        
            try
            {
                //Navigate to property information tab
                strUrl = $"https://ares.sncoapps.us/Property/PropertyInformation?propertyId={ propertyID }";
                postData = "";
                cookies = new CookieContainer();
                request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Method = "GET";
                request.Accept = "text/html";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                request.CookieContainer = cookies;
                request.Timeout = Timeout.Infinite;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                var encoding = new System.Text.UTF8Encoding();
                var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                pages.PropertyInformation = $"<div class=\"Entry URL\">{ strUrl }</div>{ responseReader.ReadToEnd() }";
                response.Close();
                responseReader.Close();
                
                //Navigate to residential building data tab
                strUrl = $"https://ares.sncoapps.us/Property/ResidentialBuildingInformation?propertyId={ propertyID }";
                postData = "";
                cookies = new CookieContainer();
                request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Method = "GET";
                request.Accept = "text/html";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                request.CookieContainer = cookies;
                request.Timeout = Timeout.Infinite;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                encoding = new System.Text.UTF8Encoding();
                responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                pages.BuildingData = responseReader.ReadToEnd();
                response.Close();
                responseReader.Close();
                

                if (pages.BuildingData.Contains("<strong>Error</strong> Data is Null."))
                {
                    //If residential building data tab is empty, navigate to commercial building data tab
                    strUrl = $"https://ares.sncoapps.us/Property/CommercialBuildingInformation?propertyId={ propertyID }";
                    postData = "";
                    cookies = new CookieContainer();
                    request = (HttpWebRequest)WebRequest.Create(strUrl);
                    request.Method = "GET";
                    request.Accept = "text/html";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = postData.Length;
                    request.CookieContainer = cookies;
                    request.Timeout = Timeout.Infinite;
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                    response = (HttpWebResponse)request.GetResponse();
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                    encoding = new System.Text.UTF8Encoding();
                    responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                    pages.BuildingData = responseReader.ReadToEnd();
                    response.Close();
                    responseReader.Close();
                }
                
                //Navigate to tax information tab
                strUrl = $"https://ares.sncoapps.us/PropertyTax/TaxInformation?propertyId={ propertyID }";
                postData = "";
                cookies = new CookieContainer();
                request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Method = "GET";
                request.Accept = "text/html";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                request.CookieContainer = cookies;
                request.Timeout = Timeout.Infinite;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                encoding = new System.Text.UTF8Encoding();
                responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                pages.TaxInformation = responseReader.ReadToEnd();
                response.Close();
                responseReader.Close();

                //Navigate to property tax tab
                pages.TaxRecordPDF = $"/Common/GetPropertyRecordCard{ Regex.Match(pages.PropertyInformation, "<a href=\"/Common/GetPropertyRecordCard(.*?)\" style=\"display:inline;\">").Groups[1].Value }";

                //Navigate to comparables tab
                strUrl = $"https://ares.sncoapps.us/Property/Comparables?propertyId={ propertyID }";
                postData = "";
                cookies = new CookieContainer();
                request = (HttpWebRequest)WebRequest.Create(strUrl);
                request.Method = "GET";
                request.Accept = "text/html";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                request.CookieContainer = cookies;
                request.Timeout = Timeout.Infinite;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                encoding = new System.Text.UTF8Encoding();
                responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                pages.Comparables = responseReader.ReadToEnd();
                response.Close();
                responseReader.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show($"Error occured when fetching webpage for QuickRef:R{ propertyID }{ NewLine }{ e.Message }");
                throw e;
            }
            return pages;
        }

        public List<PropertyList> GetEntriesList(int selectedFilter, List<Keywords> keywords, IProgress<AsyncProgress> progress)
        {
            var pl = new List<PropertyList>();
            var ap = new AsyncProgress();

            CookieContainer cookies;
            HttpWebRequest request;
            HttpWebResponse response;

            var searchBy = (SearchBy)selectedFilter;

            var responseData = "";
            var strUrl = "";
            var postData = "";

            foreach (var keyword in keywords)
            {
                try
                {
                    //Navigate to main website
                    strUrl = @"https://ares.sncoapps.us/?clearFilters=True";
                    postData = "";
                    cookies = new CookieContainer();
                    request = (HttpWebRequest)WebRequest.Create(strUrl);
                    request.Method = "GET";
                    request.Accept = "text/html";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = postData.Length;
                    request.CookieContainer = cookies;
                    request.Timeout = Timeout.Infinite;
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                    response = (HttpWebResponse)request.GetResponse();
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                    var encoding = new System.Text.UTF8Encoding();
                    var responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                    responseData = responseReader.ReadToEnd();
                    response.Close();
                    responseReader.Close();

                    strUrl = $"https://ares.sncoapps.us/BasicSearch/Results?clearFilters=true&CountyCode=089&grayCheckBox=true&SearchCriteria={ keyword.Keyword }&searchBy={ searchBy }&grayCheckBox=false";
                    postData = "";
                    cookies = new CookieContainer();
                    request = (HttpWebRequest)WebRequest.Create(strUrl);
                    request.Method = "GET";
                    request.Accept = "text/html";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                    request.ContentLength = postData.Length;
                    request.CookieContainer = cookies;
                    request.Timeout = Timeout.Infinite;
                    request.AllowAutoRedirect = true;
                    response = (HttpWebResponse)request.GetResponse();
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                    encoding = new System.Text.UTF8Encoding();
                    responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                    responseData = responseReader.ReadToEnd();
                    response.Close();
                    responseReader.Close();

                    strUrl = $"https://ares.sncoapps.us/BasicSearch/ResultsJson?searchCriteria={ keyword.Keyword }&countyCode=089&listMode=card&searchBy={ searchBy }&_=1600158783610";
                    postData = "";
                    cookies = new CookieContainer();
                    request = (HttpWebRequest)WebRequest.Create(strUrl);
                    request.Method = "GET";
                    request.Accept = "text/html";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = postData.Length;
                    request.CookieContainer = cookies;
                    request.Timeout = Timeout.Infinite;
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                    response = (HttpWebResponse)request.GetResponse();
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                    encoding = new System.Text.UTF8Encoding();
                    responseReader = new StreamReader(response.GetResponseStream(), encoding, true);
                    responseData = responseReader.ReadToEnd();
                    response.Close();
                    responseReader.Close();

                    //if (!responseData.Contains("recordsTotal\":0"))
                    pl.AddRange(ep.ParseEntriesList(keyword.Keyword, responseData, progress));

                    ap.LogText = "";
                    ap.ProgressText = $"Properties found: { pl.Count }";
                    progress.Report(ap);
                }
                catch(Exception e)
                {
                    MessageBox.Show($"Error occured when fetching Property List webpage using \"{ keyword.Keyword }\" keyword{ NewLine }{ e.Message }");
                    throw e;
                }
            }
            
            return pl;
        }
    }
}
