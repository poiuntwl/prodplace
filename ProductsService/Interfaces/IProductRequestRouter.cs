namespace ProductsService.Interfaces;

public interface IProductRequestRouter
{
    Task<object?> RouteRequestAsync(object request, CancellationToken ct);
}