using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OsanScheduler.employees;

namespace OsanScheduler.Employees 
{
  public class EmployeeName : IComparable<EmployeeName> {
    [BsonElement("first")]
    [JsonPropertyName("first")]
    public string FirstName { get; set; } = "";
    [BsonElement("middle")]
    [JsonPropertyName("middle")]
    public string MiddleName { get; set; } = "";
    [BsonElement("last")]
    [JsonPropertyName("last")]
    public string LastName { get; set; } = "";
    [BsonElement("suffix")]
    [JsonPropertyName("suffix")]
    public string Suffix { get; set; } = "";

    public int CompareTo(EmployeeName? other)
    {
      if (other != null) {
        if (this.LastName.CompareTo(other.LastName) == 0) {
          if (this.FirstName.CompareTo(other.FirstName) == 0) {
            return this.MiddleName.CompareTo(other.MiddleName);
          }
          return this.FirstName.CompareTo(other.FirstName);
        }
        return this.LastName.CompareTo(other.LastName);
      }
      return 0;
    }

    public string GetLastFirst() {
      return this.LastName + ", " + this.FirstName;
    }

    public string GetLastFirstMI() {
      if (this.MiddleName != "") {
        return this.LastName + ", " + this.FirstName + " " 
          + this.MiddleName.Substring(0,1);
      }
      return this.LastName + ", " + this.FirstName;
    }
  }

  public class Employee : IComparable<Employee> {
    [BsonElement("_id")]
    [JsonPropertyName("id")]
    public ObjectId Id { get; set; } 
    [BsonElement("team")]
    [JsonPropertyName("team")]
    public ObjectId TeamID { get; set; }
    [BsonElement("site")]
    [JsonPropertyName("site")]
    public string SiteID { get; set; } = "";
    [BsonElement("userid")]
    [JsonPropertyName("userid")]
    public ObjectId UserID { get; set; } 
    public string Email { get; set; } = "";
    public EmployeeName Name { get; set; } = new EmployeeName();
    [BsonElement("companyinfo")]
    [JsonPropertyName("companyinfo")]
    public CompanyInfo CompanyInfo { get; set; } = new CompanyInfo();
    [BsonElement("assignments")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("assignments")]
    public List<Assignment> Assignments { get; set; } = new List<Assignment>();
    public List<Variation> Variations { get; set; } = new List<Variation>();
    public List<AnnualLeave> Balances { get; set; } = new List<AnnualLeave>();
    public List<LeaveDay> Leaves { get; set; } = new List<LeaveDay>();
    public List<LeaveRequest> Requests { get; set; } = new List<LeaveRequest>();
    [BsonElement("laborCodes")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("laborCodes")]
    public List<LaborCode> LaborCodes { get; set; } 
      = new List<LaborCode>();
    [BsonIgnore]
    public Users.User User { get; set; } = new Users.User();
    [BsonIgnore]
    public List<Work> Work { get; set; } = new List<Work>();
    [BsonElement("contactinfo")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("contactinfo")]
    public List<EmployeeContact> ContactInfo { get; set; } 
      = new List<EmployeeContact>();
    [BsonIgnoreIfNull]
    public List<EmployeeSpecialty> Specialties { get; set; }
      = new List<EmployeeSpecialty>();
    [BsonElement("emails")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("emails")]
    public List<string> EmailAddresses { get; set; } = new List<string>();

    public int CompareTo(Employee? other)
    {
      if (other != null) {
        return this.Name.CompareTo(other.Name);
      }
      return 0;
    }

    public void RemoveLeaves(DateTime start, DateTime end) {
      this.Leaves.Sort();
      for (int i = this.Leaves.Count - 1; i >= 0; i--) {
        if (this.Leaves[i].LeaveDate.CompareTo(start) >= 0 
          && this.Leaves[i].LeaveDate.CompareTo(end) <= 0) {
          this.Leaves.RemoveAt(i);
        }
      }
    }

    public bool IsActive(DateTime date) {
      for (int i = 0; i < this.Assignments.Count; i++) {
        if (this.Assignments[i].UseAssignment(this.SiteID, date)) {
          return true;
        }
      }
      return false;
    }

    public bool IsAssigned(string site, string wkctr, DateTime start, 
      DateTime end) {
      for (int i = 0; i < this.Assignments.Count; i++) {
        if (this.Assignments[i].Site.ToLower().Equals(site.ToLower())
          && this.Assignments[i].Workcenter.ToLower().Equals(wkctr.ToLower())
          && this.Assignments[i].StartDate.CompareTo(end) <= 0 
          && this.Assignments[i].EndDate.CompareTo(start) >= 0) {
          return true;
        }
      }
      return false;
    }

    public bool AtSite(string site, DateTime start, DateTime end) {
      for (int i = 0; i < this.Assignments.Count; i++) {
        if (this.Assignments[i].Site.ToLower().Equals(site.ToLower())
          && this.Assignments[i].StartDate.CompareTo(end) <= 0 
          && this.Assignments[i].EndDate.CompareTo(start) >= 0) {
          return true;
        }
      }
      return false;
    }

    public Workday? GetWorkday(DateTime date) {
      Workday? answer = null;
      double work = 0.0;
      double stdWorkday = 8.0;
      DateTime lastWork = new DateTime(0);
      string siteID = "";
      string stdCode = "";
      string stdWkctr = "";
      // get current standard work day hours for date/assignment
      foreach (Assignment asgmt in this.Assignments) {
        if (asgmt.UseAssignment(this.SiteID, date)) {
          stdWorkday = asgmt.GetStandardWorkday();
          stdCode = asgmt.GetStandardCode(date);
          stdWkctr = asgmt.Workcenter;
        }
      }
      // determine if the recorded work was completed on the date and also
      // determine the last recorded work hours from timesheet data.
      foreach (Work wk in this.Work) {
        if (wk.DateWorked.Year == date.Year && wk.DateWorked.Month == date.Month 
          && wk.DateWorked.Day == date.Day) {
          work += wk.Hours;
        }
        if (wk.DateWorked.CompareTo(lastWork) > 0) {
          lastWork = new DateTime(wk.DateWorked.Ticks);
        }
      }
      // check for assignment to assign to workday
      foreach (Assignment asgmt in this.Assignments) {
        if (asgmt.StartDate.CompareTo(date) <= 0 
          && asgmt.EndDate.CompareTo(date) >= 0) {
          siteID = asgmt.Site;
          answer = asgmt.GetWorkday(date);
        }
      }
      // check for variation which will override the workday
      foreach (Variation vari in this.Variations) {
        if (vari.StartDate.CompareTo(date) <= 0 
          && vari.EndDate.CompareTo(date) >= 0) {
          answer = vari.GetWorkday(this.SiteID, date);
        }  
      }
      // if actual worked hours > 0 return the workday. If workday is null,
      // create new one with standard code and hours worked.
      if (work > 0.0) {
        if (answer != null) {
          return answer;
        }
        return new Workday() {
          Id = 0,
          Workcenter = stdWkctr,
          Code = stdCode,
          Hours = work
        };
      }
      // since actual work is empty for this date, check for leave on date
      LeaveDay? leave = null;
      foreach (LeaveDay lv in this.Leaves) {
        if (lv.LeaveDate.Year == date.Year && lv.LeaveDate.Month == date.Month
          && lv.LeaveDate.Day == date.Day) {
          if (leave == null) {
            leave = lv;
          } else if (lv.Hours > leave.Hours) {
            leave = lv;
          }
        }
      }
      if (leave != null) {
        if (answer == null) {
          answer = new Workday();
        }
        answer.Code = leave.Code;
        answer.Workcenter = "";
        answer.Hours = leave.Hours;
      }
      return answer;
    }
  }
}