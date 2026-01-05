using TR.Connector.Constants;
using TR.Connector.DTOs.Permission;
using TR.Connector.DTOs.User;
using TR.Connectors.Api.Entities;

namespace TR.Connector.Mapping;

public static class PermissionMapping
{
    public static Permission ToPermission(this RoleResponse r)
    {
        return new Permission(
            id: $"{ApiConstants.ItRole},{r.id}",
            name: r.name!,
            description: r.corporatePhoneNumber!
        );
    }

    public static Permission ToPermission(this RightResponse r)
    {
        return new Permission(
            id: $"{ApiConstants.RequestRight},{r.id}", 
            name: r.name, 
            description: null!
            );
    }
}