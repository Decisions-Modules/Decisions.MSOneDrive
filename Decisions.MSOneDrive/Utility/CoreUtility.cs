using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
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
            if (connection == null)
                throw new ArgumentNullException("connection", "GraphServiceClient object cannot be null");
        }

        public static DriveItem GetResourceInfo(GraphServiceClient connection, string id)
        {
            CheckConnectionOrException(connection);
            var request = connection.Drive.Items[id].Request();
            return request.GetAsync().Result;
        }

        public static DriveItem[] ListFolderFromId(GraphServiceClient connection, string id, ItemType type = ItemType.All, ClientType clientType = ClientType.Consumer)
        {
            CheckConnectionOrException(connection);

            var expandValue = clientType == ClientType.Consumer
                ? "thumbnails,children($expand=thumbnails)"
                : "thumbnails,children";

            DriveItem folder;
            IDriveItemRequest req;
            if (id == null)
                req = connection.Drive.Root.Request().Expand(expandValue);
            else
                req = connection.Drive.Items[id].Request().Expand(expandValue);
            folder = req.GetAsync().Result;

            return ProcessFolder(folder, type); ;
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
            if (folder.Folder != null && folder.Children.CurrentPage != null)
            {
                var items = folder.Children.CurrentPage;

                if (type == ItemType.File)
                {
                    var res = items.Where((it) => { return it.Folder == null; });
                    return res.ToArray();
                }
                else if (type == ItemType.Folder)
                {
                    var res = items.Where((it) => { return it.Folder != null; });
                    return res.ToArray();
                }
                return items.ToArray();
            }
            return new DriveItem[]{ };

        }

        public static DriveItem CreateFolder(GraphServiceClient connection, string newFoldeName, string parentFolderId)
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
                request = connection.Drive.Root.Children.Request(); //connection.Me.Drive.Root.Children.Request();
            else
                request = connection.Drive.Items[parentFolderId].Children.Request();

            var result = request.AddAsync(driveItem).Result;
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
                if (String.IsNullOrEmpty(parentFolderId))
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

        public static void DeleteFilebyID(GraphServiceClient connection, string Id)
        {
            CheckConnectionOrException(connection);
            var t = connection.Drive.Items[Id].Request().DeleteAsync();
            t.Wait();
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
                do
                {
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
