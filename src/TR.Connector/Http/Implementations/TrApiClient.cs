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
        List<RoleResponse>? allRoles = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RoleResponse>>>(ApiRoutes.RolesAll))?.data;
        return allRoles;
    }

    public async Task<List<RightResponse>> GetAllRightsAsync()
    {
        List<RightResponse>? allRights = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RightResponse>>>(ApiRoutes.RightsAll))?.data;
        return allRights;
    }

    public async Task<List<RoleResponse>?> GetUserRolesAsync(string login)
    {
        List<RoleResponse>? userRoles = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RoleResponse>>>(
            ApiRoutes.UserRoles(login: login)
        ))?.data;
        
        return userRoles;
    }

    public async Task<List<RightResponse>?> GetUserRightsAsync(string login)
    {
        List<RightResponse>? userRigths = (await _httpClient.GetFromJsonAsync<ApiResponse<List<RightResponse>>>(
            ApiRoutes.UserRights(login: login)
        ))?.data;
        return userRigths;
    }

    public async Task<List<UserResponse>?> GetAllUsersAsync()
    {
        List<UserResponse>? users = (await _httpClient.GetFromJsonAsync<ApiResponse<List<UserResponse>>>(ApiRoutes.UsersAll))?.data;
        return users;
    }

    public async Task<UserByPropertiesDto?> GetUserAsync(string login)
    {
        ApiResponse<UserByPropertiesDto>? response = (await _httpClient.GetFromJsonAsync<ApiResponse<UserByPropertiesDto>>(ApiRoutes.User(login:login)));
        if (response is not null && !response.success)
        {
            throw new Exception(response.errorText);
        }
        return response.data;
    }

    public async Task CreateUserAsync(NewUserDto user)
    {
        await _httpClient.PostAsJsonAsync(ApiRoutes.UserCreate, user);
    }

    public async Task UpdateUserAsync(UserByPropertiesDto user)
    {
        await _httpClient.PutAsJsonAsync(ApiRoutes.UserEdit, user);
    }

    public async Task AddRoleAsync(string login, int roleId)
    {
        await _httpClient.PutAsync(ApiRoutes.AddRole(
                login: login, 
                roleId: roleId), 
            null
            );
    }

    public async Task AddRightAsync(string login, int rightId)
    {
        await _httpClient.PutAsync(ApiRoutes.AddRight(
                login: login, 
                rightId: rightId), 
            null
        );
    }

    public async Task RemoveRoleAsync(string login, int roleId)
    {
        await _httpClient.DeleteAsync(ApiRoutes.DropRole(
            login: login,
            roleId: roleId
        ));
    }

    public async Task RemoveRightAsync(string login, int rightId)
    {
        await _httpClient.DeleteAsync(ApiRoutes.DropRight(
            login: login,
            rightId: rightId
        ));
    }
}