using Monet.Application.Transactions;

namespace Monet.WebApp.Transactions;

public static class AssignTransactionCategoryEndpoint
{
    public static IEndpointRouteBuilder MapAssignTransactionCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/transactions/{transactionId}/category", AssignTransactionCategoryAsync);

        return app;
    }

    private static async Task<IResult> AssignTransactionCategoryAsync(
        string transactionId,
        AssignTransactionCategoryRequest? request,
        AssignTransactionCategoryHandler handler,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Results.BadRequest(new
            {
                code = "INVALID_TRANSACTION_CATEGORY",
                message = "A request body is required."
            });
        }

        var result = await handler.HandleAsync(transactionId, request.CategoryId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Code switch
            {
                "NON_AUTHENTICATED_USER" => Results.Json(new
                {
                    code = result.Code,
                    message = "The request requires an authenticated user."
                }, statusCode: StatusCodes.Status401Unauthorized),
                "TRANSACTION_NOT_FOUND" => Results.NotFound(new
                {
                    code = result.Code,
                    message = "Transaction not found."
                }),
                "INVALID_TRANSACTION_CATEGORY" or "CATEGORY_NOT_ALLOWED_FOR_FLOW" or "CATEGORY_FORBIDDEN_FOR_NEUTRAL_FLOW"
                    => Results.BadRequest(new
                    {
                        code = result.Code,
                        message = "Invalid transaction category assignment."
                    }),
                _ => Results.BadRequest(new
                {
                    code = result.Code,
                    message = "Unable to assign transaction category."
                }),
            };
        }

        return Results.NoContent();
    }

    private sealed record AssignTransactionCategoryRequest
    {
        public string? CategoryId { get; init; }
    }
}
