using Mono.Options;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;

namespace eze.schema.extract
{
    class Program
    {
        static int Main(string[] args)
        {
            bool showHelp = false;
            string server = "Localhost", db = "TC", username = null, pwd = null;
            string restUrl = "http://localhost:5000/api/schema/";
            string connString = "";
            var p = new OptionSet() {
                { "s|server=",  "The {SERVER} where the TC Database is hosted, default = Localhost.",v => { server = v; } },
                { "d|database=","The name of the {DATABASE}, defaults = TC",v => { db = v ; } } ,
                { "u|username=","The database {USERNAME}, if not specified it will use a trusted connection",v => { username = v; } } ,
                { "p|password=","The database {PASSWORD}",v => { pwd = v; } } ,
                { "a|apiURL=",  "The {URL} used to upload data (optional)",v => { restUrl = v; } } ,
                { "h|help",     "Show this message and exit", v => showHelp = v != null },
            };
            var extra = p.Parse(args);
            if (showHelp)
            {
                ShowHelp(p);
                return 0;
            }
            //set the connection string
            if (username == null)
            {
                connString = $"Server={server};Database={db};Trusted_Connection=True;";
            }
            else
            {
                connString = $"Server={server};Database={db};User Id={username}; Password={pwd};";
            }

            var schemaResult = new SchemaData();
            try
            {
                BuildSchemaFromDb(connString, schemaResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting data from DB: " + ex.Message);
                return 1;
            }
            try
            {
                PostResults(restUrl, schemaResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading data to Eze Service: " + ex.Message);
                return 2;
            }
            return 0;
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: greet [OPTIONS]+ message");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void BuildSchemaFromDb(string connString, SchemaData schemaResult)
        {
            Console.WriteLine("Retrieving data from database...");
            // Get the Client Metadata: Name, Version, etc.
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(SqlConstants.GetClientInfo, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                var rowCaption = dr["Setting"].ToString();
                                switch (rowCaption)
                                {
                                    case "Client":
                                        schemaResult.Client = dr["Value"].ToString();
                                        break;
                                    case "Entitled":
                                        schemaResult.Entitled = dr["Value"].ToString() == "1";
                                        break;
                                    case "Version":
                                        schemaResult.Version = dr["Value"].ToString();
                                        break;
                                    default: break;
                                }
                            }
                        }
                    }
                }

                // Get the DB OBject definitions: Procedures, Tables, etc.
                using (SqlCommand cmd = new SqlCommand(SqlConstants.GetObjects, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                DbObject obj = new DbObject()
                                {
                                    Name = dr["Name"].ToString(),
                                    Type = dr["Type"].ToString().Trim(),
                                    CheckSum = dr.IsDBNull(2) ? 0 : Convert.ToInt32(dr["CheckSum"]),
                                    Definition = dr.IsDBNull(3) ? "" : dr["Definition"].ToString(),
                                };
                                schemaResult.Objects.Add(obj);
                            }
                        }
                    }
                }
            }
        }

        static void PostResults(string restUrl, SchemaData schemaResult)
        {
            if (!restUrl.EndsWith("/"))
            {
                restUrl += "/";
            }
            string json = JsonConvert.SerializeObject(schemaResult);
            Console.WriteLine("Posting results to: " + restUrl + schemaResult.Client);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(restUrl + schemaResult.Client);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine(result);
            }
        }
    }
}