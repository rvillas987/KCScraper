using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using Scraper;
using Parser;
using ExcelExport;
using static Models.EntryModel;
using static System.Environment;
using System.Threading.Tasks;

namespace KCScraper
{
    public partial class MainForm : Form
    {
        List<Keywords> keywords = new List<Keywords>();
        CheckUrlStatus c = new CheckUrlStatus();
        ScrapeEntries ge = new ScrapeEntries();
        Functions f = new Functions();
        EntryParser ep = new EntryParser();
        List<PropertyList> pl = new List<PropertyList>();
        List<AllInfoHTMLPage> pages = new List<AllInfoHTMLPage>();
        List<Entry> entries = new List<Entry>();
        List<BuildingComponents> bc = new List<BuildingComponents>();
        List<OtherImprovements> oi = new List<OtherImprovements>();
        List<ComparableData> cd = new List<ComparableData>();
        List<Address> a = new List<Address>();
        List<MailingAddress> ma = new List<MailingAddress>();
        ExportToExcel exl = new ExportToExcel();

        public MainForm()
        {
            InitializeComponent();

            //Check if website is working
            f.SetSummary(ref txtLogs, c.checkStatus());
        }

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            //Takes care of logging and progress bar inside async operation
            Progress<AsyncProgress> progress = new Progress<AsyncProgress>();
            progress.ProgressChanged += ReportProgress;
            
            //Parse multiple keywords
            keywords = ep.ParseKeywords(txtCriteria.Text);

            //Get all the Property Information from website (JSON Format)
            //pl = ge.GetEntriesList(ref progBar, ref txtLogs, cboFilter.SelectedIndex, keywords);
            int cs = cboFilter.SelectedIndex;
            pl = await Task.Run(() => GetEntriesListAsync(cs, keywords, progress));

            //Check if properties found during search
            if (pl.Count > 0)
            {
                //Get all Property Info webpages for scraping
                pages = await Task.Run(() => GetPropertyInforHTMLAsync(pl, progress));

                //Get all the entries found from the website
                entries = ge.GetAllEntries(ref txtLogs, pages);

                //Get all Building Components data from website
                bc = ge.GetAllBuildingComponents(ref txtLogs, pages);

                //Get all Other Improvements data from website
                oi = ge.GetAllOtherImprovements(ref txtLogs, pages);

                //Get all Comparables data from website
                cd = ge.GetAllComparables(pages);

                //Split address into sections
                a = ep.FormatAddress(entries);

                //Split mailing address into sections
                ma = ep.FormatMailingAddress(entries);

                txtLogs.AppendText($"Data is ready for export{ NewLine }{ NewLine }");
                progBar.Value = 100;

            }
            else
            {
                //No properties found during search
                txtLogs.AppendText($"No property found when searching by { (SearchBy)cboFilter.SelectedIndex } using the \"{ txtCriteria.Text }\" keyword");
            }
        }

        private void ReportProgress(object sender, AsyncProgress e)
        {
            txtLogs.AppendText(e.LogText);
            progBar.Value = e.ProgressValue;
            lblProgress.Text = e.ProgressText;
        }

        private List<PropertyList> GetEntriesListAsync(int selectedFilter, List<Keywords> keywords, IProgress<AsyncProgress> progress)
        {
            var pa = new AsyncProgress();
            pa.LogText = $"{ NewLine }Retrieve properties summary. . . .{ NewLine }";
            pa.ProgressText = "";
            progress.Report(pa);

            var pl = new List<PropertyList>();
            pl = ge.GetEntriesList(selectedFilter, keywords, progress);
            
            return pl;
        }

        private List<AllInfoHTMLPage> GetPropertyInforHTMLAsync(List<PropertyList> pl, IProgress<AsyncProgress> progress)
        {
            List<AllInfoHTMLPage> pages = new List<AllInfoHTMLPage>();
            pages = ge.GetAllPropertyInfoPages(pl, progress);
            return pages;
        }
        
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            //Get save file location of the excel file
            string fileLoc = exl.GetSavePath(ref txtCriteria, ref cboFilter);
            txtLogs.AppendText($"Save file location is set.{ NewLine }");

            //Create an empty excel file
            exl.CreateExcelFile(fileLoc);
            txtLogs.AppendText($"Done creating excel file.{ NewLine }");

            //Export all data to excel file
            exl.ExportEntriesToExcel(fileLoc, entries, bc, oi, a, ma, cd);
            txtLogs.AppendText($"Fill excel file with property data.{ NewLine }");

            txtLogs.AppendText($"Excel file is now ready.{ NewLine }");
            Console.WriteLine();
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            //Set default selection to comboboxes
            cboFilter.SelectedIndex = 0;

            lblProgress.Text = "";
        }
        
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            txtCriteria.Width = this.Width - 267;

            btnGenerate.Location = new Point(this.Width - 148, btnGenerate.Location.Y);

            txtLogs.Width = this.Width - 44;
            txtLogs.Height = this.Height - 211;

            progBar.Location = new Point(16, this.Height - 106);
            progBar.Width = this.Width - 44;

            lblProgress.Location = new Point(13, this.Height - 80);

            btnExportExcel.Location = new Point(this.Width - 148, this.Height - 76);

        }
    }
}
