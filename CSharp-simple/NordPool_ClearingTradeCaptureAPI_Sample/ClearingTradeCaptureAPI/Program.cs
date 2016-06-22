namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Thinktecture.IdentityModel.Client;

    public class Program
    {
        public static void Main(string[] args)
        {
            string clientId = GetAppSettingValue("IdentityServer_ClientId");
            string clientSecret = GetAppSettingValue("IdentityServer_ClientSecret");
            string scope = GetAppSettingValue("IdentityServer_Scope");

            string userName = GetAppSettingValue("Member_UserName");
            string password = GetAppSettingValue("Member_Password");

            // Always use HTTPS (not plain HTTP) when sending requests that contain your credentials!
            OAuth2Client oAuth2Client = new OAuth2Client(new Uri("https://sts.nordpoolgroupppe.com/connect/token"),
                clientId,
                clientSecret,
                OAuth2Client.ClientAuthenticationStyle.PostValues);

            Task<TokenResponse> tokenRequestTask = oAuth2Client.RequestResourceOwnerPasswordAsync(userName, password, scope);
            TokenResponse tokenResponse = tokenRequestTask.Result;

            HttpClient httpClient = new HttpClient();
            httpClient.SetBearerToken(tokenResponse.AccessToken);

            // Clearing API time parameters are always interpreted to be in UTC time
            string todayDate = DateTime.Now.Date.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm'Z'");
            Task<HttpResponseMessage> tradeRequestTask = 
                httpClient.GetAsync("https://apiclearing.test.nordpoolgroup.com/api/portfolios/trades?fromDeliveryHour=" + todayDate);
            HttpResponseMessage response = tradeRequestTask.Result;
            string responseData = response.Content.ReadAsStringAsync().Result;

            List<Trade> trades = JsonConvert.DeserializeObject<List<Trade>>(responseData);

            PrintTradesOnConsole(trades);
        }

        private static void PrintTradesOnConsole(List<Trade> trades)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            serializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            foreach (Trade trade in trades)
            {
                string formattedJsonTrade = JsonConvert.SerializeObject(trade, serializerSettings);
                Console.WriteLine(formattedJsonTrade);
                Console.WriteLine("Press enter for next trade");
                Console.ReadLine();
            }
        }

        public static string GetAppSettingValue(string appSettingKey)
        {
            string value = ConfigurationManager.AppSettings[appSettingKey];
            if (string.IsNullOrEmpty(value))
            {
                string message = string.Format("Can not find value for appSetting key '{0}'.", appSettingKey);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }
    }
}