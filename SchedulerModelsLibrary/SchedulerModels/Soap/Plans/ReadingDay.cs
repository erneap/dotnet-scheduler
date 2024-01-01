using System.Formats.Asn1;

namespace OsanScheduler.Soap.Plans
{
  public class ReadingDay : IComparable<ReadingDay>
  {
    public int Day { get; set; }
    public List<Passage> Passages { get; set; } = new List<Passage>();

    public int CompareTo(ReadingDay? other)
    {
      if (other != null) {
        return this.Day.CompareTo(other.Day);
      }
      return 0;
    }

    public void AddPassage(int bookid, string book, int chptr, int start, 
      int end) {
      int max = 0;
      if (bookid == 0 || chptr == 0) {
        throw new Exception("not enough information to add passage");
      }
      bool found = false;
      for (int i=0; i < this.Passages.Count && !found; i++) {
        if (this.Passages[i].BookId == bookid && this.Passages[i].Chapter == chptr) {
          found = true;
          if (start > 0 || end > 0) {
            this.Passages[i].StartVerse = start;
            this.Passages[i].EndVerse = end;
          }
        }
        if (this.Passages[i].Id > max) {
          max = this.Passages[i].Id;
        }
      }
      if (!found) {
        this.Passages.Add(new Passage() {
          Id = max + 1,
          BookId = bookid,
          Book = book,
          Chapter = chptr,
          StartVerse = start,
          EndVerse = end
        });
      }
      this.Passages.Sort();
    }

    public void UpdatePassage(int id, string field, string value) {
      if (id <= 0) {
        throw new Exception("not enough information to update passage");
      }
      bool found = false;
      for (int i = 0; i < this.Passages.Count && !found; i++) {
        if (this.Passages[i].Id == id) {
          found = true;
          switch (field.ToLower()) {
            case "bookid":
              this.Passages[i].BookId = Convert.ToInt32(value);
              break;
            case "book":
              this.Passages[i].Book = value;
              break;
            case "chapter":
              this.Passages[i].Chapter = Convert.ToInt32(value);
              break;
            case "start":
            case "startverse":
              this.Passages[i].StartVerse = Convert.ToInt32(value);
              break;
            case "end":
            case "endverse":
              this.Passages[i].EndVerse = Convert.ToInt32(value);
              break;
            case "text":
              this.Passages[i].Text = value;
              break;
            case "completed":
              this.Passages[i].Completed = value.ToLower().Equals("true");
              break;
          }
        }
      }
      if (!found) {
        throw new Exception("passage not found");
      }
    }

    public void UpdatePassageText(int id, string text) {
      if (id <= 0) {
        throw new Exception("not enough information to update passage");
      }
      bool found = false;
      for (int i = 0; i < this.Passages.Count && !found; i++) {
        if (this.Passages[i].Id == id) {
          found = true;
          this.Passages[i].Text = text;
        }
      }
      if (!found) {
        throw new Exception("passage not found");
      }
    }

    public void DeletePassage(int id) {
      if (id <= 0) {
        throw new Exception("not enough information to delete passage");
      }
      bool found = false;
      for (int i = 0; i < this.Passages.Count && !found; i++) {
        if (this.Passages[i].Id == id) {
          found = true;
          this.Passages.RemoveAt(i);
          this.Passages.Sort();
        }
      }
      if (!found) {
        throw new Exception("passage not found");
      }
    }

    public bool IsCompleted() {
      for (int i = 0; i < this.Passages.Count; i++) {
        if (!this.Passages[i].Completed) {
          return false;
        }
      }
      return true;
    }

    public void ResetDay() {
      for (int i=0; i < this.Passages.Count; i++) {
        this.Passages[i].Completed = false;
      }
    }
  }
}