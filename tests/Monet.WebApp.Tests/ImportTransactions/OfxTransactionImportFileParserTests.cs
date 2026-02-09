using System.Text;
using FluentAssertions;
using Monet.WebApp.ImportTransactions;

namespace Monet.WebApp.Tests.ImportTransactions;

public class OfxTransactionImportFileParserTests
{
    [Fact]
    public async Task ParseAsync_ValidOfxFile_ReturnsTransactionImport()
    {
        // Arrange
        var parser = new OfxTransactionImportFileParser();
        const string ofx = """
                           <OFX>
                             <BANKMSGSRSV1>
                               <STMTTRNRS>
                                 <STMTRS>
                                   <CURDEF>EUR
                                   <BANKACCTFROM>
                                     <ACCTID>FR761234567890
                                   </BANKACCTFROM>
                                   <BANKTRANLIST>
                                     <STMTTRN>
                                       <TRNAMT>-12.34
                                       <DTPOSTED>20260206120000
                                       <FITID>TX-001
                                       <NAME>Coffee shop
                                     </STMTTRN>
                                   </BANKTRANLIST>
                                 </STMTRS>
                               </STMTTRNRS>
                             </BANKMSGSRSV1>
                           </OFX>
                           """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ofx));

        // Act
        var result = await parser.ParseAsync(stream, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Accounts.Should().ContainSingle();
        result.Value.Accounts[0].AccountNumber.Should().Be("FR761234567890");
        result.Value.Accounts[0].Name.Should().Be("FR761234567890");
        result.Value.Transactions.Should().ContainSingle();
        result.Value.Transactions[0].TransactionId.Should().Be("TX-001");
        result.Value.Transactions[0].Amount.Should().Be(-12.34m);
        result.Value.Transactions[0].Currency.Should().Be("EUR");
        result.Value.Transactions[0].AccountNumber.Should().Be("FR761234567890");
        result.Value.Transactions[0].Description.Should().Be("Coffee shop");
        result.Value.Transactions[0].Date.Should().Be(new DateTimeOffset(2026, 2, 6, 0, 0, 0, TimeSpan.Zero));
    }
}
