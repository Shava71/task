using TR.Connector.DTOs.Permission;
using TR.Connector.DTOs.User;

namespace TR.Connector.Http.Interfaces;

public interface ITrApiClient
{
    Task<string> LoginAsync(string login, string password);
    Task<List<RoleResponse>?> GetAllRolesAsync();
    Task<List<RightResponse>> GetAllRightsAsync();
    Task<List<RoleResponse>?> GetUserRolesAsync(string login);
    Task<List<RightResponse>?> GetUserRightsAsync(string login);
    Task<List<UserResponse>?> GetAllUsersAsync();
    Task<UserByPropertiesDto?> GetUserAsync(string login);
    Task CreateUserAsync(NewUserDto user);
    Task UpdateUserAsync(UserByPropertiesDto user);
    Task AddRoleAsync(string login, int roleId);
    Task AddRightAsync(string login, int rightId);
    Task RemoveRoleAsync(string login, int roleId);
    Task RemoveRightAsync(string login, int rightId);
}