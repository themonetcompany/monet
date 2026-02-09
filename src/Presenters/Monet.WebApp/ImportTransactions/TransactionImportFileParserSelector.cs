using Monet.Domain.Shared;

namespace Monet.WebApp.ImportTransactions;

public class TransactionImportFileParserSelector(IEnumerable<ITransactionImportFileParser> parsers) : ITransactionImportFileParserSelector
{
    public IResult<ITransactionImportFileParser> Resolve(string fileName, string? contentType)
    {
        var parser = parsers.FirstOrDefault(candidate => candidate.CanParse(fileName, contentType));

        return parser is null
            ? Result<ITransactionImportFileParser>.Failure("UNSUPPORTED_IMPORT_FILE_FORMAT")
            : Result<ITransactionImportFileParser>.Success(parser);
    }
}
