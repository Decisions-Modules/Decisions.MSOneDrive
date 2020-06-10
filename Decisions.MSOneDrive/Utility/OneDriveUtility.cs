using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    public static partial class OneDriveUtility
    {


        private static void CheckConnectionOrException(GraphServiceClient connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection", "GraphServiceClient object cannot be null");
        }

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

        private static DriveItem GetResourceInfo(GraphServiceClient connection, string id)
        {
            CheckConnectionOrException(connection);
            var request = connection.Drive.Items[id].Request();
            return request.GetAsync().Result;
        }

        private static DriveItem[] ProcessFolder(IDriveItemChildrenCollectionPage children, ItemType type = ItemType.All)
        {
            if (children.CurrentPage != null)
            {
                List<DriveItem> result = new List<DriveItem>();
                var items = children.CurrentPage;

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
            return new DriveItem[] { };

        }

        private static void DeleteResourceByID(GraphServiceClient connection, string Id)
        {
            CheckConnectionOrException(connection);
            var t = connection.Drive.Items[Id].Request().DeleteAsync();
            t.Wait();
        }

        public static OneDriveResultWithData<OneDriveResourceType> DoesResourceExist(GraphServiceClient connection, string fileOrFolderId)
        {
            CheckConnectionOrException(connection);
            OneDriveResultWithData<OneDriveResourceType> result = ProcessRequest(() =>
            {
                DriveItem item = OneDriveUtility.GetResourceInfo(connection, fileOrFolderId);
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
            CheckConnectionOrException(connection);
            return ProcessRequest(() =>
            {
                OneDriveUtility.DeleteResourceByID(connection, fileOrFolderId);
            });
        }
    }
}
