using System;
using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTOs;

/// <summary>
/// DTO class that represents a Sell order  - that can be used while inserting / updating
/// </summary>
public class SellOrderRequest : OrderRequest
{

  /// <summary>
  /// Converts the current object of SellOrderRequest into a new object of SellOrder type
  /// </summary>
  /// <returns>A new object of SellOrder class</returns>
  public SellOrder ToSellOrder()
  {
    return new SellOrder()
    {
      StockSymbol = StockSymbol,
      StockName = StockName,
      DateAndTimeOfOrder = DateAndTimeOfOrder,
      Price = Price,
      Quantity = Quantity
    };
  }
}

