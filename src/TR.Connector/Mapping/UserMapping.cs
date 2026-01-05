using TR.Connector.DTOs.User;
using TR.Connectors.Api;
using TR.Connectors.Api.Entities;

namespace TR.Connector.Mapping;

public static class UserMapping
{
    public static NewUserDto ToDto(this UserToCreate user)
    {
        return new NewUserDto
        {
            login = user.Login,
            password = user.HashPassword,
            firstName = Get(user, "firstName"),
            lastName = Get(user, "lastName"),
            middleName = Get(user, "middleName"),
            telephoneNumber = Get(user, "telephoneNumber"),
            isLead = bool.TryParse(Get(user, "isLead"), out var v) ? v : null
        };
    }

    private static string Get(UserToCreate user, string name)
        => user.Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
}