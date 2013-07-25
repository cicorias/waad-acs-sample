using System.Runtime.Serialization;

namespace OAuthTest
{
  [DataContract]
  public class WLIDProfile
  {
    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "first_name")]
    public string FirstName { get; set; }

    [DataMember(Name = "last_name")]
    public string LastName { get; set; }

  }
}