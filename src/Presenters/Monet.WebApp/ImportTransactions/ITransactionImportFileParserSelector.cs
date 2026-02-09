using Monet.Domain.Shared;

namespace Monet.WebApp.ImportTransactions;

public interface ITransactionImportFileParserSelector
{
    IResult<ITransactionImportFileParser> Resolve(string fileName, string? contentType);
}
