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

            Test(
                "Via Linq lambda",
                 new[] { "en-GB", "nl-NL" },
                () =>
                {
                    var documentQuery = client
                        .CreateDocumentQuery<Document>(documentCollection.SelfLink)
                        .Where(x => x.Created > LastYear);

                    Console.WriteLine("  {0}", documentQuery);

                    documentQuery.ToList();
                });

            Test(
                "Via SqlQuerySpec query",
                 new[] { "en-GB", "nl-NL" },
                () =>
                {
                    var sqlQuerySpec = new SqlQuerySpec("SELECT * FROM root WHERE (root.created > @lastYear)");
                    sqlQuerySpec.Parameters = new SqlParameterCollection();
                    sqlQuerySpec.Parameters.Add(new SqlParameter("@lastYear", LastYear));

                    var documentQuery = client.CreateDocumentQuery(documentCollection.SelfLink, sqlQuerySpec);

                    Console.WriteLine("  {0}", documentQuery);

                    documentQuery.ToList();
                });

            Test(
                "Via custom generated query",
                 new[] { "en-GB", "nl-NL" },
                () =>
                {
                    var query = $"SELECT * FROM root WHERE (root.created > {LastYear:E10})";

                    var documentQuery = client.CreateDocumentQuery(documentCollection.SelfLink, query);

                    Console.WriteLine("  {0}", documentQuery);

                    documentQuery.ToList();
                });

            Console.ReadLine();
        }

        private static void Test(string description, string[] cultures, Action action)
        {
            foreach (var culture in cultures)
            {
                Console.WriteLine();
                Console.WriteLine("{0} ({1})", description, culture);
                SetCulture(culture);
                try
                {
                    action();
                    Console.WriteLine("  Successful!");
                }
                catch (AggregateException ex) when (ex.InnerException is DocumentClientException)
                {
                    Console.WriteLine("  Failed: {0}", ex.InnerException.Message);
                }
            }
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
