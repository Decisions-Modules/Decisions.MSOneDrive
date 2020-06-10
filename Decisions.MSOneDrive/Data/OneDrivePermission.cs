using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Decisions.MSOneDrive
{
    // read more of the following data clases
    // https://docs.microsoft.com/en-us/onedrive/developer/rest-api/resources/permission?view=odsp-graph-online

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OneDrivePermissionRole { Read, Write, SharePointOwner, SharePointMember }; // read, write, sp.owner, sp.member

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OneDriveShareType { View, Edit, Embed };

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OneDriveShareScope { Anonymous, Organization };

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OneDriveDriveType { Personal, Business, DocumentLibrary };

    [DataContract]
    public class OneDriveIdentity
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
    }

    [DataContract]
    public class OneDriveIdentitySet
    {
        [DataMember]
        public OneDriveIdentity Application { get; set; }

        [DataMember]
        public OneDriveIdentity Device { get; set; }

        [DataMember]
        public OneDriveIdentity Group { get; set; }

        [DataMember]
        public OneDriveIdentity User { get; set; }

    }

    [DataContract]
    public class OneDriveSharedInvitation
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public bool? SignInRequired { get; set; }

        [DataMember]
        public OneDriveIdentitySet InvitedBy { get; set; }
    }

    [DataContract]
    public class OneDriveLink
    {
        [DataMember]
        public OneDriveIdentity Application { get; set; }

        [DataMember]
        public OneDriveShareType Type { get; set; }

        [DataMember]
        public OneDriveShareScope? Scope { get; set; }

        [DataMember]
        public string WebHtml { get; set; }

        [DataMember]
        public string WebUrl { get; set; }
    }

    [DataContract]
    public class SharePointId
    {
        [DataMember]
        public string ListId { get; set; }

        [DataMember]
        public string ListItemId { get; set; }

        [DataMember]
        public string ListItemUniqueId { get; set; }

        [DataMember]
        public string SiteId { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public string WebId { get; set; }
    }

    [DataContract]
    public class OneDriveItemReference
    {
        [DataMember]
        public string DriveId { get; set; }

        [DataMember]
        public OneDriveDriveType DriveType { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ListId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string SharedId { get; set; }

        [DataMember]
        public SharePointId[] SharepointIds { get; set; }

        [DataMember]
        public string SiteId { get; set; }
    }


    [DataContract]
    public class OneDrivePermission
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public OneDriveIdentitySet GrantedTo { get; set; }

        [DataMember]
        public OneDriveIdentitySet[] GrantedToIdentities { get; set; }

        [DataMember]
        public OneDriveSharedInvitation SharingInvitation { get; set; }

        [DataMember]
        public OneDriveLink Link { get; set; }

        [DataMember]
        public OneDrivePermissionRole[] Roles { get; set; }

        [DataMember]
        public string SharedId { get; set; }

        public OneDrivePermission()
        {
        }
    }
}
