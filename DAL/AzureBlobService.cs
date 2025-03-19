using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Azure.Storage;
using Azure.Storage.Blobs;

namespace SML {
    public class AzureBlobService {
        private readonly string storageAccount = "storagesml";
        private readonly string accesskey = "VSv25wvlbaUnoNauf+5lysnzwYuNL9qVRNm+faowwTDD6//ZEDmE3tXQB5dZ1ZJqXbmxGkPB/IVK+ASt6U2oVw==";
        private readonly BlobServiceClient blobServiceClient;


        public AzureBlobService() {
            var credential = new StorageSharedKeyCredential(storageAccount, accesskey);
            var blobUri = $"https://{storageAccount}.blob.core.windows.net";
            blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
        }


        // List all the blob containers at top of hierarchy
        public async Task ListBlobContainersAsync() {
            var containers = blobServiceClient.GetBlobContainersAsync();

            await foreach (var container in containers) {
                System.Diagnostics.Debug.WriteLine(container.Name);
            }
        }


        // Upload blobs into directory location
        public async Task<Uri> UploadFilesAsync(string filePath, string uuid) {
            // Name of container
            var blobContainer = blobServiceClient.GetBlobContainerClient("replays");

            // Location the blob will be stored, with the replay UUID as the blob's name for replay retrieval
            var blob = blobContainer.GetBlobClient($"replays/{uuid}");

            // send it
            await blob.UploadAsync(filePath, overwrite:true);

            return blob.Uri;
        }


        // List of the blobs in a single directory
        public async Task GetFlatBlobsListAsync() {
            var blobContainer = blobServiceClient.GetBlobContainerClient("files");
            var blobs = blobContainer.GetBlobsAsync();

            await foreach (var blob in blobs) {
                System.Diagnostics.Debug.WriteLine(blob.Name);            
            }
        }


        // List of blobs in all directories within current hierarchy
        public async Task GetHierarchicalBlobsListAsync() {
            var blobContainer = blobServiceClient.GetBlobContainerClient("files");
            var blobs = blobContainer.GetBlobsByHierarchyAsync();

            await foreach (var blob in blobs)
            {
                if (blob.IsPrefix) {
                    // Write out prefix of virtual directory
                    System.Diagnostics.Debug.WriteLine("Virtual Directory prefix: {0}", blob.Prefix);

                    // Call recursively with prefix to traverse virtual directory
                    await GetHierarchicalBlobsListAsync();
                }
                else {
                    // Write the name of the blob
                    System.Diagnostics.Debug.WriteLine("Blob name {0}", blob.Blob.Name);
                }
            }

        }


        // Delete directory and all blobs within it
        public async Task DeleteBlobsAsync() {
            string fileName = "hello.txt";
            var blobContainer = blobServiceClient.GetBlobContainerClient("files");
            var blob = blobContainer.GetBlobClient($"today/{fileName}");

            await blob.DeleteIfExistsAsync();
        }

    }

}
