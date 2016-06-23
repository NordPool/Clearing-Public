namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Thinktecture.IdentityModel.Client;

    public class ApiClientBase
    {
        protected static bool MakeTokenRequest(out TokenResponse tokenResponse)
        {
            string clientId = GetAppSettingValue("IdentityServer_ClientId");
            string clientSecret = GetAppSettingValue("IdentityServer_ClientSecret");
            string scope = GetAppSettingValue("IdentityServer_Scope");

            string userName = GetAppSettingValue("Member_UserName");
            string password = GetAppSettingValue("Member_Password");

            // Always use HTTPS (not plain HTTP) when sending requests that contain your credentials!
            string tokenRequestUrl = "https://sts.nordpoolgroupppe.com/connect/token";

            Console.WriteLine("Requesting token from URL: " + tokenRequestUrl);

            OAuth2Client oAuth2Client = new OAuth2Client(new Uri(tokenRequestUrl), clientId, clientSecret, OAuth2Client.ClientAuthenticationStyle.PostValues);

            Stopwatch stopwatch = Stopwatch.StartNew();

            Task<TokenResponse> tokenRequestTask = oAuth2Client.RequestResourceOwnerPasswordAsync(userName, password, scope);
            tokenResponse = tokenRequestTask.Result;

            stopwatch.Stop();

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Requesting token FAILED with error '{0}'", tokenResponse.Error);
                if (tokenResponse.Error == "invalid_client")
                {
                    Console.WriteLine("Remember to fill in correct 'IdentityServer_ClientId' and 'IdentityServer_ClientSecret' values in App.config");
                }
                if (tokenResponse.Error == "invalid_grant")
                {
                    Console.WriteLine("Remember to fill in correct 'Member_UserName' and 'Member_Password' values in App.config");
                }

                Console.ReadLine();
                return false;
            }

            Console.WriteLine("Requesting token SUCCEEDED in {0}ms", stopwatch.ElapsedMilliseconds);
            return true;
        }

        protected static void PrintTradesOnConsoleOneByOne(IEnumerable<Trade> trades)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            serializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            foreach (Trade trade in trades)
            {
                string formattedJsonTrade = JsonConvert.SerializeObject(trade, serializerSettings);
                Console.WriteLine(formattedJsonTrade);
                Console.WriteLine("Press enter for next trade, CTRL-C to quit");
                Console.ReadLine();
            }
        }

        protected static void PrintTradesOnConsoleAsTable(IEnumerable<Trade> trades)
        {
            if (trades == null)
            {
                return;
            }

            Console.WriteLine("                   Port-    Short-                               Unit");
            Console.WriteLine("Delivery start     folio    name    Market  Area        Qty      price          Amount");
            Console.WriteLine("--------------------------------------------------------------------------------------");

            foreach (Trade trade in trades)
            {
                Console.WriteLine("{0}  {1,-7}  {2,-7}  {3,-6}  {4, -4}  {5,9:N1}  {6,9:N2}  {7,14:N2}",
                    trade.DeliveryStartTime.ToString("yyyy-MM-dd HH:mm"),
                    trade.Portfolio,
                    trade.Portfolio,
                    trade.Market,
                    trade.Area,
                    trade.Quantity,
                    trade.UnitPrice,
                    trade.Amount);
            }
        }

        /// <summary>
        /// Either gets the value from a configuration parameter (for testing purposes), or if the configuration
        /// value doesn't exist, gets the start of today's delivery day in UTC time.
        /// </summary>
        /// <returns></returns>
        protected static string GetDeliveryDate()
        {
            string deliveryDateConfig = GetAppSettingValue("DeliveryDate", true);
            DateTime deliveryDate;
            if (!string.IsNullOrEmpty(deliveryDateConfig) && DateTime.TryParse(deliveryDateConfig, out deliveryDate))
            {
                return deliveryDateConfig;
            }

            string startOfDeliveryDayToday = GetStartOfDeliveryDayToday();
            return startOfDeliveryDayToday;
        }

        protected static string GetAppSettingValue(string appSettingKey, bool canBeEmpty = false)
        {
            string value = ConfigurationManager.AppSettings[appSettingKey];
            if (!canBeEmpty && string.IsNullOrEmpty(value))
            {
                string message = string.Format("Can not find value for appSetting key '{0}'.", appSettingKey);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }

        protected static string GetStartOfDeliveryDayToday()
        {
            // Clearing API time parameters are always interpreted to be in UTC time
            return DateTime.Now.Date.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm'Z'");
        }

        protected static void CheckProtocol(string requestUrl)
        {
            if (!requestUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationErrorsException("Only use HTTPS protocol for requests, not plain HTTP");
            }
        }

        protected static void ReportExceptionOnConsole(Exception ex)
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
    }
}