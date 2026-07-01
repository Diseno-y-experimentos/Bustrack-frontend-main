namespace BusTrackBackEnd.API.IAM.Infrastructure.Tokens.JWT.Configuration;

public class TokenSettings
{
    public string Secret { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpiryHours { get; set; } = 4;
}