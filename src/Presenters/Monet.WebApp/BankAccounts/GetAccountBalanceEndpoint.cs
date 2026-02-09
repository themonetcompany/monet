using Monet.Application.BankAccounts;

namespace Monet.WebApp.BankAccounts;

public static class GetAccountBalanceEndpoint
{
    public static IEndpointRouteBuilder MapAccountBalanceEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/balances", GetAccountBalancesAsync);

        return app;
    }

    private static async Task<IResult> GetAccountBalancesAsync(
        GetAccountBalancesQueryHandler handler,
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
                    message = "Unable to fetch account balances."
                })
            };
        }

        return Results.Ok(result.Value);
    }
}
