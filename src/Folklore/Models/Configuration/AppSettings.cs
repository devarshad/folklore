using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Folklore.Models.Configuration
{
    public class AppSettings
    {
        /// <summary>
        /// Constants for Application Level settings
        /// </summary>
        public static class Application
        {
            /// <summary>
            /// Setting determines if DB lookup is used to get configuration settings
            /// </summary>
            public const string ConfigDbLookup = "Configuration.UseDbLookup";

            /// <summary>
            /// Setting determines the application environment used for Exception Logging 
            /// </summary>
            public const string Environment = "Application.Environment";

            /// <summary>
            /// Setting for application wide connection string name. Can also be used to specify
            /// the database used for logging.
            /// </summary>
            public const string ConnectionString = "Application.ConnectionString";
        }

        /// <summary>
        /// Constants for Log4Net integration
        /// </summary>
        public static class Log4Net
        {
            /// <summary>
            /// Specify the config file key used for log4net logging. If not specified the internal file is used.
            /// </summary>
            public const string ConfigFile = "log4net.config";

            /// <summary>
            /// Specify the path key used for log4net logging.
            /// </summary>
            public const string LogfilePath = "log4net.RollingFileAppender.LogfilePath";

            /// <summary>
            /// Specify the path used for log4net logging.
            /// </summary>
            public const string LogfileName = "log4net.RollingFileAppender.LogfileName";

            /// <summary>
            /// Specify the Stored Procedure key used for log4net logging.
            /// </summary>
            public const string StoredProcedure = "log4net.StoredProcedure";

            /// <summary>
            /// Specify the Logging Level key used for log4net logging level.
            /// </summary>
            public const string LoggingLevel = "log4net.LoggingLevel";

            /// <summary>
            /// Specify the ADO Appender key used to set the buffer size.
            /// </summary>
            public const string AdoAppenderBufferSize = "log4net.AdoAppender.BufferSize";

            /// <summary>
            /// Setting for logging connection string name. Can be used if different than ApplicationConnectionString 
            /// </summary>
            public const string AdoAppenderConnectionString = "log4net.AdoAppender.ConnectionString";
        }

        /// <summary>
        /// Settings stored in .config file
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// Appenders must be specified in log4net.config file
            /// </summary>
            public static class Appenders
            {
                /// <summary>
                /// Specify the log4net appender used for logging to SmtpAppender. 
                /// </summary>
                public const string Rollbar = "SmtpAppender";

                /// <summary>
                /// Specify the log4net appender used for logging to DB. 
                /// </summary>
                public const string AdoAppender = "AdoNetAppender";

                /// <summary>
                /// Specify the log4net appender used for logging to a file. 
                /// </summary>
                public const string RollingFileAppender = "RollingFileAppender";

            }
        }
        /// <summary>
        /// Retrieves appsetting specific to iSeries SP and FUNC names. Supports default schema if none is
        /// specified in the setting.
        /// Method now supports a defaul value allowing for local override of standard object names.
        /// For iSeries Schema and Library are synonymous here but are very different in the IBM world.
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetAppSetting(string keyName, string defaultValue = "")
        {
            string keyVal = defaultValue;
            ConfigManager.TryGetAppSetting(keyName, ref keyVal);

            return keyVal;
        }

    }
}