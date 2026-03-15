using Core.DTOs;

namespace ServiceContracts.StocksServices;

/// <summary>
/// Represents Stocks service that includes buy operations
/// </summary>
public interface IBuyOrderService
{
    /// <summary>
    /// Create a Buy Order
    /// </summary>
    /// <param name="request">Buy Order Object</param>
    /// <returns>Created buy Order</returns>
    Task<BuyOrderResponse> CreateBuyOrderAsync(BuyOrderRequest? request);

    /// <summary>
    /// Returns all existing buy orders
    /// </summary>
    /// <returns>Returns a list of of all buy orders</returns>
    Task<List<BuyOrderResponse>> GetBuyOrdersAsync();
}
