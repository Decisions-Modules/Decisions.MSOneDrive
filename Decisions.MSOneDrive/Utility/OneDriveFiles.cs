using Microsoft.Graph;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
    public static partial class OneDriveUtility
    {
        private static DriveItem UploadFileById(GraphServiceClient connection, string localFilePath, 
            string fileName = null, string parentFolderId = null)
        {
            CheckConnectionOrException(connection);

            if (string.IsNullOrEmpty(localFilePath))
            {
                throw new ArgumentNullException("localFilePath", "localFilePath cannot be null or empty.");
            }

            fileName ??= Path.GetFileName(localFilePath);
            
            var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open);
            try
            {
                IDriveItemContentRequest request;
                if (String.IsNullOrEmpty(parentFolderId))
                    request = connection.Drive.Root.ItemWithPath(fileName).Content.Request();
                else
                    request = connection.Drive.Items[parentFolderId].ItemWithPath(fileName).Content.Request();

                var uploadedItem = request.PutAsync<DriveItem>(stream).Result;
                
                return uploadedItem;
            }
            finally
            {
                stream.Close();
            }
        }

        private static void DownloadFilebyID(GraphServiceClient connection, string fileId, string localFilePath)
        {
            CheckConnectionOrException(connection);
            if (string.IsNullOrEmpty(localFilePath))
                throw new ArgumentNullException("localFilePath", "localFilePath cannot be null or empty.");

            var request = connection.Drive.Items[fileId].Content.Request();
            var stream = request.GetAsync().Result;
            using (var outputStream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Create))
            {
                try
                {
                    Task task = stream.CopyToAsync(outputStream);
                    task.Wait();
                }
                finally
                {
                    outputStream.Close();
                }
            }
        }

        public static OneDriveResultWithData<OneDriveFile[]> GetFiles(GraphServiceClient connection, string folderId)
        {
            CheckConnectionOrException(connection);

            OneDriveResultWithData<OneDriveFile[]> result = ProcessRequest(() =>
            {
                var rawList = GetFolderContentFromId(connection, folderId, ItemType.File);
                var list = rawList.Select((it) => { return new OneDriveFile(it); });
                return list.ToArray();
            });

            return result;
        }

        public static OneDriveResultWithData<OneDriveFile> UploadFile(GraphServiceClient connection, string localFilePath, 
            string fileName = null, string parentFolderId = null, bool usingFullFilePath = false)
        {
            CheckConnectionOrException(connection);
            
            OneDriveResultWithData<OneDriveFile> result = ProcessRequest(() =>
            {
                DriveItem item = UploadFileById(connection, localFilePath, fileName, parentFolderId);
                return new OneDriveFile(item);
            });

            return result;
        }

        public static OneDriveBaseResult DownloadFile(GraphServiceClient connection, string fileId, string localFilePath)
        {
            CheckConnectionOrException(connection);

            return ProcessRequest(() =>
            {
                OneDriveUtility.DownloadFilebyID(connection, fileId, localFilePath);
            });
        }
    }
}
