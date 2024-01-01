using System.Text.Json.Serialization;

namespace OsanScheduler.Soap.Bible 
{
  public class PullBook {
    [JsonPropertyName("display")]
    public string Display { get; set; } = "";
    [JsonPropertyName("osis")]
    public string Code { get; set; } = "";
    public string Testament { get; set; } = "";
    [JsonPropertyName("num_chapters")]
    public int NumberOfChapters { get; set; }
  }
}