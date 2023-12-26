using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace OsanScheduler.employees
{
  public class Workday : IComparable<Workday> {
    public uint Id { get; set; }
    public string Workcenter { get; set;}
    public string Code { get; set; }
    public double Hours { get; set; }

    public Workday() {
      this.Id = 0;
      this.Workcenter = "";
      this.Code = "";
      this.Hours = 0.0;
    }

    public Workday(uint id, string wkctr, string code, double hours)
    {
      this.Id = id;
      this.Workcenter = wkctr;
      this.Code = code;
      this.Hours = hours;
    }

    public int CompareTo(Workday? other) 
    {
      if (other != null && other.Id < this.Id) {
        return 1;
      }
      if (other != null && other.Id > this.Id) {
        return -1;
      }
      return 0;
    }
  }

  public class Schedule : IComparable<Schedule> {
    public uint Id { get; set; }
    public List<Workday> Workdays { get; set; }
    public bool Showdates { get; set; }

    public Schedule() 
    {
      this.Id = 0;
      this.Workdays = new List<Workday>();
      this.Showdates = false;
    }
    public int CompareTo(Schedule? other) 
    {
      if (other != null && other.Id < this.Id) {
        return 1;
      }
      if (other != null && other.Id > this.Id) {
        return -1;
      }
      return 0;
    }

    public Workday? GetWorkday(uint id) {
      if (this.Workdays != null) {
        foreach (Workday day in this.Workdays) 
        {
          if (day.Id == id) {
            return day;
          }
        }
      }
      return null;
    }

    public void UpdateWorkday(uint id, string wkctr, string code, double hours)
    {
      bool found = false;
      if (this.Workdays == null) {
        this.Workdays = new List<Workday>();
      }
      foreach (Workday day in this.Workdays)
      {
        if (day.Id == id) {
          found = true;
          day.Workcenter = wkctr;
          day.Code = code;
          day.Hours = hours;
        }
      }
      if (!found) {
        this.Workdays.Add(new Workday(id, wkctr, code, hours));
        this.Workdays.Sort();
      }
    }

    public void SetScheduleDays(int days)
    {
      if (days <= 0 || days % 7 != 0) {
        throw new ArgumentException("New Days must be greater than zero and a "
          + "multiple of 7");
      }
      this.Workdays.Sort();
      if (days > this.Workdays.Count)
      {
        for (int i = this.Workdays.Count; i < days; i++) 
        {
          this.Workdays.Add(new Workday() { Id = (uint)i });
        }
      } else if (days < this.Workdays.Count) {
        while (this.Workdays.Count > days) {
          this.Workdays.RemoveAt(days);
        }
      }
    }
  }

  public class EmployeeLaborCode : IComparable<EmployeeLaborCode>
  {
    [BsonElement("chargeNumber")]
    [JsonPropertyName("chargeNumber")]
    public string ChargeNumber { get; set; }
    public string Extension { get; set; }

    public EmployeeLaborCode() {
      this.ChargeNumber = "";
      this.Extension = "";
    }

    public int CompareTo(EmployeeLaborCode? other) 
    {
      if (other != null && other.ChargeNumber != null 
        && other.ChargeNumber.CompareTo(this.ChargeNumber) == 0) {
        return (other != null && other.Extension != null) 
          ? other.Extension.CompareTo(this.Extension) : 0;
      }
      return (other != null && other.ChargeNumber != null) 
        ? other.ChargeNumber.CompareTo(this.ChargeNumber) : 0;
    }
  }
  
  public class Assignment : IComparable<Assignment> {
    public uint Id { get; set; }
    public string Site { get; set; }
    public string Workcenter { get; set; }
    [BsonElement("startDate")]
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }
    [BsonElement("endDate")]
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }
    public List<Schedule> Schedules { get; set; }
    public DateTime Rotationdate { get; set; }
    public int Rotationdays { get; set; }
    [BsonElement("laborcodes")]
    [JsonPropertyName("laborcodes")]
    public List<EmployeeLaborCode> LaborCodes { get; set; }

    public Assignment() {
      this.Id = (uint) 0;
      this.Site = "";
      this.Workcenter = "";
      this.StartDate = new DateTime(0);
      this.EndDate = new DateTime(0);
      this.Schedules = new List<Schedule>();
      this.Rotationdate = new DateTime(0);
      this.Rotationdays = 0;
      this.LaborCodes = new List<EmployeeLaborCode>();
    }

    public int CompareTo(Assignment? other)
    {
      if (other != null) {
        if (other.StartDate.CompareTo(this.StartDate) == 0) {
          return other.EndDate.CompareTo(this.EndDate);
        }
        return other.StartDate.CompareTo(this.EndDate);
      }
      return 0;
    }

    public bool UseAssignment(string site, DateTime date) 
    {
      return (this.Site.ToLower().Equals(site.ToLower()) 
        && this.StartDate.CompareTo(date) > -1 
        && this.EndDate.CompareTo(date) < 1);
    }

    public double GetStandardWorkday() 
    {
      double weekhours = 40.0;
      int count = 0;
      Schedule sch = this.Schedules[0];
      for (int i = 0; i < sch.Workdays.Count; i++) 
      {
        if (sch.Workdays[i].Code != "") {
          count++;
        }
      }
      return weekhours / count;
    }

    public Workday? GetWorkday(DateTime date) {
      DateTime start = new DateTime(this.StartDate.Ticks);
      while (start.DayOfWeek != DayOfWeek.Sunday) {
        start = start.AddDays(-1.0);
      }
      int days = (int)(date.Subtract(start).Hours / 24.0);
      if (this.Schedules.Count == 1 || this.Rotationdays <= 0) 
      {
        int iDay = days % this.Schedules[0].Workdays.Count;
        return this.Schedules[0].GetWorkday((uint)iDay);
      } else if (this.Schedules.Count > 1)
      {
        int schID = (days / this.Rotationdays) % this.Schedules.Count;
        int iDay = days % this.Schedules[schID].Workdays.Count;
        return this.Schedules[schID].GetWorkday((uint)iDay);
      }
      return null;
    }

    public void AddSchedule(int days)
    {
      Schedule sch = new Schedule() { Id = (uint)this.Schedules.Count };
      for (int i = 0; i < days; i++) 
      {
        Workday wd = new Workday() { Id = (uint) i, Hours = 0.0 };
        sch.Workdays.Add(wd);
      }
      sch.Workdays.Sort();
      this.Schedules.Add(sch);
    }

    public void ChangeScheduleDays(uint schID, int days) 
    {
      bool found = false;
      for (int i=0; i < this.Schedules.Count && !found; i++) {
        if (this.Schedules[i].Id == schID) {
          found = true;
          this.Schedules[i].SetScheduleDays(days);
        }
      }
    }

    public void UpdateWorkday(uint schID, uint wdID, string wkctr, string code, 
      double hours) {
      bool found = false;
      for (int i=0; i < this.Schedules.Count && !found; i++) {
        if (this.Schedules[i].Id == schID) 
        {
          found = true;
          this.Schedules[i].UpdateWorkday(wdID, wkctr, code, hours);
        }
      }
      if (!found) {
        throw new Exception("Schedule not found");
      }
    }

    public void RemoveSchedule(uint schID)
    {
      if (this.Schedules.Count > 1) {
        bool found = false;
        this.Schedules.Sort();
        for (int i=0; i < this.Schedules.Count && !found; i++) {
          if (this.Schedules[i].Id == schID) {
            found = true;
            this.Schedules.RemoveAt(i);
          }
        }
        this.Schedules.Sort();
        for (int i=0; i < this.Schedules.Count; i++) {
          this.Schedules[i].Id = (uint)i;
        }
        if (this.Schedules.Count == 1) {
          this.Rotationdate = new DateTime(0);
          this.Rotationdays = 0;
        }
      } else {
        for (int i=0; i < this.Schedules[0].Workdays.Count; i++)
        {
          this.Schedules[0].Workdays[i].Workcenter = "";
          this.Schedules[0].Workdays[i].Code = "";
          this.Schedules[0].Workdays[i].Hours = 0.0;
        }
      }
    }

    public void AddLaborCode(string chgNo, string ext) {
      bool found = false;
      for (int i=0; i < this.LaborCodes.Count && !found; i++)
      {
        EmployeeLaborCode lc = this.LaborCodes[i];
        if (lc.ChargeNumber.ToLower().Equals(chgNo.ToLower()) 
          && lc.Extension.ToLower().Equals(ext.ToLower())) {
          found = true;
        }
      }
      if (!found) {
        this.LaborCodes.Add(new EmployeeLaborCode() {
          ChargeNumber = chgNo,
          Extension = ext
        });
      }
      this.LaborCodes.Sort();
    }

    public void RemoveLaborCode(string chgNo, string ext) {
      bool found = false;
      for (int i=0; i < this.LaborCodes.Count && !found; i++)
      {
        EmployeeLaborCode lc = this.LaborCodes[i];
        if (lc.ChargeNumber.ToLower().Equals(chgNo.ToLower()) 
          && lc.Extension.ToLower().Equals(ext.ToLower())) {
          found = true;
          this.LaborCodes.RemoveAt(i);
        }
      }
    }
  }

  public class Variation : IComparable<Variation> {
    public uint Id { get; set; }
    public string Site { get; set; }
    [BsonElement("mids")]
    [JsonPropertyName("mids")]
    public bool IsMids { get; set; }
    [BsonElement("startdate")]
    [JsonPropertyName("startdate")]
    public DateTime StartDate { get; set; }
    [BsonElement("enddate")]
    [JsonPropertyName("enddate")]
    public DateTime EndDate { get; set; }
    public Schedule Schedule { get; set; }

    public Variation() {
      this.Id = (uint)0;
      this.Site = "";
      this.IsMids = false;
      this.StartDate = new DateTime(0);
      this.EndDate = new DateTime(0);
      this.Schedule = new Schedule();
    }

    public int CompareTo(Variation? other)
    {
      if (other != null) {
        if (other.StartDate.CompareTo(this.StartDate) == 0) {
          return other.EndDate.CompareTo(this.EndDate);
        }
        return other.StartDate.CompareTo(this.EndDate);
      }
      return 0;
    }

    public bool UseVariation(string site, DateTime date) 
    {
      return (this.Site.ToLower().Equals(site.ToLower()) 
        && this.StartDate.CompareTo(date) > -1 
        && this.EndDate.CompareTo(date) < 1);
    }

    public void SetScheduleDays() {
      DateTime start = new DateTime(this.StartDate.Ticks);
      DateTime end = new DateTime(this.EndDate.Ticks);
      while (start.DayOfWeek != DayOfWeek.Sunday) {
        start = start.AddDays(-1.0);
      }
      while (end.DayOfWeek != DayOfWeek.Saturday) {
        end = end.AddDays(1.0);
      }
      int days = end.Subtract(start).Days + 1;
      this.Schedule.SetScheduleDays(days);
    }

    public Workday? GetWorkday(string site, DateTime date) {
      DateTime start = new DateTime(this.StartDate.Ticks);
      while (start.DayOfWeek != DayOfWeek.Sunday) {
        start = start.AddDays(-1.0);
      }
      int days = (int)(date.Subtract(start).Hours / 24.0);
      int iDay = days % this.Schedule.Workdays.Count;
      return this.Schedule.GetWorkday((uint)iDay);
    }

    public void UpdateWorkday(uint wdId, string wkctr, string code, double hours)
    {
      this.Schedule.UpdateWorkday(wdId, wkctr, code, hours);
    }
  }
}

