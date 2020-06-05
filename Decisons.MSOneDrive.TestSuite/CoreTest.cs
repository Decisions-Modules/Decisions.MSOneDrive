using System;
using Decisions.MSOneDrive;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Decisons.MSOneDrive.TestSuite
{
    [TestClass]
    public class CoreTest
    {
        private GraphServiceClient client;

        [TestInitialize]
        public void InitTests()
        {
            client = TestAuthenticationHelper.GetAuthenticatedClient();
        }

        [TestCleanup]
        public void CleanupTests()
        {
        }




    }
}
