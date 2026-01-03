namespace TR.Connector.DTOs.User;

public class UserByPropertiesDto
{
    public string? login { get; set; }
    public string? status { get; set; }
    public string? lastName { get; set; }
    public string? firstName { get; set; }
    public string? middleName { get; set; }
    public string? telephoneNumber { get; set; }
    public bool? isLead { get; set; }
}