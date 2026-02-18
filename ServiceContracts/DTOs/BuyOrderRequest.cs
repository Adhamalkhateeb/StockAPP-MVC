using Entities;

namespace ServiceContracts.DTOs;

/// <summary>
/// DTO class that represents a buy order to purchase the stocks - that can be used while inserting / updating
/// </summary>
public class BuyOrderRequest : OrderRequest
{
  /// <summary>
  /// Converts the current object of BuyOrderRequest into a new object of BuyOrder type
  /// </summary>
  /// <returns>A new object of BuyOrder class</returns>
  public BuyOrder ToBuyOrder()
  {
    return new BuyOrder
    {
      StockSymbol = StockSymbol,
      StockName = StockName,
      DateAndTimeOfOrder = DateAndTimeOfOrder,
      Price = Price,
      Quantity = Quantity
    };
  }
}
