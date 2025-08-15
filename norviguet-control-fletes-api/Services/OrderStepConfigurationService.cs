using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;

public class OrderStepConfigurationService : IOrderStepConfigurationService
{
    private readonly NorviguetDbContext _context;
    public OrderStepConfigurationService(NorviguetDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanPerformActionAsync(int step, string field, UserRole role, string action)
    {
        return await _context.OrderStepConfigurations
            .AnyAsync(cfg => cfg.Step == step && cfg.Field == field && cfg.Role == role && cfg.Action == action);
    }
}