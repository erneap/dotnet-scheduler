using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace OsanScheduler.Employees
{
  public abstract class EmployeeInfoType : IComparable<EmployeeInfoType> 
  {
    public int Id { get; set; }
    [BsonElement("sort")]
    [JsonPropertyName("sort")]
    public int SortID { get; set; }

    public int CompareTo(EmployeeInfoType? other)
    {
      if (other != null)
      {
        return this.SortID.CompareTo(other.SortID);
      }
      return 0;
    }
  }

  public class EmployeeContact : EmployeeInfoType {
    [BsonElement("typeid")]
    [JsonPropertyName("typeid")]
    public int TypeID { get; set; }
    public string Value { get; set; } = "";
  }

  public class EmployeeSpecialty : EmployeeInfoType {
    [BsonElement("specialtyid")]
    [JsonPropertyName("specialtyid")]
    public int SpecialtyID { get; set; }
    public bool Qualified { get; set; }
  }
}