using Monet.Application.Ports;
using Monet.Application.Transactions;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Infrastructure.Projections.InMemory;

public class CategoryProjection : IProjection, IProvideTransactionCategories
{
    private readonly List<CategoryReadModel> _categories = [];

    public Task ApplyAsync(IDomainEvent domainEvent)
    {
        if (domainEvent is not CategoryCreated created)
        {
            return Task.CompletedTask;
        }

        var existingIndex = _categories.FindIndex(category => category.CategoryId == created.AggregateId);
        var model = new CategoryReadModel
        {
            CategoryId = created.AggregateId,
            Name = created.Name,
            FlowType = created.FlowType.ToString(),
        };

        if (existingIndex >= 0)
        {
            _categories[existingIndex] = model;
        }
        else
        {
            _categories.Add(model);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CategoryReadModel>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<CategoryReadModel>>(
            _categories
                .OrderBy(category => category.FlowType)
                .ThenBy(category => category.Name)
                .ToList());
    }

    public Task<CategoryReadModel?> GetByIdAsync(string categoryId)
    {
        return Task.FromResult(_categories.FirstOrDefault(category => category.CategoryId == categoryId));
    }
}
