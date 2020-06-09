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
    public class ModuleTests
    {
        private GraphServiceClient connection;
        private string TestFileFullName { get { return TestData.LocalTestDir + TestData.TestFileName; } }

        private OneDriveFolder testFolder;

        [TestInitialize]
        public void InitTests()
        {
            connection = TestAuthenticationHelper.GetAuthenticatedClient();

            const int LINE_COUNT = 3;
            var stream = new System.IO.StreamWriter(TestFileFullName);
            for (int i = 0; i < LINE_COUNT; i++)
                stream.Write($"{i}qwertyuiop\n");
            stream.Close();

            testFolder = OneDriveUtility.CreateFolder(connection, TestData.TestFolderName, null)?.Data;
        }

        [TestCleanup]
        public void CleanupTests()
        {
            System.IO.File.Delete(TestFileFullName);
            if (testFolder != null)
                OneDriveUtility.DeleteResource(connection, testFolder.Id);
        }

        [TestMethod]
        public void GetFolderListTest()
        {
            var rootFolders = OneDriveUtility.GetFolders(connection, null);
            Assert.IsTrue(rootFolders.IsSucceed);

            foreach (OneDriveFolder dir in rootFolders.Data)
            {
                var cntFile = OneDriveUtility.GetFiles(connection, dir.Id);
                Assert.IsTrue(cntFile.IsSucceed);

                var cntFolder = OneDriveUtility.GetFolders(connection, dir.Id);
                Assert.IsTrue(cntFolder.IsSucceed);
            }
        }

        [TestMethod]
        public void GetFileListTest()
        {
            var rootFolders = OneDriveUtility.GetFiles(connection, null);
            Assert.IsTrue(rootFolders.IsSucceed);
        }

        [TestMethod]
        public void GetLongFileListTest()
        {
            const int FILE_COUNT = 210;
            List<OneDriveFile> fileList = new List<OneDriveFile>();

            try
            {
                for (int i = 0; i < FILE_COUNT; i++)
                {
                    var it = OneDriveUtility.UploadFile(connection, TestFileFullName, $"delete_me{i}", testFolder.Id);
                    Assert.IsTrue(it.IsSucceed);
                    fileList.Add(it.Data);
                }

                var fileListResult = OneDriveUtility.GetFiles(connection, testFolder.Id);
                Assert.IsTrue(fileListResult.IsSucceed);
                Assert.AreEqual(FILE_COUNT, fileListResult.Data.Length);
            }
            finally {
                foreach (OneDriveFile it in fileList)
                {
                    OneDriveUtility.DeleteResource(connection, it.Id);
                }
            }
        }

        [TestMethod]
        public void Delete()
        {
            var delFolder = OneDriveUtility.CreateFolder(connection, TestData.FolderToDelete, null);
            var delRes = OneDriveUtility.DeleteResource(connection, delFolder.Data.Id);
            Assert.IsTrue(delRes.IsSucceed);

            var invalidDelRes = OneDriveUtility.DeleteResource(connection, delFolder.Data.Id);
            Assert.IsFalse(invalidDelRes.IsSucceed);

            var incorrectDelRes2 = OneDriveUtility.DeleteResource(connection, "incorrectId");
            Assert.IsFalse(incorrectDelRes2.IsSucceed);
        }

        [TestMethod]
        public void CreateFolder()
        {
            OneDriveResultWithData<OneDriveFolder> parentFolder = null;
            try
            {
                parentFolder = OneDriveUtility.CreateFolder(connection, TestData.FolderToDelete, null);
                Assert.IsTrue(parentFolder.IsSucceed);
                Assert.AreEqual(parentFolder.Data.Name, TestData.FolderToDelete);

                var childFolder = OneDriveUtility.CreateFolder(connection, TestData.FolderToDelete, parentFolder.Data.Id);
                Assert.IsNotNull(childFolder.IsSucceed);
            }
            finally
            {
                OneDriveUtility.DeleteResource(connection, parentFolder.Data.Id);
            }
        }

        [TestMethod]
        public void DoesExist()
        {
            try
            {
                var file = OneDriveUtility.UploadFile(connection, TestFileFullName, null, testFolder.Id);

                var fileRes = OneDriveUtility.DoesResourceExist(connection, file.Data.Id);
                Assert.AreEqual(OneDriveResourceType.File, fileRes.Data);

                var folderRes = OneDriveUtility.DoesResourceExist(connection, testFolder.Id);
                Assert.AreEqual(OneDriveResourceType.Folder, folderRes.Data);

                OneDriveUtility.DeleteResource(connection, file.Data.Id);
                var invalidFolderRes = OneDriveUtility.DoesResourceExist(connection, file.Data.Id);
                Assert.AreEqual(OneDriveResourceType.Unavailable, invalidFolderRes.Data);

                var incorrectIdResult = OneDriveUtility.DoesResourceExist(connection, "incorrectId");
                Assert.AreEqual(OneDriveResourceType.Unavailable, incorrectIdResult.Data);
            }
            finally
            {

            }

        }

        [TestMethod]
        public void UploadTest()
        {
            OneDriveResultWithData<OneDriveFile> fileInRoot = null, file = null;
            try
            {
                fileInRoot = OneDriveUtility.UploadFile(connection, TestFileFullName, null, null);
                Assert.IsTrue(fileInRoot.IsSucceed);

                file = OneDriveUtility.UploadFile(connection, TestFileFullName, null, null);
                Assert.IsTrue(file.IsSucceed);
            }
            finally {
                if (fileInRoot?.Data != null )
                    OneDriveUtility.DeleteResource(connection, fileInRoot.Data.Id);
                if (file?.Data != null)
                    OneDriveUtility.DeleteResource(connection, file.Data.Id);
            }
        }

        [TestMethod]
        public void DownoadTest()
        {
            OneDriveResultWithData<OneDriveFile> file = null;
            string localFileName = TestFileFullName + "_";
            try
            {
                file = OneDriveUtility.UploadFile(connection, TestFileFullName, null, null);
                Assert.IsTrue(file.IsSucceed);

                var result = OneDriveUtility.DownloadFile(connection, file.Data.Id, localFileName);
                Assert.IsTrue(result.IsSucceed);
            }
            finally
            {
                System.IO.File.Delete(localFileName);
                if (file != null && file.IsSucceed)
                    OneDriveUtility.DeleteResource(connection, file.Data.Id);
            }
        }


        [TestMethod]
        public void GetPermisionTest()
        {
            OneDriveResultWithData<OneDriveFile> file = null;
            try
            {
                file = OneDriveUtility.UploadFile(connection, TestFileFullName, null, testFolder.Id);

                var filePermissionsResult = OneDriveUtility.GetPermissionList(connection, file.Data.Id);
                Assert.IsTrue(filePermissionsResult.IsSucceed);

                var folderPermissionsResult = OneDriveUtility.GetPermissionList(connection, testFolder.Id);
                Assert.IsTrue(filePermissionsResult.IsSucceed);
            }
            finally
            {
                if (file?.Data != null)
                    OneDriveUtility.DeleteResource(connection, file.Data.Id);
            }

        }

        [TestMethod]
        public void ShareLinkTest()
        {
            OneDriveResultWithData<OneDriveFile> file = null;
            try
            {
                file = OneDriveUtility.UploadFile(connection, TestFileFullName, null, testFolder.Id);

                var permissionsResult = OneDriveUtility.GetPermissionList(connection, file.Data.Id);
                Assert.IsTrue(permissionsResult.IsSucceed);

                var sharePermissionResults = new List<OneDriveResultWithData<OneDrivePermission>>();
                foreach (OneDriveShareType t in (OneDriveShareType[])Enum.GetValues(typeof(OneDriveShareType)))
                    foreach (OneDriveShareScope s in (OneDriveShareScope[])Enum.GetValues(typeof(OneDriveShareScope)))
                    {
                        var r = OneDriveUtility.CreateShareLink(connection, file.Data.Id, t, s);
                        sharePermissionResults.Add(r);
                        Assert.IsTrue(r.IsSucceed);
                    }

                var newPermissionsResult = OneDriveUtility.GetPermissionList(connection, file.Data.Id);
                Assert.IsTrue(newPermissionsResult.IsSucceed);
                Assert.IsTrue(newPermissionsResult.Data.Length > permissionsResult.Data.Length);
            }
            finally
            {
                if (file != null && file.IsSucceed)
                    OneDriveUtility.DeleteResource(connection, file.Data.Id);
            }

        }

        [TestMethod]
        public void DeletePermissionTest()
        {
            OneDriveResultWithData<OneDriveFile> file = null;
            try
            {
                file = OneDriveUtility.UploadFile(connection, TestFileFullName, null, testFolder.Id);

                var newPermissionResult = OneDriveUtility.CreateShareLink(connection, file.Data.Id,OneDriveShareType.View, OneDriveShareScope.Anonymous);

                var permissionsResult = OneDriveUtility.GetPermissionList(connection, file.Data.Id);

                var deleteResult=OneDriveUtility.DeletePermission(connection, file.Data.Id, newPermissionResult.Data.Id);
                Assert.IsTrue(deleteResult.IsSucceed);

                var newPermissionsResult = OneDriveUtility.GetPermissionList(connection, file.Data.Id);
                Assert.AreEqual(1, permissionsResult.Data.Length - newPermissionsResult.Data.Length);
            }
            finally
            {
                if (file?.Data != null )
                    OneDriveUtility.DeleteResource(connection, file.Data.Id);
            }

        }
    }
}
