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

  public class LeaveRequest: IComparable<LeaveRequest> {
    public string Id { get; set; } = "";
    [BsonElement("employeeid")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("employeeid")]
    public string EmployeeID { get; set; } = "";
    [BsonElement("requestDate")]
    [JsonPropertyName("requestDate")]
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    [BsonElement("primarycode")]
    [JsonPropertyName("primarycode")]
    public string PrimaryCode { get; set; } = "";
    [BsonElement("startdate")]
    [JsonPropertyName("startdate")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    [BsonElement("enddate")]
    [JsonPropertyName("enddate")]
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "DRAFT";
    [BsonElement("approvedby")]
    [JsonPropertyName("approvedby")]
    public string ApprovedBy { get; set; } = "";
    [BsonElement("approvalDate")]
    [JsonPropertyName("approvalDate")]
    public DateTime ApprovalDate { get; set; } = new DateTime(0);
    [BsonElement("requesteddays")]
    [JsonPropertyName("requesteddays")]
    public List<LeaveDay> RequestedDays { get; set; } = new List<LeaveDay>();
    [BsonIgnoreIfNull]
    public List<LeaveRequestComment> Comments { get; set; } 
      = new List<LeaveRequestComment>();

    public int CompareTo(LeaveRequest? other)
    {
      if (other != null) {
        if (this.StartDate.CompareTo(other.StartDate) == 0) {
          if (this.EndDate.CompareTo(other.EndDate) == 0) {
            return this.Id.CompareTo(other.Id);
          }
          return this.EndDate.CompareTo(other.EndDate);
        }
        return this.StartDate.CompareTo(other.StartDate);
      }
      return 0;
    }

    public void SetLeaveDay(DateTime date, string code, double hours) {
      foreach(LeaveDay day in this.RequestedDays) {
        if (day.LeaveDate.Equals(date)) {
          day.Code = code;
          day.Hours = hours;
        }
      }
    }

    public void SetLeaveDays(Employee emp) {
      DateTime rStart = new DateTime(this.StartDate.Ticks);
      this.RequestedDays.Clear();
      while (rStart.CompareTo(this.EndDate) <= 0) {
        var wd = emp.GetWorkdayWOLeave(rStart);
        if (wd.Code != "") {
          var hours = wd.Hours;
          if (hours == 0.0) {
            hours = emp.GetStandardWorkday(rStart);
          }
          if (this.PrimaryCode.ToLower().Equals("h")) {
            hours = 8.0;
          }
          this.RequestedDays.Add(new LeaveDay() {
            LeaveDate = new DateTime(rStart.Ticks),
            Code = this.PrimaryCode,
            Hours = hours,
            Status = "REQUESTED",
            RequestID = this.Id
          });
        }
      }
      this.RequestedDays.Sort();
    }
  }

  public class LeaveRequestUpdate {
    public string Message { get; set; } = "";
    public LeaveRequest? Request { get; set; } = null;
  }
}