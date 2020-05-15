// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace OneDriveLib
{
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Contexts;
    using System.Threading.Tasks;

    public class Browser
    {

        public class ResultItem
        {
            public Microsoft.Graph.DriveItem[] ChildrenItems;
            public Microsoft.Graph.DriveItem ParentFolder;
        }

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
        private const int UploadChunkSize = 10 * 1024 * 1024;       // 10 MB
        //private GraphServiceClient graphClient { get; set; }
        
        public static ResultItem ListFolderFromId(GraphServiceClient Connection, string id, ItemType type=ItemType.All, ClientType clientType=ClientType.Consumer)
        {
            if (null == Connection) return null;

            try
            {
                var expandString = clientType == ClientType.Consumer
                    ? "thumbnails,children($expand=thumbnails)"
                    : "thumbnails,children";

                Task<Microsoft.Graph.DriveItem> task = Task.Run<DriveItem>(async () => await Connection.Drive.Items[id].Request().Expand(expandString).GetAsync());
                var folder = task.Result;
                ResultItem ret = new ResultItem();
                ret.ParentFolder = folder;
                ret.ChildrenItems = ProcessFolder(folder, type);
                return ret;
            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }
            return null;
        }

        public static ResultItem ListFolderFromPath(GraphServiceClient Connection, string path = null, ItemType type = ItemType.All, ClientType clientType = ClientType.Consumer)
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
                /*return folder;*/
            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }

            return null;
        }

        private static Microsoft.Graph.DriveItem[] ProcessFolder(DriveItem folder, ItemType type = ItemType.All)
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
                            Console.WriteLine(obj.Id);
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

        public static GraphServiceClient SignIn(string MsaClientId)
        {
            GraphServiceClient newConnection = null;
            try
            {
                newConnection = AuthenticationHelper.GetAuthenticatedClient(MsaClientId);
            }
            catch (ServiceException exception)
            {

                PresentServiceException(exception);
                newConnection = null;
            }
            return newConnection;
        }

        public static void SignOut(GraphServiceClient Connection)
        {
            if (Connection != null)
                AuthenticationHelper.SignOut();
        }
        private static void PresentServiceException(Exception exception)
        {
            string message = null;
            var oneDriveException = exception as ServiceException;
            if (oneDriveException == null)
            {
                message = exception.Message;
            }
            else
            {
                message = string.Format("{0}{1}", Environment.NewLine, oneDriveException.ToString());
            }

            Console.WriteLine(string.Format("OneDrive reported the following error: {0}", message));
        }


        public static Microsoft.Graph.DriveItem UploadFilebyPath(GraphServiceClient Connection, string targetFolder, string uploadFileName)
        {
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
                PresentServiceException(exception);
            }
            return null;
        }

        public static Microsoft.Graph.DriveItem UploadFilebyID(GraphServiceClient Connection, string targetID, string uploadFileName)
        {
            try
            {
                var stream = new System.IO.FileStream(uploadFileName, System.IO.FileMode.Open);
                Task<Microsoft.Graph.DriveItem> task = Task.Run<DriveItem>(async () => await Connection.Drive.Items[targetID].ItemWithPath(uploadFileName).Content.Request()
                            .PutAsync<DriveItem>(stream));
                var uploadedItem = task.Result;
                return uploadedItem;
            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }
            return null;
        }

        public static Microsoft.Graph.DriveItem DownloadFilebyID(GraphServiceClient Connection, string Id, string fileName)
        {
            if (Connection == null)
                return null;
            try
            {
                //using (var stream = await Connection.Drive.Items[Id].Content.Request().GetAsync())
                Task<System.IO.Stream> task = Task.Run<System.IO.Stream>(async () => await Connection.Drive.Items[Id].Content.Request().GetAsync());
                var stream = task.Result;
                using (var outputStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
                {
                    stream.CopyToAsync(outputStream);
                }
            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }
            return null;
        }

        public static void DeleteFilebyID(GraphServiceClient Connection, string Id)
        {
            if (Connection == null)
                return;
            Task task = Task.Run(async () => await Connection.Drive.Items[Id].Request().DeleteAsync());
        }

        public static Permission CreateShareLink(GraphServiceClient Connection, string Id, string Type = "view", string Scope = "anonymous")
        {
            if (Connection == null)
                return null;
            Task<Permission> task = Task.Run<Permission>(async () => await Connection.Drive.Items[Id].CreateLink(Type, Scope).Request().PostAsync());
            return task.Result;
        }

        public static Permission[] ListPermissions(GraphServiceClient Connection, string Id)
        {
            if (Connection == null)
                return null;
            Task<IDriveItemPermissionsCollectionPage> task = Task.Run<IDriveItemPermissionsCollectionPage>(async () => await Connection.Drive.Items[Id].Permissions.Request().GetAsync());
            var link = task.Result;
            Permission[] arrayPermission = link.Cast<Permission>().ToArray();
            return arrayPermission;
        }
    }
}
