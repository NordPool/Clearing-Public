namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    public enum OrderType
    {
        Block = 1,
        SingleHourly,
        Flexi,
        Limit,
        MM_Limit, // MarketMaker limit
        IOC, // Immediate or cancel
        FOK, // Fill or kill
        IBO // Iceberg order
    }

    public enum TradeStatus
    {
        Completed = 1,
        Cancelled
    }

    public enum Commodity
    {
        Electricity = 1
    }

    public enum BuySell
    {
        Buy = 1,
        Sell,
        Zero
    }
}