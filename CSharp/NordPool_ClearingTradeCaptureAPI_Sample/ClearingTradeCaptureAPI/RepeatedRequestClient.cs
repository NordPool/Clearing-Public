namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Timers;

    using Newtonsoft.Json;

    using Thinktecture.IdentityModel.Client;

    public class RepeatedRequestClient : ApiClientBase
    {
        private readonly HttpClient httpClient;
        private readonly string requestUrl;
        private TokenResponse tokenResponse;
        private DateTime tokenExpiration;

        public RepeatedRequestClient()
        {
            httpClient = new HttpClient();

            string deliveryDate = GetDeliveryDate();
            requestUrl = GetAppSettingValue("TradeRequestBaseUrl") + deliveryDate;
        }

        private bool RefreshToken()
        {
            if (!GetToken(out tokenResponse))
            {
                return false;
            }

            tokenExpiration = DateTime.Now.AddMilliseconds(tokenResponse.ExpiresIn);
            httpClient.SetBearerToken(tokenResponse.AccessToken);

            return true;
        }

        public void MakeRepeatedRequests()
        {
            if (!RefreshToken())
            {
                return;
            }

            Timer requestTimer = new Timer(20 * 1000);
            requestTimer.Elapsed += RequestTimerOnElapsed;
            requestTimer.Start();

            RequestTimerOnElapsed(null, null);

            Console.WriteLine("Press [enter] to stop service....");
            Console.Read();
        }

        private void RequestTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // If the token expiration time is reached, request a new token before making a request for trades
            if (DateTime.Now > tokenExpiration)
            {
                RefreshToken();
            }

            List<Trade> trades = RequestTrades();

            PrintTradesOnConsoleAsTable(trades);
            Console.ReadLine();
        }

        private List<Trade> RequestTrades(bool recursiveCall = false)
        {
            Console.WriteLine("Requesting trades from URL: " + requestUrl);
            Task<HttpResponseMessage> tradeRequestTask = httpClient.GetAsync(requestUrl);
            HttpResponseMessage response = tradeRequestTask.Result;
            string responseData = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                // Avoid more than one level of recursion here
                if (!recursiveCall && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // If receiving 401 Unauthorized response from the API, refresh the token and make a new request
                    if (RefreshToken())
                    {
                        return RequestTrades(true);
                    }
                }

                Console.WriteLine("ERROR - status code '{0} {1}', contents '{2}'", (int)response.StatusCode, response.StatusCode, responseData);
                Console.ReadLine();
                return null;
            }

            List<Trade> trades = JsonConvert.DeserializeObject<List<Trade>>(responseData);
            Console.WriteLine("Requesting trades SUCCEEDED, result contains {0} trades", trades.Count);
            return trades;
        }
    }
}