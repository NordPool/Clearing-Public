namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Thinktecture.IdentityModel.Client;

    public class SingleRequestClient : ApiClientBase
    {
        public void MakeSingleRequest()
        {
            TokenResponse tokenResponse;
            if (!GetToken(out tokenResponse))
            {
                return;
            }

            HttpClient httpClient = new HttpClient();
            httpClient.SetBearerToken(tokenResponse.AccessToken);

            string todayDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            string requestUrl = "https://apiclearing.test.nordpoolgroup.com/api/portfolios/trades?fromDeliveryHour=" + todayDate;

            Console.WriteLine("Requesting trades from URL: " + requestUrl);
            Task<HttpResponseMessage> tradeRequestTask = httpClient.GetAsync(requestUrl);
            HttpResponseMessage response = tradeRequestTask.Result;
            string responseData = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR - status code '{0} {1}', contents '{2}'", (int)response.StatusCode, response.StatusCode, responseData);
                Console.ReadLine();
                return;
            }

            List<Trade> trades = JsonConvert.DeserializeObject<List<Trade>>(responseData);
            Console.WriteLine("Requesting trades SUCCEEDED, result contains {0} trades", trades.Count);

            PrintTradesOnConsole(trades);
            Console.ReadLine();
        }
    }
}