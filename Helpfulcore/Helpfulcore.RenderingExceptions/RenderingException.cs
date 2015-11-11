using System;

namespace Helpfulcore.RenderingExceptions
{
	[Serializable]
	public class RenderingException : Exception
	{
		public RenderingException(string message)
			:base(message)
		{
		}

		public RenderingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
