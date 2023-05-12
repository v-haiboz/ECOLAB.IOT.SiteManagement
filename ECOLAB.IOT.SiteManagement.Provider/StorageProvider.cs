namespace ECOLAB.IOT.SiteManagement.Provider
{
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Sas;
    using Flurl.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Reflection.Metadata;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using BlobProperties = Azure.Storage.Blobs.Models.BlobProperties;

    public interface IStorageProvider
    {
        public Task<string> DownloadToText(string connectionString, string blobContainerName, string Url);

        public Task<string> GetBlobMD5(string connectionString, string blobContainerName, string Url);

        public Task<string> GetBlobMD5ByBlobClient(string AccountName, string AccountKey, string Url);

        public Task<string> CopyToTargetContainer(string connectionString, string blobContainerName, string Url,string blobContainerName_Target,string targetRelativePath);

        public string UploadJsonToBlob(string connectionString, string blobContainerName,string fileName, string json);

        public Task<string> GetAllowListSASUrl(string url);
    }

    public class StorageProvider : IStorageProvider
    {
        private readonly IConfiguration _config;
        private string SASUrl;
        private string StorageAPICertificate;
        public StorageProvider(IConfiguration config)
        {
            _config = config;
            SASUrl = _config["StorageAPI:SASUrl"];
            StorageAPICertificate = _config["StorageAPI:StorageAPICertificate"];
        }

        public async Task<string> CopyToTargetContainer(string connectionString, string blobContainerName, string Url, string blobContainerName_Target,string targetRelativePath)
        {
            try
            {
                var blobClient_Souce = new BlobClient(
                 connectionString: connectionString,
                 blobContainerName: blobContainerName,
                 blobName: Url
                );

                var sourceBlobSasToken = blobClient_Souce.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddHours(2));
                var fileName = HttpUtility.UrlDecode(blobClient_Souce.Uri.Segments.Last());
                var targetUrl = @$"{targetRelativePath}/{fileName}";

                var blobClient_Target = new BlobClient(
                 connectionString: connectionString,
                 blobContainerName: blobContainerName_Target,
                 blobName: targetUrl
                );

                var result=await blobClient_Target.StartCopyFromUriAsync(sourceBlobSasToken);
                return blobClient_Target.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UploadJsonToBlob(string connectionString, string blobContainerName, string fileName, string json)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
            BlobUploadOptions options = new BlobUploadOptions
            {
                TransferOptions = new StorageTransferOptions
                {
                    // Set the maximum number of workers that 
                    // may be used in a parallel transfer.
                    MaximumConcurrency = 8,

                    // Set the maximum length of a transfer to 50MB.
                    MaximumTransferSize = 50 * 1024 * 1024
                }
            };

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            BlobClient blob = containerClient.GetBlobClient(fileName);
            blob.Upload(ms, options);
            return blob.Uri.ToString();
        }

        public async Task<string> DownloadToText(string connectionString, string blobContainerName, string Url)
        {
            try
            {
                var blobClient = new BlobClient(
                 connectionString: connectionString,
                 blobContainerName: blobContainerName,
                 blobName: Url
                );

                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                string downloadedData = downloadResult.Content.ToString();

                return downloadedData;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<string> GetBlobMD5(string connectionString, string blobContainerName, string blobName)
        {

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);//storage connection string

            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = cloudBlobClient.GetContainerReference(blobContainerName);//container name

            //Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            await blockBlob.FetchAttributesAsync();//

            string md5 = blockBlob.Properties.ContentMD5;

            return md5;
        }

        public async Task<string> GetBlobMD5ByBlobClient(string AccountName, string AccountKey, string Url)
        {
            Uri uri = new Uri(Url);
            var storageSharedKeyCredential = new StorageSharedKeyCredential(AccountName, AccountKey);
            BlobClient blobClient = new BlobClient(uri, storageSharedKeyCredential);
            BlobProperties properties = await blobClient.GetPropertiesAsync();
            var hee = await blobClient.GetTagsAsync();
            return properties.Metadata.ToString();
        }

        public async Task<string> GetAllowListSASUrl(string url)
        {
            try
            {
                var list = new List<SASUrl>();
                list.Add(new SASUrl() { noneKeyUrl = url });

                var result = await SASUrl.WithHeader("Storage-API-Certificate", StorageAPICertificate).PostJsonAsync(list).ReceiveJson<Root>();

                if (result.status == 200 && result.data != null && result.data.Count > 0)
                {
                    return result.data[0].fileSASUrl;
                }

                return "";
            }
            catch (Exception ex)
            {
                return "";
            }

        }
    }

    public class SASUrl
    {
        public string noneKeyUrl { get; set; }
    }
    public class DataItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string fileSASUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fileFullPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string containerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string storageAccountName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int uploadUtcTime { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string errors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float timestamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<DataItem> data { get; set; }
    }

    public class TokenInfoDto
    {
        public string? TokenType { get; set; }
        public int Expires_In { get; set; }

        public int Ext_Expires_In { get; set; }

        public string? Access_Token { get; set; }

    }
}
