namespace TR.Connector.DTOs.Auth;

public sealed class TokenResponse
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
}