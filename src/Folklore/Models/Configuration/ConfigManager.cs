using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using log4net;
using Folklore.Models.Exception;

namespace Folklore.Models.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public static class ConfigManager
    {

        #region LOCAL VARIABLES

        static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion


        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool GetAppSettingBool(string keyName)
        {
            return bool.Parse(_GetAppSetting("", keyName, true));
        }

        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>Int value of the key specified if it is found</returns>
        public static int GetAppSettingInt(string keyName)
        {
            return int.Parse(_GetAppSetting("", keyName, true));
        }


        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>Int value of the key specified if it is found</returns>
        public static double GetAppSettingDouble(string keyName)
        {
            return double.Parse(_GetAppSetting("", keyName, true));
        }

        #region TRYGETAPPSETTING

        /// <summary>
        /// Retrieves a configuration value from the application database allowing for local override of the value in the 
        /// .config file supressing the Exception that is normally thrown so it can be evealuated by the calling program.
        /// 
        /// This is useful for instance where you don't want to use try/catch error handling in the calling program.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="keyValue"></param>
        /// <returns>String value of the key specified if it is found</returns>
        public static bool TryGetAppSetting(string keyName, ref string keyValue)
        {
            var keyVal = _GetAppSetting("", keyName, false);

            if (string.IsNullOrEmpty(keyVal))
                return false;


            keyValue = keyVal;
            return true;
        }



        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="keyValue">Placeholder variable for the return value if found</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool TryGetAppSetting(string keyName, ref bool keyValue)
        {
            var keyVal = _GetAppSetting("", keyName, false);

            if (string.IsNullOrEmpty(keyVal))
                return false;

            keyValue = bool.Parse(keyVal);
            return true;
        }


        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="keyValue">Placeholder variable for the return value if found</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool TryGetAppSetting(string keyName, ref int keyValue)
        {
            var keyVal = _GetAppSetting("", keyName, false);

            if (string.IsNullOrEmpty(keyVal))
                return false;

            int output;
            if (!int.TryParse(keyVal, out output))
                return false;

            keyValue = output;
            return true;
        }


        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="keyValue">Placeholder variable for the return value if found</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool TryGetAppSetting(string keyName, ref double keyValue)
        {
            var keyVal = _GetAppSetting("", keyName, false);

            if (string.IsNullOrEmpty(keyVal))
                return false;

            double output;
            if (!double.TryParse(keyVal, out output))
                return false;

            keyValue = output;
            return true;
        }

        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="keyValue">Placeholder variable for the return value if found</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool TryGetAppSetting(string keyName, ref short keyValue)
        {
            var keyVal = _GetAppSetting("", keyName, false);

            if (string.IsNullOrEmpty(keyVal))
                return false;

            short output;
            if (!short.TryParse(keyVal, out output))
                return false;

            keyValue = output;
            return true;
        }

        #endregion

        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>String value of the key specified if it is found</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static string _GetAppSetting(string keyName)
        {
            // ReSharper disable once IntroduceOptionalParameters.Local
            return _GetAppSetting("", keyName, false);
        }

        /// <summary>
        /// Retrieves a configuration connection string value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>String value of the key specified if it is found</returns>
        public static string GetConnectionString(string keyName)
        {
            return _GetConnectionString("", keyName, true);
        }



        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool TryGetConnectionString(string keyName)
        {
            var result = !string.IsNullOrEmpty(_GetAppSetting("", keyName, false));
            return result;
        }

        /// <summary>
        /// Retrieves a configuration value from the application .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="keyValue">Placeholder variable for the return value if found</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool TryGetConnectionString(string keyName, ref string keyValue)
        {
            var keyVal = _GetConnectionString("", keyName, false);

            if (string.IsNullOrEmpty(keyVal)) return false;

            keyValue = keyVal;
            return true;
        }

        /// <summary>
        /// Retrieves a configuration value from the application database allowing for local override of the value in the 
        /// .config file.
        /// </summary>
        /// <param name="appName">Name of the application for retrieving settings from database</param>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>Bool value of the key specified if it is found</returns>
        public static bool GetAppSettingBool(string appName, string keyName)
        {
            return bool.Parse(_GetAppSetting(appName, keyName, true));
        }


        /// <summary>
        /// Retrieves a configuration value from the application database allowing for local override of the value in the 
        /// .config file.
        /// </summary>
        /// <param name="appName">Name of the application for retrieving settings from database</param>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>Int value of the key specified if it is found</returns>
        public static int GetAppSettingInt(string appName, string keyName)
        {
            return int.Parse(_GetAppSetting(appName, keyName, true));
        }



        /// <summary>
        /// Retrieves a configuration value from the application database allowing for local override of the value in the 
        /// .config file.
        /// </summary>
        /// <param name="appName">Name of the application for retrieving settings from database</param>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="throwException">bool value to determine if an exception is thrown</param>
        /// <returns>String value of the key specified if it is found</returns>
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static string _GetConnectionString(string appName, string keyName, bool throwException)
        {
            // ReSharper disable once RedundantAssignment
            var keyValue = "";

            // try to get connection string from local config file first then DB
            var cs = ConfigurationManager.ConnectionStrings[keyName];
            keyValue = cs.ToString() ?? _GetAppSetting(appName, keyName, throwException);
            return keyValue;
        }




        /// <summary>
        /// Retrieves a configuration value from the application .config file. 
        /// .config file.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>String value of the key specified if it is found</returns>
        public static string GetAppSetting(string keyName)
        {
            return _GetAppSetting("", keyName, true);
        }

        /// <summary>
        /// Retrieves a configuration value from the application database allowing for local override of the value in the 
        /// .config file.
        /// </summary>
        /// <param name="appName">Name of the application for retrieving settings from database</param>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <returns>String value of the key specified if it is found</returns>
        public static string GetAppSetting(string appName, string keyName)
        {
            return _GetAppSetting(appName, keyName, true);
        }


        /// <summary>
        /// Retrieves a configuration value from the application database allowing for local override of the value in the 
        /// .config file.
        /// </summary>
        /// <param name="appName">Name of the application for retrieving settings from database</param>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="throwException">Determines if an exception is thrown when value isn't found</param>
        /// <returns>String value of the key specified if it is found</returns>
        private static string _GetAppSetting(string appName, string keyName, bool throwException)
        {
            //TODO: pull dblookup from config
            var dbLookUp = true;

            //TryGetAppSetting(AppSettings.Application.ConfigDbLookup, ref dbLookUp);
            var cgfDbLookup = ConfigurationManager.AppSettings[AppSettings.Application.ConfigDbLookup];
            if (!String.IsNullOrWhiteSpace(cgfDbLookup))
                dbLookUp = bool.Parse(cgfDbLookup);



            var keyValue = ConfigurationManager.AppSettings[keyName];

            if (!string.IsNullOrEmpty(keyValue))
            {
                return keyValue;
            }



            #region GET CONFIG FROM DB

            try
            {
                var connection = ConfigurationManager.ConnectionStrings["Application.ConnectionString"];

                if (connection != null)
                {
                    var sqlConnectionString = connection.ToString();
                    keyValue = _GetAppSetting(appName, keyName, false, sqlConnectionString);
                }
            }
            catch (System.Exception e)
            {
                Logger.Fatal("Error reading Application.ConnectionString from web.config", e);
            }

            #endregion




            if (string.IsNullOrEmpty(keyValue) && throwException)
            {
                throw new MissingOrInvalidConfigurationSettingException(String.Format("KeyName: {0}", keyName));
            }

            return keyValue;
        }


        /// <summary>
        /// Retrieves a configuration value directly from the application database
        /// .config file.
        /// </summary>
        /// <param name="appName">Name of the application for retrieving settings from database</param>
        /// <param name="keyName">Key for the value that needs to be retrieved</param>
        /// <param name="throwException">Determines if an exception is thrown when value isn't found</param>
        /// <param name="sqlConnectionString">SQL connection string to the data source</param>
        /// <returns>String value of the key specified if it is found</returns>
        private static string _GetAppSetting(string appName, string keyName, bool throwException, string sqlConnectionString)
        {
            string keyValue = null;
            try
            {

                if (!string.IsNullOrEmpty(sqlConnectionString))
                {
                    //Logger.DebugFormat("_GetAppSetting sqlConnectionString:{0} appName: {1} keyName: {2}", sqlConnectionString, appName, keyName);

                    using (var sqlCon = new SqlConnection(sqlConnectionString))
                    {
                        sqlCon.Open();

                        using (var sqlCmd = sqlCon.CreateCommand())
                        {
                            sqlCmd.CommandType = CommandType.Text;
                            sqlCmd.CommandText = "SELECT dbo.GetAppSetting(@pAppName, @pKeyName)";
                            sqlCmd.Parameters.AddWithValue("@pAppName", appName);
                            sqlCmd.Parameters.AddWithValue("@pKeyName", keyName);

                            keyValue = sqlCmd.ExecuteScalar().ToString();

                            if (string.IsNullOrEmpty(keyValue))
                            {
                                //Logger.DebugFormat("_GetAppSetting sqlConnectionString:{0} appName: {1} keyName: {2}", sqlConnectionString, "Default", keyName);
                                sqlCmd.Parameters["@pAppName"].Value = "Default";
                                keyValue = sqlCmd.ExecuteScalar().ToString();

                                if (string.IsNullOrEmpty(keyValue))
                                {
                                    if (throwException)
                                        throw new MissingOrInvalidConfigurationSettingException(
                                            String.Format("DB LOOKUP-AppName: {0} KeyName: {1} ConnectionString: {2}", appName, keyName, sqlConnectionString));
                                }
                            }
                        }
                    }
                }
                else
                {
                    Logger.Debug("_GetAppSetting sqlConnectionString:EMPTY");
                }

            }
            catch (System.Exception e)
            {
                Logger.Fatal(e);

                // since this is a low level api we want to make sure we
                // throw all errors up stream so they can be handled properly
                // e.g. some UI 
                // this will also help to ensure the calling program doesn't need 
                // to be sprinkled with a bunch of if then statements when working
                // with a lot of configuration settings.
                if (throwException)
                    throw;
            }

            return keyValue;
        }

        /// <summary>
        /// Save a configuration value to the applications config database.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be saved</param>
        /// <param name="keyValue">The value to be saved.</param>
        /// <returns>true if the value was saved</returns>
        public static bool TrySaveAppSetting(string keyName, object keyValue)
        {
            return _SaveAppSetting(keyName, keyValue.ToString(), false);
        }

        /// <summary>
        /// Save a configuration value to the applications config database.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be saved</param>
        /// <param name="keyValue">The value to be saved.</param>
        /// <returns>true if the value was saved</returns>
        public static bool SaveAppSetting(string keyName, object keyValue)
        {
            return _SaveAppSetting(keyName, keyValue.ToString(), true);
        }

        /// <summary>
        /// Save a configuration value to the applications config database.
        /// </summary>
        /// <param name="keyName">Key for the value that needs to be saved</param>
        /// <param name="keyValue">The value to be saved.</param>
        /// <param name="throwException"></param>
        /// <returns>true if the value was saved</returns>
        private static bool _SaveAppSetting(string keyName, string keyValue, bool throwException)
        {
            var returnValue = false;

            var connectionString = "";
            try
            {

                var connection = ConfigurationManager.ConnectionStrings[AppSettings.Application.ConnectionString];
                if (connection != null)
                {
                    connectionString = connection.ToString();
                }
            }
            catch (System.Exception e)
            {
                Trace.WriteLine(e);
            }

            if (!String.IsNullOrWhiteSpace(connectionString))
            {
                using (var sqlCon = new SqlConnection(connectionString))
                {
                    sqlCon.Open();

                    using (var sqlCmd = sqlCon.CreateCommand())
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.CommandText = "dbo.SaveAppSetting";
                        sqlCmd.Parameters.AddWithValue("@pKeyName", keyName);
                        sqlCmd.Parameters.AddWithValue("@pKeyValue", keyValue);


                        using (var dr = sqlCmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                dr.Read();
                                returnValue = dr.GetInt32(0) > 0;
                            }
                        }

                        if (!returnValue && throwException)
                            throw new MissingOrInvalidConfigurationSettingException(keyName);

                    }
                }
            }
            else
            {
                if (throwException)
                {
                    throw new MissingOrInvalidConfigurationSettingException("DB LOOKUP- KeyName: {AppSettings.Application.ConnectionString}");
                }
            }

            return returnValue;
        }

    }
}

