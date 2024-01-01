using System.Data.Common;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OsanScheduler.Soap.Plans
{
  public class ReadingPlan : IComparable<ReadingPlan> {
    [BsonElement("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Title { get; set; } = "";
    [BsonIgnoreIfNull]
    public string? UserID { get; set; } = null;
    [BsonElement("start")]
    [BsonIgnoreIfNull]
    [JsonPropertyName("start")]
    public DateTime StartDate { get; set; } = new DateTime(0);
    public List<ReadingPeriod> Periods { get; set; } 
      = new List<ReadingPeriod>();

    public int CompareTo(ReadingPlan? other)
    {
      if (other != null) {
        if (this.UserID != null) {
          if (this.UserID.CompareTo(other.UserID) == 0) {
            return this.StartDate.CompareTo(other.StartDate);
          }
          return this.UserID.CompareTo(other.UserID);
        } else {
          return this.Title.CompareTo(other.Title);
        }
      }
      return -1;
    }

    public bool IsCompleted() {
      for (int i=0; i < this.Periods.Count; i++) {
        if (!this.Periods[i].IsCompleted()) {
          return false;
        }
      }
      return true;
    }

    public void ResetPlan() {
      for (int i=0; i < this.Periods.Count; i++) {
        this.Periods[i].ResetPeriod();
      }
    }

    public void AddPeriod(int days) {
      ReadingPeriod prd = new ReadingPeriod() {
        Id = this.Periods.Count + 1
      };
      for (int d = 0; d < days; d++) {
        prd.AddReadingDay(d+1, 0, "", 0, 0, 0);
      }
      this.Periods.Add(prd);
    }

    public void UpdatePeriod(int prdId, int day, int id, string field, string value) {
      this.Periods.Sort();
      bool found = false;
      for (int i = 0; i < this.Periods.Count && !found; i++) {
        if (this.Periods[i].Id == prdId) {
          found = true;
          if (field.ToLower().Equals("sort") && day == 0) {
            if (value.ToLower().Equals("up") && i > 0) {
              int temp = this.Periods[i].Id;
              this.Periods[i].Id = this.Periods[i-1].Id;
              this.Periods[i-1].Id = temp;
            } else if (value.ToLower().Equals("down") && i < this.Periods.Count - 1) {
              int temp = this.Periods[i].Id;
              this.Periods[i].Id = this.Periods[i+1].Id;
              this.Periods[i+1].Id = temp;
            }
            this.Periods.Sort();
          } else {
            this.Periods[i].UpdateReadingDay(day, id, field, value);
          }
        }
      }
      if (!found) {
        throw new Exception("reading plan period not found");
      }
    }

    public void DeletePeriod(int id) {
      bool found = false;
      for (int i=0; i < this.Periods.Count && !found; i++) {
        if (this.Periods[i].Id == id) {
          found = true;
          this.Periods.RemoveAt(i);
        }
      }
      if (!found) {
        throw new Exception("Period not found for deletion");
      }
      this.Periods.Sort();
      for (int i=0; i < this.Periods.Count; i++) {
        this.Periods[i].Id = i+1;
      }
    }
  }
}