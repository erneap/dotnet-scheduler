namespace OsanScheduler.Soap.Plans 
{
  public class ReadingPeriod : IComparable<ReadingPeriod> {
    public int Id { get; set; }
    public List<ReadingDay> Days { get; set; } = new List<ReadingDay>();

    public int CompareTo(ReadingPeriod? other)
    {
      if (other != null) {
        return this.Id.CompareTo(other.Id);
      }
      return 0;
    }

    public bool IsCompleted() {
      foreach (ReadingDay day in this.Days) {
        if (!day.IsCompleted()) {
          return false;
        }
      }
      return true;
    }

    public void AddReadingDay(int day, int bookid, string book, int chapter, 
      int start, int end) {
      if (day <= 0) {
        throw new Exception("day can't be zero or below");
      }
      bool found = false;
      for (int i=0; i < this.Days.Count && !found; i++) {
        if (this.Days[i].Day == day) {
          found = true;
          this.Days[i].AddPassage(bookid, book, chapter, start, end);
        }
      }
      if (!found) {
        ReadingDay rday = new ReadingDay() {
          Day = day
        };
        rday.AddPassage(bookid, book, chapter, start, end);
        this.Days.Add(rday);
        this.Days.Sort();
      }
    }
    
    public void UpdateReadingDay(int day, int id, string field, string value) {
      if (day <= 0) {
        throw new Exception("not enough information to update reading day");
      }
      bool found = false;
      for (int i = 0; i < this.Days.Count && !found; i++) {
        if (this.Days[i].Day == day) {
          found = true;
          if (field.ToLower() != "delete") {
            this.Days[i].UpdatePassage(id, field, value);
          } else {
            this.Days[i].DeletePassage(id);
          }
        }
      }
      if (!found) {
        throw new Exception("day not found");
      }
    }

    public void DeleteReadingDay(int day) {
      if (day <= 0) {
        throw new Exception("not enough information to delete reading day");
      }
      bool found = false;
      for (int i = 0; i < this.Days.Count && !found; i++) {
        if (this.Days[i].Day == day) {
          found = true;
          this.Days.RemoveAt(i);
          this.Days.Sort();
        }
      }
      if (!found) {
        throw new Exception("passage not found");
      }
    }

    public void ResetPeriod() {
      for (int i=0; i < this.Days.Count; i++) {
        this.Days[i].ResetDay();
      }
    }
  }
}