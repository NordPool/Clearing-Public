namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;

    using Newtonsoft.Json;

    using Thinktecture.IdentityModel.Client;

    public class RepeatedRequestClient : ApiClientBase
    {
        protected readonly HttpClient HttpClient;
        private TokenResponse tokenResponse;
        private DateTime tokenExpiration;
        private bool requestInProgress;

        public RepeatedRequestClient()
        {
            HttpClient = new HttpClient();
        }

        private bool RefreshToken()
        {
            // synchronize getting the token to make sure only one request thread gets token at the same time
            lock (this)
            {
                if (!GetToken(out tokenResponse))
                {
                    return false;
                }

                tokenExpiration = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                HttpClient.SetBearerToken(tokenResponse.AccessToken);
            }

            return true;
        }

        public void MakeRepeatedRequests()
        {
            Console.WriteLine("Making repeated requests, press Enter or CTRL-C to stop");

            if (!RefreshToken())
            {
                return;
            }

            Timer requestTimer = new Timer(20 * 1000);
            requestTimer.Elapsed += RequestTimerOnElapsed;
            requestTimer.Start();

            RequestTimerOnElapsed(null, null);

            Console.Read();
        }

        private void RequestTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            /* A new Elapsed event from the Timer might be triggered before the previous request has finished.
             * To not start a new request before the previous has finished, we implement synchronization logic
             * to skip making a request if one is already in progress.
             */ 

            bool thisThreadHasRequestInProgress = false;

            try
            {
                lock (this)
                {
                    if (requestInProgress)
                    {
                        Console.WriteLine("Request already in progress, not starting a new one");
                        return;
                    }
                    else
                    {
                        requestInProgress = true;
                        thisThreadHasRequestInProgress = true;
                    }
                }

                // If the token expiration time is reached, request a new token before making a request for trades
                if (DateTime.Now > tokenExpiration)
                {
                    Console.WriteLine("Token has expired, refreshing token");
                    RefreshToken();
                }

                List<Trade> trades = RequestTrades();

                PrintTradesOnConsoleAsTable(trades);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Unexpected exception: '{0}'", ex.Message);

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    sb.AppendFormat(" '{0}'", ex.Message);
                }

                Console.WriteLine(sb.ToString());
            }
            finally
            {
                if (thisThreadHasRequestInProgress)
                {
                    lock (this)
                    {
                        requestInProgress = false;
                    }
                }
            }
        }

        private List<Trade> RequestTrades(bool recursiveCall = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            HttpResponseMessage response = HandleRequest();
            string responseData = response.Content.ReadAsStringAsync().Result;

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                // Avoid more than one level of recursion here
                if (!recursiveCall && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Got 401 Unauthorized response, refreshing token");
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
            Console.WriteLine("Requesting trades SUCCEEDED in {0}ms, result contains {1} trades", stopwatch.ElapsedMilliseconds, trades.Count);
            return trades;
        }

        protected virtual HttpResponseMessage HandleRequest()
        {
            string deliveryDate = GetDeliveryDate();
            string requestUrl = GetAppSettingValue("TradeRequestBaseUrl") + deliveryDate;

            Console.WriteLine("Requesting trades from URL: " + requestUrl);
            Task<HttpResponseMessage> tradeRequestTask = HttpClient.GetAsync(requestUrl);

            return tradeRequestTask.Result;
        }
    }
}