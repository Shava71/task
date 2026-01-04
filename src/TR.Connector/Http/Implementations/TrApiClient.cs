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
        // HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ApiRoutes.Login,
        //     new UIAuthConfig
        //     {
        //         login = login,
        //         password = password
        //     });
        //
        // ApiResponse<TokenResponse> result = (await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponse>>())!;
        //
        // if (!result.success)
        // {
        //     throw new Exception(result.errorText);
        // }

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Login)
        {
            Content = JsonContent.Create(new UIAuthConfig // body для post-запроса 
            {
                login = login,
                password = password
            })
        };
        
        ApiResponse<TokenResponse>? result = await SendRequestAsync<TokenResponse>(request);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", result.data.access_token); // сохраняем jwt-token для следующих запросов
        
        return result.data.access_token;
    }

    public async Task<List<RoleResponse>?> GetAllRolesAsync()
    { 
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.RolesAll);
        ApiResponse<List<RoleResponse>>? result = await SendRequestAsync<List<RoleResponse>>(request);
        return result.data;
    }

    public async Task<List<RightResponse>> GetAllRightsAsync()
    {
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.RightsAll);
        ApiResponse<List<RightResponse>>? result = await SendRequestAsync<List<RightResponse>>(request);
        return result.data;
    }

    public async Task<List<RoleResponse>?> GetUserRolesAsync(string login)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.UserRoles(login: login));
        ApiResponse<List<RoleResponse>>? result = await SendRequestAsync<List<RoleResponse>>(request);
        return result.data;
    }

    public async Task<List<RightResponse>?> GetUserRightsAsync(string login)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.UserRights(login: login));
        ApiResponse<List<RightResponse>>? result = await SendRequestAsync<List<RightResponse>>(request);
        return result.data;
    }

    public async Task<List<UserResponse>?> GetAllUsersAsync()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.UsersAll);
        ApiResponse<List<UserResponse>>? response = await SendRequestAsync<List<UserResponse>>(request);
        return response.data;
    }

    public async Task<UserByPropertiesDto?> GetUserAsync(string login)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.User(login));
        ApiResponse<UserByPropertiesDto> result = await SendRequestAsync<UserByPropertiesDto>(request);
        return result.data;
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

    private async Task<ApiResponse<T>> SendRequestAsync<T>(HttpRequestMessage request)
    {
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HTTP Error: {response.StatusCode}", null, response.StatusCode);
        }
        
        ApiResponse<T> responseData = (await response.Content.ReadFromJsonAsync<ApiResponse<T>>())!;

        if (!responseData.success)
        {
            throw new Exception(responseData.errorText);
        }
        return responseData;
    }
}