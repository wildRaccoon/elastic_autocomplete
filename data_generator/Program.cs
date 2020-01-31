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
        static ElasticClient CreateCli()
        {
            var connection = new ConnectionSettings()
                .DefaultMappingFor<UserData>(i => i
                    .IndexName("idx_test")
                    .IdProperty(p => p.id));

            var cli = new ElasticClient(connection);

            return cli;
        }
        static void Generate()
        {
            var id = 0L;

            var testUsers = new Faker<UserData>()
                .CustomInstantiator(r => new UserData() { id = Interlocked.Add(ref id, 1).ToString("000000000000000") })
                .RuleFor(p => p.firstName, f => f.Name.FirstName())
                .RuleFor(p => p.lastName, f => f.Name.LastName())
                .RuleFor(p => p.model, f => f.Vehicle.Model())
                .RuleFor(p => p.manufacturer, f => f.Vehicle.Manufacturer())
                .RuleFor(p => p.email, (f, u) => f.Internet.Email(u.firstName, u.lastName))
                .RuleFor(p => p.tags, (f, u) =>
                    new List<string>()
                    {
                        f.Vehicle.Manufacturer(), f.Vehicle.Model(), f.Address.Country(), f.Address.City()
                    })
                .RuleFor(p => p.suggest, (f, u) =>
                    new List<string>()
                    {
                        f.Vehicle.Manufacturer().ToLower(), f.Vehicle.Model().ToLower(), f.Address.Country().ToLower(), f.Address.City().ToLower()
                    });

            var cli = CreateCli();

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

            for (int i = 0; i < CYCLES; i++)
            {
                var items = testUsers.Generate(REC_PER_CYCLES);
                var ops = items.Select(x => new BulkIndexOperation<UserData>(x)).ToArray();
                var res = cli.Bulk(new BulkRequest()
                {
                    Operations = new List<IBulkOperation>(ops)
                });

                Console.Clear();
                Console.WriteLine(i);
            }
        }

        static void PrintSugestion(ElasticClient cli, string text)
        {
            var name = "my-suggest";

            var result = cli.Search<UserData>(
                s => s.Suggest(
                    sel => sel.Completion(name,
                        suggest => suggest.Prefix(text).Field(f => f.suggest).SkipDuplicates()
                    )
                )
            );

            if (result.IsValid)
            {
                var suggest = result.Suggest[name];
                var search = suggest.FirstOrDefault();

                Console.WriteLine($"[{search.Text}] - {result.Took}ms");

                foreach (var item in search.Options)
                {
                    Console.WriteLine($"{item.Text}");
                }
            }
            else
            {
                Console.WriteLine($"Invalid - {result.ToString()}");
            }
        }


        static void AutoCommplete()
        {
            var cli = CreateCli();

            //PrintSugestion(cli,"au");


            var phrase = "";

            Console.Clear();

            var initial = Console.CursorTop;

            while (true)
            {
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Backspace && phrase.Length > 0)
                {
                    phrase = phrase.Substring(0, phrase.Length - 1);
                }
                else if (key.KeyChar != '\0')
                {
                    phrase += key.KeyChar;
                }


                Console.Clear();
                Console.SetCursorPosition(0, initial);
                Console.WriteLine("                                                                                   ");

                if (phrase.Length > 0)
                {
                    PrintSugestion(cli, phrase);
                }

                Console.SetCursorPosition(0, initial);
                Console.Write(phrase);
            }
        }

        static void Main(string[] args)
        {
            if (args.FirstOrDefault().ToLower() == "generate")
            {
                Console.WriteLine("Generate data for test");
                Generate();
            }

            AutoCommplete();
        }
    }
}
