using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace TripAdvisor_Crawling
{
    public class Japan
    {
        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        string url = "https://www.tripadvisor.com.hk/Attractions-g294232-Activities-Japan.html";
        Database db = new Database();
        public void Crawl()
        {
            HtmlDocument doc = new HtmlDocument();
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"html\" + "Attractions-g294232-Activities-Japan.html"))
            {
                doc = GetHtml(url,  @"html\" + "Attractions-g294232-Activities-Japan.html");
            }
            else
            {
                doc.LoadHtml(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"html\" + "Attractions-g294232-Activities-Japan.html"));
            }
            Capture(doc);
        }

        public HtmlDocument GetHtml(string url, string name)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            //request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                doc.LoadHtml(data);

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + name, data);

                response.Close();
                readStream.Close();
            }
            return doc;

        }

       
        public void Capture(HtmlDocument doc)
        {
            List<string> urlList = new List<string>();
            foreach (HtmlNode hyperLink in doc.DocumentNode.SelectNodes("//a"))
            {
                if (hyperLink.InnerHtml.Contains("查看全部") && hyperLink.Attributes["href"].Value.Contains(".html"))
                {
                    urlList.Add(hyperLink.Attributes["href"].Value.ToString());
                }
            }
            Thread.Sleep(1000);
            List<string> firstLvlUrlList = new List<string>();
            for (int i = 0; i < urlList.Count; i++)
            {
                HtmlDocument firstLevelDoc = new HtmlDocument();
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"html\" + urlList[i]))
                {
                    firstLevelDoc = GetHtml("https://www.tripadvisor.com.hk/" + urlList[i], @"html\" + urlList[i]);
                }
                else
                {
                    firstLevelDoc.LoadHtml(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"html\" + urlList[i]));
                }

                foreach (HtmlNode hyperLink in firstLevelDoc.DocumentNode.SelectNodes("//a"))
                {
                    if (hyperLink.InnerHtml.Contains("更多資訊") && hyperLink.Attributes["href"].Value.Contains(".html"))
                    {
                        HtmlNode div = hyperLink.SelectSingleNode("div");
                        if (div.HasClass("display_text") && div.HasClass("ui_button") && div.HasClass("original") && div.InnerHtml == "更多資訊")
                        {
                            firstLvlUrlList.Add(hyperLink.Attributes["href"].Value.ToString());
                        }
                    }
                }
            }
            Thread.Sleep(1000);
            Console.WriteLine("Total first lvl url = " + firstLvlUrlList.Count);
            for (int i = 0; i < firstLvlUrlList.Count; i++)
            {
                
                location l = new location();
                l.country_en = "Japan";
                l.country_tc = "日本";
                l.url = firstLvlUrlList[i];
                Console.WriteLine("First lvl:" + (i + 1) + "/" + firstLvlUrlList.Count);
                try
                {
                    if (!db.CheckCrawled(firstLvlUrlList[i]))
                    {
                        Console.WriteLine("First lvl:" + (i + 1) + "/" + firstLvlUrlList.Count);
                        HtmlDocument secondLvlDoc = new HtmlDocument();
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"html\" + firstLvlUrlList[i]))
                        {
                            secondLvlDoc = GetHtml("https://www.tripadvisor.com.hk/" + firstLvlUrlList[i], @"html\" + firstLvlUrlList[i]);
                        }
                        else
                        {
                            secondLvlDoc.LoadHtml(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"html\" + firstLvlUrlList[i]));
                        }

                        foreach (HtmlNode h1 in secondLvlDoc.DocumentNode.SelectNodes("//h1"))
                        {
                            if (h1.Id == "HEADING" && h1.HasClass("heading_title"))
                            {
                                if (h1.InnerHtml.IndexOf("<") > -1)
                                {
                                    l.name_tc = h1.InnerHtml.Substring(0, h1.InnerHtml.IndexOf("<")).Trim();
                                    HtmlNode span = h1.SelectSingleNode("span");
                                    if (span != null)
                                    {
                                        l.name_en = span.InnerHtml;
                                    }
                                }
                                else
                                {
                                    l.name_tc = h1.InnerHtml;
                                    l.name_en = h1.InnerHtml;
                                }
                                break;
                            }
                            else if (h1.Id == "HEADING" && h1.HasClass("ui_header") && h1.HasClass("h1"))
                            {
                                if (h1.InnerHtml.IndexOf("<") > -1)
                                {
                                    l.name_tc = h1.InnerHtml.Substring(0, h1.InnerHtml.IndexOf("<")).Trim();
                                    HtmlNode div = h1.SelectSingleNode("div");
                                    if (div != null)
                                    {
                                        l.name_en = div.InnerHtml;
                                    }
                                }
                                else
                                {
                                    l.name_tc = h1.InnerHtml;
                                    l.name_en = h1.InnerHtml;
                                }
                                break;
                            }
                        }

                        foreach (HtmlNode div in secondLvlDoc.DocumentNode.SelectNodes("//div"))
                        {
                            if ( div.HasClass("detail"))
                            {
                                string value = "";
                                value += div.InnerHtml + " ";
                   
                                if (value.Contains("大眾運輸系統"))
                                {
                                    l.type_id = 1;
                                    break;
                                }
                                else
                                {
                                    l.type_id = 2;
                                    break;
                                }
                            }
                        }

                        foreach (HtmlNode span in secondLvlDoc.DocumentNode.SelectNodes("//span"))
                        {
                            if (span.HasClass("detail"))
                            {
                                string spanInner = span.InnerHtml;
                                foreach(HtmlNode childSpan in span.SelectNodes("span"))
                                {
                                    if (childSpan.HasClass("locality"))
                                    {
                                        l.district_tc = childSpan.InnerHtml;
                                    }
                                    if (childSpan.HasClass("street-address"))
                                    {
                                        l.address_tc = childSpan.InnerHtml;
                                    }
                                    if (childSpan.HasClass("postal-code"))
                                    {
                                        l.postal_code = childSpan.InnerHtml;
                                    }
                                    if (childSpan.HasClass("extended-address"))
                                    {
                                        l.extend_address = childSpan.InnerHtml;
                                    }
                                    spanInner = spanInner.Replace(childSpan.OuterHtml, "").Replace("|", "").Trim();
                                }
                                l.area_tc = spanInner;
                            }
                        }

                        foreach (HtmlNode div in secondLvlDoc.DocumentNode.SelectNodes("//div"))
                        {
                            if (div.HasClass("blEntry") && div.HasClass("address") && div.HasClass("clickable") && div.HasClass("colCnt2"))
                            {
                                string html = div.InnerHtml;
                                foreach (HtmlNode childDiv in div.SelectNodes("span"))
                                {
                                    if (childDiv.HasClass("locality"))
                                    {
                                        if (l.district_tc != "")
                                        {
                                            l.district_tc = childDiv.InnerHtml;
                                        }
                                    }
                                    if (childDiv.HasClass("street-address"))
                                    {
                                        if (l.address_tc != "")
                                        {
                                            l.address_tc = childDiv.InnerHtml;
                                        }
                                    }
                                    if (childDiv.HasClass("postal-code"))
                                    {
                                        if (l.postal_code != "")
                                        {
                                            l.postal_code = childDiv.InnerHtml;
                                        }
                                    }
                                    if (childDiv.HasClass("extended-address"))
                                    {
                                        if (l.extend_address != "")
                                        {
                                            l.extend_address = childDiv.InnerHtml;
                                        }
                                    }
                                    html = html.Replace(childDiv.OuterHtml, "");
                                }
                                if (l.area_tc != "")
                                {
                                    l.area_tc = html.Replace("|", "").Replace(",", "").Trim();
                                }
                            }
                        }

                        foreach (HtmlNode div in secondLvlDoc.DocumentNode.SelectNodes("//div"))
                        {
                            if (div.HasClass("blEntry") && div.HasClass("phone"))
                            {
                                foreach (HtmlNode childSpan in div.SelectNodes("span"))
                                {
                                    if (!childSpan.HasClass("ui_icon") && !childSpan.HasClass("phone"))
                                    {
                                        l.phone = childSpan.InnerHtml;
                                        break;
                                    }
                                }
                                break;
                            }
                        }

                        l.imageList = new List<string>();
                        foreach (HtmlNode img in secondLvlDoc.DocumentNode.SelectNodes("//img"))
                        {
                            if (img.Attributes["src"] != null)
                            {
                                if (img.Attributes["src"].Value.Contains("photo-s") || img.Attributes["src"].Value.Contains("photo-w"))
                                {
                                    l.imageList.Add(img.Attributes["src"].Value);
                                }
                            }
                        }

                        Thread.Sleep(2000);

                        HtmlDocument secondLvlDocEn = new HtmlDocument();
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"htmlEn\" + firstLvlUrlList[i]))
                        {
                            secondLvlDocEn = GetHtml("https://en.tripadvisor.com.hk/" + firstLvlUrlList[i], @"htmlEN\" + firstLvlUrlList[i]);
                        }
                        else
                        {
                            secondLvlDocEn.LoadHtml(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"htmlEn\" + firstLvlUrlList[i]));
                        }

                        foreach (HtmlNode span in secondLvlDocEn.DocumentNode.SelectNodes("//span"))
                        {
                            if (span.HasClass("detail"))
                            {
                                string spanInner = span.InnerHtml;
                                if (span.SelectNodes("span") != null)
                                {
                                    foreach (HtmlNode childSpan in span.SelectNodes("span"))
                                    {
                                        if (childSpan.HasClass("locality"))
                                        {
                                            l.district_en = childSpan.InnerHtml.Replace(",", "").Trim();
                                        }
                                        if (childSpan.HasClass("street-address"))
                                        {
                                            l.address_en = childSpan.InnerHtml.Replace(",", "").Trim();
                                        }
                                        spanInner = spanInner.Replace(childSpan.OuterHtml, "");
                                    }
                                }
                                l.area_en = spanInner.Replace("|", "").Replace(",", "").Trim();
                            }
                        }

                        foreach (HtmlNode div in secondLvlDocEn.DocumentNode.SelectNodes("//div"))
                        {
                            //blEntry address  clickable colCnt0
                            if (div.HasClass("blEntry") && div.HasClass("address") && div.HasClass("clickable"))
                            {
                                string html = div.InnerHtml;
                                
                                foreach (HtmlNode childDiv in div.SelectNodes("span"))
                                {
                                    if (childDiv.HasClass("locality"))
                                    {
                                        if (l.district_en != "")
                                        {
                                            l.district_en = childDiv.InnerHtml.Trim().Replace(",", "");
                                        }
                                    }
                                    if (childDiv.HasClass("street-address"))
                                    {
                                        if (l.address_en != "")
                                        {
                                            l.address_en = childDiv.InnerHtml;
                                        }
                                    }
                                    html = html.Replace(childDiv.OuterHtml, "");
                                }
                                if (l.area_en != "")
                                {
                                    l.area_en = html.Replace(",", "").Replace("|", "");
                                }
                            }
                        }

                        db.InsertDB(l);
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", l.url + " , crawling error" + System.Environment.NewLine);
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", ex.Message + System.Environment.NewLine);
                }
            }
        }
    }
}
