using Monet.Application.ImportTransactions;
using Monet.Domain.Shared;

namespace Monet.WebApp.ImportTransactions;

public interface ITransactionImportFileParser
{
    bool CanParse(string fileName, string? contentType);
    Task<IResult<TransactionImport>> ParseAsync(Stream stream, CancellationToken cancellationToken);
}
