namespace norviguet_control_fletes_api.Common.Middlewares;

public class UnauthorizedException(string message) : Exception(message);
