using BCrypt.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using OsanScheduler.Exceptions;

namespace OsanScheduler.Users;

public class User : IComparable<User>, IEquatable<User>
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  [JsonPropertyName("id")]
  public string? Id { get; set;}

  [BsonElement("emailAddress")]
  [JsonPropertyName("emailAddress")]
  public string EmailAddress { get; set;} = null!;

  [BsonElement("password")]
  [JsonIgnore]
  public string Password { get; set;} = null!;

  [BsonElement("passwordExpires")]
  [JsonPropertyName("passwordExpires")]
  public DateTime PasswordExpires {get; set;} = DateTime.UtcNow;

  [BsonElement("badAttempts")]
  [JsonPropertyName("badAttempts")]
  public uint BadAttempts { get; set;} = 0;

  [BsonElement("firstName")]
  [JsonPropertyName("firstName")]
  public string FirstName { get; set;} = null!;

  [BsonElement("middleName")]
  [BsonIgnoreIfNull]
  [JsonPropertyName("middleName")]
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string MiddleName { get; set; } = null!;

  [BsonElement("lastName")]
  [JsonPropertyName("lastName")]
  public string LastName { get; set; } = null!;

  [BsonElement("workgroups")]
  [JsonPropertyName("workgroups")]
  public List<String> Workgroups { get; set; } = null!;

  [BsonElement("resettoken")]
  [BsonIgnoreIfNull]
  [JsonIgnore]
  public string ResetToken {get; set;} = null!;

  [BsonElement("resettokenexp")]
  [BsonIgnoreIfNull]
  [JsonIgnore]
  public DateTime? ResetTokenExpires { get; set; } = null;

  public int CompareTo(User? other)
  {
      if (other == null) return 1;

      if (this.LastName.ToLower().Equals(other.LastName.ToLower())) {
        if (this.FirstName.ToLower().Equals(other.FirstName.ToLower())) {
          return this.MiddleName.ToLower().CompareTo(other.MiddleName.ToLower());
        }
        return this.FirstName.ToLower().CompareTo(other.FirstName.ToLower());
      }

      return this.LastName.ToLower().CompareTo(other.LastName.ToLower());
  }

  public bool Equals(User? other)
  {
    if (other == null) return false;
    return this.EmailAddress.ToLower().Equals(other.EmailAddress.ToLower());
  }

  public void SetPassword(string passwd)  {
    try {
      var hashed = BCrypt.Net.BCrypt.HashPassword(passwd, workFactor: 12);
      this.Password = hashed;
      this.BadAttempts = 0;
      this.PasswordExpires = DateTime.UtcNow.AddDays(90);
    } catch (SaltParseException) {
      throw;
    }
  }

  public void Authenticate(string passwd) {
    try {
      var result = BCrypt.Net.BCrypt.Verify(passwd, this.Password);

      // Check for password expired
      if (this.PasswordExpires.CompareTo(DateTime.UtcNow) < 0) {
        this.BadAttempts++;
        throw new AuthenticationException("Password Expired");
      }

      // Check if account is locked, > 2 bad attempts
      if (this.BadAttempts > 2) {
        throw new AuthenticationException("Account Locked");
      }

    } catch (Exception) {
      this.BadAttempts++;
      throw new AuthenticationException("Email Address/Password mismatch!");
    }
    this.BadAttempts = 0;
  }

  public void Unlock() {
    this.BadAttempts = 0;
  }

  public string GetFullName() {
    if (this.MiddleName != null && this.MiddleName != "") {
      return $"{this.FirstName} {this.MiddleName.Substring(1, 1)}. " 
        + $"{this.LastName}"; 
    }
    return $"{this.FirstName} {this.LastName}";
  }

  public string GetLastFirst() {
    if (this.MiddleName != null && this.MiddleName != "") {
      return $"{this.LastName}, {this.FirstName} " 
        + $"{this.MiddleName.Substring(1,1)}."; 
    }
    return $"{this.LastName}, {this.FirstName}";
  }
}