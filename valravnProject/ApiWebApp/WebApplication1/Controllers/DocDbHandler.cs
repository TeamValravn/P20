using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace WebApplication1.Controllers
{
    class DocDbHandler
    {
        private DocumentClient client;

        private const string EndpointUri = "https://testdbmjs.documents.azure.com:443/";
        private const string PrimaryKey = "kBIqzqyIIr5zHfYe8vq7TRvWYzMElIuqoWsSo3BIOTUtLA425Fkngob1PPmAYUz3QC9H4Ug9MrH47jNs8YtvCg==";

        private const string DocDbUri = "https://testdbmjs.documents.azure.com:443/";
        private const string DocDbKey = "kBIqzqyIIr5zHfYe8vq7TRvWYzMElIuqoWsSo3BIOTUtLA425Fkngob1PPmAYUz3QC9H4Ug9MrH47jNs8YtvCg==";
        private const string DocDBName = "ValravnDb";
        private const string DocDbCollection = "ValravnDbCol";

        public async Task PostUWPToDocDb(Valravn msg)
        {
            this.client = new DocumentClient(new Uri(DocDbUri), DocDbKey);

            await this.CreateDatabaseIfNotExists(DocDBName);

            await this.CreateDocumentCollectionIfNotExists(DocDBName, DocDbCollection);

            await this.CreateDocIfNotExists(DocDBName, DocDbCollection, msg);
        }


        private async Task CreateDatabaseIfNotExists(string databaseName)
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = databaseName });
                    this.WriteToConsoleAndPromptToContinue("Created {0}", databaseName);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocumentCollectionIfNotExists(string databaseName, string collectionName)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    // Configure collections for maximum query flexibility including string range queries.
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // Here we create a collection with 400 RU/s.
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        collectionInfo,
                        new RequestOptions { OfferThroughput = 400 });

                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateDocIfNotExists(string databaseName, string collectionName, Valravn valravnmsg)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, valravnmsg.Id));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), valravnmsg);
                    this.WriteToConsoleAndPromptToContinue("Created valravn doc {0}", valravnmsg.Id);
                }
                else
                {
                    throw;
                }
            }
        }



        private async Task ReplaceBotDocument(string databaseName, string collectionName, string botId, Valravn updatedBotDoc)
        {
            try
            {
                await this.client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, botId), updatedBotDoc);
                this.WriteToConsoleAndPromptToContinue("Replaced BotInteration {0}", botId);
            }
            catch (DocumentClientException de)
            {
                throw;
            }
        }

        private void WriteToConsoleAndPromptToContinue(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }


    }
}
