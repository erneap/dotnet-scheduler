using MongoDB.Driver;
using MongoDB.Bson;

namespace testapp;

class Program
{
    static void Main(string[] args)
    {
        var connStr = "mongodb://ernies:zse45RDXzse45RDX@rayosanweb:27017";

        var client = new MongoClient(connStr);

        var collection = client.GetDatabase("authenticate")
            .GetCollection<BsonDocument>("users");

        var filter = Builders<BsonDocument>.Filter.Eq("emailAddress", "ernea5956@gmail.com");

        var document = collection.Find(filter).First();

        Console.WriteLine(document);
    }
}
