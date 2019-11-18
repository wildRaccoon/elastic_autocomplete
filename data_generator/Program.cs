using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Bogus;
using Elasticsearch.Net;
using Nest;

namespace data_generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var id = 0l;
            
            var testUsers = new Faker<UserData>()
                .CustomInstantiator(r => new UserData() {id = Interlocked.Add(ref id, 1).ToString("000000000000000")})
                .RuleFor(p => p.firstName, f => f.Name.FirstName())
                .RuleFor(p => p.lastName, f => f.Name.LastName())
                .RuleFor(p => p.email, (f, u) => f.Internet.Email(u.firstName,u.lastName))
                .RuleFor(p => p.tags, (f, u) =>
                new List<string>()
                    {
                        f.Address.Country(), f.Address.City(), f.Vehicle.Manufacturer()
                    });

            var connection = new ConnectionSettings()
                .DefaultMappingFor<UserData>(i => i
                    .IndexName("idx_test")
                    .IdProperty(p => p.id));

            var cli = new ElasticClient(connection);

            var catResp = cli.Cat.Indices(r => r.Index("idx_test"));
            if (catResp.IsValid && catResp.Records.Count > 0)
            {
                foreach (var idx in catResp.Records)
                {
                    Console.WriteLine($"[OK] Found index - {idx.Index}");
                }
            }
            else
            {
                Console.WriteLine($"[Fail] Not Found index.");
                return;
            }

            const int CYCLES = 1000;
            const int REC_PER_CYCLES = 5000;

            for (int i = 0; i < 1000; i++)
            {
                var items = testUsers.Generate(REC_PER_CYCLES);
                var ops = items.Select(x => new BulkIndexOperation<UserData>(x)).ToArray();
                var res = cli.Bulk(new BulkRequest()
                {
                    Operations = new List<IBulkOperation>(ops)
                });
                
                Console.WriteLine(i);
            }
        }
    }
}
