using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
    public static partial class OneDriveUtility
    {
        private static DriveItem[] GetFolderContentFromId(GraphServiceClient connection, string id, ItemType type = ItemType.All, ClientType clientType = ClientType.Consumer)
        {
            IDriveItemChildrenCollectionRequest req;
            if (id == null)
                req = connection.Drive.Root.Children.Request();
            else
                req = connection.Drive.Items[id].Children.Request();

            List<DriveItem> result = new List<DriveItem>();
            do
            {
                var children = req.GetAsync().Result;
                result.AddRange(ProcessFolder(children, type));
                req = children.NextPageRequest;
            }
            while (req != null);

            return result.ToArray();
        }

        private static DriveItem CreateFolderById(GraphServiceClient connection, string newFoldeName, string parentFolderId)
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

        public static OneDriveResultWithData<OneDriveFolder[]> GetFolders(GraphServiceClient connection, string folderId)
        {
            CheckConnectionOrException(connection);

            OneDriveResultWithData<OneDriveFolder[]> result = ProcessRequest(() =>
            {
                var rawList = OneDriveUtility.GetFolderContentFromId(connection, folderId, ItemType.Folder);
                var list = rawList.Select((it) => { return new OneDriveFolder(it); });
                return list.ToArray();
            });

            return result;
        }

        public static OneDriveResultWithData<OneDriveFolder> CreateFolder(GraphServiceClient connection, string newFoldeName, string parentFolderId)
        {
            CheckConnectionOrException(connection);
            OneDriveResultWithData<OneDriveFolder> result = ProcessRequest((Func<OneDriveFolder>)(() =>
            {
                var item = OneDriveUtility.CreateFolderById((GraphServiceClient)connection, (string)newFoldeName, (string)parentFolderId);
                return new OneDriveFolder((DriveItem)item);
            }));
            return result;
        }
    }
}
