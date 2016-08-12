using ClosedXML.Excel;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Car_Scrapper
{
    class Scrapper
    {
        public static DataTable table1 = new DataTable("Cars");

        public static string today = DateTime.Today.ToString("dd-MM-yyyy");

        public static ManualResetEvent mre = new ManualResetEvent(true);

        public static int Scrapped1 = 0;
        public static int Scrapped2 = 0;

        public static Thread workerThread;
        public static Thread workerThread2;


        public static void Log(string text)
        {
            MainWindow.UI.Dispatcher.Invoke((Action)(() =>
            {
                MainWindow.UI.LogBox.Text += text + "\n";
            }));
        }

        public static void Log2(string text2)
        {
            MainWindow.UI.Dispatcher.Invoke((Action)(() =>
            {
                MainWindow.UI.LogBox2.Text += text2 + "\n";
            }));
        }

        public static void CreateTable()
        {
            foreach (string a in Variables.ExcelColumns)
            {
                table1.Columns.Add(a, typeof(String));
            }
        }



        public static void Scrape(int pageNumber, int threadNumber)
        {
            string Url = string.Empty;
            MainWindow.UI.Dispatcher.Invoke((Action)(() =>
            {
                Url = MainWindow.UI.SearchLink.Text;
            }));

            HtmlWeb web = new HtmlWeb();
            Url = Utility.RemoveQueryStringByKey(Url, "page") + "&page=" + pageNumber.ToString();
            //Url = Url + "&page=" + pageNumber.ToString();
            if(threadNumber == 1)
            {
                MainWindow.UI.Dispatcher.Invoke((Action)(() =>
                {
                    MainWindow.UI.pageLabel1.Content = pageNumber.ToString();
                }));
            }
            else
            {
                MainWindow.UI.Dispatcher.Invoke((Action)(() =>
                {
                    MainWindow.UI.pageLabel2.Content = pageNumber.ToString();
                }));
            }

            HtmlDocument doc = web.Load(Url);
            HtmlNodeCollection allElementsWithClass = doc.DocumentNode.SelectNodes("//*[contains(@class,'listingTitle')]");


            foreach(HtmlNode node in allElementsWithClass)
            {
                mre.WaitOne();
                //Log(node.Descendants("a").First().Attributes["href"].Value);
                HtmlDocument carDoc = web.Load("http://www.trademe.co.nz" + node.Descendants("a").First().Attributes["href"].Value);
                if(carDoc.GetElementbyId("ListingTitle_title") == null)
                {
                    Log("No car on this page!");
                    return;
                }

                Dictionary<string, string> props = Regex.Match(carDoc.DocumentNode.OuterHtml, string.Format("\\[(.|\n)*?\\]")).Value
                    .Trim(new Char[] { '{', '}', '[', ']' }).Replace("\"", "")
                    .Split(',')
                    .Select(x => x.Split(':'))
                    .Where(x => x.Length == 2)
                    .ToDictionary(x => x[0], x => x[1]);

                DataRow workRow = table1.NewRow();
                foreach (KeyValuePair<string, string> entry in props)
                {
                    workRow[entry.Key.Trim()] = entry.Value.Trim(); ;
                }

                foreach (KeyValuePair<string, string> currentId in Variables.IDList)
                {
                    HtmlNode element = carDoc.GetElementbyId(currentId.Key);
                    if (element != null)
                    {
                        //Log((carDoc.GetElementbyId(currentId.Key).InnerHtml.Trim()));
                        workRow[currentId.Value] = element.InnerText.Trim().Replace("\n", "");
                        //Log(element.InnerText.Trim().Replace("\n",""));
                    }
                    else
                    {
                        workRow[currentId.Value] = "N/A";
                    }
                }

                HtmlNodeCollection FuelSaverEle = carDoc.DocumentNode.SelectNodes("//*[contains(@class,'FuelSaverText')]");
                if (FuelSaverEle != null)
                {
                    workRow["Fuel Economy"] = FuelSaverEle.First().InnerText.Replace("\n", ","); ;
                }
                else
                {
                    workRow["Fuel Economy"] = "N/A";
                }

                HtmlNode SafetyEle = carDoc.GetElementbyId("DriverSafetyControl_safetyRatingStarsCount");
                if (SafetyEle != null)
                {
                    workRow["Safaty Rating"] = SafetyEle.InnerText.Replace("\n", ","); ;
                }
                else
                {
                    workRow["Safaty Rating"] = "N/A";
                }

                if(threadNumber == 1)
                {
                    MainWindow.UI.Dispatcher.Invoke((Action)(() =>
                    {
                        MainWindow.UI.LogBox.AppendText(carDoc.GetElementbyId("ListingTitle_title").InnerHtml.Trim() + "\n\n");
                        MainWindow.UI.LogBox.Focus();
                        MainWindow.UI.LogBox.CaretIndex = MainWindow.UI.LogBox.Text.Length;
                        MainWindow.UI.LogBox.ScrollToEnd();
                    }));
                }
                else
                {
                    MainWindow.UI.Dispatcher.Invoke((Action)(() =>
                    {
                        MainWindow.UI.LogBox2.AppendText(carDoc.GetElementbyId("ListingTitle_title").InnerHtml.Trim() + "\n\n");
                        MainWindow.UI.LogBox2.Focus();
                        MainWindow.UI.LogBox2.CaretIndex = MainWindow.UI.LogBox.Text.Length;
                        MainWindow.UI.LogBox2.ScrollToEnd();
                    }));
                }


                HtmlNodeCollection AttributesTableRows = carDoc.GetElementbyId("ListingAttributes").SelectNodes("tr"); ;
                foreach (HtmlNode row in AttributesTableRows)
                {
                    string Attribute = row.SelectNodes("th").First().InnerText.Trim().Replace("\n","");
                    //Log(row.SelectNodes("th").First().InnerText.Trim().Replace("\n", ""));
                    string Value = row.SelectNodes("td").First().InnerText.Replace(",", "").Trim(); ;
                    switch (Attribute)
                    {
                        case "On Road Costs:&nbsp;":
                            workRow["On Road Costs"] = Value.Replace("\n", ",");
                            break;
                        case "Number plate:":
                            workRow["Number Plate"] = Value.Replace("\n", ",");
                            break;
                        case "Body:":
                            workRow["Body"] = Value.Replace("\n", ",");
                            break;
                        case "Fuel type:":
                            workRow["Fuel Type"] = Value.Replace("\n", ",");
                            break;
                        case "Engine:":
                            workRow["Engine"] = Value.Replace("\n", ",");
                            break;
                        case "Transmission:":
                            workRow["Transmisssion"] = Value.Replace("\n", ",");
                            break;
                        case "4WD:":
                            workRow["4WD"] = Value.Replace("\n", ",");
                            break;
                        case "History:":
                            workRow["History"] = Value.Replace("\n", ",");
                            break;
                        case "Registration expires:":
                            workRow["Regitration Expires"] = Value.Replace("\n", ",");
                            break;
                        case "WOF expires:":
                            workRow["WOF Expires"] = Value.Replace("\n", ",");
                            break;
                        case "Stereo description:":
                            workRow["Stereo description"] = Value.Replace("\n", ",");
                            break;
                        case "Model Detail:":
                            workRow["Model Detail"] = Value.Replace("\n", ",");
                            break;
                        case "Features:":
                            Value = Value.Replace("\n", ",");
                            workRow["Features"] = Value.Replace("\n", ",");
                            break;
                        case "Engine size:":
                            Value = Value.Replace("\n", ",");
                            workRow["Engine Size"] = Value.Replace("\n", ",");
                            break;
                        case "Import history:":
                            Value = Value.Replace("\n", ",");
                            workRow["Import History"] = Value;
                            break;
                    }
                }
                if(threadNumber == 1)
                {
                    Scrapped1++;
                    MainWindow.UI.Dispatcher.Invoke((Action)(() =>
                    {
                        MainWindow.UI.Scrapped1Lab.Content = Scrapped1.ToString();
                    }));

                }
                else
                {
                    Scrapped2++;
                    MainWindow.UI.Dispatcher.Invoke((Action)(() =>
                    {
                        MainWindow.UI.Scrapped2Lab.Content = Scrapped2.ToString();
                    }));
                }
                MainWindow.UI.Dispatcher.Invoke((Action)(() =>
                {
                    MainWindow.UI.TotalLab.Content = (Scrapped1 + Scrapped2).ToString();
                }));
                mre.WaitOne();
                table1.Rows.Add(workRow);
            }         
        }

        public static void botWorker1()
        {
            for (int i = 1; i<=10000; i++)
            {
                if (i % 2 != 0)
                {
                    Scrape(i,1);

                    XLWorkbook workbook = new XLWorkbook();
                    try
                    {
                        IXLWorksheets sheets = workbook.Worksheets;
                        sheets.Add(table1);
                        workbook.SaveAs(today + " Carz.xlsx");
                        sheets = null;
                    }
                    catch {   }
                    workbook = null;
                    Log("Saved xml.");
                    Log("Thread 1 : Next page");
                }
            }
            
        }
        public static void botWorker2()
        {
            for (int i = 1; i <= 10000; i++)
            {
                if (i % 2 == 0)
                {
                    Scrape(i,2);
                    Log2("Thread 2 : Next page");
                }                 
            }
        }
        

        public static void botStuff()
        {
            Log("Starting..");
            Log2("Starting..");

            foreach (string a in Variables.ExcelColumns)
            {
                table1.Columns.Add(a, typeof(String));
            }
            workerThread = new Thread(botWorker1);
            workerThread.Start();
            workerThread2 = new Thread(botWorker2);
            workerThread2.Start();
        }
    }
}
