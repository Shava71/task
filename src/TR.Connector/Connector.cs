using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TR.Connector.Constants;
using TR.Connector.DTOs.Auth;
using TR.Connector.DTOs.Permission;
using TR.Connector.DTOs.User;
using TR.Connector.Extentsions;
using TR.Connector.Http.Interfaces;
using TR.Connector.Mapping;
using TR.Connector.Options;
using TR.Connectors.Api.Entities;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class Connector : IConnector
    {
        public ILogger Logger { get; set; }

        private ITrApiClient _api;

        private Dictionary<string, Func<string, int, Task>> _permissionHandlers; // обработка switch-case permissions

        //Пустой конструктор
        public Connector() {}

        public void StartUp(string connectionString)
        {
            // Пасрим строку подключения
            Logger.Debug("Строка подключения: " + connectionString);
            ConnectorOptionsValues options = ConnectorOptions.ParseConnectionString(connectionString);

            ServiceCollection services = new ServiceCollection();
            services.AddJwtTokenStore();
            services.AddTrHttpClient(options.Url);
            
            using ServiceProvider provider = services.BuildServiceProvider();
            _api = provider.GetRequiredService<ITrApiClient>();
            // проходим аутентификацию на сервере
            _api.LoginAsync(options.Login, options.Password).GetAwaiter().GetResult();

            _permissionHandlers = new Dictionary<string, Func<string, int, Task>>()
            {
                [ApiConstants.ItRole] = async (userLogin, roleId) => _api.AddRoleAsync(userLogin, roleId),
                [ApiConstants.RequestRight] = async (userLogin, roleId) => _api.RemoveRightAsync(userLogin, roleId)
            };
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            //Получаем ИТРоли
            List<RoleResponse>? roles = _api.GetAllRolesAsync()
                .GetAwaiter()
                .GetResult();
            
            //Получаем права
            List<RightResponse>? rights = _api.GetAllRightsAsync()
                .GetAwaiter()
                .GetResult();

            return roles.Select(r => r.ToPermission())
                .Concat(
                    rights.Select(r => r.ToPermission())
                );
            
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            //Получаем ИТРоли
            List<RoleResponse>? roles = _api.GetUserRolesAsync(login: userLogin)
                .GetAwaiter()
                .GetResult();
            
            //Получаем права
            List<RightResponse>? rights = _api.GetUserRightsAsync(login: userLogin)
                .GetAwaiter()
                .GetResult();

            return roles.Select(r => r.ToPermission().Id)
                .Concat(
                    rights.Select(r => r.ToPermission().Id)
                );
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
             //проверяем что пользователь не залочен.
             EnsureUserUnlocked(userLogin);
             //Назначаем права.
             foreach (string rightId in rightIds)
             {
                 string[] rightStr = rightId.Split(',');
                 // switch (rightStr[0])
                 // {
                 //     case ApiConstants.ItRole:
                 //         _api.AddRoleAsync(userLogin, int.Parse(rightStr[1]))
                 //             .GetAwaiter().GetResult();
                 //         break;
                 //
                 //     case ApiConstants.RequestRight:
                 //         _api.AddRightAsync(userLogin, int.Parse(rightStr[1]))
                 //             .GetAwaiter().GetResult();
                 //         break;
                 //     default:
                 //         throw new Exception($"Тип доступа {rightStr[0]} не определен");
                 // }
                 if (_permissionHandlers.TryGetValue(rightStr[0], out var handler)) // получаем нужную функцию из словаря
                 {
                     throw new Exception($"Тип доступа {rightStr[0]} не определён", null);
                 }
                 handler!(userLogin, int.Parse(rightStr[1])).GetAwaiter().GetResult();
             }
             
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            //проверяем что пользователь не залочен.
            EnsureUserUnlocked(userLogin);
             //отзываем права.

             foreach (string rightId in rightIds)
             {
                 string[] rightStr = rightId.Split(',');
                 if (_permissionHandlers.TryGetValue(rightStr[0], out var handler)) // получаем нужную функцию из словаря
                 {
                     throw new Exception($"Тип доступа {rightStr[0]} не определён", null);
                 }
                 handler!(userLogin, int.Parse(rightStr[1])).GetAwaiter().GetResult();
             }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return typeof(UserByPropertiesDto)
                .GetProperties()
                .Where(p => p.Name != "login")
                .Select(p => new Property(p.Name, p.Name));
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            // var httpClient = new HttpClient();
            // httpClient.BaseAddress = new Uri(url);
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            //
            // var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            // var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);
            //
            // var user = userResponse.data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");
            //
            // if (user.status == "Lock")
            //     throw new Exception($"Невозможно получить свойства, пользователь {userLogin} залочен");
            //
            // return user.GetType().GetProperties()
            //     .Select(_ => new UserProperty(_.Name, _.GetValue(user) as string));
            UserByPropertiesDto user = GetUnlockedUser(userLogin);

            return user.GetType()
                .GetProperties()
                .Select(p => new UserProperty(p.Name, p.GetValue(user)?.ToString()!)); 
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            UserByPropertiesDto user = GetUnlockedUser(userLogin);

            foreach (UserProperty property in properties)
            {
                foreach (PropertyInfo userProp in user.GetType().GetProperties())
                {
                    if (property.Name == userProp.Name)
                    {
                        userProp.SetValue(user, property.Value);
                    }
                }
            }

            _api.UpdateUserAsync(user)
                .GetAwaiter().GetResult();
        }

        public bool IsUserExists(string userLogin)
        {
            //вместо перебора всех полученных пользователей в памяти системы, отдадим поиск на внешний апи-сервер
            UserByPropertiesDto? user = _api.GetUserAsync(userLogin).GetAwaiter().GetResult();
            if(user is not null)
                return true;
            return false;
        }

        public void CreateUser(UserToCreate user)
        {
            NewUserDto newUser = user.ToDto();
            _api.CreateUserAsync(newUser)
                .GetAwaiter()
                .GetResult();
        }
        
        private void EnsureUserUnlocked(string login)
        {
            UserByPropertiesDto user = _api.GetUserAsync(login)
                .GetAwaiter()
                .GetResult();

            if (user.status == ApiConstants.Locked)
            {
                Logger.Error($"Пользователь {login} залочен.");
                throw new InvalidOperationException($"User {login} is locked");
            }
            Logger.Error($"Пользователь {login} не залочен.");
        }

        private UserByPropertiesDto GetUnlockedUser(string login)
        {
            var user = _api.GetUserAsync(login)
                .GetAwaiter()
                .GetResult();

            if (user == null)
                throw new InvalidOperationException($"Пользователь {login} не найден");

            if (user.status == ApiConstants.Locked)
                throw new InvalidOperationException($"Невозможно получить свойства, пользователь {login} залочен");

            return user;
        }
    }
}
