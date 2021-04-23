using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using Models;
using static Models.EntryModel;
using static System.Environment;

namespace Parser
{
    public class EntryParser
    {
        public List<Keywords> ParseKeywords(string input)
        {
            var keywords = new List<Keywords>();

            var i = input.Split(',');
            foreach (var _i in i)
            {
                keywords.Add(new Keywords { Keyword = _i.Trim() });
            }

            return keywords;
        }
        public List<PropertyList> ParseEntriesList(string keyword, string pageText, IProgress<AsyncProgress> progress)
        {
            try
            {
                List<PropertyList> pl = new List<PropertyList>();
                var ap = new AsyncProgress();

                if (pageText.Contains("recordsTotal\":0,"))
                {
                    ap.LogText = $"{ NewLine }Parsing properties summary for keyword: { keyword }{ NewLine }0 Entries found{ NewLine }";
                    //ap.ProgressValue = 100;
                    progress.Report(ap);
                    return pl;
                }
                else
                {
                    string[] entries = Regex.Split(pageText, "},{");
                    for (int i=0; i<entries.Length; i++)
                        pl.Add( new PropertyList() { Keyword = keyword, PropertyID = Regex.Match(entries[i], "propertyId\":(.+?),\"countyCode").Groups[1].Value });

                    ap.LogText = $"{ NewLine }Parsing properties summary for keyword: { keyword }{ NewLine }{ pl.Count } Entries found{ NewLine }";
                    progress.Report(ap);

                    return pl;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error occured when parsing Properties List using \"{ keyword }\" keyword{ NewLine }{ e.Message }");
                throw e;
            }
        }

        public Entry ParseEntries(AllInfoHTMLPage pages)
        {
            try
            {
                Entry entry = new Entry();

                //Parse Personal Information
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(pages.PropertyInformation);
                var divNodes = doc.DocumentNode.SelectNodes(".//div");
                for (int i = 0; i < divNodes.Count - 1; i++)
                {
                    if (divNodes[i].OuterHtml.Contains("Entry URL"))
                        entry.PropertyUrl = divNodes[i].InnerText;
                    if (divNodes[i].InnerHtml.Contains("Parcel Id") && divNodes[i].InnerHtml.Length < 50)
                        entry.ParcelID = removeHTMLChars(divNodes[i + 1].InnerText);
                    if (divNodes[i].InnerHtml.Contains("Property Address") && divNodes[i].InnerHtml.Length < 50)
                        entry.Address = removeHTMLChars(divNodes[i + 1].InnerText);
                    if (divNodes[i].InnerHtml.Contains("Owner") && divNodes[i].InnerHtml.Length < 50)
                        entry.Owner = removeHTMLChars(divNodes[i + 1].InnerText);
                    if (divNodes[i].InnerHtml.Contains("Mailing Address") && divNodes[i].InnerHtml.Length < 50)
                        entry.MailingAddress = removeHTMLChars(divNodes[i + 1].InnerText);
                    if (divNodes[i].InnerHtml.Contains("Neighborhood") && divNodes[i].InnerHtml.Length < 50)
                        entry.Neighborhood = removeHTMLChars(divNodes[i + 1].InnerText);
                    if (divNodes[i].InnerHtml.Contains("Subdivision") && divNodes[i].InnerHtml.Length < 50)
                        entry.Subdivision = removeHTMLChars(divNodes[i + 1].InnerText);
                    if (divNodes[i].InnerHtml.Contains("Appraised Value") && divNodes[i].OuterHtml.Contains("visible-xs hidden-sm visible-md hidden-lg visible-xl"))
                        entry.CurrAppraisalValue = removeHTMLChars(divNodes[i + 1].InnerText);
                }

                string tbl = doc.DocumentNode.SelectSingleNode("//table[@class='table table-striped text-center nomargin-bottom']").OuterHtml;
                var tblDoc = new HtmlAgilityPack.HtmlDocument();
                tblDoc.LoadHtml(tbl);
                entry.PropertyClass = removeHTMLChars(tblDoc.DocumentNode.SelectSingleNode("//td").InnerText);

                //Parse Building Data
                if (!pages.BuildingData.Contains("<strong>Error</strong> Data is Null."))
                {
                    var bdDoc = new HtmlAgilityPack.HtmlDocument();
                    bdDoc.LoadHtml(pages.BuildingData);
                    var bdNodes = bdDoc.DocumentNode.SelectNodes(".//div");

                    for (int i = 0; i < bdNodes.Count - 1; i++)
                    {
                        if (bdNodes[i].OuterHtml.Contains("Bedrooms") && bdNodes[i].InnerHtml.Length < 50)
                            entry.BedRooms = Convert.ToInt32(removeHTMLChars(bdNodes[i + 1].InnerText));
                        if (bdNodes[i].InnerHtml.Contains("Fullbaths") && bdNodes[i].InnerHtml.Length < 50)
                            entry.FullBaths = Convert.ToInt32(removeHTMLChars(bdNodes[i + 1].InnerText));
                        if (bdNodes[i].InnerHtml.Contains("Halfbaths") && bdNodes[i].InnerHtml.Length < 50)
                            entry.HalfBaths = Convert.ToInt32(removeHTMLChars(bdNodes[i + 1].InnerText));
                        if (bdNodes[i].InnerHtml.Contains("Family Rooms") && bdNodes[i].InnerHtml.Length < 50)
                            entry.FamilyRooms = Convert.ToInt32(removeHTMLChars(bdNodes[i + 1].InnerText));
                        if (bdNodes[i].InnerHtml.Contains("Living Area (Abv. Grade)") && bdNodes[i].InnerHtml.Length < 100)
                            entry.LivingArea = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Ground Floor Ar.") && bdNodes[i].InnerHtml.Length < 100)
                            entry.GroundFlrArea = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Finished/Rec Bsmt Area") && bdNodes[i].InnerHtml.Length < 50)
                            entry.BasementArea = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Upper Floor Area") && bdNodes[i].InnerHtml.Length < 50)
                            entry.UpperFlrArea = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Year Built") && bdNodes[i].InnerHtml.Length < 50)
                            entry.YearBuilt = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Style") && bdNodes[i].InnerHtml.Length < 50)
                            entry.Style = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Foundation") && bdNodes[i].InnerHtml.Length < 50)
                            entry.Foundation = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Basement Type") && bdNodes[i].InnerHtml.Length < 50)
                            entry.BasementType = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Last Remodel") && bdNodes[i].InnerHtml.Length < 50)
                            entry.LastRemodel = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Con. Quality") && bdNodes[i].OuterHtml.Contains("hidden-xs visible-sm hidden-md visible-lg hidden-xl") && bdNodes[i].InnerHtml.Length < 100)
                            entry.ConstructionQuality = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("Phys. Condition") && bdNodes[i].OuterHtml.Contains("hidden-xs visible-sm hidden-md hidden-lg hidden-xl") && bdNodes[i].InnerHtml.Length < 100)
                            entry.PhysicalCondition = removeHTMLChars(bdNodes[i + 1].InnerText);
                        if (bdNodes[i].InnerHtml.Contains("CDU") && bdNodes[i].OuterHtml.Contains("fa fa-question-circle title-popover") && bdNodes[i].InnerHtml.Length < 600)
                            entry.CDU = removeHTMLChars(bdNodes[i + 1].InnerText);
                    }
                }

                //Parse Tax Information
                if (!pages.TaxInformation.Contains("<strong>Error</strong> Data is Null."))
                {
                    var tiDoc = new HtmlAgilityPack.HtmlDocument();
                    tiDoc.LoadHtml(pages.TaxInformation);
                    var tiNodes = tiDoc.DocumentNode.SelectNodes(".//div");
                    for (int i = 0; i < tiNodes.Count - 1; i++)
                    {
                        if (tiNodes[i].InnerHtml.Contains("Tax:") && tiNodes[i].OuterHtml.Contains("class=\"col-xs-6\">20") && tiNodes[i].InnerHtml.Length < 100)
                            entry.PrevYearTax = removeHTMLChars(tiNodes[i + 1].InnerText);
                        if (tiNodes[i].OuterHtml.Contains("style=\"padding-right:15px;\">Total:") && tiNodes[i].InnerHtml.Length < 100)
                            entry.TotalCurrentBalance = removeHTMLChars(tiNodes[i + 1].InnerText);
                    }
                }

                ///Parse Link of Tax Record PDF
                if (pages.TaxRecordPDF != "")
                {
                    entry.TaxRecordPDF = $"https://ares.sncoapps.us{ pages.TaxRecordPDF }";
                }

                return entry;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error occured when parsing Property Information{ NewLine }{ e.Message }");
                throw e;
            }            

        }
        
        public List<BuildingComponents> ParseBuildingComponents(string page)
        {
            var bldgComponents = new List<BuildingComponents>();

            //Get parcel id
            string parcelID = Regex.Match(page, "<div class=\"data-group-sidebar\">\r\n                                    <div>Parcel Number</div>(.+?)<div>(.+?)</div>(.+?)</div>", RegexOptions.Singleline).Groups[2].Value;

            string table = "";
            var bdDoc = new HtmlAgilityPack.HtmlDocument();
            bdDoc.LoadHtml(page);
            var tblNodes = bdDoc.DocumentNode.SelectNodes(".//table");
            foreach (var tblNode in tblNodes)
            {
                if (tblNode.OuterHtml.Contains(">Component<") && !tblNode.OuterHtml.Contains(">Occupancy<"))
                {
                    table = tblNode.OuterHtml;
                    break;
                }
            }

            var m = Regex.Matches(table, "<tr>(.+?)</tr>", RegexOptions.Singleline);
            foreach (Match _m in m)
            {
                var bldgComponent = new BuildingComponents();
                if (!_m.Value.Contains(">Component<"))
                {
                    Match x = Regex.Match(_m.Value, "<td>(.+?)</td>(.+?)<td>(.+?)</td>(.+?)<td>(.+?)</td>(.+?)<td>(.+?)</td>", RegexOptions.Singleline);
                    bldgComponent.ParcelID = parcelID;
                    bldgComponent.Component = removeHTMLChars(x.Groups[1].Value);
                    bldgComponent.Units = removeHTMLChars(x.Groups[3].Value);
                    bldgComponent.YearAdded = removeHTMLChars(x.Groups[5].Value);
                    bldgComponent.Percent = removeHTMLChars(x.Groups[7].Value);

                    bldgComponents.Add(bldgComponent);
                }
            }

            return bldgComponents;
        }

        public List<OtherImprovements> ParseOtherImprovements(string page)
        {
            var otherImprovements = new List<OtherImprovements>();

            //Get parcel id
            string parcelID = Regex.Match(page, "<div class=\"data-group-sidebar\">\r\n                                    <div>Parcel Number</div>(.+?)<div>(.+?)</div>(.+?)</div>", RegexOptions.Singleline).Groups[2].Value;
            if (parcelID == "")
                parcelID = Regex.Match(page, "<div class=\"data-group-sidebar\">\r\n                                <div>Parcel Number</div>\r\n                                    <div>(.+?)</div>\r\n                            </div>\r\n                        </div>", RegexOptions.Multiline).Groups[1].Value;
            
           //Get Other Improvements table
            string table = "";
            var bdDoc = new HtmlAgilityPack.HtmlDocument();
            bdDoc.LoadHtml(page);
            var tblNodes = bdDoc.DocumentNode.SelectNodes(".//table");
            foreach (var tblNode in tblNodes)
            {
                if (tblNode.OuterHtml.Contains(">Occupancy<") && tblNode.OuterHtml.Contains(">Stories<"))
                {
                    table = tblNode.OuterHtml;
                    break;
                }
            }
            
            var m = Regex.Matches(table, "<tr>(.+?)</tr>", RegexOptions.Singleline);
            foreach (Match _m in m)
            {
                var otherImprovement = new OtherImprovements();
                if (!_m.Value.Contains(">Occupancy<"))
                {
                    Match x = Regex.Match(_m.Value, "</td>\r\n                                            <td>(.+?)</td>\r\n                                            <td>(.+?)</td>\r\n                                            <td>(.*?)</td>\r\n                                            <td>(.*?)</td>\r\n                                            <td>(.*?)</td>\r\n                                            <td>(.*?)</td>\r\n                                            <td>(.*?)</td>\r\n                                        </tr>");
                    otherImprovement.ParcelID = parcelID;
                    otherImprovement.Number = x.Groups[1].Value;
                    otherImprovement.Occupancy = x.Groups[2].Value;
                    otherImprovement.Quantity = x.Groups[3].Value;
                    otherImprovement.YearBuilt = x.Groups[4].Value;
                    otherImprovement.Stories = x.Groups[5].Value;
                    otherImprovement.Condition = x.Groups[6].Value;
                    otherImprovement.Function = x.Groups[7].Value;

                    otherImprovements.Add(otherImprovement);
                }
            }

            return otherImprovements;
        }

        public List<ComparableData> ParseComparables(string page, string pBuildingData)
        {
            var comparables = new List<ComparableData>();

            //Get parcel id
            string parcelID = Regex.Match(pBuildingData, "<div class=\"data-group-sidebar\">\r\n                                    <div>Parcel Number</div>(.+?)<div>(.+?)</div>(.+?)</div>", RegexOptions.Singleline).Groups[2].Value;
            if (parcelID == "")
                parcelID = Regex.Match(pBuildingData, "<div class=\"data-group-sidebar\">\r\n                                <div>Parcel Number</div>\r\n                                    <div>(.+?)</div>\r\n                            </div>\r\n                        </div>", RegexOptions.Multiline).Groups[1].Value;

            //Parse Comparables
            if (!page.Contains("<strong>Error</strong> Please search and select a property."))
            {
                var compDoc = new HtmlAgilityPack.HtmlDocument();
                compDoc.LoadHtml(page);
                HtmlNode compNodes;
                for (int i = 1; i < 6; i++)
                {
                    compNodes = compDoc.DocumentNode.SelectSingleNode($".//div[@id='{ i }']");

                    string cAddress = string.Empty;
                    string cSaleDate = string.Empty;
                    string cActualPrice = string.Empty;
                    string cAdjustedPrice = string.Empty;
                    string cPropUrl = string.Empty;

                    var xDoc = new HtmlAgilityPack.HtmlDocument();
                    xDoc.LoadHtml(compNodes.InnerHtml);
                    var xnodes = xDoc.DocumentNode.SelectNodes("//div");
                    for (int x=0; x<xnodes.Count; x++)
                    {
                        if (xnodes[x].InnerText == Convert.ToString(i))
                            cAddress = xnodes[x + 1].InnerText;
                        if (xnodes[x].InnerText == "Sale Date")
                            cSaleDate = xnodes[x + 1].InnerText;
                        if (xnodes[x].InnerText == "Actual Sale Price")
                            cActualPrice = xnodes[x + 1].InnerText;
                        if (xnodes[x].InnerText.Trim() == "Adjusted Sale Price&nbsp;")
                            cAdjustedPrice = xnodes[x + 1].InnerText;
                    }

                    cPropUrl = $"https://ares.sncoapps.us{ Regex.Match(compNodes.InnerHtml, "<a target=\"_blank\" href=\"(.*?)\">").Groups[1].Value }";

                    comparables.Add(new ComparableData { SubjectParcelID = parcelID, Address = cAddress, SaleDate = cSaleDate, ActualSalePrice = cActualPrice, AdjustedSalePrice = cAdjustedPrice, PropertyUrl = cPropUrl });
                }
            }

            return comparables;
        }

        public List<Address> FormatAddress(List<Entry> entries)
        {
            var address = new List<Address>();

            foreach (var entry in entries)
            {
                address.Add(new Address { ParcelID = entry.ParcelID, AddressValue = entry.Address });
                //if (entry.Address != "")
                //{
                //    List<string> add1 = SplitAddress(entry.Address);

                //    foreach (string a in add1)
                //        address.Add(new Address { ParcelID = entry.ParcelID, AddressValue = a });
                //}
            }
            return address;
        }

        public List<MailingAddress> FormatMailingAddress(List<Entry> entries)
        {
            var ma = new List<MailingAddress>();
            foreach (var entry in entries)
            {
                ma.Add(new MailingAddress { ParcelID = entry.ParcelID, MailingAddressValue = entry.MailingAddress });
                //if (entry.MailingAddress != "")
                //{
                //    List<string> add1 = SplitAddress(entry.MailingAddress);

                //    foreach (string a in add1)
                //        ma.Add(new MailingAddress { ParcelID = entry.ParcelID, MailingAddressValue = a });
                //}
            }
            return ma;
        }

        private List<string> SplitAddress(string address)
        {
            var s = new List<string>();
            var zip = "";

            s = address.Split(',').ToList();
            if (s.Count > 1)
                s[1] = s[1].Trim();
            zip = Regex.Match(s[s.Count - 1], "\\d+").Value;
            s[s.Count - 1] = s[s.Count - 1].Replace(zip, "").Trim();
            s.Add(zip);

            return s;
        }

        private string removeHTMLChars(string str)
        {
            return str.Replace("\n", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("&#xD;&#xA;", ", ")
                .Replace("&amp;", "&")
                .Replace("&#x2B;", "")
                .Replace("&ensp;", " ")
                .Replace("<div>", "").Replace("</div>", "")
                .Trim();
        }
    }
}