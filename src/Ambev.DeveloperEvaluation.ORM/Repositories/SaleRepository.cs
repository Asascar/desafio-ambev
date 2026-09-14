using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// EF Core implementation of the ISaleRepository.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of SaleRepository.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <inheritdoc />
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string? order = null,
        string? customer = null,
        string? branch = null,
        SaleStatus? status = null,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.Items)
            .AsNoTracking();

        // Filtering
        if (!string.IsNullOrWhiteSpace(customer))
        {
            var customerFilter = customer.Trim().ToLower();
            query = query.Where(s =>
                s.CustomerName.ToLower().Contains(customerFilter) ||
                s.CustomerId.ToString().ToLower().Contains(customerFilter));
        }

        if (!string.IsNullOrWhiteSpace(branch))
        {
            var branchFilter = branch.Trim().ToLower();
            query = query.Where(s =>
                s.BranchName.ToLower().Contains(branchFilter) ||
                s.BranchId.ToString().ToLower().Contains(branchFilter));
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (minDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= minDate.Value);
        }

        if (maxDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= maxDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Ordering
        query = ApplyOrdering(query, order);

        // Pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
        {
            return query.OrderByDescending(s => s.SaleDate);
        }

        // Clean order expression (handle possible quotes)
        var cleaned = order.Trim('\"', '\'').Trim();
        var parts = cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries);
        bool isFirst = true;
        IOrderedQueryable<Sale>? orderedQuery = null;

        foreach (var part in parts)
        {
            var segments = part.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = segments[0].ToLowerInvariant();
            var isDescending = segments.Length > 1 && segments[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            orderedQuery = (field, isFirst, isDescending) switch
            {
                ("salenumber", true, false) => query.OrderBy(s => s.SaleNumber),
                ("salenumber", true, true) => query.OrderByDescending(s => s.SaleNumber),
                ("salenumber", false, false) => orderedQuery!.ThenBy(s => s.SaleNumber),
                ("salenumber", false, true) => orderedQuery!.ThenByDescending(s => s.SaleNumber),

                ("totalamount", true, false) => query.OrderBy(s => s.TotalAmount),
                ("totalamount", true, true) => query.OrderByDescending(s => s.TotalAmount),
                ("totalamount", false, false) => orderedQuery!.ThenBy(s => s.TotalAmount),
                ("totalamount", false, true) => orderedQuery!.ThenByDescending(s => s.TotalAmount),

                ("customername" or "customer", true, false) => query.OrderBy(s => s.CustomerName),
                ("customername" or "customer", true, true) => query.OrderByDescending(s => s.CustomerName),
                ("customername" or "customer", false, false) => orderedQuery!.ThenBy(s => s.CustomerName),
                ("customername" or "customer", false, true) => orderedQuery!.ThenByDescending(s => s.CustomerName),

                ("branchname" or "branch", true, false) => query.OrderBy(s => s.BranchName),
                ("branchname" or "branch", true, true) => query.OrderByDescending(s => s.BranchName),
                ("branchname" or "branch", false, false) => orderedQuery!.ThenBy(s => s.BranchName),
                ("branchname" or "branch", false, true) => orderedQuery!.ThenByDescending(s => s.BranchName),

                ("status", true, false) => query.OrderBy(s => s.Status),
                ("status", true, true) => query.OrderByDescending(s => s.Status),
                ("status", false, false) => orderedQuery!.ThenBy(s => s.Status),
                ("status", false, true) => orderedQuery!.ThenByDescending(s => s.Status),

                // default: date / saledate
                (_, true, false) => query.OrderBy(s => s.SaleDate),
                (_, true, true) => query.OrderByDescending(s => s.SaleDate),
                (_, false, false) => orderedQuery!.ThenBy(s => s.SaleDate),
                (_, false, true) => orderedQuery!.ThenByDescending(s => s.SaleDate)
            };

            isFirst = false;
        }

        return orderedQuery ?? query.OrderByDescending(s => s.SaleDate);
    }
}
