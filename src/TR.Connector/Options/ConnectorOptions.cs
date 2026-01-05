namespace TR.Connector.Options;

/// <summary>
/// Парсер строки подключения
/// </summary>
public static class ConnectorOptions
{
    /// <summary>
    /// Метод парсинга
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    /// <returns>Структура {url, login, password}</returns>
    public static ConnectorOptionsValues ParseConnectionString(string connectionString)
    {
        string Url = string.Empty;
        string Login = string.Empty;
        string Password = string.Empty;
        
        foreach (var item in connectionString.Split(';'))
        {
            if (item.StartsWith("url")) Url = item.Split('=')[1];
            if (item.StartsWith("login")) Login = item.Split('=')[1];
            if (item.StartsWith("password")) Password = item.Split('=')[1];
        }
        
        ConnectorOptionsValues values = new ConnectorOptionsValues()
        {
            Url = Url,
            Login = Login,
            Password = Password
        };
        
        return values;
    }
}

/// <summary>
/// Структура хранения информации парсинга строки подключения
/// </summary>
public struct ConnectorOptionsValues 
{
    public string Url { get; set; }
    public string Login { get; set; }
    public string Password {get; set;}
}