using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;       
using Sitecore.Data.Items;
using Sitecore.Foundation.ImageCompression.Model;

namespace Sitecore.Foundation.ImageCompression.Services
{
    /// <summary>
    /// Call 
    /// </summary>
    public class KrakenImageGateway : TinyPngApiGateway, IImageCompressionService
    {
        private const string REMOTE_ERROR = "Kraken Remote API caused an error";
        private const string TIMY_CONNETION_ERROR = "Could not download Image from Kraken";
        private const string LOCATON_RESPONSE = "Location";
        private const string COMPRESSION_COUNT = "Compression-Count";

        public bool ShouldContinue { get; set; }

        public KrakenImageGateway()
        {
            ShouldContinue = true;
        }

        public override string CompressImage(Item currentItem)
        {
            var uploadedImage = SendToKrakenForCompression(currentItem);
            if (uploadedImage == null)
                return null;
            DownloadImage(currentItem, uploadedImage);
            return uploadedImage.Location;
        }

        public ImageUpload SendToKrakenForCompression(Item currentItem)
        {
            var client = new RestClient("https://api.kraken.io/v1/upload");
            //client.Authenticator = new HttpBasicAuthenticator("Api", ImageCompressionSettings.GetApiEndpointKey());
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

        public new RestRequest CreateUploadRequest(Item currentItem, RestClient client)
        {
            

            var ApiKey = "BLANK";
            var ApiSecret = "BLANK";
            //apiRequest.Body.Dev = false;
            //var isSet = apiRequest.Body is IOptimizeSetWaitRequest || apiRequest.Body is IOptimizeSetUploadWaitRequest;

            var _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } },
                NullValueHandling = NullValueHandling.Ignore
            };
                       
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

                    var requestJson = $"{{\"auth\":{{\"api_key\": \"{ApiKey}\", \"api_secret\": \"{ApiSecret}\"}}, \"wait\":true," +
                        "\"lossy\": true,"+
                        "\"webp\": true"+
                        $"}}";
                    //var json = JsonConvert.SerializeObject(apiRequest.Body, _serializerSettings);
                    request.AddParameter("data", requestJson);
                    //request.AddFile("data", requestJson, "application/json");
                    request.AddFile("upload", imageBytes, currentItem.Name, currentItem.Name);
                    request.AddHeader("Content-Type", "multipart/form-data");
                }
            }
            return request;
        }

        public byte[] ReadMediaStream(MediaItem _currentMedia)
        {
            Stream stream = _currentMedia.GetMediaStream();
            long fileSize = stream.Length;
            byte[] buffer = new byte[(int)fileSize];
            stream.Read(buffer, 0, (int)stream.Length);
            stream.Close();

            return buffer;
        }

        public new string DownloadImage(MediaItem currentItem, ImageUpload img)
        {
            var client = new RestClient(img.Location);
            //client.Authenticator = new HttpBasicAuthenticator("Api", ImageCompressionSettings.GetApiEndpointKey());

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
                UpdateImageInformation(currentItem, sizeBefore, sizeAfter, ImageCompressionConstants.Messages.OPTIMISED_BY_KRAKEN);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(TIMY_CONNETION_ERROR, ex);
                RecordError(currentItem, ex.Message);
            }
            return "API ISSUE";
        }

        internal class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
            {
                var contract = base.CreateDictionaryContract(objectType);

                contract.DictionaryKeyResolver = propertyName => propertyName;

                return contract;
            }
        }
    }
}