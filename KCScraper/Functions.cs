using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Environment;

namespace KCScraper
{
    class Functions
    {
        public void SetSummary(ref TextBox t, bool status)
        {
            if (status)
            {
                t.Text = $"Website is online. Ready to scrape.{ NewLine }";
                t.ForeColor = Color.Green;
            }
            else
                t.Text = $"Could not connect to website.{ NewLine }";
        }

        public List<string> ParseMultipleKeywords(string keyword)
        {
            var keywordList = new List<string>();

            keywordList = keyword.Split(',').ToList();

            return keywordList;
        }
    }
}
  