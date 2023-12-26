using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace OsanScheduler.Employees
{
  public class AnnualLeave : IComparable<AnnualLeave> 
  {
    public int Year { get; set; }
    public double Annual { get; set; }
    public double Carryover { get; set; }

    public int CompareTo(AnnualLeave? other)
    {
      if (other != null) {
        return this.Year.CompareTo(other.Year);
      }
      return 0;
    }
  }

  public class LeaveDay : IComparable<LeaveDay> 
  {
    public int Id { get; set; }
    [BsonElement("leavedate")]
    [JsonPropertyName("leavedate")]
    public DateTime LeaveDate { get; set; } = new DateTime(0);
    public string Code { get; set; } = "";
    public double Hours { get; set; } = 0.0;
    public string Status { get; set; } = "DRAFT";
    [BsonElement("requestid")]
    [JsonPropertyName("requestid")]
    public string RequestID { get; set; } = "";

    public int CompareTo(LeaveDay? other)
    {
      if (other != null) {
        return this.LeaveDate.CompareTo(other.LeaveDate);
      }
      return 0;
    }
  }

  public class LeaveRequestComment : IComparable<LeaveRequestComment>
  {
    [BsonElement("commentdate")]
    [JsonPropertyName("commentdate")]
    public DateTime CommentDate { get; set; } = DateTime.UtcNow;
    public string Comment { get; set; } = "";

    public int CompareTo(LeaveRequestComment? other)
    {
      if (other != null) {
        return this.CommentDate.CompareTo(other.CommentDate);
      }
      return 0;
    }
  }
}