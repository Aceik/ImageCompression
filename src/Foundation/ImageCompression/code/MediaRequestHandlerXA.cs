using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Pipelines;
using Sitecore.Resources.Media;
using Sitecore.Resources.Media.Streaming;
using Sitecore.Web;
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

            var chrome = context.Request.UserAgent.ToLower().Contains("chrome");
            var safari = !context.Request.UserAgent.ToLower().Contains("chrome") && context.Request.UserAgent.ToLower().Contains("safari");

            if (safari)
            {
                return base.DoProcessRequest(context, request, media);
            }

            if (request.InnerRequest.Path.ToLower().IndexOf(".webp") > -1)
            {
                //MediaStream normalStreamFromCache = GetStreamFromCache(media, request.Options);
                //if (normalStreamFromCache != null)
                //{
                //    ProcessMediaStreamCache(normalStreamFromCache, request, media, context);
                //    return true;
                //}


                // So why use a link field here ... Sitecore has a limitation on the default attachement field called Blob
                // You can only have one of these
                // So unfortunately you can't sore the WEBP and the JPEG binary data on the one Media Item in Sitecore. 
                // At this stage the best way to do this for thie module is have two Media Items. 
                Data.Fields.LinkField relatedField = media.MediaData.MediaItem.InnerItem.Fields["Related Compressed File"];
                if (relatedField != null && relatedField.TargetItem != null)
                {
                    var myMedia = new Data.Items.MediaItem(relatedField.TargetItem);
                    media = new Media(new MediaData(myMedia));

                    //MediaStream webpStreamFromCache = GetStreamFromCache(webpMedia, request.Options);
                    //if (webpStreamFromCache != null)
                    //{

                    //}

                    //MediaManager.Cache.AddStream(media, request.Options, webpMedia.GetStream(), out webpStreamFromCache);
                    //ProcessMediaStreamCache(webpStreamFromCache, request, media, context);
                    // return true;
                }
            }
            return base.DoProcessRequest(context, request, media);
        }

        //private void ProcessMediaStreamCache(MediaStream streamFromCache, MediaRequest request, Media media, HttpContext context)
        //{
        //    RaiseEvent("media:request", request);
        //    if (Configuration.Settings.Media.EnableRangeRetrievalRequest && Configuration.Settings.Media.CachingEnabled)
        //    {
        //        using (streamFromCache)
        //        {
        //            SendMediaHeaders(media, context);
        //            RangeRetrievalRequest request2 = RangeRetrievalRequest.BuildRequest(context, media);
        //            RangeRetrievalResponse rangeRetrievalResponse = new RangeRetrievalResponse(request2, streamFromCache);
        //            rangeRetrievalResponse.ExecuteRequest(context);
        //            return;
        //        }
        //    }
        //    SendMediaHeaders(media, context);
        //    SendStreamHeaders(streamFromCache, context);
        //    using (streamFromCache)
        //    {
        //        context.Response.AddHeader("Content-Length", streamFromCache.Stream.Length.ToString());
        //        WebUtil.TransmitStream(streamFromCache.Stream, context.Response, Configuration.Settings.Media.StreamBufferSize);
        //    }
        //}

        //private MediaStream GetStreamFromCache(Media media, MediaOptions options)
        //{
        //    return MediaManager.Cache.GetStream(media, options);
        //}
    }
}