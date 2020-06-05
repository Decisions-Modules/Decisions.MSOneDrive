using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
    public static class ModuleUtility
    {
        private static OneDriveBaseResult ProcessRequest(System.Action action)
        {
            var result = new OneDriveBaseResult();
            try
            {
                action();
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

        private static OneDriveResultWithData<T> ProcessRequest<T>(System.Func<T> req)
        {
            var result = new OneDriveResultWithData<T>();
            try
            {
                result.Data = req();
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

        public static OneDriveResultWithData<OneDriveResourceType> DoesResourceExist(GraphServiceClient connection, string fileOrFolderId)
        {
            OneDriveResultWithData<OneDriveResourceType> result = ProcessRequest(() =>
            {
                DriveItem item = CoreUtility.GetResourceInfo(connection, fileOrFolderId);
                if (item.Folder != null)
                    return OneDriveResourceType.Folder;
                else
                    return OneDriveResourceType.File;
            });

            if (!result.IsSucceed && result.ErrorInfo.HttpErrorCode == HttpStatusCode.NotFound)
            {
                result.Data = OneDriveResourceType.Unavailable;
                result.IsSucceed = true;
            };
            return result;
        }

        public static OneDriveBaseResult DeleteResource(GraphServiceClient connection, string fileOrFolderId)
        {
            return ProcessRequest(() =>
            {
                CoreUtility.DeleteFilebyID(connection, fileOrFolderId);
            });
        }

        public static OneDriveResultWithData<OneDriveFile[]> GetFiles(GraphServiceClient connection, string folderId)
        {
            OneDriveResultWithData<OneDriveFile[]> result = ProcessRequest(() =>
            {
                var rawList = CoreUtility.ListFolderFromId(connection, folderId, ItemType.File);
                var list = rawList.Select((it) => { return new OneDriveFile(it); });
                return list.ToArray();
            });

            return result;
        }

        public static OneDriveResultWithData<OneDriveFolder[]> GetFolders(GraphServiceClient connection, string folderId)
        {
            OneDriveResultWithData<OneDriveFolder[]> result = ProcessRequest(() =>
            {
                var rawList = CoreUtility.ListFolderFromId(connection, folderId, ItemType.Folder);
                var list = rawList.Select((it) => { return new OneDriveFolder(it); });
                return list.ToArray();
            });

            return result;
        }

        public static OneDriveResultWithData<OneDriveFolder> CreateFolder(GraphServiceClient connection, string newFoldeName, string parentFolderId)
        {
            OneDriveResultWithData<OneDriveFolder> result = ProcessRequest(() =>
            {
                var item = CoreUtility.CreateFolder(connection, newFoldeName, parentFolderId);
                return new OneDriveFolder(item);
            });
            return result;
        }






        /*public static GoogleDriveResultWithData<GoogleDriveFolder> CreateFolder(Connection connection, string folderName, string parentFolderId = null)
            public static GoogleDriveBaseResult DownloadFile(Connection connection, string fileId, string localFilePath, Action<IDownloadProgress> progressTracker = null)
            public static GoogleDriveResultWithData<GoogleDriveFile> UploadFile(Connection connection, string localFilePath, string fileName = null, string parentFolderId = null, Action<IUploadProgress> progessUpdate = null)
            public static GoogleDriveResultWithData<GoogleDriveResourceType> DoesResourceExist(Connection connection, string fileOrFolderId)
            public static GoogleDriveBaseResult DeleteResource(Connection connection, string fileOrFolderId)
            public static GoogleDriveResultWithData<GoogleDrivePermission> SetResourcePermissions(Connection connection, string fileOrFolderId, GoogleDrivePermission permission)
            public static GoogleDriveResultWithData<GoogleDrivePermission[]> GetResourcePermissions(Connection connection, string fileOrFolderId)*/
    }
}
