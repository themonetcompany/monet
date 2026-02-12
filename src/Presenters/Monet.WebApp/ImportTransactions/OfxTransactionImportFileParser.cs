using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Monet.Application.ImportTransactions;
using Monet.Domain.Shared;

namespace Monet.WebApp.ImportTransactions;

public partial class OfxTransactionImportFileParser : ITransactionImportFileParser
{
    [GeneratedRegex("<STMTRS>(.*?)</STMTRS>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StatementResponseRegex();

    [GeneratedRegex("<LEDGERBAL>(.*?)</LEDGERBAL>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex LedgerBalanceRegex();

    [GeneratedRegex("<AVAILBAL>(.*?)</AVAILBAL>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AvailableBalanceRegex();

    [GeneratedRegex("<STMTTRN>(.*?)</STMTTRN>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StatementRegex();

    [GeneratedRegex("<(?<tag>[A-Z0-9]+)>(?<value>[^<\\r\\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex TagRegex();

    public bool CanParse(string fileName, string? contentType)
    {
        if (fileName.EndsWith(".ofx", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.Contains("ofx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IResult<TransactionImport>> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result<TransactionImport>.Failure("EMPTY_IMPORT_FILE");
        }

        var currency = ReadTag(content, "CURDEF");
        if (string.IsNullOrWhiteSpace(currency))
        {
            return Result<TransactionImport>.Failure("OFX_CURRENCY_NOT_FOUND");
        }

        var statementResponses = StatementResponseRegex().Matches(content);

        if (statementResponses.Count == 0)
        {
            return Result<TransactionImport>.Failure("OFX_NO_TRANSACTION_FOUND");
        }

        var transactions = new List<TransactionImport.Transaction>();
        var accountBalances = new Dictionary<string, List<TransactionImport.Account.Balance>>(StringComparer.OrdinalIgnoreCase);

        foreach (Match statementResponse in statementResponses)
        {
            var statementContent = statementResponse.Groups[1].Value;
            var statementAccountNumber = ReadTag(statementContent, "ACCTID");
            if (!string.IsNullOrWhiteSpace(statementAccountNumber))
            {
                if (!accountBalances.TryGetValue(statementAccountNumber, out var balances))
                {
                    balances = [];
                    accountBalances[statementAccountNumber] = balances;
                }

                if (TryReadLedgerBalance(statementContent, currency, out var balance))
                    balances.Add(balance);
                else if(TryReadAvailableBalance(statementContent, currency, out var availableBalance))
                    balances.Add(availableBalance);
            }

            var statements = StatementRegex().Matches(statementContent);

            foreach (Match statement in statements)
            {
                var transaction = statement.Groups[1].Value;
                var transactionId = ReadTag(transaction, "FITID");
                var amountAsString = ReadTag(transaction, "TRNAMT");
                var postedDateAsString = ReadTag(transaction, "DTPOSTED");
                var transactionType = ReadTag(transaction, "TRNTYPE");
                var accountNumber = ReadTag(transaction, "ACCTID") ?? statementAccountNumber;
                var description = ReadTag(transaction, "NAME") ?? ReadTag(transaction, "MEMO") ?? transactionId;

                if (string.IsNullOrWhiteSpace(transactionId)
                    || string.IsNullOrWhiteSpace(amountAsString)
                    || string.IsNullOrWhiteSpace(postedDateAsString)
                    || string.IsNullOrWhiteSpace(accountNumber)
                    || string.IsNullOrWhiteSpace(description)
                    || !TryParseAmount(amountAsString, transactionType, out var amount)
                    || !TryParseDate(postedDateAsString, out var postedDate))
                {
                    return Result<TransactionImport>.Failure("OFX_INVALID_TRANSACTION");
                }

                transactions.Add(new TransactionImport.Transaction
                {
                    TransactionId = transactionId,
                    Amount = amount,
                    Date = postedDate,
                    Description = description,
                    AccountNumber = accountNumber,
                    Currency = currency,
                });
            }
        }

        if (transactions.Count == 0)
        {
            return Result<TransactionImport>.Failure("OFX_NO_TRANSACTION_FOUND");
        }

        var accounts = transactions
            .Select(transaction => transaction.AccountNumber)
            .Concat(accountBalances.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(accountNumber => new TransactionImport.Account
            {
                AccountNumber = accountNumber,
                Name = accountNumber,
                Balances = accountBalances.TryGetValue(accountNumber, out var balances)
                    ? [.. balances]
                    : [],
            })
            .ToArray();

        return Result<TransactionImport>.Success(new TransactionImport
        {
            Accounts = accounts,
            Transactions = [.. transactions]
        });
    }

    private static string? ReadTag(string content, string tag)
    {
        return TagRegex().Matches(content)
            .FirstOrDefault(match => string.Equals(match.Groups["tag"].Value, tag, StringComparison.OrdinalIgnoreCase))
            ?.Groups["value"].Value
            .Trim();
    }

    private static bool TryParseAmount(string rawValue, string? transactionType, out decimal amount)
    {
        var normalized = rawValue.Replace(',', '.');

        var hasAmount = decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out amount);

        if (!hasAmount)
        {
            return false;
        }

        var type = transactionType?.Trim().ToUpperInvariant();
        if (amount > 0 && type is "DEBIT" or "PAYMENT" or "DIRECTDEBIT" or "FEE" or "ATM" or "WITHDRAWAL")
        {
            amount *= -1;
        }

        return true;
    }

    private static bool TryParseDate(string rawValue, out DateTimeOffset date)
    {
        date = default;

        var digits = new string(rawValue.TakeWhile(char.IsDigit).ToArray());
        if (digits.Length < 8)
        {
            return false;
        }

        if (!int.TryParse(digits[..4], out var year)
            || !int.TryParse(digits.Substring(4, 2), out var month)
            || !int.TryParse(digits.Substring(6, 2), out var day))
        {
            return false;
        }

        try
        {
            date = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private static bool TryReadLedgerBalance(
        string statementContent,
        string currency,
        out TransactionImport.Account.Balance balance)
    {
        balance = default!;

        var ledgerBalanceMatch = LedgerBalanceRegex().Match(statementContent);
        if (!ledgerBalanceMatch.Success)
        {
            return false;
        }

        var ledgerBalanceContent = ledgerBalanceMatch.Groups[1].Value;
        var amountAsString = ReadTag(ledgerBalanceContent, "BALAMT");
        var dateAsString = ReadTag(ledgerBalanceContent, "DTASOF");

        if (string.IsNullOrWhiteSpace(amountAsString)
            || string.IsNullOrWhiteSpace(dateAsString)
            || !TryParseAmount(amountAsString, null, out var amount)
            || !TryParseDate(dateAsString, out var date))
        {
            return false;
        }

        balance = new TransactionImport.Account.Balance
        {
            Amount = amount,
            Date = date,
            Currency = currency
        };
        return true;
    }

    private static bool TryReadAvailableBalance(
        string statementContent,
        string currency,
        out TransactionImport.Account.Balance balance)
    {
        balance = default!;

        var ledgerBalanceMatch = AvailableBalanceRegex().Match(statementContent);
        if (!ledgerBalanceMatch.Success)
        {
            return false;
        }

        var ledgerBalanceContent = ledgerBalanceMatch.Groups[1].Value;
        var amountAsString = ReadTag(ledgerBalanceContent, "BALAMT");
        var dateAsString = ReadTag(ledgerBalanceContent, "DTASOF");

        if (string.IsNullOrWhiteSpace(amountAsString)
            || string.IsNullOrWhiteSpace(dateAsString)
            || !TryParseAmount(amountAsString, null, out var amount)
            || !TryParseDate(dateAsString, out var date))
        {
            return false;
        }

        balance = new TransactionImport.Account.Balance
        {
            Amount = amount,
            Date = date,
            Currency = currency
        };
        return true;
    }
}
