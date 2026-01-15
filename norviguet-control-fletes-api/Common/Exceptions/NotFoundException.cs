namespace norviguet_control_fletes_api.Common.Middlewares;

public class NotFoundException(string message) : Exception(message);
