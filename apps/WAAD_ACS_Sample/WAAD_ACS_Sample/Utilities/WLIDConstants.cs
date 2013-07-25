
namespace OAuthTest
{
  public static class WLIDConstants
  {
    #region Endpoints
    public const string OAuthAuthUrl = "https://login.live.com/oauth20_authorize.srf";
    public const string OAuthTokenUrl = "https://login.live.com/oauth20_token.srf";
    public const string ApisUrl = "https://apis.live.net/v5.0/";
    #endregion

    #region Cookie Names
    public const string WlCookie = "wl_auth";
    #endregion

    #region Identity Provider
    public const string IdentityProvider = "uri:WindowsLiveID";
    #endregion

    #region Code Messages
    public const string Error = "error";
    public const string Code = "code";
    public const string Message = "message";
    #endregion
  }
}