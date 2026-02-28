using System;
using ServiceContracts.DTOs;

namespace ServiceContracts.Interfaces;

/// <summary>
/// Represents Stocks service that includes operations like buy order, sell order
/// </summary>
public interface IStocksService
{

    /// <summary>
    /// Create a Buy Order 
    /// </summary>
    /// <param name="request">Buy Order Object</param>
    /// <returns>Created buy Order</returns>
    Task<BuyOrderResponse> CreateBuyOrderAsync(BuyOrderRequest request);

    /// <summary>
    /// Create a Sell Order 
    /// </summary>
    /// <param name="buyOrderRequest">Sell Order Object</param>
    /// <returns>Created sell Order</returns>

    Task<SellOrderResponse> CreateSellOrderAsync(SellOrderRequest request);


    /// <summary>
    /// Returns all existing buy orders
    /// </summary>
    /// <returns>Returns a list of of all buy orders</returns>
    Task<List<BuyOrderResponse>> GetBuyOrdersAsync();

    /// <summary>
    /// Returns all existing sell orders
    /// </summary>
    /// <returns>Returns a list of of all sell orders</returns>
    Task<List<SellOrderResponse>> GetSellOrdersAsync();
}
