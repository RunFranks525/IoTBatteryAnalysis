using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using BatteryWebApp.Models;

namespace BatteryWebApp.HttpHelpers
{
    public class DataAccess
    {
        private static string username = "RunFranks525";
        private static string password = "1048Boston";
        private static string datasource = "iotbatterydatabase.database.windows.net";
        private static SqlConnectionStringBuilder connectionString = null;

        public DataAccess()
        {
            connectionString = new SqlConnectionStringBuilder();
            connectionString.DataSource = datasource;
            connectionString.InitialCatalog = "IoTBatteryDatabase";
            connectionString.Encrypt = true;
            connectionString.TrustServerCertificate = false;
            connectionString.UserID = username;
            connectionString.Password = password;
            connectionString.ConnectTimeout = 30;
        }

        public void storeNewRow(ResponseOutput response)
        {
            using (SqlConnection conn = new SqlConnection(connectionString.ToString()))
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    try
                    {
                        conn.Open();
                        string query = "INSERT INTO ResponseOutput VALUES ('{0}', '{1}', '{2}')";
                        string cmdText = String.Format(query, response.DateStamp, response.prediction, response.evaluation);
                        command.CommandText = cmdText;
                        command.ExecuteScalar();
                        conn.Close();
                    } catch(Exception ex)
                    {

                    }
                    
                }
            }
        }

        public List<ResponseOutput> getResponseItems()
        {
            List<ResponseOutput> responseItems = new List<ResponseOutput>();
            using (SqlConnection conn = new SqlConnection(connectionString.ToString()))
            {
                using (SqlCommand command = conn.CreateCommand())
                {
                    conn.Open();
                    string query = "Select * From ResponseOutput";
                    command.CommandText = query;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ResponseOutput responseOutput = new ResponseOutput
                        {
                            DateStamp = reader.GetDateTime(0),
                            prediction = (double)reader.GetSqlDouble(1),
                            evaluation = (double)reader.GetSqlDouble(2)
                        };
                        responseItems.Add(responseOutput);
                    }
                    conn.Close();
                }
            }
            return responseItems;
        }
    }
}