namespace ECOLAB.IOT.SiteManagement.Provider
{
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Sas;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;

    public interface IStorageProvider
    {
        public Task<string> DownloadToText(string connectionString, string blobContainerName, string Url);

        public Task<string> GetBlobMD5(string connectionString, string blobContainerName, string Url);

        public Task<string> CopyToTargetContainer(string connectionString, string blobContainerName, string Url,string blobContainerName_Target,string targetRelativePath);

        public Task<string> UploadJsonToBlob(string connectionString, string blobContainerName,string fileName, string json);
    }

    public class StorageProvider : IStorageProvider
    {
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

        public async Task<string> UploadJsonToBlob(string connectionString, string blobContainerName, string fileName, string json)
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
            await blob.UploadAsync(ms, options);
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

        public async Task<string> GetBlobMD5(string connectionString, string blobContainerName, string Url)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);//storage connection string

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(blobContainerName);//container name

            //Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(Url);

            await blockBlob.FetchAttributesAsync();//

            string md5 = blockBlob.Properties.ContentMD5;

            return md5;
        }
    }
}
