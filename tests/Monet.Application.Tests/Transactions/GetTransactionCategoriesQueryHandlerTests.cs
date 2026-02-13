using FluentAssertions;
using Monet.Application.Ports;
using Monet.Application.Tests.Shared;
using Monet.Application.Transactions;
using Monet.Domain;

namespace Monet.Application.Tests.Transactions;

public class GetTransactionCategoriesQueryHandlerTests
{
    private readonly DeterministicAuthenticationGateway _authenticationGateway = new();
    private readonly StubCategoryProvider _categoryProvider = new();

    private GetTransactionCategoriesQueryHandler Handler => new(_categoryProvider, _authenticationGateway);

    [Fact]
    public async Task Given_AuthenticatedUser_When_Handle_Then_ShouldReturnCategories()
    {
        _authenticationGateway.ConnectedUser(new ConnectedUser
        {
            Id = Guid.NewGuid(),
            Email = "john.doe@acme.com",
        });

        _categoryProvider.Categories.Add(new CategoryReadModel
        {
            CategoryId = "Category-Expense-Alimentation",
            Name = "Alimentation",
            FlowType = "Expense",
        });

        var result = await Handler.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].CategoryId.Should().Be("Category-Expense-Alimentation");
    }

    [Fact]
    public async Task Given_NoAuthenticatedUser_When_Handle_Then_ShouldReturnFailure()
    {
        var result = await Handler.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("NON_AUTHENTICATED_USER");
    }

    private class StubCategoryProvider : IProvideTransactionCategories
    {
        public List<CategoryReadModel> Categories { get; } = [];

        public Task<IReadOnlyList<CategoryReadModel>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<CategoryReadModel>>(Categories.AsReadOnly());
        }

        public Task<CategoryReadModel?> GetByIdAsync(string categoryId)
        {
            return Task.FromResult(Categories.FirstOrDefault(category => category.CategoryId == categoryId));
        }
    }
}
