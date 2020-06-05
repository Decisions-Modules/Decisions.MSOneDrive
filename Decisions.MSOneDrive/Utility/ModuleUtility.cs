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
        public static OneDriveResultWithData<OneDriveResourceType> DoesResourceExist(GraphServiceClient connection, string fileOrFolderId)
        {

            var rawResult = CoreUtility.GetResourceInfo(connection, fileOrFolderId);
            var result = new OneDriveResultWithData<OneDriveResourceType>(rawResult);

            if (rawResult.IsSucceed)
            {
                if (rawResult.Data.Folder != null) 
                    result.Data=OneDriveResourceType.Folder;
                else 
                    result.Data = OneDriveResourceType.File;
            }
            else
            if (rawResult.ErrorInfo.HttpErrorCode == HttpStatusCode.NotFound)
            {
                result.Data = OneDriveResourceType.Unavailable;
                result.IsSucceed = true;
            };

            return result;
        }

        public static OneDriveBaseResult DeleteResource(GraphServiceClient connection, string fileOrFolderId)
        {
            return CoreUtility.DeleteFilebyID(connection, fileOrFolderId);
        }

        public static OneDriveResultWithData<OneDriveFile[]> GetFiles(GraphServiceClient connection, string folderId)
        {
            var rawList = CoreUtility.ListFolderFromId(connection, folderId, ItemType.File);
            var result = new OneDriveResultWithData<OneDriveFile[]>(rawList);
            if (rawList.IsSucceed)
            {
                var list = rawList.Data.Select((it) => { return new OneDriveFile(it); });
                result.Data = list.ToArray();
            }

            return result;
        }

        public static OneDriveResultWithData<OneDriveFolder[]> GetFolders(GraphServiceClient connection, string folderId)
        {
            var rawList = CoreUtility.ListFolderFromId(connection, folderId, ItemType.Folder);
            var result = new OneDriveResultWithData<OneDriveFolder[]>(rawList);
            if (rawList.IsSucceed)
            {
                var list = rawList.Data.Select((it) => { return new OneDriveFolder(it); });
                result.Data = list.ToArray();
            }
            return result;

        }

        public static OneDriveResultWithData<OneDriveFolder> CreateFolder(GraphServiceClient connection, string newFoldeName, string parentFolderId)
        {
            var rawResult = CoreUtility.CreateFolder(connection, newFoldeName, parentFolderId);
            var result = new OneDriveResultWithData<OneDriveFolder>(rawResult);
            if (rawResult.IsSucceed)
                result.Data = new OneDriveFolder(rawResult.Data);
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
