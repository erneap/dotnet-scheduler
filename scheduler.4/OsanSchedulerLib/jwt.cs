using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

namespace OsanScheduler.Users {
  public class JWTToken: JwtSecurityToken {
    [JsonPropertyName("userid")]
    public string UserID { get; set; } = "";

    [JsonPropertyName("emailAddress")]
    public string EmailAddress { get; set;} = "";

    public JWTToken(string id, string email): base() {
      this.UserID = id;
      this.EmailAddress = email;
    }
  }
}