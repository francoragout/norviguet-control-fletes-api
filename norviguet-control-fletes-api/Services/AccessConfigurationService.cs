using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;

public class AccessConfigurationService : IAccessConfigurationService
{
    private readonly NorviguetDbContext _context;
    public AccessConfigurationService(NorviguetDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasAccessAsync(string route, string httpMethod, UserRole role, string action)
    {
        return await _context.AccessConfigurations
            .AnyAsync(ac => ac.Route == route && ac.HttpMethod == httpMethod && ac.Role == role && ac.Action == action);
    }
}