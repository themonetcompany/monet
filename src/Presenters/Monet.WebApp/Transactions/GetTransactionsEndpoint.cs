using Monet.Application.Transactions;

namespace Monet.WebApp.Transactions;

public static class GetTransactionsEndpoint
{
    public static IEndpointRouteBuilder MapTransactionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions", GetTransactionsAsync);

        return app;
    }

    private static async Task<IResult> GetTransactionsAsync(
        GetTransactionsQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Code switch
            {
                "NON_AUTHENTICATED_USER" => Results.Json(new
                {
                    code = result.Code,
                    message = "The request requires an authenticated user."
                }, statusCode: StatusCodes.Status401Unauthorized),
                _ => Results.BadRequest(new
                {
                    code = result.Code,
                    message = "Unable to fetch transactions."
                })
            };
        }

        return Results.Ok(result.Value);
    }
}
