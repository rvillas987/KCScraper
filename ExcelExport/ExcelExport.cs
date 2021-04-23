using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Models;
using OfficeOpenXml;
using static Models.EntryModel;

namespace ExcelExport
{
    public class ExportToExcel
    {
        public string GetSavePath(ref TextBox txt, ref ComboBox cboFilter)
        {
            //Make a dialog appear for the user to select a save file location
            string filePath = "";
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "xlsx";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                filePath = sfd.FileName;
            }
            
            //Use the first 8 letters of the keyword followed by the criteria as the filename for the excel file
            return $"{ filePath }";
        }

        public void CreateExcelFile(string filePath)
        {
            //Make an empty excel file in the location selected by the user
            using (ExcelPackage excel = new ExcelPackage())
            {
                ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Property Info");                
                FileInfo excelFile = new FileInfo(filePath);
                excel.SaveAs(excelFile);
            }
        }

        public void ExportEntriesToExcel(string filePath, List<Entry> entries, List<BuildingComponents> bc, List<OtherImprovements> oi, List<Address> a, List<MailingAddress> ma, List<ComparableData> cd)
        {
            FileInfo file = new FileInfo(filePath);

            using (ExcelPackage exlPackage = new ExcelPackage(file))
            {
                ExcelWorkbook wb = exlPackage.Workbook;

                //Save parsed entries into Property Info Sheet in excel
                ExcelWorksheet wsEntries = exlPackage.Workbook.Worksheets["Property Info"];
                wsEntries.Cells.LoadFromCollection(entries, true);

                //Make a new sheet called Building Components and save parsed entries into the sheet
                ExcelWorksheet wsBldgComponents = exlPackage.Workbook.Worksheets.Add("Building Components");
                wsBldgComponents.Cells.LoadFromCollection(bc, true);

                //Make a new sheet called Other Improvements and save parsed entries into the sheet
                ExcelWorksheet wsOtherImprovements = exlPackage.Workbook.Worksheets.Add("Other Improvements");
                wsOtherImprovements.Cells.LoadFromCollection(oi, true);

                //Make a new sheet called Address and save formatted address into the sheet
                ExcelWorksheet wsAddress = exlPackage.Workbook.Worksheets.Add("Address");
                wsAddress.Cells.LoadFromCollection(a, true);

                //Make a new sheet called Mailing Address and save formatted mailing address into the sheet
                ExcelWorksheet wsMailing = exlPackage.Workbook.Worksheets.Add("Mailing Address");
                wsMailing.Cells.LoadFromCollection(ma, true);

                //Make a new sheet called Comparables and save parsed entries into the sheet
                ExcelWorksheet wsComparables = exlPackage.Workbook.Worksheets.Add("Comparables");
                wsComparables.Cells.LoadFromCollection(cd, true);

                //Save all changes into the excel file
                exlPackage.Save();
            }


            //Format property and mailing address sheet
            FormatAddress(file);
        }

        private void FormatAddress(FileInfo f)
        {
            using (ExcelPackage ep = new ExcelPackage(f))
            {
                ExcelWorkbook ewb = ep.Workbook;
                ExcelWorksheets ews = ewb.Worksheets;

                foreach (var _ews in ews)
                {
                    if (_ews.Name == "Address" || _ews.Name == "Mailing Address")
                    {
                        SetColumnHeader(_ews);
                        char col = 'B';
                        
                        for (int i = 0; i < _ews.Dimension.Rows - 1; i++)
                        {
                            string fullAddress = _ews.Cells[$"{ col }{ i + 2 }"].Text;
                            var splitAddress = SplitAddress(fullAddress);
                            foreach (var a in splitAddress)
                            {
                                _ews.Cells[$"{ col }{ i + 2 }"].Value = a;
                                col++;
                            }

                            col = 'B';
                        }

                        ep.Save();
                    }
                }
            }
        }

        private void SetColumnHeader(ExcelWorksheet e)
        {
            e.Cells[$"B1"].Value = "Address1";
            e.Cells[$"C1"].Value = "Address2";
            e.Cells[$"D1"].Value = "City";
            e.Cells[$"E1"].Value = "State";
            e.Cells[$"F1"].Value = "Zip Code";
        }
        
        private List<string> SplitAddress(string address)
        {
            var s = new List<string>();
            string zip = "";

            //skip split if address is n/a
            if (address == "N/A")
                return s;                

            //split address by comma
            s = address.Split(',').ToList();
            if (s.Count > 1)
                s[1] = s[1].Trim();

            //get zip
            zip = GetZip(s[s.Count - 1]);
            s[s.Count - 1] = s[s.Count - 1].Replace(zip, "").Trim();
            s.Add(zip);

            //remove Attn: and C/O
            if (s[0].Contains("Attn:") || s[0].Contains("C/O"))
                s.RemoveAt(0);

            if (s.Count == 4)
            {
                s.Insert(1, "");
                return s;
            }
            else
            {
                if (!s[0].Any(char.IsDigit) || !s[0].Contains(" N ") || !s[0].Contains(" NE ") || !s[0].Contains(" NW ") || !s[0].Contains(" S ") || !s[0].Contains(" SE ") || !s[0].Contains(" SW "))
                {
                    s.RemoveAt(0);

                    if (s.Count == 4)
                    {
                        s.Insert(1, "");
                        return s;
                    }
                }
                else
                {
                    if (s.Count == 5)
                        return s;
                    else
                    {
                        s[0] = $"{ s[0] }, { s[1] }";
                        s.RemoveAt(1);

                        if (s.Count == 5)
                            return s;
                        else
                        {
                            s[0] = $"{ s[0] }, { s[1] }";
                            s.RemoveAt(1);
                            return s;
                        }
                    }
                }
            }

            return s;
        }

        private string GetZip(string s)
        {
            if (s.Contains("-"))
                return Regex.Match(s, "\\d+-\\d+").Value;
            else
                return Regex.Match(s, "\\d+").Value;
        }
    }
}
