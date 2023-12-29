using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Internal;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using OsanScheduler.employees;
using ZstdSharp.Unsafe;

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

    public Workday? GetWorkdayWOLeave(DateTime date) {
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
      return answer;
    }

    public double GetStandardWorkday(DateTime date) {
      for (int i = 0; i < this.Assignments.Count; i++) {
        if (this.Assignments[i].UseAssignment(this.SiteID, date)) {
          return this.Assignments[i].GetStandardWorkday();
        }
      }
      return 0.0;
    }

    public void AddAssignment(string site, string wkctr, DateTime start) {
      this.Assignments.Sort();
      Assignment newAsgmt = new Assignment() {
        Id = (uint)this.Assignments.Count,
        Site = site,
        Workcenter = wkctr,
        StartDate = new DateTime(start.Ticks),
        EndDate = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc)
      };
      newAsgmt.AddSchedule(7);
      for (int i=1; i < 6; i++) {
        newAsgmt.Schedules[0].UpdateWorkday((uint)i, wkctr, "D", 8.0);
      }
      this.Assignments[this.Assignments.Count - 1].EndDate 
        = start.AddHours(-6.0);
      this.Assignments.Add(newAsgmt);
      this.Assignments.Sort();
    }

    public void RemoveAssignment(uint id) {
      this.Assignments.Sort();
      bool found = false;
      for (int i = 0; i < this.Assignments.Count && !found; i++) 
      {
        if (this.Assignments[i].Id == id) {
          if (i > 0 && i < this.Assignments.Count - 1) {
            this.Assignments[i-1].EndDate 
              = this.Assignments[i+1].StartDate.AddHours(-6.0);
          } else if (i == this.Assignments.Count - 1) {
            this.Assignments[this.Assignments.Count - 2].EndDate
              = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
          }
          found = true;
          this.Assignments.RemoveAt(i);
        }
      }
    }

    public bool PurgeOldData(DateTime date) {
      // remove variations before date
      this.Variations.Sort();
      for (int i = this.Variations.Count - 1; i >= 0; i--) {
        if (this.Variations[i].EndDate.CompareTo(date) < 0) {
          this.Variations.RemoveAt(i);
        }
      }

      // remove leaves before date
      for (int i = this.Leaves.Count - 1; i >= 0; i--) {
        if (this.Leaves[i].LeaveDate.CompareTo(date) <= 0) {
          this.Leaves.RemoveAt(i);
        }
      }

      // remove leave requests before date, based on end date
      this.Requests.Sort();
      for (int i = this.Requests.Count - 1; i >= 0; i--) {
        if (this.Requests[i].EndDate.CompareTo(date) < 0) {
          this.Requests.RemoveAt(i);
        }
      }

      // remove leave/annual balances before the year of the date
      this.Balances.Sort();
      for (int i = this.Balances.Count - 1; i >= 0; i--) {
        if (this.Balances[i].Year < date.Year ) {
          this.Balances.RemoveAt(i);
        }
      }

      // check to see if employee quit before date based on last 
      // assignment enddate.
      this.Assignments.Sort();
      Assignment asgmt = this.Assignments[this.Assignments.Count - 1];
      return (asgmt.EndDate.CompareTo(date) < 0);
    }

    public void CreateLeaveBalance(int year) {
      bool found = false;
      double lastAnnual = 0.0;
      double lastCarry = 0.0;
      for (int i = 0; i < this.Balances.Count && !found; i++) {
        if (this.Balances[i].Year == year) {
          found = true;
        } else if (this.Balances[i].Year == year - 1) {
          lastAnnual = this.Balances[i].Annual;
          lastCarry = this.Balances[i].Carryover;
        }
      }
      if (!found) {
        AnnualLeave balance = new AnnualLeave() {
          Year = year,
          Annual = lastAnnual,
          Carryover = 0.0
        };
        if (lastAnnual == 0.0) {
          balance.Annual = 120.0;
        } else {
          double carry = lastAnnual + lastCarry;
          for (int i = 0; i < this.Leaves.Count; i++) {
            if (this.Leaves[i].LeaveDate.Year == year - 1 
              && this.Leaves[i].Code.ToLower().Equals("v")
              && this.Leaves[i].Status.ToLower().Equals("actual")) {
              carry -= this.Leaves[i].Hours;
            }
            balance.Carryover = carry;
          }
        }
        this.Balances.Add(balance);
        this.Balances.Sort();
      }
    }

    public void UpdateAnnualLeave(int year, double annual, double carry) {
      bool found = false;
      for (int i=0; i < this.Balances.Count && !found; i++) {
        if (this.Balances[i].Year == year) {
          found = true;
          this.Balances[i].Annual = annual;
          this.Balances[i].Carryover = carry;
        }
      }
      if (!found) {
        AnnualLeave balance = new AnnualLeave() {
          Year = year,
          Annual = annual,
          Carryover = carry
        };
        this.Balances.Add(balance);
        this.Balances.Sort();
      }
    }

    public void AddLeave(int id, DateTime date, string code, 
      string status, double hours, ObjectId? requestID) {
      bool found = false;
      int max = 0;
      for (int i=0; i < this.Leaves.Count && !found; i++) {
        if ((this.Leaves[i].LeaveDate.Equals(date)
          && this.Leaves[i].Code.ToLower() == code.ToLower())
          || this.Leaves[i].Id == id) {
          found = true;
          this.Leaves[i].Status = status;
          this.Leaves[i].Hours = hours;
          if (requestID != null) {
#pragma warning disable CS8601 // Possible null reference assignment.
            this.Leaves[i].RequestID = requestID.ToString();
#pragma warning restore CS8601 // Possible null reference assignment.
          }
        } else if (this.Leaves[i].Id > max) {
          max = this.Leaves[i].Id;
        }
      }
      if (!found) {
        LeaveDay day = new LeaveDay() {
          Id = max + 1,
          LeaveDate = new DateTime(date.Ticks),
          Code = code,
          Hours = hours,
          Status = status,
        };
        if (requestID != null) {
#pragma warning disable CS8601 // Possible null reference assignment.
          day.RequestID = requestID.ToString();
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        this.Leaves.Add(day);
        this.Leaves.Sort();
      }
    }

    public LeaveDay? UpdateLeave(int id, string field, string value) {
      bool found = false;
      for (int i=0; i < this.Leaves.Count && !found; i++) {
        if (this.Leaves[i].Id == id) {
          LeaveDay day = new LeaveDay() {
            Id = this.Leaves[i].Id,
            LeaveDate = new DateTime(this.Leaves[i].LeaveDate.Ticks),
            Hours = this.Leaves[i].Hours,
            Status = this.Leaves[i].Status,
            RequestID = this.Leaves[i].RequestID
          };
          switch (field.ToLower()) {
            case "date":
              this.Leaves[i].LeaveDate 
                = DateTime.ParseExact(value, "MM/dd/yyyy", null);
              break;
            case "code":
              this.Leaves[i].Code = value;
              break;
            case "hours":
              this.Leaves[i].Hours = double.Parse(value, null);
              break;
            case "status":
              this.Leaves[i].Status = value;
              break;
            case "requestid":
              this.Leaves[i].RequestID = value;
              break;
          }
          return day;
        }
      }
      return null;
    }

    public LeaveDay? DeleteLeave(int id) {
      for (int i=0; i < this.Leaves.Count; i++) {
        if (this.Leaves[i].Id == id) {
          LeaveDay day = this.Leaves[i];
          this.Leaves.RemoveAt(i);
          return day;
        }
      }
      return null;
    }

    public double GetLeaveHours(DateTime start, DateTime end) {
      double answer = 0.0;
      this.Leaves.Sort();
      for (int i=0; i < this.Leaves.Count; i++) {
        if (this.Leaves[i].LeaveDate.CompareTo(start) >= 0 
          && this.Leaves[i].LeaveDate.CompareTo(end) < 0 
          && this.Leaves[i].Status.ToLower() == "actual") {
          answer += this.Leaves[i].Hours;
        }
      }
      return answer;
    }

    public double GetPTOHours(DateTime start, DateTime end) {
      double answer = 0.0;
      this.Leaves.Sort();
      for (int i=0; i < this.Leaves.Count; i++) {
        if (this.Leaves[i].LeaveDate.CompareTo(start) >= 0 
          && this.Leaves[i].LeaveDate.CompareTo(end) < 0 
          && this.Leaves[i].Status.ToLower() == "actual"
          && this.Leaves[i].Code.ToLower() == "v") {
          answer += this.Leaves[i].Hours;
        }
      }
      return answer;
    }

    public LeaveRequest? NewLeaveRequest(string code, DateTime start, DateTime end, 
    string comment) {
      // check to see if a leave request already exists for time period
      // entered.
      bool found = false;
      for (int i = 0; i < this.Requests.Count && !found; i++) {
        if (this.Requests[i].StartDate.CompareTo(start) == 0 
          && this.Requests[i].EndDate.CompareTo(end) == 0) {
          return this.Requests[i];
        }
      }
      // create new leave request if period is not found
      if (!found) {
        LeaveRequest lr = new LeaveRequest() {
          Id = ObjectId.GenerateNewId().ToString(),
          EmployeeID = this.Id.ToString(),
          RequestDate = DateTime.UtcNow,
          PrimaryCode = code,
          StartDate = start,
          EndDate = end,
          Status = "DRAFT"
        };
        LeaveRequestComment lrc = new LeaveRequestComment() {
          CommentDate = DateTime.UtcNow,
          Comment = "Leave Request created."
        };
        lr.Comments.Add(lrc);
        if (comment != "") {
          lrc = new LeaveRequestComment() {
            CommentDate = DateTime.UtcNow,
            Comment = comment
          };
          lr.Comments.Add(lrc);
        }
        lr.Comments.Sort();

        // create the the request days for the period but only for
        // days where the employee is already scheduled for work.
        DateTime rStart = new DateTime(start.Ticks);
        while (rStart.CompareTo(end) <= 0) {
          Workday? wd = this.GetWorkdayWOLeave(rStart);
          if (wd != null && wd.Code != "") {
            var hours = wd.Hours;
            if (hours == 0.0) {
              hours = this.GetStandardWorkday(rStart);
            }
            if (code.ToLower() == "h") {
              hours = 8.0;
            }
            LeaveDay day = new LeaveDay() {
              LeaveDate = new DateTime(rStart.Ticks),
              Code = code,
              Hours = hours,
              Status = "DRAFT",
              RequestID = lr.Id
            };
            lr.RequestedDays.Add(day);
          }
          rStart = rStart.AddDays(1.0);
        }
        lr.RequestedDays.Sort();
        this.Requests.Add(lr);
        this.Requests.Sort();
        return lr;
      }
      return null;
    }

    public LeaveRequestUpdate UpdateLeaveRequest(string id, string field, 
    string value) {
      var message = "";
      LeaveRequest? request = null;
      bool found = false;
      for (int i = 0; i < this.Requests.Count && !found; i++) {
        if (this.Requests[i].Id.Equals(id)) {
          request = this.Requests[i];
          switch (field.ToLower()) {
            case "startdate":
            case "start":
              DateTime date = DateTime.ParseExact(value, "yyyy-MM-dd", null);
              if (date.CompareTo(request.StartDate) < 0 
              || date.CompareTo(request.EndDate) > 0) {
                if (request.Status.ToLower() == "approved") {
                  request.Status = "REQUESTED";
                  request.ApprovalDate = new DateTime(0);
                  request.ApprovedBy = "";
                  message = "Leave Request from " + this.Name.GetLastFirst()
                    + ": Starting date changed needs reapproval";
                }
                // since date changes invalidate the leave request approval,
                // remove any approved leaves for the associated request id
                this.Leaves.Sort();
                for (int j = this.Leaves.Count - 1; j >= 0; j--) {
                  if (this.Leaves[j].RequestID.Equals(request.Id) 
                    && !this.Leaves[j].Status.ToLower().Equals("actual")) {
                    this.Leaves.RemoveAt(j);
                  }
                }
              }
              break;
          }
        }
      }
      return new LeaveRequestUpdate();
    }
  }
}