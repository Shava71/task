using System.Net.Http.Headers;
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
            // var httpClient = new HttpClient();
            // httpClient.BaseAddress = new Uri(url);
            // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            //
            // //Получаем ИТРоли
            // var response = httpClient.GetAsync($"api/v1/users/{userLogin}/roles").Result;
            // var itRoleResponse = JsonSerializer.Deserialize<UserRoleResponse>(response.Content.ReadAsStringAsync().Result);
            // var result1 = itRoleResponse.data.Select(_ => $"ItRole,{_.id}").ToList();
            //
            // //Получаем права
            // response = httpClient.GetAsync($"api/v1/users/{userLogin}/rights").Result;
            // var RightResponse = JsonSerializer.Deserialize<UserRoleResponse>(response.Content.ReadAsStringAsync().Result);
            // var result2 = RightResponse.data.Select(_ => $"RequestRight,{_.id}").ToList();
            //
            // return result1.Concat(result2).ToList();
            
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
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

             //проверяем что пользователь не залочен.
             var response = httpClient.GetAsync($"api/v1/users/all").Result;
             var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
             var user = userResponse.data.FirstOrDefault(_ => _.login == userLogin);

             if (user != null && user.status == "Lock")
             {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
             }
             //Назначаем права.
             else if (user != null && user.status == "Unlock")
             {
                 foreach (var rightId in rightIds)
                 {
                     var rightStr = rightId.Split(',');
                     switch (rightStr[0])
                     {
                        case "ItRole":
                            httpClient.PutAsync($"api/v1/users/{userLogin}/add/role/{rightStr[1]}", null).Wait();
                            break;
                        case "RequestRight":
                            httpClient.PutAsync($"api/v1/users/{userLogin}/add/right/{rightStr[1]}", null).Wait();
                            break;
                        default: 
                            throw new Exception($"Тип доступа {rightStr[0]} не определен");
                     }
                 }
             }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //проверяем что пользователь не залочен.
            var response = httpClient.GetAsync($"api/v1/users/all").Result;
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse.data.FirstOrDefault(_ => _.login == userLogin);

            if (user != null && user.status == "Lock")
            {
                Logger.Error($"Пользователь {userLogin} залочен.");
                return;
            }
             //отзываем права.
            else if (user != null && user.status == "Unlock")
            {
                foreach (var rightId in rightIds)
                {
                    var rightStr = rightId.Split(',');
                    switch (rightStr[0])
                    {
                        case "ItRole":
                            httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/role/{rightStr[1]}").Wait();
                            break;
                        case "RequestRight":
                            httpClient.DeleteAsync($"api/v1/users/{userLogin}/drop/right/{rightStr[1]}").Wait();
                            break;
                        default:
                            throw new Exception($"Тип доступа {rightStr[0]} не определен");
                    }
                }
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var props = new List<Property>();
            foreach (var propertyInfo in new UserPropertyData().GetType().GetProperties())
            {
                if(propertyInfo.Name == "login") continue;

                props.Add(new Property(propertyInfo.Name, propertyInfo.Name));
            }
            return props;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);

            var user = userResponse.data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");

            if (user.status == "Lock")
                throw new Exception($"Невозможно получить свойства, пользователь {userLogin} залочен");

            return user.GetType().GetProperties()
                .Select(_ => new UserProperty(_.Name, _.GetValue(user) as string));
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = httpClient.GetAsync($"api/v1/users/{userLogin}").Result;
            var userResponse = JsonSerializer.Deserialize<UserPropertyResponse>(response.Content.ReadAsStringAsync().Result);

            var user = userResponse.data ?? throw new NullReferenceException($"Пользователь {userLogin} не найден");
            if (user.status == "Lock")
                throw new Exception($"Невозможно обновить свойства, пользователь {userLogin} залочен");

            foreach (var property in properties)
            {
                foreach (var userProp in user.GetType().GetProperties())
                {
                    if (property.Name == userProp.Name)
                    {
                        userProp.SetValue(user, property.Value);
                    }
                }
            }

            var content = new StringContent(JsonSerializer.Serialize(user), UnicodeEncoding.UTF8, "application/json");
            httpClient.PutAsync("api/v1/users/edit", content).Wait();
        }

        public async Task<bool> IsUserExists(string userLogin)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = httpClient.GetAsync($"api/v1/users/all").Result;
            var userResponse = JsonSerializer.Deserialize<UserResponse>(response.Content.ReadAsStringAsync().Result);
            var user = userResponse.data.FirstOrDefault(_ => _.login == userLogin);

            if(user != null) return true;

            return false;
        }

        public async Task CreateUser(UserToCreate user)
        {
            NewUserDto newUser = user.ToDto();
            await _api.CreateUserAsync(newUser);
        }
        
        private 
    }
}
