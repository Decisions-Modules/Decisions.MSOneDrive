using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Graph;

namespace Decisions.MSOneDrive
{
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OneDriveResourceType { Unavailable = 0, File = 1, Folder = 2 }

    [DataContract]
    public abstract class OneDriveResource
    {
        internal OneDriveResource(string id, string name, string desc, string link)
        {
            Id = id;
            Name = name;
            Description = desc;
            SharingLink = link;
        }
        internal OneDriveResource(DriveItem driveItem) : this(driveItem.Id, driveItem.Name, driveItem.Description, driveItem.WebUrl) { }

        [DataMember]
        public string Id { get; }
        [DataMember]
        public string Name { get; }
        [DataMember]
        public string Description { get; }
        [DataMember]
        public string SharingLink { get; }

    }

    [DataContract]
    public class OneDriveFile : OneDriveResource
    {
        internal OneDriveFile(string id, string name, string desc, string link) : base(id, name, desc, link){ }
        internal OneDriveFile(DriveItem driveItem) : base(driveItem) { }
    }

    [DataContract]
    public class OneDriveFolder : OneDriveResource
    {
        internal OneDriveFolder(string id, string name, string desc, string link) : base(id, name, desc, link) {}
        internal OneDriveFolder(DriveItem driveItem) : base(driveItem) { }
    }


}
