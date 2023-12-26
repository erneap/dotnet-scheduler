namespace OsanScheduler.employees
{

  public class CompanyInfo
  {
    public string Company { get; set; }
    [BsonElement("employeeid")]
    public string EmployeeID { get; set; }
    [BsonElement("alternateid")]
    public string AlternateID { get; get;}
    [BsonElement("jobtitle")]
    public string JobTitle { get; set; }
    [BsonElement("costcenter")]
    public string CostCenter { get; set; }
    public string Division { get; set; }
  }
}