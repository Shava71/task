namespace TR.Connector.Http.JwtToken;

public class TokenStore : ITokenStore
{
    public string? AccessToken { get; set; }
}