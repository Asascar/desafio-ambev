using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Sale aggregate operations.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Creates a new sale record in the database.
    /// </summary>
    /// <param name="sale">The sale to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created sale.</returns>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its unique identifier including its items.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sale if found, null otherwise.</returns>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by its business sale number including its items.
    /// </summary>
    /// <param name="saleNumber">The sale number to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sale if found, null otherwise.</returns>
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing sale in the repository.
    /// </summary>
    /// <param name="sale">The sale to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated sale.</returns>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sale by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the sale was deleted, false if not found.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of sales with filtering and ordering.
    /// </summary>
    /// <param name="pageNumber">Current page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="order">Ordering expression (e.g. 'saleDate desc', 'totalAmount asc').</param>
    /// <param name="customer">Optional customer filter (name or ID).</param>
    /// <param name="branch">Optional branch filter (name or ID).</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="minDate">Optional minimum date filter.</param>
    /// <param name="maxDate">Optional maximum date filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple with the list of sales and the total count.</returns>
    Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? order = null,
        string? customer = null,
        string? branch = null,
        SaleStatus? status = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        CancellationToken cancellationToken = default);
}
