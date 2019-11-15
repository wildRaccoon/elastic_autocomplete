using System;
using System.Collections.Generic;
using System.Text.Json;
using Bogus;

namespace data_generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var id = 1;
            var testUsers = new Faker<UserData>()
                .CustomInstantiator(r => new UserData() {Id = id++})
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.Email, (f, u) => f.Internet.Email(u.FirstName,u.LastName))
                .RuleFor(p => p.Tags, (f, u) =>
                new List<string>()
                    {
                        f.Address.Country(), f.Address.City(), f.Vehicle.Manufacturer()
                    });
            
            Console.WriteLine(JsonSerializer.Serialize(testUsers.Generate()));
            Console.WriteLine(JsonSerializer.Serialize(testUsers.Generate()));
            Console.WriteLine(JsonSerializer.Serialize(testUsers.Generate()));
            Console.WriteLine(JsonSerializer.Serialize(testUsers.Generate()));
            Console.WriteLine(JsonSerializer.Serialize(testUsers.Generate()));
            Console.WriteLine(JsonSerializer.Serialize(testUsers.Generate()));
        }
    }
}
