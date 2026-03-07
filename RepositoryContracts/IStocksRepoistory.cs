using Entities;

public interface IStocksRepository
{
    /// <summary>
    /// Inserts a new buy order into the database table called 'BuyOrders'.
    /// </summary>
    /// <param name="buyOrder">Buy order to insert</param>
    /// <returns>The inserted object of buy order</returns>
    Task<BuyOrder> CreateBuyOrderAsync(BuyOrder buyOrder);

    /// <summary>
    /// Inserts a new sell order into the database table called 'SellOrders'.
    /// </summary>
    /// <param name="sellOrder">Sell order to insert</param>
    /// <returns>The inserted object of sell order</returns>
    Task<SellOrder> CreateSellOrderAsync(SellOrder sellOrder);

    /// <summary>
    /// Returns the existing list of buy orders retrieved from database table called 'BuyOrders'.
    /// </summary>
    /// <returns>List of Buy Orders</returns>
    Task<List<BuyOrder>> GetBuyOrdersAsync();

    /// <summary>
    /// Returns the existing list of sell orders retrieved from database table called 'SellOrders'.
    /// </summary>
    /// <returns>List of Buy Orders</returns>
    Task<List<SellOrder>> GetSellOrdersAsync();
}
