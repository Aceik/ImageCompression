using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Sitecore.Data.Items;
using Sitecore.Foundation.ImageCompression.Model;
using Sitecore.Foundation.ImageCompression.Services;
using Sitecore.Foundation.ImageCompression.Settings;
using Sitecore.IO;
using Sitecore.Resources.Media;

namespace Sitecore.Foundation.ImageCompression.Services
{
    /// <summary>
    /// Call 
    /// </summary>
    public class TinyPngApiGateway : IImageCompressionService
    {
        private const string REMOTE_ERROR = "Tiny PNG Remote API caused an error";
        
        private const string TIMY_CONNETION_ERROR = "Could not download Image from Tiny PNG";
        private const string LOCATON_RESPONSE = "Location";
        private const string COMPRESSION_COUNT = "Compression-Count";

        public bool ShouldContinue { get; set; }

        public static readonly string[] SizeSuffixes =
            { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public TinyPngApiGateway()
        {
            ShouldContinue = true;
        }

        public virtual string CompressImage(Item currentItem)
        {
            var uploadedImage = SendToTinyForCompression(currentItem);
            if (uploadedImage == null)
                return null;
            DownloadImage(currentItem, uploadedImage);
            return uploadedImage.Location;
        }

        public virtual ImageUpload SendToTinyForCompression(Item currentItem)
        {
            var client = new RestClient(ImageCompressionSettings.GetApiEndpoint());
            client.Authenticator = new HttpBasicAuthenticator("Api", ImageCompressionSettings.GetApiEndpointKey());
            var request = CreateUploadRequest(currentItem, client);
            client.Timeout = 300000;

            try
            {
                var response = client.Execute<ImageUpload>(request);
                var content = response.Content;

                if(response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Sitecore.Diagnostics.Log.Info($"Image Upload failed {response.StatusCode} | response content: {response.Content}", this);
                    ShouldContinue = false;
                    return null;
                }

                response.Data.Location = GetHeader(response, LOCATON_RESPONSE);

                if (string.IsNullOrEmpty(response.Data.Location))
                    return null;

                Diagnostics.Log.Info($"Image Uploaded to Tiny PNG {response.Data.Location} | Compression count so far: {GetHeader(response, COMPRESSION_COUNT)}", this);

                return response.Data;
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(REMOTE_ERROR, ex);
                RecordError(currentItem, ex.Message);
            }
            return null;
        }

        protected string GetHeader(IRestResponse response, string key)
        {
            var headers = response.Headers.ToList();

            if (!headers.Exists(x => x.Name == key))
                return null;

             return headers.Find(x => x.Name == key).Value.ToString();
        }

        public virtual RestRequest CreateUploadRequest(Item currentItem, RestClient client)
        {
            var request = new RestRequest(Method.POST);
            request.Parameters.Clear();
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");

            if (currentItem != null)
            {
                var _mediaItem = new MediaItem(currentItem);

                if (_mediaItem != null)
                {
                    var imageBytes = ReadMediaStream(_mediaItem);

                    request.AddHeader("content-length", imageBytes.Length.ToString());
                    request.AddHeader("accept-encoding", "gzip, deflate");
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddHeader("Content-Disposition",
                        string.Format("file; filename=\"{0}\"; documentid={1}; fileExtension=\"{2}\"",
                        _mediaItem.Name, 1234, _mediaItem.Extension));
                    request.AddParameter("application/x-www-form-urlencoded", imageBytes, ParameterType.RequestBody);

                }
            }
            return request;
        }

        protected byte[] ReadMediaStream(MediaItem _currentMedia)
        {
            Stream stream = _currentMedia.GetMediaStream();
            long fileSize = stream.Length;
            byte[] buffer = new byte[(int)fileSize];
            stream.Read(buffer, 0, (int)stream.Length);
            stream.Close();

            return buffer;
        }

        public virtual string DownloadImage(MediaItem currentItem, ImageUpload img)
        {
            var client = new RestClient(img.Location);
            client.Authenticator = new HttpBasicAuthenticator("Api", ImageCompressionSettings.GetApiEndpointKey());

            var request = new RestRequest(Method.GET);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            client.Timeout = 300000;

            try
            {
                byte [] responseData = client.DownloadData(request);
                string sizeBefore = currentItem.InnerItem.Fields["Size"].Value;
                UpdateImageFile(currentItem, responseData);
                string sizeAfter = currentItem.InnerItem.Fields["Size"].Value;
                UpdateImageInformation(currentItem, sizeBefore, sizeAfter, ImageCompressionConstants.Messages.OPTIMISED_BY);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(TIMY_CONNETION_ERROR, ex);
                RecordError(currentItem, ex.Message);
            }
            return "API ISSUE";
        }

        protected void RecordError(MediaItem currentItem, string message)
        {
            currentItem.InnerItem.Editing.BeginEdit();
            currentItem.InnerItem.Fields[ImageCompressionSettings.GetInformationField()].Value = message;
            currentItem.InnerItem.Editing.EndEdit();
        }

        protected void UpdateImageFile(MediaItem currentItem, byte[] responseData)
        {
            currentItem.BeginEdit();
            Media media = MediaManager.GetMedia(currentItem);
            Stream stream = new MemoryStream(responseData);
            media.SetStream(stream, currentItem.Extension);
            currentItem.EndEdit();
        }

        protected void UpdateImageInformation(MediaItem currentItem, string sizeBefore, string sizeAfter, string optimisedBy)
        {
            currentItem.InnerItem.Editing.BeginEdit();
            string sizeBeforeStr = $"{SizeSuffix(Int64.Parse(sizeBefore))}";
            string sizeAfterStr = $"{SizeSuffix(Int64.Parse(sizeAfter))}";
            var compressionEntry = $"{optimisedBy} | Before: {sizeBeforeStr} | After: {sizeAfterStr}";
            currentItem.InnerItem.Fields[ImageCompressionSettings.GetInformationField()].Value = compressionEntry;
            Diagnostics.Log.Info($"{currentItem.ID} {currentItem.Name} {compressionEntry}", optimisedBy);
            currentItem.InnerItem.Editing.EndEdit();
        }

        public static double ConvertBytesToMegabytes(string bytes)
        {
            return (Int32.Parse(bytes) / 1024f) / 1024f;
        }

        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}