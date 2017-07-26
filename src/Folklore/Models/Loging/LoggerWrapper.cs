using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Folklore.Models.Loging
{
    public class Log
    {
        #region Private Data Member

        /// <summary>
        /// main logger object to access log4net member and functions
        /// </summary>
        private static log4net.ILog _logger { get; set; }

        #endregion

        #region Public Data Members

        #endregion

        #region Static Data Member

        #endregion

        #region Constructor
        /// <summary>
        /// Static constructor
        /// </summary>
        static Log()
        {
            _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
        #endregion

        #region Private Member Functions

        #endregion

        #region Public Member Function

        #endregion

        #region Static Member Function

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="message"></param>
        public static void Info(object message)
        {
            _logger.Info(message);
        }

        /// <summary>
        /// Information with format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void InfoFormat(string format, params string[] args)
        {
            _logger.InfoFormat(format, args);
        }

        /// <summary>
        /// Log your debug messages
        /// </summary>
        /// <param name="message">Object: Message to log</param>
        public static void Debug(object message)
        {
            _logger.Debug(message);
        }

        /// <summary>
        /// Log your debug messages and exception
        /// </summary>
        /// <param name="message">Object: Message to log</param>
        /// <param name="exception">Exception: Exception to log</param>
        public static void Debug(object message, System.Exception exception)
        {
            _logger.Debug(message, exception);
        }

        /// <summary>
        /// With format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void DebugFormat(string format,params string[] args)
        {
            _logger.DebugFormat(format, args);
        }

        /// <summary>
        /// Log your error messages
        /// </summary>
        /// <param name="message">Object: Message to log</param>
        public static void Error(object message)
        {
            _logger.Error(message);
        }

        /// <summary>
        /// Log your error messages and exception
        /// </summary>
        /// <param name="message">Object: Message to log</param>
        /// <param name="exception">Exception: Exception to log</param>
        public static void Error(object message, System.Exception exception)
        {
            _logger.Error(message, exception);
        }

        /// <summary>
        /// With format
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void ErrorFormat(string format, params string[] args)
        {
            _logger.ErrorFormat(format, args);
        }
        #endregion
    }
}