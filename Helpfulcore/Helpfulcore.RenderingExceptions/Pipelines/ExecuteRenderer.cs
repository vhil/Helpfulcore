using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Helpfulcore.Logging;
using Sitecore.Mvc.Pipelines;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;
using Helpfulcore.RenderingExceptions.Controllers;
using Helpfulcore.RenderingExceptions.Models;
using Sitecore.Configuration;

namespace Helpfulcore.RenderingExceptions.Pipelines.RenderRendering
{
	public class ExecuteRenderer : Sitecore.Mvc.Pipelines.Response.RenderRendering.ExecuteRenderer
	{
	    protected virtual string CurrentUrl
		{
			get { return HttpContext.Current != null ? HttpContext.Current.Request.Url.AbsoluteUri : string.Empty; }
		}

        public override void Process(RenderRenderingArgs args)
        {
	        try
	        {
		        base.Process(args);
	        }
            catch (Exception ex)
            {
                args.Cacheable = false;

                var renderingErrorException = this.GetRenderingException(ex);
	            
	            if (renderingErrorException != null)
                {
                    this.RenderRenderingError(args, renderingErrorException as RenderingException);

                    if (Logger != null)
                    {
                        Logger.Warn(
                            "Error while rendering view [{0}] on page '{1}. Please, make sure the rendering is configured properly. {2}",
                            this,
                            null,
                            this.GetRenderingName(args),
                            this.CurrentUrl,
                            renderingErrorException.Message);
                    }
                }
                else
                {
					this.RenderUnhandledException(args, ex);

                    if (Logger != null)
                    {
                        Logger.Error(
                            "Error while rendering view [{0}] on page '{1}'. {2}",
                            this,
                            ex,
                            this.GetRenderingName(args),
                            this.CurrentUrl,
                            ex.Message);
                    }
                }
            }
        }

		public virtual string RenderViewToString(string viewName, object model, ControllerContext controllerContext)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controllerContext.RouteData.GetRequiredString("action");
            }

            controllerContext.Controller.ViewData.Model = model;

            using (var stringWriter = new StringWriter())
            {
                var viewEngineResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);
                
                var viewContext = new ViewContext(
                    controllerContext, 
                    viewEngineResult.View,
                    controllerContext.Controller.ViewData,
                    controllerContext.Controller.TempData,
                    stringWriter);

                viewEngineResult.View.Render(viewContext, stringWriter);

                return stringWriter.GetStringBuilder().ToString();
            }
        }

		protected virtual RenderingExceptionViewModel GetRenderingErrorModel(RenderRenderingArgs args, Exception ex)
        {
			return new RenderingExceptionViewModel(this.GetRenderingName(args), ex);
        }

		protected virtual string GetRenderingName(RenderRenderingArgs args)
		{
			return args.Rendering.RenderingItem != null ? args.Rendering.RenderingItem.Name : string.Empty;
		}

		protected virtual ControllerContext GetControllerContext(MvcPipelineArgs args)
        {
			return new ControllerContext(args.PageContext.RequestContext, new RenderingExceptionsController());
        }

		protected virtual Exception GetRenderingException(Exception ex)
		{
			if (ex is RenderingException)
			{
				return ex;
			}

			if (ex.InnerException != null)
			{
				return this.GetRenderingException(ex.InnerException);
			}

			return null;
		}

		protected virtual void RenderRenderingError(RenderRenderingArgs args, RenderingException ex)
		{
			var viewName = "RenderingExceptions/" + this.GetViewNameFromException(ex);

			args.Writer.Write(this.RenderViewToString(viewName, this.GetRenderingErrorModel(args, ex), this.GetControllerContext(args)));
		}

		protected virtual void RenderUnhandledException(RenderRenderingArgs args, Exception ex)
		{
			var viewName = "RenderingExceptions/UnhandledException";

			args.Writer.Write(this.RenderViewToString(viewName, this.GetRenderingErrorModel(args, ex), this.GetControllerContext(args)));
		}

		protected virtual string GetViewNameFromException(Exception exception)
		{
			return exception.GetType().Name;
		}

        private static readonly object LoggerSyncRoot = new object();
        private static ILoggingService logger;
        protected static ILoggingService Logger
        {
            get
            {
                if (logger == null)
                {
                    lock (LoggerSyncRoot)
                    {
                        if (logger == null)
                        {
                            logger = Factory.CreateObject("helpfulcore/renderingExceptions/logger", false) as ILoggingService;
                        }
                    }
                }

                return logger;
            }
        }
    }
}
