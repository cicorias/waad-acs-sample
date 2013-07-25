using System.Runtime.Serialization;

namespace OAuthTest
{
  [DataContract]
  public class WLIDError
  {
    [DataMember(Name = WLIDConstants.Error)]
    public WLIDErrorData ErrorData { get; set; }
  }

  [DataContract]
  public class WLIDErrorData
  {
    [DataMember(Name = WLIDConstants.Code)]
    public string Code { get; set; }

    [DataMember(Name = WLIDConstants.Message)]
    public string Message { get; set; }
  }
}