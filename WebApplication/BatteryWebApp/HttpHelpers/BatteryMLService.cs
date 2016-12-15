using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using BatteryWebApp.Models;

namespace BatteryWebApp.HelperClasses
{
    public class BatteryMLService
    {
        private static string username = "RunFranks525";
        private static string password = "1048Boston";
        private static string datasource = "iotbatterydatabase.database.windows.net";
        public static async Task<MLResponse> GetMLResponse()
        {
            SqlConnectionStringBuilder connectionString = buildConnectionString();
            BatteryTable dataPoint = getDatabaseValue(connectionString);
            string currentDateTime = dataPoint.DateStamp.ToString("yyyy-MM-dd HH:mm:ss");
            string futureDateTime = dataPoint.DateStamp.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
            var scoreRequest = new
            {
                Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "DateTime", futureDateTime
                                            },
                                }
                            }
                        },
                        {
                            "input2",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "DateTime", currentDateTime
                                            },
                                }
                            }
                        },
                    },
                GlobalParameters = new Dictionary<string, string>()
                {
                }
            };
            using (var client = new HttpClient())
            {
                const string apiKey = "9eKcgSItZYwTQTPujl42shH+QfoNhlU16vGXSrPT/B3IJGsJonA6n/qyy0I2m8XR10Ty/0xMS0XsjAZbAagi2A==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/subscriptions/4769386f1cdf4fb6bc3250b9336fc5bc/services/15086b35646f42c6a3ddb43193821092/execute?api-version=2.0&format=swagger");

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    MLResponse mlResponse = new MLResponse()
                    {
                        StatusCode = response.StatusCode,
                        Response = result,
                        dateStamp = dataPoint.DateStamp
                    };
                    return mlResponse;
                }
                else
                {
                    string responseBuilder = string.Format("The request failed with status code: {0}", response.StatusCode);
                    MLResponse mlResponse = new MLResponse()
                    {
                        StatusCode = response.StatusCode,
                        Response = responseBuilder
                    };
                    return mlResponse;
                }
            }
        }

        public static async Task<List<MLResponse>> GetMLResponse(int seed)
        {
            SqlConnectionStringBuilder connectionString = buildConnectionString();
            List<BatteryTable> dataPoints = getDatabaseValue(connectionString, seed);
            List<MLResponse> responses = new List<MLResponse>();
            for (int i = 0; i < dataPoints.Count; i++)
            {
                BatteryTable currentPoint = dataPoints[i];
                string currentDateTime = currentPoint.DateStamp.ToString("yyyy-MM-dd HH:mm:ss");
                string futureDateTime = currentPoint.DateStamp.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "DateTime", futureDateTime
                                            },
                                }
                            }
                        },
                        {
                            "input2",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "DateTime", currentDateTime
                                            },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                MLResponse response = await makeRequest(scoreRequest, currentPoint);
                responses.Add(response);
            }
            return responses;              
        }

        private static async Task<MLResponse> makeRequest(object scoreRequest, BatteryTable dataPoint)
        {
            using (var client = new HttpClient())
            {
                const string apiKey = "9eKcgSItZYwTQTPujl42shH+QfoNhlU16vGXSrPT/B3IJGsJonA6n/qyy0I2m8XR10Ty/0xMS0XsjAZbAagi2A==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/subscriptions/4769386f1cdf4fb6bc3250b9336fc5bc/services/15086b35646f42c6a3ddb43193821092/execute?api-version=2.0&format=swagger");

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    MLResponse mlResponse = new MLResponse()
                    {
                        StatusCode = response.StatusCode,
                        Response = result,
                        dateStamp = dataPoint.DateStamp
                    };
                    return mlResponse;
                }
                else
                {
                    string responseBuilder = string.Format("The request failed with status code: {0}", response.StatusCode);
                    MLResponse mlResponse = new MLResponse()
                    {
                        StatusCode = response.StatusCode,
                        Response = responseBuilder
                    };
                    return mlResponse;
                }
            }
        }

        private static List<BatteryTable> getDatabaseValue(SqlConnectionStringBuilder connectionString, int seed)
        {
            List<BatteryTable> dataPoints = new List<BatteryTable>();
            using (SqlConnection conn = new SqlConnection(connectionString.ToString()))
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    conn.Open();
                    string builder = "SELECT * From(SELECT TOP {0} * FROM dbo.BatteryTable ORDER BY DateStamp DESC) Q ORDER BY DateStamp ASC";
                    string query = String.Format(builder, seed);
                    string commandText = query;
                    command.CommandText = commandText;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        BatteryTable dataPoint = new BatteryTable();
                        dataPoint.DateStamp = reader.GetDateTime(0);
                        dataPoint.BatteryCurrent = (double)reader.GetSqlDouble(1);
                        dataPoint.BatteryVoltage = (double)reader.GetSqlDouble(2);
                        dataPoint.BatteryPower = (double)reader.GetSqlDouble(3);
                        dataPoints.Add(dataPoint);
                    }
                }
            }
            return dataPoints;
        }

        private static SqlConnectionStringBuilder buildConnectionString()
        {
            SqlConnectionStringBuilder connString1Builder = new SqlConnectionStringBuilder();
            connString1Builder.DataSource = datasource;
            connString1Builder.InitialCatalog = "IoTBatteryDatabase";
            connString1Builder.Encrypt = true;
            connString1Builder.TrustServerCertificate = false;
            connString1Builder.UserID = username;
            connString1Builder.Password = password;
            connString1Builder.ConnectTimeout = 30;
            return connString1Builder;
        }

        private static BatteryTable getDatabaseValue(SqlConnectionStringBuilder connectionString)
        {
            BatteryTable dataPoint = null;
            using (SqlConnection conn = new SqlConnection(connectionString.ToString()))
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    conn.Open();
                    string query = "SELECT * FROM dbo.BatteryTable WHERE DateStamp = (SELECT MAX(DateStamp)  FROM dbo.BatteryTable)";
                    string commandText = query;
                    command.CommandText = commandText;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        dataPoint = new BatteryTable();
                        dataPoint.DateStamp = reader.GetDateTime(0);
                        dataPoint.BatteryCurrent = (double)reader.GetSqlDouble(1);
                        dataPoint.BatteryVoltage = (double)reader.GetSqlDouble(2);
                        dataPoint.BatteryPower = (double)reader.GetSqlDouble(3);
                    }                   
                }
            }
            return dataPoint;
        }
    }
}