using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace DocumentDbSerializationTest
{
    internal class Program
    {
        private static readonly long LastYear = DateTime.Now.AddYears(-1).Ticks;

        private static void Main(string[] args)
        {

            var client = new DocumentClient(new Uri("https://url.com:443/"), "**********");
            var database = GetDatabase(client);
            var documentCollection = GetDocumentCollection(client, database);

            SetCulture("en-GB");

            var query1 = client
                .CreateDocumentQuery<Document>(documentCollection.SelfLink)
                .Where(x => x.Created > LastYear);

            // query1 is now: 'SELECT * FROM root WHERE (root.created > 6.35430269777489E+17)'

            query1.ToList(); // works

            SetCulture("nl-NL"); 

            var query2 = client
                .CreateDocumentQuery<Document>(documentCollection.SelfLink)
                .Where(x => x.Created > LastYear);

            // query2 is now: 'SELECT * FROM root WHERE (root.created > 6,35430269777489E+17)'

            query2.ToList(); // throws exception {"Message: {\"errors\":[{\"severity\":\"Error\",\"location\":{\"start\":42,\"end\":43},\"code\":\"SC1001\",\"message\":\"Syntax error, incorrect syntax near ','.\"}]}}
        }

        private static DocumentCollection GetDocumentCollection(DocumentClient client, Database database)
        {
            var documentCollection = client
                .CreateDocumentCollectionQuery(database.CollectionsLink)
                .Where(c => c.Id == "Documents")
                .AsEnumerable()
                .FirstOrDefault();
            return documentCollection;
        }

        private static Database GetDatabase(DocumentClient client)
        {
            var database = client
                .CreateDatabaseQuery()
                .Where(x => x.Id == "TestDocumentDatabase")
                .AsEnumerable()
                .FirstOrDefault();
            return database;
        }

        private static void SetCulture(string culture)
        {
            var ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

    }
}
