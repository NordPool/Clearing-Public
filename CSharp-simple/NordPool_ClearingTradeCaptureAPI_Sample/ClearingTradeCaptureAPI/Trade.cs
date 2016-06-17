namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;

    public class Trade
    {
        public string ProductCode { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Commodity Commodity { get; set; }
        public DateTime TradeTime { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TradeId { get; set; }
        public TradeStatus TradeStatus { get; set; }
        public int OrderNo { get; set; }
        public BuySell BuySell { get; set; }
        public string Area { get; set; }
        public string Portfolio { get; set; }
        public string Market { get; set; }
        public string Label { get; set; }
        public string Ccp { get; set; }
        public DateTime DeliveryStartTime { get; set; }
        public DateTime DeliveryEndTime { get; set; }
        public OrderType OrderType { get; set; }
    }
}