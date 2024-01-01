using MongoDB.Bson.Serialization.Attributes;
using OsanScheduler.Soap.Plans;
using ZstdSharp.Unsafe;

namespace OsanScheduler.Soap.Bible
{
  public class BibleChapter : IComparable<BibleChapter> {
    public int Id { get; set; }
    [BsonIgnoreIfNull]
    public List<Passage> Passages { get; set; } = new List<Passage>();

    public int CompareTo(BibleChapter? other)
    {
      if (other != null) {
        return this.Id.CompareTo(other.Id);
      }
      return -1;
    }

    public bool IsComplete() {
      if (this.Passages.Count == 0) {
        return false;
      }
      this.Passages.Sort();
      int current = 0;
      foreach (Passage psg in this.Passages) {
        if (psg.StartVerse != current + 1) {
          return false;
        }
        current = psg.EndVerse;
      }
      return true;
    }

    public Passage AddPassage(int bookid, string book, int chapter, int start, 
      int end) {
      if (bookid <= 0 || chapter <= 0) {
        throw new Exception("not enough information to add passage");
      }
      for (int i=0; i < this.Passages.Count; i++) {
        if (this.Passages[i].BookId == bookid 
          && this.Passages[i].Chapter == chapter) {
          this.Passages[i].StartVerse = start;
          this.Passages[i].EndVerse = end;
          return this.Passages[i];
        }
      }
      Passage psg = new Passage(){
        Id = this.Passages.Count + 1,
        BookId = bookid,
        Book = book,
        Chapter = chapter,
        StartVerse = start,
        EndVerse = end
      };
      this.Passages.Add(psg);
      return psg;
    }

    public Passage UpdatePassage(int id, string field, string value) {
      if (id <= 0) {
        throw new Exception("not enough information to update passage");
      }
      for (int i = 0; i < this.Passages.Count; i++) {
        if (this.Passages[i].Id == id) {
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
              this.Passages[i].Completed = (value.ToLower().Equals("true"));
              break;
          }
          return this.Passages[i];
        }
      }
      throw new Exception("passage not found");
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
  }
}