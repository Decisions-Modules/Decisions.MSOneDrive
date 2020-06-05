// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
  /*  public class ResultItem
    {
        public Microsoft.Graph.DriveItem[] ChildrenItems;
        public Microsoft.Graph.DriveItem ParentFolder;
    }*/

    public enum ClientType
    {
        Consumer,
        Business
    }

    public enum ItemType
    {
        Folder,
        File,
        All,
    }

    public static class CoreUtility
    {
        private const int UploadChunkSize = 10 * 1024 * 1024;       // 10 MB
                                                                    //private GraphServiceClient graphClient { get; set; }

        private static void CheckConnectionOrException(GraphServiceClient connection)
        {
            if (connection==null)
                throw new ArgumentNullException("connection", "GraphServiceClient object cannot be null");
        }

        private static OneDriveResultWithData<DriveItem>  ExecuteRequest(IDriveItemRequest request)
        {
            var result = new OneDriveResultWithData<DriveItem>();
            try
            {
                result.Data = request.GetAsync().Result;
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

        public static OneDriveResultWithData<DriveItem> GetResourceInfo(GraphServiceClient connection, string id)
        {
            CheckConnectionOrException(connection);
            var req = connection.Drive.Items[id].Request();
            return ExecuteRequest(req);
        }

        public static OneDriveResultWithData<DriveItem[]> ListFolderFromId(GraphServiceClient connection, string id, ItemType type=ItemType.All, ClientType clientType=ClientType.Consumer)
        {
            CheckConnectionOrException(connection);

            var expandValue = clientType == ClientType.Consumer
                ? "thumbnails,children($expand=thumbnails)"
                : "thumbnails,children";

            var result = new OneDriveResultWithData<DriveItem[]>();
            try
            {
                DriveItem folder;
                IDriveItemRequest req;
                if (id == null)
                    req = connection.Drive.Root.Request().Expand(expandValue);
                else
                    req = connection.Drive.Items[id].Request().Expand(expandValue);
                folder = req.GetAsync().Result;

                result.Data = ProcessFolder(folder, type); ;
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

        /*       public static ResultItem ListFolderFromPath(GraphServiceClient Connection, string path = null, ItemType type = ItemType.All, ClientType clientType = ClientType.Consumer)
               {
                   if (null == Connection) return null;

                   DriveItem folder;

                   try
                   {
                       var expandValue = clientType == ClientType.Consumer
                           ? "thumbnails,children($expand=thumbnails)"
                           : "thumbnails,children";

                       if (path == null)
                       {
                           Task<Microsoft.Graph.DriveItem> task = Task.Run<DriveItem>(async () => await Connection.Drive.Root.Request().Expand(expandValue).GetAsync());
                           folder = task.Result;
                       }
                       else
                       {                      
                           Task<Microsoft.Graph.DriveItem> task = Task.Run<DriveItem>(async () => await
                                   Connection.Drive.Root.ItemWithPath("/" + path)
                                       .Request()
                                       .Expand(expandValue)
                                       .GetAsync());
                           folder = task.Result;
                       }

                       ResultItem result = new ResultItem();

                       result.ChildrenItems = ProcessFolder(folder, type);
                       result.ParentFolder = folder;

                       return result;

                   }
                   catch (Exception exception)
                   {
                       PresentServiceException(exception);
                   }

                   return null;
               }*/

        private static DriveItem[] ProcessFolder(DriveItem folder, ItemType type = ItemType.All)
        {
            if (folder != null)
            {
                if (folder.Folder != null && folder.Children.CurrentPage != null)
                {
                    var items = folder.Children.CurrentPage;
                    int nLength = items.Count, i = 0, nCount = 0;
                    Microsoft.Graph.DriveItem[] CurrentItems = null;
                    if (type == ItemType.All)
                    {
                        CurrentItems = new Microsoft.Graph.DriveItem[nLength];
                        foreach (var obj in items)
                        {
                            CurrentItems[i++] = obj;
                        }
                    }
                    else if (type == ItemType.File)
                    {
                        nCount = i = 0;
                        foreach (var obj in items)
                        {
                            if (obj.Folder != null)
                                continue;
                            nCount++;
                        }
                        CurrentItems = new Microsoft.Graph.DriveItem[nCount];
                        foreach (var obj in items)
                        {
                            if (obj.Folder != null)
                                continue;
                            CurrentItems[i++] = obj;
                        }
                    }
                    else if (type == ItemType.Folder)
                    {
                        nCount = i = 0;
                        foreach (var obj in items)
                        {
                            if (obj.Folder == null)
                                continue;
                            nCount++;
                        }
                        CurrentItems = new Microsoft.Graph.DriveItem[nCount];
                        foreach (var obj in items)
                        {
                            if (obj.Folder == null)
                                continue;
                            CurrentItems[i++] = obj;
                        }
                    }
                    return CurrentItems;
                }
            }
            return null;
        }

        public static OneDriveResultWithData<DriveItem> CreateFolder(GraphServiceClient connection, string newFoldeName, string parentFolderId)
        {
            CheckConnectionOrException(connection);
            if (String.IsNullOrEmpty(newFoldeName))
                throw new ArgumentNullException("newFoldeName");

            var driveItem = new DriveItem
            {
                Name = newFoldeName,
                Folder = new Folder()
            };

            IDriveItemChildrenCollectionRequest request;
            if (parentFolderId == null)
                request = connection.Me.Drive.Root.Children.Request();
            else
                request = connection.Drive.Items[parentFolderId].Children.Request();

            var result = new OneDriveResultWithData<DriveItem>();
            try
            {
                result.Data = request.AddAsync(driveItem).Result;
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

    /*    public static Microsoft.Graph.DriveItem UploadFilebyPath(GraphServiceClient Connection, string targetFolder, string uploadFileName)
        {
            CheckConnectionOrException(connection);

            string folderPath = targetFolder;

            var uploadPath = folderPath + "/" + Uri.EscapeUriString(System.IO.Path.GetFileName(uploadFileName));

            try
            {
                var stream = new System.IO.FileStream(uploadFileName, System.IO.FileMode.Open);
                Task<Microsoft.Graph.DriveItem> task = Task.Run<DriveItem>(async () => await Connection.Drive.Root.ItemWithPath(uploadPath).Content.Request().PutAsync<DriveItem>(stream));
                var uploadedItem = task.Result;
                return uploadedItem;
            }
            catch (Exception exception)
            {
                //PresentServiceException(exception);
            }
            return null;
        }*/

        public static OneDriveResultWithData<DriveItem> UploadFilebyID(GraphServiceClient connection, string localFilePath, string fileName = null, string parentFolderId = null)
        {
            CheckConnectionOrException(connection);

            if (string.IsNullOrEmpty(localFilePath))
                throw new ArgumentNullException("localFilePath", "localFilePath cannot be null or empty.");

            if (fileName == null)
                fileName = System.IO.Path.GetFileName(localFilePath);

            var result = new OneDriveResultWithData<DriveItem>();
            try
            {
                var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open);
                IDriveItemContentRequest request; 
                if(String.IsNullOrEmpty(parentFolderId))
                    request = connection.Drive.Root.ItemWithPath(fileName).Content.Request();
                else
                    request = connection.Drive.Items[parentFolderId].ItemWithPath(fileName).Content.Request();

                var uploadedItem = request.PutAsync<DriveItem>(stream).Result;

                result.Data = uploadedItem;
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

        public static OneDriveBaseResult DownloadFilebyID(GraphServiceClient connection, string fileId, string fileName)
        {
            CheckConnectionOrException(connection);
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "fileName cannot be null or empty.");

            var result = new OneDriveBaseResult();
            try
            {
                var request = connection.Drive.Items[fileId].Content.Request();
                var stream = request.GetAsync().Result;
                using (var outputStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
                {
                    Task task = stream.CopyToAsync(outputStream);
                    task.Wait();
                }
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

        public static OneDriveBaseResult DeleteFilebyID(GraphServiceClient connection, string Id)
        {
            CheckConnectionOrException(connection);

            var result = new OneDriveBaseResult();
            try
            {
                var t = connection.Drive.Items[Id].Request().DeleteAsync();
                t.Wait();
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }

        public static OneDriveResultWithData<Permission> CreateShareLink(GraphServiceClient connection, string resourceId, string Type = "view", string Scope = "anonymous")
        {
            CheckConnectionOrException(connection);

            var result = new OneDriveResultWithData<Permission>();
            try
            {
                var request = connection.Drive.Items[resourceId].CreateLink(Type, Scope).Request();
                result.Data = request.PostAsync().Result;
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;

        }

        public static OneDriveResultWithData<Permission[]> ListPermissions(GraphServiceClient connection, string resourceId)
        {
            CheckConnectionOrException(connection);
            var result = new OneDriveResultWithData<Permission[]>();
            try
            {
                var request = connection.Drive.Items[resourceId].Permissions.Request();

                var permissions = new List<Permission>();
                do {
                    var link = request.GetAsync().Result;
                    request = link.NextPageRequest;
                    permissions.AddRange(link);
                } while (request != null);

                result.Data = permissions.ToArray();
                result.IsSucceed = true;
            }
            catch (Exception exception)
            {
                if (!result.FillFromException(exception))
                    throw;
            }
            return result;
        }
    }
}
