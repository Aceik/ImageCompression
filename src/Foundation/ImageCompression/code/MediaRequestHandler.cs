using System;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Pipelines.GetResponseCacheHeaders;
using Sitecore.Resources.Media;
using Sitecore.Resources.Media.Streaming;
using Sitecore.Web;
using Sitecore.XA.Foundation.MediaRequestHandler.Pipelines.MediaRequestHandler;

namespace Sitecore.Foundation.ImageCompression
{
    public class MediaRequestHandler : Sitecore.Resources.Media.MediaRequestHandler
    {
        public new bool DoProcessRequest(HttpContext context, MediaRequest request, Media media)
        { 
            media = DetermineImageResponse(context, request, media);
            bool result = base.DoProcessRequest(context, request, media);
            return result;
        }

        public Media DetermineImageResponse(HttpContext context, MediaRequest request, Media media)
        {
            Diagnostics.Assert.ArgumentNotNull(context, "context");
            Diagnostics.Assert.ArgumentNotNull(request, "request");
            Diagnostics.Assert.ArgumentNotNull(media, "media");

            if(ShouldSkipWebProcessing(context))
            {
                return media;
            }

            // So why use a link field here ... Sitecore has a limitation on the default attachement field called Blob
            // You can only have one of these
            // So unfortunately you can't sore the WEBP and the JPEG binary data on the one Media Item in Sitecore. 
            // At this stage the best way to do this for thie module is have two Media Items. 
            Data.Fields.LinkField relatedField = media.MediaData.MediaItem.InnerItem.Fields["Related Compressed File"];
            if (relatedField != null && relatedField.TargetItem != null)
            {
                var myMedia = new Data.Items.MediaItem(relatedField.TargetItem);
                media = new Media(new MediaData(myMedia));
            }
            return media;
        }

        private bool ShouldSkipWebProcessing(HttpContext context)
        {
            // But note that some browsers are lying: Chrome for example reports both as Chrome and Safari. So to detect Safari you have to check for the Safari string and the absence of the Chrome string, Chromium often reports itself as Chrome too or Seamonkey sometimes reports itself as Firefox.
            var safari = !context.Request.UserAgent.ToLower().Contains("chrome") && context.Request.UserAgent.ToLower().Contains("safari");
            var isIe = context.Request.Browser.Type.ToUpper() == "IE";
            if (safari || isIe)
            {
                return true;
            }

            if (context.Request.Path.Contains("/sitecore/shell/"))
                return true;

            var extensionParam = (string)context.Request.Params["extension"];
            if(!string.IsNullOrWhiteSpace(extensionParam))
            {
                var extensionIsNotWebp = extensionParam != "webp";
                if (extensionIsNotWebp)
                {
                    return true;
                }
            }

            return false;
        }
    }
}