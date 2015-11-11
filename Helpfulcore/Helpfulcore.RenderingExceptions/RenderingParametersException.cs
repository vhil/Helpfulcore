using System;

namespace Helpfulcore.RenderingExceptions
{
    [Serializable]
    public class RenderingParametersException : RenderingException
    {
        private const string ExceptionMessageFormat = "'{0}' rendering parameter should not be empty. Select 'More' -> 'Edit the Component Properties' and make sure this property is set to relevant value.";
        private const string ExceptionMessageFormatWithMessage = "'{0}' {1}. Select 'More' -> 'Edit the Component Properties' and make sure this property is set to relevant value.";

        public RenderingParametersException(string parameterName)
            : base(string.Format(ExceptionMessageFormat, parameterName))
        {
        }

        public RenderingParametersException(string parameterName, Exception innerException)
            : base(string.Format(ExceptionMessageFormat, parameterName), innerException)
        {
        }

        public RenderingParametersException(string parameterName, string additionalMessage)
            : base(string.Format(ExceptionMessageFormatWithMessage, parameterName, additionalMessage))
        {
        }
    }
}