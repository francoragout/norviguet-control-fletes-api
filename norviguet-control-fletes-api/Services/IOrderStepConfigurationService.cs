using norviguet_control_fletes_api.Entities;

public interface IOrderStepConfigurationService
{
    Task<bool> CanPerformActionAsync(int step, string field, UserRole role, string action);
}