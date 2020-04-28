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

        //public bool DoProcessMediaRequest(HttpContext context, MediaRequest request, Media media)
        //{
        //    Assert.ArgumentNotNull(context, "context");
        //    Assert.ArgumentNotNull(request, "request");
        //    Assert.ArgumentNotNull(media, "media");
        //    if (Modified(context, media, request.Options) == Tristate.False)
        //    {
        //        RaiseEvent("media:request", request);
        //        SendMediaHeaders(media, context);
        //        context.Response.StatusCode = 304;
        //        return true;
        //    }
        //    ProcessImageDimensions(request, media);
        //    MediaStream mediaStream = GetMediaStream(media, request);
        //    if (mediaStream == null)
        //    {
        //        return false;
        //    }
        //    RaiseEvent("media:request", request);
        //    if (Sitecore.Configuration.Settings.Media.EnableRangeRetrievalRequest && Sitecore.Configuration.Settings.Media.CachingEnabled)
        //    {
        //        using (mediaStream)
        //        {
        //            SendMediaHeaders(media, context);
        //            RangeRetrievalRequest request2 = RangeRetrievalRequest.BuildRequest(context, media);
        //            RangeRetrievalResponse rangeRetrievalResponse = new RangeRetrievalResponse(request2, mediaStream);
        //            rangeRetrievalResponse.ExecuteRequest(context);
        //            return true;
        //        }
        //    }
        //    SendMediaHeaders(media, context);
        //    SendStreamHeaders(mediaStream, context);
        //    using (mediaStream)
        //    {
        //        context.Response.AddHeader("Content-Length", mediaStream.Stream.Length.ToString());
        //        WebUtil.TransmitStream(mediaStream.Stream, context.Response, Sitecore.Configuration.Settings.Media.StreamBufferSize);
        //    }
        //    return true;
        //}

        //private void ProcessImageDimensions(MediaRequest request, Media media)
        //{
        //    Assert.ArgumentNotNull(request, "request");
        //    Assert.ArgumentNotNull(media, "media");
        //    Sitecore.Data.Items.Item innerItem = media.MediaData.MediaItem.InnerItem;
        //    int.TryParse(innerItem["Height"], out int result);
        //    int.TryParse(innerItem["Width"], out int result2);
        //    bool flag = false;
        //    int maxHeight = Sitecore.Configuration.Settings.Media.Resizing.MaxHeight;
        //    if (maxHeight != 0 && request.Options.Height > Math.Max(maxHeight, result))
        //    {
        //        flag = true;
        //        request.Options.Height = Math.Max(maxHeight, result);
        //    }
        //    int maxWidth = Sitecore.Configuration.Settings.Media.Resizing.MaxWidth;
        //    if (maxWidth != 0 && request.Options.Width > Math.Max(maxWidth, result2))
        //    {
        //        flag = true;
        //        request.Options.Width = Math.Max(maxWidth, result2);
        //    }
        //    if (flag)
        //    {
        //        Log.Warn($"Requested image exceeds allowed size limits. Requested URL:{request.InnerRequest.RawUrl}", this);
        //    }
        //}

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

        //protected virtual void SendMediaHeaders(Media media, HttpContext context)
        //{
        //    SendMediaHeaders(media, new HttpContextWrapper(context));
        //}

        /// <summary>
        /// Sets the headers.
        /// </summary>
        /// <param name="media">
        /// The media.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        //public new void SendMediaHeaders(Media media, HttpContextBase context)
        //{
        //    ResponseCacheHeaders responseCacheHeaders = new ResponseCacheHeaders();
        //    responseCacheHeaders.Vary = GetVaryHeader(media, context);
        //    GetResponseCacheHeadersArgs getResponseCacheHeadersArgs = new GetResponseCacheHeadersArgs(media.MediaData.MediaItem.InnerItem, responseCacheHeaders);
        //    getResponseCacheHeadersArgs.RequestType = RequestTypes.Media;
        //    GetResponseCacheHeadersPipeline.Run(getResponseCacheHeadersArgs);
        //    HttpCachePolicyBase cache = context.Response.Cache;
        //    if (getResponseCacheHeadersArgs.CacheHeaders.LastModifiedDate.HasValue)
        //    {
        //        cache.SetLastModified(getResponseCacheHeadersArgs.CacheHeaders.LastModifiedDate.Value);
        //    }
        //    if (!string.IsNullOrEmpty(getResponseCacheHeadersArgs.CacheHeaders.ETag))
        //    {
        //        cache.SetETag(getResponseCacheHeadersArgs.CacheHeaders.ETag);
        //    }
        //    if (getResponseCacheHeadersArgs.CacheHeaders.Cacheability.HasValue)
        //    {
        //        cache.SetCacheability(getResponseCacheHeadersArgs.CacheHeaders.Cacheability.Value);
        //    }
        //    if (getResponseCacheHeadersArgs.CacheHeaders.MaxAge.HasValue)
        //    {
        //        cache.SetMaxAge(getResponseCacheHeadersArgs.CacheHeaders.MaxAge.Value);
        //    }
        //    if (getResponseCacheHeadersArgs.CacheHeaders.ExpirationDate.HasValue)
        //    {
        //        cache.SetExpires(getResponseCacheHeadersArgs.CacheHeaders.ExpirationDate.Value);
        //    }
        //    if (!string.IsNullOrEmpty(getResponseCacheHeadersArgs.CacheHeaders.CacheExtension))
        //    {
        //        cache.AppendCacheExtension(getResponseCacheHeadersArgs.CacheHeaders.CacheExtension);
        //    }
        //    if (!string.IsNullOrEmpty(getResponseCacheHeadersArgs.CacheHeaders.Vary))
        //    {
        //        context.Response.AppendHeader("vary", getResponseCacheHeadersArgs.CacheHeaders.Vary);
        //    }
        //}

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