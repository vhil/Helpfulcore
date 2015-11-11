using System;

namespace Helpfulcore.RenderingExceptions.Models
{
    public class RenderingExceptionViewModel
    {
        public RenderingExceptionViewModel(string renderingName, Exception exception)
        {
            Exception = exception;
            RenderingName = renderingName;
        }

        public string RenderingName { get; private set; }
        public Exception Exception { get; private set; }
    }
}
