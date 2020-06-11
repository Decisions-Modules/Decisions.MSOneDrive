﻿using System;
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
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);                                
                            }));
                    return graphClient;
        }
    }
}
