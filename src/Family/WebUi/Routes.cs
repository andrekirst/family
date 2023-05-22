namespace WebUi;

public static class Routes
{
    public const string AreaRouteName = "areaRoute";
    public const string AreaRoutePattern = "{area:exists}/{controller}/{action}";
    public const string AreaRouteTemplate = "[area]/[controller]/[action]";
}