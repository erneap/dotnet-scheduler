using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OsanScheduler.Employees
{
  public class Work : IComparable<Work> {
    [BsonElement("dateWorked")]
    [JsonPropertyName("dateWorked")]
    public DateTime DateWorked { get; set; } = DateTime.UtcNow;
    [BsonElement("chargeNumber")]
    [JsonPropertyName("chargeNumber")]
    public string ChargeNumber { get; set; } = "";
    public string Extension { get; set; } = "";
    [BsonElement("payCode")]
    [JsonPropertyName("payCode")]
    public int PayCode { get; set; }
    public double Hours { get; set; }

    public int CompareTo(Work? other)
    {
      if (other != null) {
        if (this.DateWorked.CompareTo(other.DateWorked) == 0) {
          if (this.ChargeNumber.CompareTo(other.ChargeNumber) == 0) {
            return this.Extension.CompareTo(other.Extension);
          }
          return this.ChargeNumber.CompareTo(other.ChargeNumber);
        }
        return this.DateWorked.CompareTo(other.DateWorked);
      }
      return 0;
    }
  }

  public class EmployeeWorkRecord : IComparable<EmployeeWorkRecord> {
    [BsonElement("_id")]
    [JsonPropertyName("id")]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();
    [BsonElement("employeeID")]
    [JsonPropertyName("employeeID")]
    public ObjectId EmployeeID { get; set; } = ObjectId.GenerateNewId();
    public uint Year { get; set; }
    public List<Work> Work { get; set; } = new List<Work>();

    public int CompareTo(EmployeeWorkRecord? other)
    {
      if (other != null) {
        if (this.EmployeeID.CompareTo(other.EmployeeID) == 0) {
          return this.Year.CompareTo(other.Year);
        }
        return this.EmployeeID.CompareTo(other.EmployeeID);
      }
      return 0;
    }

    public void RemoveWork(DateTime start, DateTime end) {
      this.Work.Sort();
      for (int i = this.Work.Count - 1; i >= 0; i--) {
        if (this.Work[i].DateWorked.CompareTo(start) >= 0 
          && this.Work[i].DateWorked.CompareTo(end) <= 0) {
          this.Work.RemoveAt(i);
        }
      }
    }

    public void Purge(DateTime purgeDate) {
      this.Work.Sort();
      for (int i = this.Work.Count - 1; i >= 0; i--) {
        if (this.Work[i].DateWorked.CompareTo(purgeDate) <= 0) {
          this.Work.RemoveAt(i);
        }
      }
    }
  }
}