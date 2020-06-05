using Decisions.MSOneDrive;
using Decisons.MSOneDrive.TestSuite;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decisons.MSOneDrive.TestSuite
{
    [TestClass]
    public class ModuleTest
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

        [TestMethod]
        public void GetFolderList()
        {
            var rootFolders = ModuleUtility.GetFolders(client, null);
            Assert.IsTrue(rootFolders.IsSucceed);

            foreach (OneDriveFolder dir in rootFolders.Data)
            {
                var cntFile = ModuleUtility.GetFiles(client, dir.Id);
                Assert.IsTrue(cntFile.IsSucceed);

                var cntFolder = ModuleUtility.GetFolders(client, dir.Id);
                Assert.IsTrue(cntFolder.IsSucceed);
            }
        }

        [TestMethod]
        public void GetFileList()
        {
            var rootFolders = ModuleUtility.GetFiles(client, null);
            Assert.IsTrue(rootFolders.IsSucceed);
        }

        [TestMethod]
        public void Delete()
        {
            var delFolder = ModuleUtility.CreateFolder(client, TestData.FolderToDelete, null);
            var delRes = ModuleUtility.DeleteResource(client, delFolder.Data.Id);
            Assert.IsTrue(delRes.IsSucceed);

            var invalidDelRes = ModuleUtility.DeleteResource(client, delFolder.Data.Id);
            Assert.IsFalse(invalidDelRes.IsSucceed);

            var incorrectDelRes2=ModuleUtility.DeleteResource(client, "incorrectId");
            Assert.IsFalse(incorrectDelRes2.IsSucceed);
        }

        [TestMethod]
        public void CreateFolder()
        {
            OneDriveResultWithData<OneDriveFolder> parentFolder = null;
            try
            {
                parentFolder = ModuleUtility.CreateFolder(client, TestData.FolderToDelete, null );
                Assert.IsTrue(parentFolder.IsSucceed);
                Assert.AreEqual(parentFolder.Data.Name, TestData.FolderToDelete);

                var childFolder = ModuleUtility.CreateFolder(client, TestData.FolderToDelete, parentFolder.Data.Id);
                Assert.IsNotNull(childFolder.IsSucceed);
            }
            finally
            { 
                 ModuleUtility.DeleteResource(client, parentFolder.Data.Id);
            }
        }

        [TestMethod]
        public void DoesExist()
        {
            OneDriveResultWithData<OneDriveFolder> folder = null;

            try
            {
                folder = ModuleUtility.CreateFolder(client, TestData.FolderToDelete, null);
                Assert.IsTrue(folder.IsSucceed);

                var folderRes = ModuleUtility.DoesResourceExist(client, folder.Data.Id);
                Assert.AreEqual(OneDriveResourceType.Folder, folderRes.Data);

                var fileRes = ModuleUtility.DoesResourceExist(client, "40CAD082E16C9AAE!123");//FIXME
                Assert.AreEqual(OneDriveResourceType.File, fileRes.Data);

                ModuleUtility.DeleteResource(client, folder.Data.Id);
                var invalidFolderRes = ModuleUtility.DoesResourceExist(client, folder.Data.Id);
                Assert.AreEqual(OneDriveResourceType.Unavailable, invalidFolderRes.Data);

                var incorrectIdResult = ModuleUtility.DoesResourceExist(client, "incorrectId");
                Assert.AreEqual(OneDriveResourceType.Unavailable, incorrectIdResult.Data);
            }
            finally
            {
                ModuleUtility.DeleteResource(client, folder.Data.Id);
            }

        }
    }
}
