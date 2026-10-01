using System.Runtime.Serialization;

namespace Fika.Core.Networking.Models;

[DataContract]
public struct MatchJoinRequest
{
    [DataMember(Name = "serverId")]
    public string ServerId;

    [DataMember(Name = "profileId")]
    public string ProfileId;

    [DataMember(Name = "password")]
    public string Password;

    public MatchJoinRequest(string serverId, string profileId, string password = null)
    {
        ServerId = serverId;
        ProfileId = profileId;
        Password = password;
    }
}