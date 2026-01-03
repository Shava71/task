using System.Net.Http.Headers;
using System.Net.Http.Json;
using TR.Connector.DTOs.Auth;
using TR.Connector.DTOs.CommonResponse;
using TR.Connector.DTOs.Permission;
using TR.Connector.DTOs.User;
using TR.Connector.Http.Interfaces;
using TR.Connectors.Api.Api;

namespace TR.Connector.Http.Implementations;

public class TrApiClient : ITrApiClient
{
    private readonly HttpClient _httpClient;

    public TrApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> LoginAsync(string login, string password)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ApiRoutes.Login,
            new UIAuthConfig
            {
                login = login,
                password = password
            });
        
        ApiResponse<TokenResponse> result = (await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>())!;

        if (!result.success)
        {
            throw new Exception(result.errorText);
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", result.data.access_token); // сохраняем jwt-token для следующих запросов
        
        return result.data.access_token;
    }

    public async Task<List<RoleResponse>?> GetAllRolesAsync()
    {
        List<RoleResponse>? allRolesAsync = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RoleResponse>>>(ApiRoutes.RolesAll))?.data;
        return allRolesAsync;
    }

    public async Task<List<RightResponse>> GetAllRightsAsync()
    {
        List<RightResponse>? allRightsAsync = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RightResponse>>>(ApiRoutes.RightsAll))?.data;
        return allRightsAsync;
    }

    public async Task<List<RoleResponse>> GetUserRolesAsync(string login)
    {
        
    }

    public async Task<List<RightResponse>> GetUserRightsAsync(string login)
    {
        
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        
    }

    public async Task<UserByPropertiesDto> GetUserAsync(string login)
    {
        
    }

    public async Task CreateUserAsync(NewUserDto user)
    {
        
    }

    public async Task UpdateUserAsync(UserByPropertiesDto user)
    {
        
    }

    public async Task AddRoleAsync(string login, int roleId)
    {
        
    }

    public async Task AddRightAsync(string login, int rightId)
    {
        
    }

    public async Task RemoveRoleAsync(string login, int roleId)
    {
        
    }

    public async Task RemoveRightAsync(string login, int rightId)
    {
        
    }
}