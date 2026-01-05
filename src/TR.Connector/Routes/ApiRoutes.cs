namespace TR.Connectors.Routes;

/// <summary>
/// Статический класс api-эндпоинтов сервиса
/// TODO: вынести в отдельный файл конфигурации appsettings (убрать хард-коддинг)
/// </summary>
public static class ApiRoutes
{
    public const string commonPath = "api/v1/"; // данный момент не влияет на производительность, т.к. clr заранее конкатенирует строки на этапе компиляции
    // Admin
    public const string Login = commonPath+"login";
    public const string Health = commonPath+"isServiceWorking";
    
    // Roles & rights
    public const string RolesAll = commonPath+"roles/all";
    public const string RightsAll = commonPath+"rights/all";
    
    // Users
    public const string UsersAll = commonPath+"users/all";
    public static string User(string login) => commonPath+$"users/{login}";
    public static string UserRoles(string login) => commonPath+$"users/{login}/roles";
    public static string UserRights(string login) => $"users/{login}/rights";
    public const string UserCreate = commonPath+"users/create";
    public const string UserEdit = commonPath+"users/edit";
    public static string AddRole(string login, int roleId) => commonPath+$"users/{login}/add/role/{roleId}";
    public static string AddRight(string login, int rightId) => commonPath+$"users/{login}/add/right/{rightId}";
    public static string DropRole(string login, int roleId) => commonPath+$"users/{login}/drop/role/{roleId}";
    public static string DropRight(string login, int rightId) => commonPath+$"users/{login}/drop/right/{rightId}";
    public static string LockUser(string login) => commonPath+$"users/{login}/lock";
    public static string UnlockUser(string login) => commonPath+$"users/{login}/unlock";
  
}