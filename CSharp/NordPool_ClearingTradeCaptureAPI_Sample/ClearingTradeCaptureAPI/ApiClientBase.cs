namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Thinktecture.IdentityModel.Client;

    public class ApiClientBase
    {
        protected static bool GetToken(out TokenResponse tokenResponse)
        {
            string clientId = GetAppSettingValue("IdentityServer_ClientId");
            string clientSecret = GetAppSettingValue("IdentityServer_ClientSecret");
            string scope = GetAppSettingValue("IdentityServer_Scope");

            string userName = GetAppSettingValue("Member_UserName");
            string password = GetAppSettingValue("Member_Password");

            string tokenRequestUrl = "https://sts.nordpoolgroupppe.com/connect/token";

            Console.WriteLine("Requesting token from URL: " + tokenRequestUrl);

            OAuth2Client oAuth2Client = new OAuth2Client(new Uri(tokenRequestUrl), clientId, clientSecret, OAuth2Client.ClientAuthenticationStyle.PostValues);

            Task<TokenResponse> tokenRequestTask = oAuth2Client.RequestResourceOwnerPasswordAsync(userName, password, scope);
            tokenResponse = tokenRequestTask.Result;

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

            Console.WriteLine("Requesting token SUCCEEDED");
            return true;
        }

        protected static void PrintTradesOnConsole(List<Trade> trades)
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
    }
}