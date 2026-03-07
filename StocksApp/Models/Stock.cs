public class Stock
{
    public string? StockSymbol { get; set; }

    public string? StockName { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (obj is not Stock)
            return false;

        Stock other = (Stock)obj;
        return StockSymbol == other.StockSymbol && StockName == other.StockName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StockName, StockSymbol);
    }
}
