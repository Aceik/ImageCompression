using System;
using System.Web;
using Sitecore.Pipelines;
using Sitecore.Resources.Media;
using Sitecore.XA.Foundation.MediaRequestHandler.Pipelines.MediaRequestHandler;

namespace Sitecore.Foundation.ImageCompression
{
    public class MediaRequestHandlerXA : Sitecore.XA.Foundation.MediaRequestHandler.MediaRequestHandler
    {
        protected override bool DoProcessRequest(HttpContext context)
        {
            var mediaRequestHandlerArgs = new MediaRequestHandlerArgs(context);
            CorePipeline.Run("mediaRequestHandler", mediaRequestHandlerArgs, failIfNotExists: false);
            if (mediaRequestHandlerArgs.Aborted)
            {
                return mediaRequestHandlerArgs.Result;
            }
            var media = new MediaRequestHandler().DetermineImageResponse(mediaRequestHandlerArgs.Context, mediaRequestHandlerArgs.Request, mediaRequestHandlerArgs.Media);
            return DoProcessRequest(mediaRequestHandlerArgs.Context, mediaRequestHandlerArgs.Request, media);
        }
    }
}