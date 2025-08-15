using norviguet_control_fletes_api.Entities;

public interface IAccessConfigurationService
{
    Task<bool> HasAccessAsync(string route, string httpMethod, UserRole role, string action);
}