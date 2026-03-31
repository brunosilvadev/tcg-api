namespace TcgApi.Endpoints;

public class RootEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new { message = "TCG API is running" }));
    }
}
