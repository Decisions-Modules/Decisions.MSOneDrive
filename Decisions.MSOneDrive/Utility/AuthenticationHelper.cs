using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Decisions.MSOneDrive
{
    public static class AuthenticationHelper
    {
        public static GraphServiceClient GetAuthenticatedClient(string accessToken)
        {
                    var graphClient = new GraphServiceClient(
                        "https://graph.microsoft.com/v1.0",
                        new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                //var token = @"EwBgA8l6BAAUO9chh8cJscQLmU+LSWpbnr0vmwwAAaw4AYML2zfBGBqJqWT54yajqK0Tf0EqVNtRnyRX6+CUlnJp0HuzM/4CeHTA0phDf86ln5bdXXzyTbMnK9wfa5ypTD4bOGamLfnaPInx8gV8ISoQCPYUB0I7nzrtD5xlJEG5DEy2KWDCx30pbzX31uXqdLorX3CdoXijDbq80nMSNLyDdNiIGy6c9SniY7kZLc5iABxWdJb3iLSCd4qNHMDHvcQmiKpU+AvZ21Fx6A2dyMOQlFel7lD+2I7pvfasrmyGpvZo0eFxmhM47+0g/n8ubBErCfHcT21PfnZRz/yRMhtktp0YaRlVEcbsG5LKV3qKxZdIZp0GyO/u5nNdV4kDZgAACLVq0uJH8/eWMAJz4KKjcoL4dq0MsK31dSiOMdAVAJ6dTXEF1mxQ3MaGGkYYDkJekWhrERqM46c1mkjNvARCDUwf+EgkqjLX/a8beXFfh2a8/E6RaXNKbtztu4Rz1T1Zz3lcTbk8550ve6mgPI2oVjiY/W5veSEl7t0hJwi0T7chFVUwtUcBOA2AsET7vza2XkHTnHGnl+7wIwyI6r3O38HcUAxi4sE59A+jWEyW59xfnUxD67tiwCBiDUKXqkLkFAq/toQWXdXIHeFVN2SPakfdaYoS+QKGBGAk5lNOijcruCWBitJZIaSjvuprSZlEgW4G31ldjSdTaXT+/Xc1QVNoSkukAAtP/k5TfMGtseboGUVOOEWSu+4KwQEvXM9YDA8QY2JsGh0rcyWXZ7I3idkHPqBPM7D3gbuPhAsaAnLNdv6SwZl1U/FF9paLXt9WYo1fz2LeUNSMQE2ri4A3QyE00aOIwRdkgLcESQDp9MfK2KCnbkqwOyklBWcP4d8yn2x8cgzBJRKC3HMOfaPR8Fz9CNPV5xzAUTQ5QAyd4ikztpuJsFtp7ecDGwcd3BcLYEeD5PuenNGXC8Yod6SKjp8V/k4RLOi4URApomXjG3XLXs8YKnK6AAHAwTZEpdJ9DCT5WajhxNWnVx5Ki/IE/0up4HYTlK8BbrnQqD8UbmKsAwicOX6zI0L6CdlJXTPml78a1sE3wjqELPD6NfUj35Pg9i3bZu8zmCn+Ctf8ktL8+Ott6nFPgJn5x3QC";
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);                                // This header has been added to identify our sample in the Microsoft Graph service.  If extracting this code for your project please remove.
                                //requestMessage.Headers.Add("SampleOneDriverLibID", "uwp-csharp-api");

                            }));
                    return graphClient;
        }
    }
}
