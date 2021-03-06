﻿using BestProductsApp.Models.Services;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BestProductsApp.Services.Queue
{
    public class QueueService : IQueueService, IDisposable
    {
        public AzureStorageOptions StorageOptions { get; set; }
        public QueueService(IOptions<AzureStorageOptions> storageOptions)
        {
            this.StorageOptions = storageOptions?.Value;
        }

        public QueueService(AzureStorageOptions storageOptions)
        {
            this.StorageOptions = storageOptions;
        }

        public QueueService() { }

        ~QueueService()
        {
            Dispose();
        }

        public bool AddQueue<T>(QueueTypes type, T message)
        {
            try
            {
                CloudStorageAccount storage = CloudStorageAccount.Parse(StorageOptions.ConnectionString);
                var queueClient = storage.CreateCloudQueueClient();
                var queueRef = queueClient.GetQueueReference(type.GetDescriptionString());

                queueRef.CreateIfNotExistsAsync().Wait();

                queueRef.AddMessageAsync(new Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage(JsonConvert.SerializeObject(message))).Wait();
                
                GC.SuppressFinalize(queueRef);
                GC.SuppressFinalize(queueClient);
                GC.SuppressFinalize(storage);
                return true;
            }
            catch { return false; }
        }

        public void Dispose()
        {
            GC.WaitForPendingFinalizers();
        }

    }
}
