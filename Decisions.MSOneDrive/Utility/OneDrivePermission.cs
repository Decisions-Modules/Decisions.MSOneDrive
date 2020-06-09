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
        private static Dictionary<string, OneDrivePermissionRole> roleDict = new Dictionary<string, OneDrivePermissionRole>
            {
                {"read", OneDrivePermissionRole.Read},
                {"write", OneDrivePermissionRole.Write},
                {"sp.owner",OneDrivePermissionRole.SharePointOwner},
                {"sp.member", OneDrivePermissionRole.SharePointMember}
            };

        private static T ParseOneDriveEnum<T>(string textValue) where T : System.Enum
        {
            if (typeof(T) is OneDrivePermissionRole)
            {
                return (T)((System.Enum)roleDict[textValue.ToLower()]);
            }
            else
            {
                return (T)Enum.Parse(typeof(T), textValue, true);
            }
        }

        private static Permission[] ListPermissionsById(GraphServiceClient connection, string resourceId)
        {
            var request = connection.Drive.Items[resourceId].Permissions.Request();

            var permissions = new List<Permission>();
            do
            {
                var link = request.GetAsync().Result;
                request = link.NextPageRequest;
                permissions.AddRange(link);
            } while (request != null);

            return permissions.ToArray();
        }

        private static void DeletePermissionsById(GraphServiceClient connection, string resourceId, string permissionId)
        {
            var request = connection.Drive.Items[resourceId].Permissions[permissionId].Request();
            request.DeleteAsync().Wait();
        }

        private static Permission CreateShareLinkById(GraphServiceClient connection, string resourceId, string Type, string Scope)
        {
            var request = connection.Drive.Items[resourceId].CreateLink(Type, Scope).Request();
            var res = request.PostAsync().Result;
            return res;
        }

        private static OneDriveIdentity CreateOneDriveIdentity(Identity identity)
        {
            if (identity == null) return null;
            return new OneDriveIdentity
            {
                Id = identity.Id,
                DisplayName = identity.DisplayName
            };
        }
        private static OneDriveIdentitySet CreateOneDriveIdentitySet(IdentitySet identitySet)
        {
            if (identitySet == null) return null;
            return new OneDriveIdentitySet
            {
                Application = CreateOneDriveIdentity(identitySet.Application),
                Device = CreateOneDriveIdentity(identitySet.Device),
                User = CreateOneDriveIdentity(identitySet.User),

            };
        }
        private static OneDriveIdentitySet[] CreateOneDriveIdentities(IEnumerable<IdentitySet> identitySet)
        {
            if (identitySet == null) return null;
            return identitySet.Select((it) => { return CreateOneDriveIdentitySet(it); }).ToArray();
        }

        private static OneDriveSharedInvitation CreateOneDriveSharedInvitation(SharingInvitation invitation)
        {
            if (invitation == null) return null;
            return new OneDriveSharedInvitation
            {
                Email = invitation.Email,
                SignInRequired = invitation.SignInRequired,
                InvitedBy = CreateOneDriveIdentitySet(invitation.InvitedBy)
            };
        }
        private static OneDriveLink CreateOneDriveLink(SharingLink link)
        {
            if (link == null) return null;
            var res = new OneDriveLink();
            res.Application = CreateOneDriveIdentity(link.Application);
            if (link.Type != null)
                res.Type = ParseOneDriveEnum<OneDriveShareType>(link.Type);
            if (link.Scope != null)
                res.Scope = ParseOneDriveEnum<OneDriveShareScope>(link.Scope);
            res.WebHtml = link.WebHtml;
            res.WebUrl = link.WebUrl;

            return res;
        }

        private static OneDrivePermission CreateOneDrivePermission(Permission perm)
        {
            if (perm == null) return null;
            return new OneDrivePermission
            {
                Id = perm.Id,
                GrantedTo = CreateOneDriveIdentitySet(perm.GrantedTo),
                GrantedToIdentities = CreateOneDriveIdentities(perm.GrantedToIdentities),
                SharingInvitation = CreateOneDriveSharedInvitation(perm.Invitation),
                Link = CreateOneDriveLink(perm.Link),
                Roles = perm.Roles.Select((it) => { return ParseOneDriveEnum<OneDrivePermissionRole>(it); }).ToArray(),
                SharedId = perm.ShareId
            };
        }


        public static OneDriveResultWithData<OneDrivePermission[]> GetPermissionList(GraphServiceClient connection, string resourceId)
        {
            CheckConnectionOrException(connection);

            OneDriveResultWithData<OneDrivePermission[]> result = ProcessRequest<OneDrivePermission[]>(() =>
            {
                var permissions = ListPermissionsById(connection, resourceId);
                OneDrivePermission[] data = new OneDrivePermission[permissions.Length];
                for (int i = 0; i < permissions.Length; i++)
                {
                    data[i] = CreateOneDrivePermission(permissions[i]);
                }
                return data;
            });

            return result;
        }

        public static OneDriveBaseResult DeletePermission(GraphServiceClient connection, string resourceId, string permissionId)
        {
            CheckConnectionOrException(connection);

            OneDriveBaseResult result = ProcessRequest(() =>
            {
                DeletePermissionsById(connection, resourceId, permissionId);
            });
            return result;
        }

        public static OneDriveResultWithData<OneDrivePermission> CreateShareLink(GraphServiceClient connection, string resourceId, OneDriveShareType shareType, OneDriveShareScope shareScope)
        {
            CheckConnectionOrException(connection);

            OneDriveResultWithData<OneDrivePermission> result = ProcessRequest<OneDrivePermission>(() =>
            {
                string type = Enum.GetName(typeof(OneDriveShareType), shareType).ToLower();
                string scope = Enum.GetName(typeof(OneDriveShareScope), shareScope).ToLower();
                var perm = CreateShareLinkById(connection, resourceId, type, scope);
                return CreateOneDrivePermission(perm);
            });

            return result;
        }
    }
}
