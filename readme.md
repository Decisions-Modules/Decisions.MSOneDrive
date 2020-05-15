API List



public const string MsaClientId = "a4ff7c3c-d1e5-47eb-a654-e00381ra89s";
Connection = Browser.SignIn(MsaClientId);


public static GraphServiceClient SignIn(string MsaClientId)


- public static ResultItem ListFolderFromPath(GraphServiceClient Connection, string path = null, ItemType type = ItemType.All, ClientType clientType =                   ClientType.Consumer)
- public static ResultItem ListFolderFromId(GraphServiceClient Connection, string id, ItemType type=ItemType.All, ClientType clientType=ClientType.Consumer)



- public static Microsoft.Graph.DriveItem UploadFilebyPath(GraphServiceClient Connection, string targetFolder, string uploadFileName) 
- public static Microsoft.Graph.DriveItem UploadFilebyID(GraphServiceClient Connection, string targetID, string uploadFileName)



- public static Microsoft.Graph.DriveItem DownloadFilebyID(GraphServiceClient Connection, string Id, string fileName)
- public static void DeleteFilebyID(GraphServiceClient Connection, string Id)



- public static Permission CreateShareLink(GraphServiceClient Connection, string Id, string Type = "view", string Scope = "anonymous")
- public static Permission[] ListPermissions(GraphServiceClient Connection, string Id)

