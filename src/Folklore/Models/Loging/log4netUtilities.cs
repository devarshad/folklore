using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Xml;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Folklore.Models.Configuration;

namespace Folklore.Models.Logging
{

    /// <summary>
    /// 
    /// </summary>

    public static class Log4NetUtilities
    {

        #region LOCAL VARIABLES
        static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Hierarchy L4NHierarchy;
        private static readonly object L4NLock = new object(); //used for manipulating log4net 
        #endregion

        /// <summary>
        /// 
        /// </summary>
        static Log4NetUtilities()
        {
            L4NHierarchy = LogManager.GetRepository() as Hierarchy;
        }

        /// <summary>
        /// Refreshes log4net Logging Level using PennFoster.Core.Configuration.GetAppsetting() to retrieve dynamic values from the configuration table.
        /// This is useful if you need to change the logging level dynamically at runtime.
        /// </summary>
        public static void RefreshLogLevel()
        {
            RefreshLogLevel(GetLevel(GetLoggingLevel()));
        }

        /// <summary>
        /// Refreshes log4net Logging Level to specified log4net.Level.
        /// This is useful if you need to change the logging level dynamically at runtime.
        /// </summary>
        /// <param name="logLevel"></param>
        public static void RefreshLogLevel(Level logLevel)
        {
            // Get the Hierarchy object that organizes the loggers
            //var hier = LogManager.GetRepository() as Hierarchy;

            if (L4NHierarchy == null)
                return;

            lock (L4NLock)
            {
                L4NHierarchy.Root.Level = logLevel;
                // Get Appenders
                var logAppenders = L4NHierarchy.GetAppenders();
                //.Where(appender => appender.GetType().Name.Equals("ADONetAppender", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var t in logAppenders)
                {
                    //RollbarAppender has it's own LogLevel setting.
                    if (t.GetType().Name == AppSettings.Logging.Appenders.Rollbar)
                        continue;
                    try
                    {
                        var appender = (AppenderSkeleton)t;
                        appender.Threshold = logLevel;

                        appender.ActivateOptions();

                    }
                    catch (System.Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                        Trace.WriteLine(ex);
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Refreshes log4net Logging Level to specified log4net.Level.
        /// This is useful if you need to change the logging level dynamically at runtime.
        /// </summary>
        /// <param name="appenderName"></param>
        /// <param name="logLevel"></param>
        public static bool RefreshLogLevel(string appenderName, Level logLevel)
        {
            if (string.IsNullOrWhiteSpace(appenderName))
                return false;

            var returnValue = false;
            // Get the Hierarchy object that organizes the loggers
            //var hier = LogManager.GetRepository() as Hierarchy;

            if (L4NHierarchy == null)
                return false;

            lock (L4NLock)
            {
                var logAppenders = L4NHierarchy.GetAppenders();

                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var t in logAppenders)
                {
                    var appender = (AppenderSkeleton)t;

                    if (!appender.Name.ToLower().Equals(appenderName.ToLower()))
                        continue;

                    appender.Threshold = logLevel;
                    appender.ActivateOptions();
                    returnValue = true;
                    break; //should only be one with this name
                }
            }
            return returnValue;
        }


        /// <summary>
        /// Initializes log4net using  .GetAppsetting() to retrieve dynamic values from the configuration table
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="localOnly">Determines if database lookups for values are performed</param>
        /// <param name="logToFile">Determines if logging to a file is performed</param>
        /// <param name="configFile"></param>
        /// <param name="configureAndWatch"></param>
        /// <param name="request">HttpRequest object</param>
        /// <returns>bool</returns>
        /// http://logging.apache.org/log4net/release/sdk/log4net.Layout.PatternLayout.html        
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // ReSharper disable once UnusedParameter.Local - used for backwards compatibility
        private static bool InitializeLog4Net(string appName, bool localOnly, bool logToFile, string configFile, bool configureAndWatch, HttpRequest request = null)
        {
            bool returnValue;


            Logger.Debug("Initialize log4net");

            if (string.IsNullOrWhiteSpace(appName)) 
            {
                throw new ArgumentNullException(appName.GetType().DeclaringType.Name);
            }
            try
            {
                string machineName ;
                string processName;
                int processId;
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();

                SetEventId(0);

                SetAppName(appName);

                #region SET LOG FILE NAME

                string logFileName = null;

                if (logToFile)
                {
                    var logFilePath = "";
                    var logFileFolder = appName;


                    //for web apps set the logfile Folder to hostName (except localhost)
                    if ( request != null)
                    {
                        if (!request.Url.Host.ToLower().Contains("localhost"))
                        {
                            logFileFolder = !string.IsNullOrWhiteSpace(request.ApplicationPath)
                                ? String.Format("{0}.{1}",request.Url.Host,request.ApplicationPath.Replace("/", ""))
                                : String.Format("{0}",request.Url.Host);
                        }
                        else
                        {
                            //local host put log file in \bin folder
                            //logFileFolder = @"bin";
                            //I believe the application pool keeps reloading locally because the contentes of the \bin folder
                            //keep changing with the log file being updated.
                            logFileFolder = @"logs";

                        }
                    }
                    
                    //if we have a path specifed we want the logs in a seperate folder by application
                    ConfigManager.TryGetAppSetting(AppSettings.Log4Net.LogfilePath, ref logFilePath);
                    
                    logFilePath = Path.Combine(logFilePath, logFileFolder);


                    // allow override of default log file naming schema
                    if (!ConfigManager.TryGetAppSetting(AppSettings.Log4Net.LogfileName, ref logFileName))
                    {
                        
                        //for web apps set the logfile name to hostName.log (except localhost)
                        if ( request != null && !request.Url.Host.ToLower().Contains("localhost"))
                        {
                            if(!string.IsNullOrWhiteSpace(request.ApplicationPath))
                            logFileName = Path.Combine(logFilePath, String.Format("{0}.{1}.log",request.Url.Host,request.ApplicationPath.Replace("/", "")));    
                        }
                        else
                        {
                            //not a web app so logfile name is same as app name
                            logFileName = Path.Combine(logFilePath, appName + ".log");    
                        }
                        
                    }
                    SetLogFileName(logFileName);
                }
                else
                {
                    logFileName = "";
                }
            
                
                #endregion

 

                #region SET STORED PROCEDURE NAME

                var log4NetInsertSp = "InsertLogEntry";
                ConfigManager.TryGetAppSetting(AppSettings.Log4Net.StoredProcedure, ref log4NetInsertSp);
                SetInsertSpName(log4NetInsertSp);

                GlobalContext.Properties["SPName"] = log4NetInsertSp; // backward compatibility
                #endregion

                #region SET MACHINE NAME
                try
                {
                    machineName = Environment.MachineName;

                }
                catch (InvalidOperationException)
                {
                    machineName = "InvalidOperationException";
                }
                catch (System.Exception)
                {
                    machineName = "UNKNOWN";
                }

                SetMachineName(machineName);
                //GlobalContext.Properties["MachineName"] = machineName;
                #endregion

                #region SET PROCESS NAME
                try
                {
                    processName = currentProcess.MainModule.FileName;
                }
                catch (InvalidOperationException )
                {
                    processName = "InvalidOperationException";
                }
                catch (PlatformNotSupportedException )
                {
                    processName = "PlatformNotSupportedException";
                }
                catch (System.Exception)
                {

                    processName = " UNKNOWN";
                }
                SetProcessName(processName);
                //GlobalContext.Properties["ProcessName"] = processName;
                #endregion

                #region SET PROCES ID

                try
                {
                    processId = currentProcess.Id;
                }
                catch (InvalidOperationException )
                {
                    processId = 0;
                }

                catch (PlatformNotSupportedException )
                {
                    processId = 0;
                }

                catch (System.Exception)
                {

                    processId = 0;
                }
                
                SetProcessId(processId.ToString(CultureInfo.InvariantCulture));
                //GlobalContext.Properties["ProcessId"] = processId;
                #endregion

                #region SET LOGGING LEVEL

                var loggingLevel = "INFO";
                ConfigManager.TryGetAppSetting(AppSettings.Log4Net.LoggingLevel, ref loggingLevel);
                
                SetLoggingLevel(loggingLevel);

                #endregion

                #region SET ADO APPENDER CONNECTION STRING

                // if DreamFactory is set to off or there is no SP defined fallback to Ado.Net Appender
                string adoConn = null;
                    if (!ConfigManager.TryGetConnectionString(
                            AppSettings.Log4Net.AdoAppenderConnectionString, ref adoConn))
                    {
                        //if no specific connection string was specified, check to see if there is an application level.
                        if (ConfigManager.TryGetConnectionString(AppSettings.Application.ConnectionString, ref adoConn))
                        {
                            //check to see if the application has it's own logging table.
                            using (var sqlConn = new SqlConnection(adoConn))
                            {
                                using (var sqlCmd = sqlConn.CreateCommand())
                                {
                                    sqlCmd.CommandType = CommandType.Text;
                                    sqlCmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Log'";
                                    sqlConn.Open();
                                    var result = sqlCmd.ExecuteScalar();

                                    if (result == null)
                                    { 
                                        //fallback to the logging DB
                                        var sqlCon = new SqlConnection(adoConn);
                                        adoConn = sqlCon.ConnectionString.Replace(sqlCon.Database, "Logging");
                                    }
                                }
                            }
                        }

                    SetAdoConnectionString(adoConn);
                }

                #endregion

                #region SET log4net AdoAppender BufferSize
                var bufferSize = 10;
                if (adoConn != null)
                {
                    
                    ConfigManager.TryGetAppSetting(AppSettings.Log4Net.AdoAppenderBufferSize, ref bufferSize);
                    SetAdoBufferSize(bufferSize);
                }

                #endregion

                RefreshLogLevel();
                
                returnValue = true;
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
				Trace.WriteLine(ex);
                returnValue = false;
            }
            return returnValue;
        }

        /// <summary>
        /// Dynamically configures the log4net ADO Appender ConnectionString
        /// </summary>
        /// <param name="adoAppenderConnectionString">The Connection String to apply</param>
        /// <param name="appenderName">Optional name of specific appender to change</param>
        public static void SetAdoNetAppenderConnectionString(string adoAppenderConnectionString, string appenderName = null)
        {
            #region LOG4NET ADO APPENDER CONFIG
            /*
                This block of code is used to dynamically configure the log4net ADO appender based on the configuration 
                value stored in our config table. Change the connectionString setting of the ADONetAppender at runtime 
                to the value of the connectionstring.
            */

            Logger.Debug("Configuring log4net ADO appender connections strings");


            // Get the Hierarchy object that organizes the loggers
            //var hier = LogManager.GetRepository() as Hierarchy;

            if (L4NHierarchy == null)
                return;

            lock (L4NLock)
            {
                // Get Appenders
                var logAppenders = L4NHierarchy.GetAppenders();

                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var t in logAppenders)
                {
                    if (t.GetType().Name != AppSettings.Logging.Appenders.AdoAppender)
                        continue;

                    var adoAppender = (AdoNetAppender)t;
                    if ((!string.IsNullOrEmpty(appenderName) && adoAppender.Name != appenderName) ||
                        adoAppender.ConnectionString != "{dynamic}")
                        continue;

                    adoAppender.ConnectionString = adoAppenderConnectionString;
                    adoAppender.ActivateOptions();
                    Logger.DebugFormat("log4net ADOAppender configured. AppenderName: {0} ConnectionString: {1}",
                        adoAppender.Name, adoAppender.ConnectionString);
                }
            }

            #endregion
        }



        /// <summary>
        /// Dynamically configures the log4net ADO Appender BufferSize
        /// </summary>
        /// <param name="bufferSize">The buffer sizer for the apender</param>
        /// <param name="appenderName">Optional name of specific appender to change</param>
        public static void SetAdoNetAppenderBufferSize(int bufferSize, string appenderName = null)
        {
            #region LOG4NET ADO APPENDER CONFIG
            /*
                This block of code is used to dynamically configure the log4net ADO appender based on the configuration 
                value stored in our config table. Change the BufferSize setting of the ADONetAppender at runtime 
                to the value of bufferSize.
            */

            // Get the Hierarchy object that organizes the loggers
            //var hier = LogManager.GetRepository() as Hierarchy;

            if (L4NHierarchy == null)
                return;

            lock (L4NLock)
            {
                // Get Appenders
                var logAppenders = L4NHierarchy.GetAppenders();

                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var t in logAppenders)
                {
                    if (t.GetType().Name != AppSettings.Logging.Appenders.AdoAppender)
                        continue;

                    var adoAppender = (AdoNetAppender)t;
                    if ((!string.IsNullOrEmpty(appenderName) && adoAppender.Name != appenderName))
                        continue;

                    adoAppender.BufferSize = bufferSize;
                    adoAppender.ActivateOptions();
                    Logger.DebugFormat("log4net ADOAppender configured. AppenderName: {0} BufferSize: {1}",
                        adoAppender.Name, adoAppender.BufferSize);
                }
            }

            #endregion
        }

        /// <summary>
        /// Dynamically configures the log4net rolling file appender to the specified file
        /// </summary>
        /// <param name="fileName">full path to the log file to be used or created</param>
        /// <param name="appenderName">Optional name of specific appender to change</param>
        public static void SetRollingFileAppenderLogFileName(string fileName, string appenderName = null)
        {
            #region LOG4NET ROLLING FILE APPENDER LOG FILE NAME
            /*
                This block of code is used to dynamically configure the log4net ADO appender based on the configuration 
                value stored in our config table. Change the connectionString setting of the ADONetAppender at runtime 
                to the value of the connectionstring.
            */

            Logger.Debug("Configuring log4net RollingFileAppender File Name");


            // Get the Hierarchy object that organizes the loggers
            //var hier = LogManager.GetRepository() as Hierarchy;


            if (L4NHierarchy == null)
                return;
            lock (L4NLock)
            {
                // Get Appenders
                var logAppenders = L4NHierarchy.GetAppenders();

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < logAppenders.Length; i++)
                {
                    if (logAppenders[i].GetType().Name != AppSettings.Logging.Appenders.RollingFileAppender)
                        continue;

                    var fileAppender = (RollingFileAppender)logAppenders[i];
                    if ((!string.IsNullOrEmpty(appenderName) && fileAppender.Name != appenderName) ||
                        fileAppender.File != "{dynamic}")
                        continue;

                    // APPLY TO ALL RollingFileAppenders OR just the Named instance
                    fileAppender.File = fileName;
                    fileAppender.ActivateOptions();
                    Logger.DebugFormat("log4net FileAppender configured. AppenderName: {0} File: {1}", fileAppender.Name,
                        fileAppender.File);
                }
            }

            #endregion
        }

        /// <summary>
        /// Returns a log4net Level object for the given string name
        /// </summary>

        public static Level GetLevel(string levelName)
        {

            var dict = new Dictionary<string, Level>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"off", Level.Off},
                {"emergency", Level.Emergency},
                {"fatal", Level.Fatal},
                {"alert", Level.Alert},
                {"critical", Level.Critical},
                {"severe", Level.Severe},
                {"error", Level.Error},
                {"warn", Level.Warn},
                {"notice", Level.Notice},
                {"info", Level.Info},
                {"debug", Level.Debug},
                {"fine", Level.Fine},
                {"trace", Level.Trace},
                {"finer", Level.Finer},
                {"verbose", Level.Verbose},
                {"finest", Level.Finest},
                {"all", Level.All}
            };

            return dict[levelName.ToLower().Trim()];

            #region logging levels retrieved from the log4net source code
            /*
            logging levels retrieved from the log4net source code
            Off: int.MaxValue (2,147,483,647; 0x7FFFFFFF)
            Emergency: 120000
            Fatal: 110000
            Alert: 100000
            Critical: 90000
            Severe: 80000
            Error: 70000
            Warn: 60000
            Notice: 50000
            Info: 40000
            Debug: 30000
            Fine: 30000
            Trace: 20000
            Finer: 20000
            Verbose: 10000
            Finest: 10000
            All: int.MinValue (-2,147,483,648; 0x80000000)
            */
            #endregion
        }


        /// <summary>
        /// Sets the Application Name for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetAppName(string value)
        {
            GlobalContext.Properties["AppName"] = value;
        }

        /// <summary>
        /// Sets the LogFileName for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetLogFileName(string value)
        {
            GlobalContext.Properties["LogFileName"] = value;
        }

        /// <summary>
        /// Sets the LoggingLevel for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetLoggingLevel(string value)
        {
            GlobalContext.Properties["LoggingLevel"] = value;
        }

        /// <summary>
        /// Sets the LoggingLevel for logging
        /// </summary>

        public static string GetLoggingLevel()
        {
            return GlobalContext.Properties["LoggingLevel"].ToString();
        }

        /// <summary>
        /// Sets the ADOConnectionString for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetAdoConnectionString(string value)
        {
            GlobalContext.Properties["ADOConnectionString"] = value;
        }

        /// <summary>
        /// Sets the ADOBufferSize for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetAdoBufferSize(int value)
        {
            GlobalContext.Properties["ADOBufferSize"] = value.ToString();
        }

        /// <summary>
        /// Sets the InsertSPName for logging
        /// </summary>
        /// <param name="value"></param>
        public static void SetInsertSpName(string value)
        {
            GlobalContext.Properties["InsertSPName"] = value;
            GlobalContext.Properties["SPName"] = value; // backward compatibility
        }


        /// <summary>
        /// Sets the MachineName for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetMachineName(string value)
        {
            GlobalContext.Properties["MachineName"] = value;
        }

        /// <summary>
        /// Sets the ProcessName for logging
        /// </summary>
        /// <param name="value"></param>
        public static void SetProcessName(string value)
        {
            GlobalContext.Properties["ProcessName"] = value;
        }


        /// <summary>
        /// Sets the ProcessId for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetProcessId(string value)
        {
            GlobalContext.Properties["ProcessId"] = value;
        }

        /// <summary>
        /// Sets the EventId for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetEventId(int value)
        {
            GlobalContext.Properties["EventId"] = value;
        }


        /// <summary>
        /// Gets the Application Name for logging
        /// </summary>

        public static string GetAppName()
        {
            return GlobalContext.Properties["AppName"].ToString();
        }

        /// <summary>
        /// Sets the LogFileName for logging
        /// </summary>

        public static string GetLogFileName()
        {
            return GlobalContext.Properties["LogFileName"].ToString();
        }

        /// <summary>
        /// Sets the ADOConnectionString for logging
        /// </summary>

        public static string GetAdoConnectionString()
        {
            return GlobalContext.Properties["ADOConnectionString"].ToString();
        }

        /// <summary>
        /// Sets the ADOConnectionString for logging
        /// </summary>

        public static int GetAdoBufferSize()
        {
            return (int)GlobalContext.Properties["ADOBufferSize"];
        }

        /// <summary>
        /// Sets the InsertSPName for logging
        /// </summary>

        public static string GetInsertSpName()
        {
            return GlobalContext.Properties["InsertSPName"].ToString();
        }


        /// <summary>
        /// Sets the MachineName for logging
        /// </summary>

        public static string GetMachineName()
        {
            return GlobalContext.Properties["MachineName"].ToString();
        }
        /// <summary>
        /// Sets the ProcessName for logging
        /// </summary>

        public static string GetProcessName()
        {
            return GlobalContext.Properties["ProcessName"].ToString();
        }


        /// <summary>
        /// Sets the ProcessId for logging
        /// </summary>

        public static string GetProcessId()
        {
            return GlobalContext.Properties["ProcessId"].ToString();
        }

        /// <summary>
        /// Sets the EventId for logging
        /// </summary>

        public static int GetEventId()
        {
            return int.Parse(GlobalContext.Properties["EventId"].ToString());
        }

        /// <summary>
        /// gets the Exception for logging
        /// </summary>

        public static string GetException()
        {
            return GlobalContext.Properties["Exception"].ToString();
        }

        /// <summary>
        /// Sets the EventId for logging
        /// </summary>
        /// <param name="value"></param>

        public static void SetException(string value)
        {
            GlobalContext.Properties["Exception"] = value;
        }
    }
}
