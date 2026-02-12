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
        result.Value.Accounts[0].Balances.Should().BeEmpty();
        result.Value.Transactions.Should().ContainSingle();
        result.Value.Transactions[0].TransactionId.Should().Be("TX-001");
        result.Value.Transactions[0].Amount.Should().Be(-12.34m);
        result.Value.Transactions[0].Currency.Should().Be("EUR");
        result.Value.Transactions[0].AccountNumber.Should().Be("FR761234567890");
        result.Value.Transactions[0].Description.Should().Be("Coffee shop");
        result.Value.Transactions[0].Date.Should().Be(new DateTimeOffset(2026, 2, 6, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task ParseAsync_OfxWithDebitTypeAndPositiveAmount_ShouldMapAmountAsNegative()
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
                                       <TRNTYPE>DEBIT
                                       <TRNAMT>12.34
                                       <DTPOSTED>20260206120000
                                       <FITID>TX-002
                                       <NAME>Supermarket
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
        result.Value.Transactions.Should().ContainSingle();
        result.Value.Transactions[0].Amount.Should().Be(-12.34m);
    }

    [Fact]
    public async Task ParseAsync_OfxWithTwoStatementResponses_ShouldKeepTransactionAccountPerStatement()
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
                                     <ACCTID>FR111111111111
                                   </BANKACCTFROM>
                                   <BANKTRANLIST>
                                     <STMTTRN>
                                       <TRNAMT>-10.00
                                       <DTPOSTED>20260201120000
                                       <FITID>TX-100
                                       <NAME>Txn account 1
                                     </STMTTRN>
                                   </BANKTRANLIST>
                                 </STMTRS>
                               </STMTTRNRS>
                               <STMTTRNRS>
                                 <STMTRS>
                                   <CURDEF>EUR
                                   <BANKACCTFROM>
                                     <ACCTID>FR222222222222
                                   </BANKACCTFROM>
                                   <BANKTRANLIST>
                                     <STMTTRN>
                                       <TRNAMT>-20.00
                                       <DTPOSTED>20260202120000
                                       <FITID>TX-200
                                       <NAME>Txn account 2
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
        result.Value.Accounts.Select(account => account.AccountNumber)
            .Should()
            .BeEquivalentTo(["FR111111111111", "FR222222222222"]);
        result.Value.Transactions.Should().HaveCount(2);
        result.Value.Transactions.Should().Contain(transaction =>
            transaction.TransactionId == "TX-100" && transaction.AccountNumber == "FR111111111111");
        result.Value.Transactions.Should().Contain(transaction =>
            transaction.TransactionId == "TX-200" && transaction.AccountNumber == "FR222222222222");
    }

    [Fact]
    public async Task ParseAsync_OfxWithLedgerBalance_ShouldMapBalanceInImportedAccount()
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
                                       <TRNAMT>-10.00
                                       <DTPOSTED>20260201120000
                                       <FITID>TX-100
                                       <NAME>Txn
                                     </STMTTRN>
                                   </BANKTRANLIST>
                                   <LEDGERBAL>
                                     <BALAMT>1250.75
                                     <DTASOF>20260205120000
                                   </LEDGERBAL>
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
        var account = result.Value.Accounts[0];
        account.AccountNumber.Should().Be("FR761234567890");
        account.Balances.Should().ContainSingle();
        account.Balances[0].Amount.Should().Be(1250.75m);
        account.Balances[0].Currency.Should().Be("EUR");
        account.Balances[0].Date.Should().Be(new DateTimeOffset(2026, 2, 5, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task ParseAsync_OfxWithAvailableBalance_ShouldMapBalanceInImportedAccount()
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
                                       <TRNAMT>-10.00
                                       <DTPOSTED>20260201120000
                                       <FITID>TX-100
                                       <NAME>Txn
                                     </STMTTRN>
                                   </BANKTRANLIST>
                                   <AVAILBAL>
                                     <BALAMT>1250.75
                                     <DTASOF>20260205120000
                                   </AVAILBAL>
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
        var account = result.Value.Accounts[0];
        account.AccountNumber.Should().Be("FR761234567890");
        account.Balances.Should().ContainSingle();
        account.Balances[0].Amount.Should().Be(1250.75m);
        account.Balances[0].Currency.Should().Be("EUR");
        account.Balances[0].Date.Should().Be(new DateTimeOffset(2026, 2, 5, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task ParseAsync_OfxWithTwoStatementResponsesAndLedgerBalances_ShouldMapOneBalancePerAccount()
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
                                     <ACCTID>FR111111111111
                                   </BANKACCTFROM>
                                   <BANKTRANLIST>
                                     <STMTTRN>
                                       <TRNAMT>-10.00
                                       <DTPOSTED>20260201120000
                                       <FITID>TX-100
                                       <NAME>Txn account 1
                                     </STMTTRN>
                                   </BANKTRANLIST>
                                   <LEDGERBAL>
                                     <BALAMT>500.00
                                     <DTASOF>20260205120000
                                   </LEDGERBAL>
                                 </STMTRS>
                               </STMTTRNRS>
                               <STMTTRNRS>
                                 <STMTRS>
                                   <CURDEF>EUR
                                   <BANKACCTFROM>
                                     <ACCTID>FR222222222222
                                   </BANKACCTFROM>
                                   <BANKTRANLIST>
                                     <STMTTRN>
                                       <TRNAMT>-20.00
                                       <DTPOSTED>20260202120000
                                       <FITID>TX-200
                                       <NAME>Txn account 2
                                     </STMTTRN>
                                   </BANKTRANLIST>
                                   <LEDGERBAL>
                                     <BALAMT>900.00
                                     <DTASOF>20260206120000
                                   </LEDGERBAL>
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
        result.Value.Accounts.Should().HaveCount(2);

        var firstAccount = result.Value.Accounts.Single(account => account.AccountNumber == "FR111111111111");
        firstAccount.Balances.Should().ContainSingle();
        firstAccount.Balances[0].Amount.Should().Be(500m);
        firstAccount.Balances[0].Date.Should().Be(new DateTimeOffset(2026, 2, 5, 0, 0, 0, TimeSpan.Zero));

        var secondAccount = result.Value.Accounts.Single(account => account.AccountNumber == "FR222222222222");
        secondAccount.Balances.Should().ContainSingle();
        secondAccount.Balances[0].Amount.Should().Be(900m);
        secondAccount.Balances[0].Date.Should().Be(new DateTimeOffset(2026, 2, 6, 0, 0, 0, TimeSpan.Zero));
    }
}
