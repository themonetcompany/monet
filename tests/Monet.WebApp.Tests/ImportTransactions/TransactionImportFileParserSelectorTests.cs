using FluentAssertions;
using Monet.WebApp.ImportTransactions;

namespace Monet.WebApp.Tests.ImportTransactions;

public class TransactionImportFileParserSelectorTests
{
    [Fact]
    public void Resolve_OfxFileName_ReturnsOfxParser()
    {
        // Arrange
        ITransactionImportFileParser[] parsers = [new OfxTransactionImportFileParser()];
        var selector = new TransactionImportFileParserSelector(parsers);

        // Act
        var result = selector.Resolve("statement.ofx", "text/plain");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<OfxTransactionImportFileParser>();
    }

    [Fact]
    public void Resolve_UnsupportedFileFormat_ReturnsFailure()
    {
        // Arrange
        ITransactionImportFileParser[] parsers = [new OfxTransactionImportFileParser()];
        var selector = new TransactionImportFileParserSelector(parsers);

        // Act
        var result = selector.Resolve("statement.csv", "text/csv");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("UNSUPPORTED_IMPORT_FILE_FORMAT");
    }
}
