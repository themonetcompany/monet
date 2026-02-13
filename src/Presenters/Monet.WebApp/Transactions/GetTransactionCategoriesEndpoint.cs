using Monet.Application.Transactions;

namespace Monet.WebApp.Transactions;

public static class GetTransactionCategoriesEndpoint
{
    public static IEndpointRouteBuilder MapTransactionCategoriesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions/categories", GetTransactionCategoriesAsync);

        return app;
    }

    private static async Task<IResult> GetTransactionCategoriesAsync(
        GetTransactionCategoriesQueryHandler handler,
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
                    message = "Unable to fetch transaction categories."
                })
            };
        }

        return Results.Ok(result.Value);
    }
}
