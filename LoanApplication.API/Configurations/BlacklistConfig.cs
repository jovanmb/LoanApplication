namespace LoanApplication.API.Configurations;
public class BlacklistConfig
{
    public List<string> MobileNumbers { get; set; }
    public List<string> EmailDomains { get; set; }
}
