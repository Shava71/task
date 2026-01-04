using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using TR.Connector.Http.Handlers;
using TR.Connector.Http.Implementations;
using TR.Connector.Http.Interfaces;
using TR.Connector.Http.JwtToken;

namespace TR.Connector.Extentsions;

public static class HttpServiceCollectionExtensions
{
    public static IServiceCollection AddJwtTokenStore(this IServiceCollection services)
    {
        services.AddScoped<ITokenStore, TokenStore>();
        return services;
    }
    
    public static IServiceCollection AddTrHttpClient(this IServiceCollection services, string url) //login и password в типе не храним,
                                                                                                   //это должна знать только бизнес-логика
    {
        services.AddHttpClient<ITrApiClient, TrApiClient>(client =>
        {
            client.BaseAddress = new Uri(url);
        })
        .AddHttpMessageHandler<AuthHandler>()
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(5))
        .AddPolicyHandler(GetCircuitBreakerPolicy());
        return services;
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // данный момент необходимо перечитать, т.к. http-клиенты могут синхронизироваться и одновременное "стучать" в сервис (см. статью про polly от DoDoPizza)
            );

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
}