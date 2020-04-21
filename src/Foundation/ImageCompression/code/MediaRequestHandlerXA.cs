using System;
using System.Collections.Generic;
using System.Linq;
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
            return DoProcessRequest(mediaRequestHandlerArgs.Context, mediaRequestHandlerArgs.Request, mediaRequestHandlerArgs.Media);
        }

        protected override bool DoProcessRequest(HttpContext context, MediaRequest request, Media media)
        {
            Diagnostics.Assert.ArgumentNotNull(context, "context");
            Diagnostics.Assert.ArgumentNotNull(request, "request");
            Diagnostics.Assert.ArgumentNotNull(media, "media");

            if (context?.Request.QueryString?["extension"] == "webp" || (context.BrowserSupportsWebP()))
            {
                Sitecore.Data.Fields.LinkField relatedField = media.MediaData.MediaItem.InnerItem.Fields["Related Compressed File"];
                if (relatedField != null && relatedField.TargetItem != null)
                {
                    var myMedia = new Data.Items.MediaItem(relatedField.TargetItem);
                    return base.DoProcessRequest(context, request, new Media(new MediaData(myMedia)));
                }
            }
            return base.DoProcessRequest(context, request, media);
        }
    }

    public static class Helpers
    {
        public static bool BrowserSupportsWebP(this HttpContext context)
        {
            return context?.Request.AcceptTypes != null && context.Request.AcceptTypes.Contains("image/webp");
        }

        public static bool BrowserSupportsWebP(this MediaOptions mediaOptions)
        {
            return mediaOptions.GetCustomExtension() == "webp";
        }

        public static string GetCustomExtension(this MediaOptions mediaOptions)
        {
            return mediaOptions.CustomOptions["extension"];
        }
    }
}