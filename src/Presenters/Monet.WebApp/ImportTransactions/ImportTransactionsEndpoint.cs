using Monet.Application.ImportTransactions;

namespace Monet.WebApp.ImportTransactions;

public static class ImportTransactionsEndpoint
{
    public static IEndpointRouteBuilder MapTransactionImportEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions/import", ImportTransactionsAsync)
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> ImportTransactionsAsync(
        IFormFile? file,
        ITransactionImportFileParserSelector parserSelector,
        TransactionImportHandler handler,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new
            {
                code = "MISSING_IMPORT_FILE",
                message = "No file was uploaded."
            });
        }

        var parserResult = parserSelector.Resolve(file.FileName, file.ContentType);
        if (!parserResult.IsSuccess)
        {
            return Results.Json(new
            {
                code = parserResult.Code,
                message = "No parser available for this file format."
            }, statusCode: StatusCodes.Status415UnsupportedMediaType);
        }

        await using var fileStream = file.OpenReadStream();
        var importToExecute = await parserResult.Value.ParseAsync(fileStream, cancellationToken);

        if (!importToExecute.IsSuccess)
        {
            return Results.BadRequest(new
            {
                code = importToExecute.Code,
                message = "Unable to parse the uploaded file."
            });
        }

        var importResult = await handler.HandleAsync(importToExecute.Value, cancellationToken);

        if (!importResult.IsSuccess)
        {
            return importResult.Code switch
            {
                "NON_AUTHENTICATED_USER" => Results.Json(new
                {
                    code = importResult.Code,
                    message = "The request requires an authenticated user."
                }, statusCode: StatusCodes.Status401Unauthorized),
                _ => Results.BadRequest(new
                {
                    code = importResult.Code,
                    message = "Import command execution failed."
                })
            };
        }

        return Results.Ok(new
        {
            importedAccounts = importResult.Value.ImportedAccounts,
            importedTransactions = importResult.Value.ImportedTransactions,
            ignoredTransactions = importResult.Value.IgnoredTransactions,
        });
    }
}
