namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class DeltaSearchClient : RepeatedRequestClient
    {
        private string deltaTag;

        protected override HttpResponseMessage HandleRequest()
        {
            string startOfDeliveryDayToday = GetStartOfDeliveryDayToday();
            string requestUrl = GetAppSettingValue("DeltaTradeRequestBaseUrl") + startOfDeliveryDayToday;
            CheckProtocol(requestUrl);

            if (deltaTag != null)
            {
                requestUrl += "&lastDeltaTag=" + deltaTag;
            }

            Console.WriteLine("Requesting trades from URL: " + requestUrl);
            Task<HttpResponseMessage> tradeRequestTask = HttpClient.GetAsync(requestUrl);
            HttpResponseMessage responseMessage = tradeRequestTask.Result;

            deltaTag = null;
            IEnumerable<string> deltaTagValues;
            if (responseMessage.Headers.TryGetValues("delta-tag", out deltaTagValues))
            {
                List<string> deltaTagValuesList = deltaTagValues.ToList();
                if (deltaTagValuesList.Count == 1)
                {
                    deltaTag = deltaTagValuesList[0];
                }
            }

            return responseMessage;
        }
    }
}