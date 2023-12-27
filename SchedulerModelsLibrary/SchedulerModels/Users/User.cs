using System.Text.Json.Serialization;
using BCrypt.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OsanScheduler.Users
{
  public class User : IComparable<User> {
    [BsonElement("_id")]
    [JsonPropertyName("id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    [BsonElement("emailAddress")]
    [JsonPropertyName("emailAddress")]
    public string EmailAddress { get; set; } = "";
    public string Password { get; set; } = "";
    [BsonElement("passwordExpires")]
    [JsonPropertyName("passwordExpires")]
    public DateTime PasswordExpires { get; set; } = DateTime.UtcNow.AddDays(90.0);
    [BsonElement("badAttempts")]
    [JsonPropertyName("badAttempts")]
    public uint BadAttempts { get; set; }
    [BsonElement("firstName")]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = "";
    [BsonElement("middleName")]
    [JsonPropertyName("middleName")]
    public string MiddleName { get; set; } = "";
    [BsonElement("lastName")]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = "";
    public List<string> Workgroups { get; set; } = new List<string>();
    [BsonElement("resettoken")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string ResetToken { get; set; } = "";
    [BsonElement("resettokenexp")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public DateTime ResetTokenExp { get; set; } = new DateTime(0);

    public int CompareTo(User? other)
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

    public bool IsInGroup(string app, string group) {
      string name = app + "-" + group;
      for (int i = 0; i < this.Workgroups.Count; i++) {
        if (this.Workgroups[i].ToLower().Equals(name)) {
          return true;
        }
      }
      return false;
    }

    public void SetPassword(string passwd) {
      this.Password = BCrypt.Net.BCrypt.HashPassword(passwd);
      this.BadAttempts = 0;
      this.PasswordExpires = DateTime.UtcNow.AddDays(90.0);
    }

    public bool Authenticate(string passwd) {
      if (!BCrypt.Net.BCrypt.Verify(passwd, this.Password)) {
        this.BadAttempts++;
        throw new Exception("Email address or password is incorrect");
      }
      if (this.PasswordExpires.CompareTo(DateTime.UtcNow) < 0) {
        this.BadAttempts++;
        throw new Exception("Password expired");
      }
      if (this.BadAttempts > 2) {
        throw new Exception("Account locked");
      }
      this.BadAttempts = 0;
      return false;
    }

    public string GetFullName() {
      if (!this.MiddleName.Equals("")) {
        return this.FirstName + " " + this.MiddleName.Substring(0,1) + ". "
          + this.LastName;
      }
      return this.FirstName + " " + this.LastName;
    }

    public string GetLastFirst() {

      if (!this.MiddleName.Equals("")) {
        return this.LastName + ", " + this.FirstName + " " 
          + this.MiddleName.Substring(0,1) + ".";
      }
      return this.LastName + ", " + this.FirstName;
    }
  }
}