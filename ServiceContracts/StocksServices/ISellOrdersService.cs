using System;
using Core.DTOs;

namespace ServiceContracts.StocksServices;

/// <summary>
/// Represents Stocks service that includes sell operations
/// </summary>
public interface ISellOrderService
{
    /// <summary>
    /// Create a Sell Order
    /// </summary>
    /// <param name="buyOrderRequest">Sell Order Object</param>
    /// <returns>Created sell Order</returns>
    Task<SellOrderResponse> CreateSellOrderAsync(SellOrderRequest? request);

    /// <summary>
    /// Returns all existing sell orders
    /// </summary>
    /// <returns>Returns a list of of all sell orders</returns>
    Task<List<SellOrderResponse>> GetSellOrdersAsync();
}
