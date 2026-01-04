namespace TR.Connector.Http.JwtToken;

public interface ITokenStore
{
    string? AccessToken { get; set; }
}