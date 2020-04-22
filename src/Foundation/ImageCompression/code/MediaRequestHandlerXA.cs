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

            // But note that some browsers are lying: Chrome for example reports both as Chrome and Safari. So to detect Safari you have to check for the Safari string and the absence of the Chrome string, Chromium often reports itself as Chrome too or Seamonkey sometimes reports itself as Firefox.
            var safari = !context.Request.UserAgent.ToLower().Contains("chrome") && context.Request.UserAgent.ToLower().Contains("safari");

            if (safari || context.Request.Browser.Type.ToUpper() == "IE")
            {
                return base.DoProcessRequest(context, request, media);
            }

            if (request.InnerRequest.Path.ToLower().IndexOf(".webp") > -1)
            {
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
            }
            return base.DoProcessRequest(context, request, media);
        }
    }
}