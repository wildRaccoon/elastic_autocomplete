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
                .CustomInstantiator(r => new UserData() {id = id++})
                .RuleFor(p => p.firstName, f => f.Name.FirstName())
                .RuleFor(p => p.lastName, f => f.Name.LastName())
                .RuleFor(p => p.email, (f, u) => f.Internet.Email(u.firstName,u.lastName))
                .RuleFor(p => p.tags, (f, u) =>
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
