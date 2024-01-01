using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using ThirdParty.Json.LitJson;

namespace OsanScheduler.Soap.Plans 
{
  public class Passage : IComparable<Passage> {
    public int Id { get; set; }
    [BsonElement("bookid")]
    [JsonPropertyName("bookid")]
    public int BookId { get; set; }
    public string Book { get; set; } = "";
    public int Chapter { get; set; }
    [BsonElement("startverse")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("startverse")]
    public int StartVerse { get; set; }
    [BsonElement("endverse")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("endverse")]
    public int EndVerse { get; set; }
    [BsonIgnoreIfNull]
    public string Text { get; set; } = "";
    public bool Completed { get; set; }

    public int CompareTo(Passage? other)
    {
      if (other != null) {
        if (this.BookId.CompareTo(other.BookId) == 0) {
          if (this.Chapter.CompareTo(other.Chapter) == 0) {
            if (this.StartVerse.CompareTo(other.StartVerse) == 0) {
              return this.EndVerse.CompareTo(other.EndVerse);
            }
            return this.StartVerse.CompareTo(other.StartVerse);
          }
          return this.Chapter.CompareTo(other.Chapter);
        }
        return this.BookId.CompareTo(other.BookId);
      }
      return 0;
    }
  }
}