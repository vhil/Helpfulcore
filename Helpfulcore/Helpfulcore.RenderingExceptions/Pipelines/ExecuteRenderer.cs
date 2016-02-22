using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Sitecore.Mvc.Pipelines;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;
using Helpfulcore.RenderingExceptions.Controllers;
using Helpfulcore.RenderingExceptions.Models;
using Sitecore.Diagnostics;

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

                var renderingErrorException = GetRenderingException(ex);
	            
	            if (renderingErrorException != null)
                {
                    this.RenderRenderingError(args, renderingErrorException as RenderingException);
					
					Log.Warn(
						string.Format(
							"Error while rendering view [{0}] on page {1}. Please, make sure the rendering is configured properly. {2}", 
							this.GetRenderingName(args),
 							this.CurrentUrl,
							renderingErrorException.Message),
						null,
						this);
                }
                else
                {
					this.RenderUnhandledException(args, ex);

					Log.Error(
						string.Format(
							"Error while rendering view [{0}] on page {1}. {2}",
							this.GetRenderingName(args),
							this.CurrentUrl,
							ex.Message),
						ex,
						this);
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
			return new RenderingExceptionViewModel(GetRenderingName(args), ex);
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
				return GetRenderingException(ex.InnerException);
			}

			return null;
		}

		protected virtual void RenderRenderingError(RenderRenderingArgs args, RenderingException ex)
		{
			var viewName = "RenderingExceptions/" + GetViewNameFromException(ex);

			args.Writer.Write(this.RenderViewToString(viewName, GetRenderingErrorModel(args, ex), GetControllerContext(args)));
		}

		protected virtual void RenderUnhandledException(RenderRenderingArgs args, Exception ex)
		{
			var viewName = "RenderingExceptions/UnhandledException";

			args.Writer.Write(this.RenderViewToString(viewName, GetRenderingErrorModel(args, ex), GetControllerContext(args)));
		}

		protected virtual string GetViewNameFromException(Exception exception)
		{
			return exception.GetType().Name;
		}
    }
}
