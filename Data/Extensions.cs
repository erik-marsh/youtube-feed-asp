namespace youtube_feed_asp.Data;

public static class Extensions
{
    public static void CreateDbIfNotExists(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<VideoContext>();
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
    }
}