using System.Collections.Generic;

namespace OsanScheduler.employees
{
  public class Workday : IComparable {
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

    public int CompareTo(object obj) 
    {
      Workday other = obj as Workday;
      if (other.Id < this.Id) {
        return 1;
      }
      if (other.Id > this.Id) {
        return -1;
      }
      return 0;
    }
  }

  public class Schedule : IComparable {
    public uint Id { get; set; }
    public List<Workday> Workdays { get; set; }
    public bool Showdates { get; set; }

     public int CompareTo(object obj) 
    {
      Workday other = obj as Workday;
      if (other.Id < this.Id) {
        return 1;
      }
      if (other.Id > this.Id) {
        return -1;
      }
      return 0;
    }

    public Workday GetWorkday(uint id) {
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
          this.Workdays.Add(new Workday() { Id = uint(i)});
        }
      } else if (days < this.Workdays.Count) {
        while (this.Workdays.Count > days) {
          this.Workdays.RemoveAt(days);
        }
      }
    }
  }

  public class EmployeeLaborCode : IComparable
  {
    [BsonElement("chargeNumber")]
    public string ChargeNumber { get; set; }
    public string Extension { get; set; }

    public int CompareTo(object obj) 
    {
      EmployeeLaborCode other = obj as EmployeeLaborCode;
      if (other.ChargeNumber.CompareTo(this.ChargeNumber) == 0) {
        return other.Extension.CompareTo(this.Extension);
      }
      return other.ChargeNumber.CompareTo(this.ChargeNumber);
    }
  }
  
  public class Assignment {
    public uint Id { get; set; }
    public string Site { get; set; }
    public string Workcenter { get; set; }
    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }
    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }
    public List<Schedule> Schedules { get; set; }
    public DateTime Rotationdate { get; set; }
    public int Rotationdays { get; set; }

  }
}

