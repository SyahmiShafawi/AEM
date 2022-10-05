using AEMWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static AEMWebApplication.Models.PlatformWellActual;

namespace AEMWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        string baseURL = "http://test-demo.aemenersol.com";
        public string bearerTokenFilter;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            //-----start post request Login-----
            var login = new Login("user@aemenersol.com", "Test@123");

            var json = JsonConvert.SerializeObject(login);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            var response = await client.PostAsync(baseURL + "/api/Account/Login", data);
                      
            var bearerResult = await response.Content.ReadAsStringAsync();

            bearerTokenFilter = bearerResult.Trim(new Char[] { '/', '"' });
            Console.WriteLine(bearerResult);
            //-----end post request Login-----


            //-----get request GetPlatformWellActual-----
            using var getClient = new HttpClient();
            getClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerTokenFilter);

            var responseGetPlatformWellActual = await getClient.GetAsync(baseURL + "/api/PlatformWell/GetPlatformWellActual");

            var getPlatformWellActualResult = await responseGetPlatformWellActual.Content.ReadAsStringAsync();

            Console.WriteLine(getPlatformWellActualResult);
           
            List<PlatformWellActual> platformWellActualList = JsonConvert.DeserializeObject<List<PlatformWellActual>>(getPlatformWellActualResult);
            platformWellActualList.ForEach(Console.WriteLine);


            for (int i = 0; i < platformWellActualList.Count; i++)
            {
                InsertDBPlatformWellActual(platformWellActualList[i]);

                
                if(platformWellActualList[i].well.Count > 0) 
                {
                    for (int y = 0; y < platformWellActualList[i].well.Count; y++)
                    {
                        InsertDBWell(platformWellActualList[i].well[y]);
                    }
                }
            }
            //-----end get request GetPlatformWellActual-----


            return View();
        }

        public bool InsertDBPlatformWellActual(PlatformWellActual platformWellActual)

        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = null;

            bool result = false;
            try
            {
                using (con = new SqlConnection("server=localhost\\sqlexpress;database=aemdb;trusted_connection=true"))
                {
                    con.Open();
                    var currDateTime = DateTime.Now;
                    cmd = new SqlCommand();
                    cmd.Connection = con;

                    cmd.CommandText = "Select * from PlatformWellActuals where id = "+ platformWellActual.id;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    var ds = new DataSet();
                    adapter.Fill(ds);

                    string json = JsonConvert.SerializeObject(ds.Tables[0]);
                    List<PlatformWellActual> platformWellActualListFromDB = JsonConvert.DeserializeObject<List<PlatformWellActual>>(json);

                    if(platformWellActualListFromDB.Count >0) 
                    {
                        if (platformWellActualListFromDB[0].id == platformWellActual.id
                            && platformWellActualListFromDB[0].id != null)
                        {
                            cmd.CommandText = "UPDATE PlatformWellActuals SET uniqueName = @uniqueName, latitude = @latitude, longitude = @longitude, createdAt = @createdAt, updatedAt = @updatedAt " +
                                                " WHERE id = @id";

                            cmd = SetSQLParamInt(cmd, SqlDbType.Int, "@id", platformWellActual.id);
                            cmd = SetSQLParamString(cmd, SqlDbType.VarChar, "@uniqueName", platformWellActual.uniqueName);
                            cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@latitude", ToFloat(platformWellActual.latitude));
                            cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@longitude", ToFloat(platformWellActual.longitude));
                            cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@createdAt", platformWellActual.createdAt == DateTime.MinValue ? DateTime.Now : platformWellActual.createdAt);
                            cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@updatedAt", platformWellActual.updatedAt == DateTime.MinValue ? DateTime.Now : platformWellActual.createdAt); //DateTime.Now

                            cmd.ExecuteNonQuery();
                        }

                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO PlatformWellActuals (id, uniqueName, latitude, longitude, createdAt, updatedAt) " +
                                            " VALUES (@id, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt) ";

                        cmd = SetSQLParamInt(cmd, SqlDbType.Int, "@id", platformWellActual.id);
                        cmd = SetSQLParamString(cmd, SqlDbType.VarChar, "@uniqueName", platformWellActual.uniqueName);
                        cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@latitude", ToFloat(platformWellActual.latitude));
                        cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@longitude", ToFloat(platformWellActual.longitude));
                        cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@createdAt", platformWellActual.createdAt == DateTime.MinValue ? DateTime.Now : platformWellActual.createdAt);
                        cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@updatedAt", platformWellActual.updatedAt == DateTime.MinValue ? DateTime.Now : platformWellActual.createdAt);

                        cmd.ExecuteNonQuery();

                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }

            return result;
        }

        public bool InsertDBWell(Well well)

        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection con = null;

            bool result = false;
            try
            {
                using (con = new SqlConnection("server=localhost\\sqlexpress;database=aemdb;trusted_connection=true"))
                {
                    con.Open();
                    var currDateTime = DateTime.Now;
                    cmd = new SqlCommand();
                    cmd.Connection = con;

                    cmd.CommandText = "Select * from Wellss where id = " + well.id;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    var ds = new DataSet();
                    adapter.Fill(ds);

                    string json = JsonConvert.SerializeObject(ds.Tables[0]);
                    List<Well> wellFromDB = JsonConvert.DeserializeObject<List<Well>>(json);

                    if (wellFromDB.Count > 0)
                    {
                        if (wellFromDB[0].id == well.id
                        && wellFromDB[0].id != null)
                        {
                            cmd.CommandText = "UPDATE Wellss SET platformId = @platformId, uniqueName = @uniqueName, latitude = @latitude, longitude = @longitude, createdAt = @createdAt, updatedAt = @updatedAt " +
                                                " WHERE id = @id";

                            cmd = SetSQLParamInt(cmd, SqlDbType.Int, "@id", well.id);
                            cmd = SetSQLParamString(cmd, SqlDbType.VarChar, "@platformId", well.platformId);
                            cmd = SetSQLParamString(cmd, SqlDbType.VarChar, "@uniqueName", well.uniqueName);
                            cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@latitude", ToFloat(well.latitude));
                            cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@longitude", ToFloat(well.longitude));
                            cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@createdAt", well.createdAt == DateTime.MinValue ? DateTime.Now : well.createdAt);
                            cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@updatedAt", well.updatedAt == DateTime.MinValue ? DateTime.Now : well.createdAt); //DateTime.Now

                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO Wellss (id, platformId, uniqueName, latitude, longitude, createdAt, updatedAt) " +
                                        " VALUES (@id, @platformId, @uniqueName, @latitude, @longitude, @createdAt, @updatedAt) ";

                        cmd = SetSQLParamInt(cmd, SqlDbType.Int, "@id", well.id);
                        cmd = SetSQLParamString(cmd, SqlDbType.VarChar, "@platformId", well.platformId);
                        cmd = SetSQLParamString(cmd, SqlDbType.VarChar, "@uniqueName", well.uniqueName);
                        cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@latitude", ToFloat(well.latitude));
                        cmd = SetSQLParamFloat(cmd, SqlDbType.Float, "@longitude", ToFloat(well.longitude));
                        cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@createdAt", well.createdAt == DateTime.MinValue ? DateTime.Now : well.createdAt);
                        cmd = SetSQLParamDateTime(cmd, SqlDbType.DateTime, "@updatedAt", well.updatedAt == DateTime.MinValue ? DateTime.Now : well.createdAt);

                        cmd.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
            }


            return result;
        }

        public static SqlCommand SetSQLParamInt(SqlCommand cmd, SqlDbType dbType, string paramName, int paramValue)
        {
            cmd.Parameters.Add(paramName, dbType);
            cmd.Parameters[paramName].Value = paramValue;

            return cmd;
        }

        public static SqlCommand SetSQLParamString(SqlCommand cmd, SqlDbType dbType, string paramName, String paramValue)
        {
            cmd.Parameters.Add(paramName, dbType);
            cmd.Parameters[paramName].Value = paramValue;

            return cmd;
        }

        public static SqlCommand SetSQLParamFloat(SqlCommand cmd, SqlDbType dbType, string paramName, float paramValue)
        {
            cmd.Parameters.Add(paramName, dbType);
            cmd.Parameters[paramName].Value = paramValue;

            return cmd;
        }

        public static SqlCommand SetSQLParamDateTime(SqlCommand cmd, SqlDbType dbType, string paramName, DateTime paramValue)
        {
            cmd.Parameters.Add(paramName, dbType);
            cmd.Parameters[paramName].Value = paramValue;

            return cmd;
        }

        public static float ToFloat(double value)
        {
            return (float)value;
        }

        public async Task<IActionResult> Privacy()
        {
            //-----start post request Login-----
            var login = new Login("user@aemenersol.com", "Test@123");

            var json = JsonConvert.SerializeObject(login);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            var response = await client.PostAsync(baseURL + "/api/Account/Login", data);

            var bearerResult = await response.Content.ReadAsStringAsync();

            bearerTokenFilter = bearerResult.Trim(new Char[] { '/', '"' });
            Console.WriteLine(bearerResult);
            //-----end post request Login-----


            //-----start get request GetPlatformWellDummy-----
            using var getClient = new HttpClient();
            getClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerTokenFilter);

            var responseGetPlatformWellActual = await getClient.GetAsync(baseURL + "/api/PlatformWell/GetPlatformWellDummy");

            var getPlatformWellActualResult = await responseGetPlatformWellActual.Content.ReadAsStringAsync();

            Console.WriteLine(getPlatformWellActualResult);

            List<PlatformWellActual> platformWellActualList = JsonConvert.DeserializeObject<List<PlatformWellActual>>(getPlatformWellActualResult);
            platformWellActualList.ForEach(Console.WriteLine);


            for (int i = 0; i < platformWellActualList.Count; i++)
            {
                InsertDBPlatformWellActual(platformWellActualList[i]);


                if (platformWellActualList[i].well.Count > 0)
                {
                    for (int y = 0; y < platformWellActualList[i].well.Count; y++)
                    {
                        InsertDBWell(platformWellActualList[i].well[y]);
                    }
                }
            }
            //-----end get request GetPlatformWellDummy-----


            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
