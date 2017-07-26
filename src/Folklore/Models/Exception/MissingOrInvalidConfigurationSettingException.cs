using System;

namespace Folklore.Models.Exception
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MissingOrInvalidConfigurationSettingException : System.Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public MissingOrInvalidConfigurationSettingException()
        {

            //this.Message = "A missing or invalid configuration setting was returned";

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public MissingOrInvalidConfigurationSettingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MissingOrInvalidConfigurationSettingException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

    }
}
