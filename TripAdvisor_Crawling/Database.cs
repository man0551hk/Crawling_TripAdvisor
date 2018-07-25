using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Web.Script.Serialization;

namespace TripAdvisor_Crawling
{
    public class Database
    {
        MySqlConnection cn = new MySqlConnection(ConfigurationManager.ConnectionStrings["sq_traffiter"].ConnectionString);

        public void InsertDB(location l)
        {
            try
            {
                cn.Open();
                MySqlCommand checkCountryCmd = new MySqlCommand("select country_id from country where country_tc = @country_tc and country_en = @country_en", cn);
                checkCountryCmd.CommandType = CommandType.Text;
                checkCountryCmd.Parameters.Add("@country_tc", MySqlDbType.VarChar).Value = l.country_tc;
                checkCountryCmd.Parameters.Add("@country_en", MySqlDbType.VarChar).Value = l.country_en;
                int country_id = 0;
                if (checkCountryCmd.ExecuteScalar() != null)
                { 
                    country_id = Int32.Parse(checkCountryCmd.ExecuteScalar().ToString());
                }
                if (country_id == 0)
                {
                    MySqlCommand insertCountryCmd = new MySqlCommand("insert into country (country_en, country_tc, country_sc) values (@country_en, @country_tc, @country_sc); select last_insert_id();", cn);
                    insertCountryCmd.CommandType = CommandType.Text;
                    insertCountryCmd.Parameters.Add("@country_en", MySqlDbType.VarChar).Value = l.country_en;
                    insertCountryCmd.Parameters.Add("@country_tc", MySqlDbType.VarChar).Value = l.country_tc;
                    insertCountryCmd.Parameters.Add("@country_sc", MySqlDbType.VarChar).Value = Microsoft.VisualBasic.Strings.StrConv(l.country_tc, VbStrConv.SimplifiedChinese, 2052);
                    country_id = Int32.Parse(insertCountryCmd.ExecuteScalar().ToString());
                }

                int area_id = 0;
                if (!string.IsNullOrEmpty(l.area_tc) && !string.IsNullOrEmpty(l.area_en))
                {
                    MySqlCommand checkAreaCmd = new MySqlCommand("select area_id from area where area_tc = @area_tc and area_en = @area_en and country_id = @country_id", cn);
                    checkAreaCmd.CommandType = CommandType.Text;
                    checkAreaCmd.Parameters.Add("@area_tc", MySqlDbType.VarChar).Value = l.area_tc;
                    checkAreaCmd.Parameters.Add("@area_en", MySqlDbType.VarChar).Value = l.area_en;
                    checkAreaCmd.Parameters.Add("@country_id", MySqlDbType.Int32).Value = country_id;
                    if (checkAreaCmd.ExecuteScalar() != null)
                    {
                        area_id = Int32.Parse(checkAreaCmd.ExecuteScalar().ToString());
                    }

                    if (area_id == 0)
                    {
                        MySqlCommand insertAreaCmd = new MySqlCommand("insert into area (country_id, area_en, area_tc, area_sc) values (@country_id, @area_en, @area_tc, @area_sc); select last_insert_id();", cn);
                        insertAreaCmd.CommandType = CommandType.Text;
                        insertAreaCmd.Parameters.Add("@country_id", MySqlDbType.Int32).Value = country_id;
                        insertAreaCmd.Parameters.Add("@area_en", MySqlDbType.VarChar).Value = l.area_en;
                        insertAreaCmd.Parameters.Add("@area_tc", MySqlDbType.VarChar).Value = l.area_tc;
                        insertAreaCmd.Parameters.Add("@area_sc", MySqlDbType.VarChar).Value = Microsoft.VisualBasic.Strings.StrConv(l.area_tc, VbStrConv.SimplifiedChinese, 2052);
                        area_id = Int32.Parse(insertAreaCmd.ExecuteScalar().ToString());
                    }
                }

                int district_id = 0;
                if (!string.IsNullOrEmpty(l.district_en) && !string.IsNullOrEmpty(l.district_tc))
                {
                    MySqlCommand checkDistrictCmd = new MySqlCommand("select district_id from district where district_tc = @district_tc and district_en = @district_en and area_id = @area_id", cn);
                    checkDistrictCmd.CommandType = CommandType.Text;
                    checkDistrictCmd.Parameters.Add("@district_tc", MySqlDbType.VarChar).Value = l.area_tc;
                    checkDistrictCmd.Parameters.Add("@district_en", MySqlDbType.VarChar).Value = l.area_en;
                    checkDistrictCmd.Parameters.Add("@area_id", MySqlDbType.Int32).Value = area_id;
                    
                    if (checkDistrictCmd.ExecuteScalar() != null)
                    {
                        district_id = Int32.Parse(checkDistrictCmd.ExecuteScalar().ToString());
                    }

                    if (district_id == 0)
                    {
                        MySqlCommand insertDistrictCmd = new MySqlCommand(@"insert into district (country_id, area_id, district_en, district_tc, district_sc) values 
                                                                    (@country_id, @area_id, @district_en, @district_tc, @district_sc); 
                                                                                            select last_insert_id();", cn);
                        insertDistrictCmd.CommandType = CommandType.Text;
                        insertDistrictCmd.Parameters.Add("@country_id", MySqlDbType.Int32).Value = country_id;
                        insertDistrictCmd.Parameters.Add("@area_id", MySqlDbType.Int32).Value = area_id;
                        insertDistrictCmd.Parameters.Add("@district_en", MySqlDbType.VarChar).Value = l.district_en;
                        insertDistrictCmd.Parameters.Add("@district_tc", MySqlDbType.VarChar).Value = l.district_tc;
                        insertDistrictCmd.Parameters.Add("@district_sc", MySqlDbType.VarChar).Value = Microsoft.VisualBasic.Strings.StrConv(l.district_tc, VbStrConv.SimplifiedChinese, 2052);
                        district_id = Int32.Parse(insertDistrictCmd.ExecuteScalar().ToString());
                    }

                }
                string address = "";
                if (!string.IsNullOrEmpty(l.address_en))
                {
                    address += l.address_en + ",";
                }
                if (!string.IsNullOrEmpty(l.extend_address))
                {
                    address += l.extend_address + ",";
                }
                if (!string.IsNullOrEmpty(l.district_en))
                {
                    address += l.district_en + ",";
                }
                if (!string.IsNullOrEmpty(l.area_en))
                {
                    address += l.area_en + ",";
                }
                address += l.country_en;
                GoogleMapAPI response = GetGoogleLatLon(address);

                MySqlCommand checkLocationCmd = new MySqlCommand("select location_id from location where address_en = @address_en and address_tc = @address_tc and district_id = @district_id", cn);
                checkLocationCmd.CommandType = CommandType.Text;
                checkLocationCmd.Parameters.Add("@address_en", MySqlDbType.VarChar).Value = l.address_en;
                checkLocationCmd.Parameters.Add("@address_tc", MySqlDbType.VarChar).Value = l.address_tc;
                checkLocationCmd.Parameters.Add("@district_id", MySqlDbType.Int32).Value = district_id;
                int location_id = 0;
                location_id = Int32.Parse(checkLocationCmd.ExecuteScalar() != null ? checkLocationCmd.ExecuteScalar().ToString() : "0");
                if (location_id == 0)
                {
                    MySqlCommand insertLocationCmd = new MySqlCommand(@"insert into location (name_en, name_tc, name_sc, phone, website, country_id, area_id, 
                                                                        district_id, address_en, address_tc, address_sc, extend_address, postal_code, lat, lon) values (
                                                                        @name_en, @name_tc, @name_sc, @phone, @website, @country_id, @area_id, 
                                                                        @district_id, @address_en, @address_tc, @address_sc, @extend_address, @postal_code, @lat, @lon); select last_insert_id();", cn);
                    insertLocationCmd.Parameters.Add("@name_en", MySqlDbType.VarChar).Value = l.name_en;
                    insertLocationCmd.Parameters.Add("@name_tc", MySqlDbType.VarChar).Value = l.name_tc;
                    insertLocationCmd.Parameters.Add("@name_sc", MySqlDbType.VarChar).Value = Microsoft.VisualBasic.Strings.StrConv(l.name_tc, VbStrConv.SimplifiedChinese, 2052);
                    insertLocationCmd.Parameters.Add("@phone", MySqlDbType.VarChar).Value = l.phone;
                    insertLocationCmd.Parameters.Add("@website", MySqlDbType.VarChar).Value = l.website;
                    insertLocationCmd.Parameters.Add("@country_id", MySqlDbType.Int32).Value = country_id;
                    insertLocationCmd.Parameters.Add("@area_id", MySqlDbType.Int32).Value = area_id;
                    insertLocationCmd.Parameters.Add("@district_id", MySqlDbType.Int32).Value = district_id;
                    insertLocationCmd.Parameters.Add("@address_en", MySqlDbType.VarChar).Value = l.address_en;
                    insertLocationCmd.Parameters.Add("@address_tc", MySqlDbType.VarChar).Value = l.address_tc;
                    insertLocationCmd.Parameters.Add("@address_sc", MySqlDbType.VarChar).Value = Microsoft.VisualBasic.Strings.StrConv(l.address_tc, VbStrConv.SimplifiedChinese, 2052);
                    insertLocationCmd.Parameters.Add("@extend_address", MySqlDbType.VarChar).Value = l.extend_address;
                    insertLocationCmd.Parameters.Add("@postal_code", MySqlDbType.VarChar).Value = l.postal_code;
                    if (response.results.Count > 0)
                    {
                        insertLocationCmd.Parameters.Add("@lat", MySqlDbType.VarChar).Value = response.results[response.results.Count - 1].geometry.Location.lat.ToString();
                        insertLocationCmd.Parameters.Add("@lon", MySqlDbType.VarChar).Value = response.results[response.results.Count - 1].geometry.Location.lng.ToString();
                    }
                    else
                    {
                        insertLocationCmd.Parameters.Add("@lat", MySqlDbType.VarChar).Value = "0";
                        insertLocationCmd.Parameters.Add("@lon", MySqlDbType.VarChar).Value = "0";
                    }
                    insertLocationCmd.Parameters.Add("@location_type_id", MySqlDbType.Int32).Value = l.type_id;
                    location_id = Int32.Parse(insertLocationCmd.ExecuteScalar().ToString());
                }

                MySqlCommand deletePendCmd = new MySqlCommand("delete from pending_image where location_id = @location_id", cn);
                deletePendCmd.CommandType = CommandType.Text;
                deletePendCmd.Parameters.Add("@location_id", MySqlDbType.Int32).Value = location_id;
                deletePendCmd.ExecuteNonQuery();

                for (int i = 0; i < l.imageList.Count; i++)
                {
                    MySqlCommand insertCmd = new MySqlCommand("insert into pending_image (location_id, image_order, source) values (@location_id, @image_order, @source)", cn);
                    insertCmd.CommandType = CommandType.Text;
                    insertCmd.Parameters.Add("@location_id", MySqlDbType.Int32).Value = location_id;
                    insertCmd.Parameters.Add("@image_order", MySqlDbType.Int32).Value = i + 1;
                    insertCmd.Parameters.Add("@source", MySqlDbType.VarChar).Value = l.imageList[i];
                    insertCmd.ExecuteNonQuery();
                }

                MySqlCommand completeCmd = new MySqlCommand("insert into complete_url (url) values (@url)", cn);
                completeCmd.CommandType = CommandType.Text;
                completeCmd.Parameters.Add("@url", MySqlDbType.VarChar).Value = l.url;
                completeCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", l.url + " , db error" + System.Environment.NewLine);
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", ex.StackTrace + System.Environment.NewLine);
            }
            finally
            {
                cn.Close();
            }
        }

        public  GoogleMapAPI GetGoogleLatLon(string address)
        {
            GoogleMapAPI googlemapResult = new GoogleMapAPI();
            string url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + address + "&key=AIzaSyAambBvgsmatjUZlZFBjF2odFC0Lr20RIU";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
                googlemapResult = new JavaScriptSerializer().Deserialize<GoogleMapAPI>(data);

                response.Close();
                readStream.Close();
            }
            return googlemapResult ;
        }

        public bool CheckCrawled(string url)
        {
            bool existed = true;
            try
            {
                cn.Open();
                MySqlCommand checkCmd = new MySqlCommand("select id from complete_url where url = @url", cn);
                checkCmd.CommandType = CommandType.Text;
                checkCmd.Parameters.Add("@url", MySqlDbType.VarChar).Value = url;
                MySqlDataReader dr = checkCmd.ExecuteReader();
                if (!dr.HasRows)
                {
                    existed = false;
                }
                dr.Close();
            }
            catch (Exception ex)
            { }
            finally
            {
                cn.Close();
            }
            return existed;
        }

    }
}
