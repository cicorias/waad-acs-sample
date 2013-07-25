using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using OAuthTest;

namespace WAAD_ACS_Sample.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      var callback = this.HttpContext.Request.Url.AbsoluteUri;
      var queryString = this.HttpContext.Request.Url.Query;
      if (!String.IsNullOrEmpty(queryString))
      {
        callback = callback.Replace(queryString, "");
      }
      ClaimsPrincipal cp = ClaimsPrincipal.Current;

      // WLID only issues Name Identifier and Identity Provider claims.
      // You can access additional user information by querying the Live servers.
      // This example adds this information as claims to the current principal.
      var identityprovider = cp.FindFirst("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider");
      var name = cp.FindFirst(ClaimTypes.Name);
      var authorizatonCode = this.HttpContext.Request.QueryString[OAuthConstants.Code];
      if (identityprovider != null &&
        identityprovider.Value == "uri:WindowsLiveID" &&
        name == null)
      {
        // Get an WLID authorization code if we haven't already got one.
        if (authorizatonCode == null)
        {
          GetAuthorizationCodeFromLiveServers(callback);
          return null;
        }

        // Now we have an authorization code we can get the User data from 
        // the Live servers.
        GetUserDataFromLiveServers(cp, authorizatonCode, callback);
      }

      var givenNameClaim = cp.FindFirst(ClaimTypes.GivenName);
      var surnameClaim = cp.FindFirst(ClaimTypes.Surname);


      if (givenNameClaim == null | surnameClaim == null)
      {
        ViewBag.Message = string.Format("Oops, no useful claims available");
      }
      else
      {
        string fullname =
             string.Format("{0} {1}", givenNameClaim.Value,
             surnameClaim.Value);
        ViewBag.Message = string.Format("Dear {0}, welcome to the WAAD ACS Sample App",
             fullname);
      }
      

      return View();
    }

    public ActionResult Manage()
    {
      ViewBag.Authorized = false;
      if (!User.IsInRole("Managers"))
      {
        ViewBag.Message = "You are not authorized for this page!";
      }
      else
      {
        ViewBag.Authorized = true;
        ViewBag.Message = "Your managers only page.";
      }

      return View();
    }

    public ActionResult About()
    {
      ViewBag.Message = "Your app description page.";

      return View();
    }

    public ActionResult Contact()
    {
      ViewBag.Message = "Your contact page.";

      return View();
    }

    private void GetUserDataFromLiveServers(ClaimsPrincipal cp, String authorizationCode, String callback)
    {
      OAuthToken token;
      OAuthError error;
      if (!string.IsNullOrEmpty(authorizationCode))
      {
        // Get an Access Token from the Live Servers using the Authorization Code.
        RequestAccessTokenByAuthorizationCode(callback, authorizationCode, out token, out error);

        if (error != null)
        {
          // Save any OAuth errors to a cookie.
          SaveOAuthErrorToCookie(error);
          return;
        }

        // Use the Access Token to request the User data.
        WLIDProfile user = GetUserData(token);
        if (user != null)
        {
          AddWLIDClaims(user, cp);
        }
      }
    }

    private void SaveOAuthErrorToCookie(OAuthError error)
    {
      Dictionary<string, string> cookieValues = new Dictionary<string, string>();
      HttpCookie cookie = this.HttpContext.Request.Cookies[WLIDConstants.WlCookie];
      HttpCookie newCookie = new HttpCookie(WLIDConstants.WlCookie);
      newCookie.Path = "/";
      newCookie.Domain = this.HttpContext.Request.Headers["Host"];

      if (cookie != null && cookie.Values != null)
      {
        foreach (string key in cookie.Values.AllKeys)
        {
          newCookie[key] = cookie[key];
        }
      }

      if (error != null)
      {
        newCookie[OAuthConstants.Error] = HttpUtility.UrlEncode(error.Code);
        newCookie[OAuthConstants.ErrorDescription] = HttpUtility.UrlPathEncode(error.Description);
      }

      this.HttpContext.Response.Cookies.Add(newCookie);
    }

    private void SaveWLIDErrorToCookie(WLIDError error)
    {
      Dictionary<string, string> cookieValues = new Dictionary<string, string>();
      HttpCookie cookie = this.HttpContext.Request.Cookies[WLIDConstants.WlCookie];
      HttpCookie newCookie = new HttpCookie(WLIDConstants.WlCookie);
      newCookie.Path = "/";
      newCookie.Domain = this.HttpContext.Request.Headers["Host"];

      if (cookie != null && cookie.Values != null)
      {
        foreach (string key in cookie.Values.AllKeys)
        {
          newCookie[key] = cookie[key];
        }
      }

      if (error != null)
      {
        newCookie[WLIDConstants.Code] = HttpUtility.UrlEncode(error.ErrorData.Code);
        newCookie[WLIDConstants.Message] = HttpUtility.UrlPathEncode(error.ErrorData.Message);
      }

      this.HttpContext.Response.Cookies.Add(newCookie);
    }

    // If we don't have an Authorization code, we must redirect the browser
    // to the Live servers.
    // In this scenario, the user has already authenticated. Therefore the Live
    // servers will redirect the browser back to this page passing the 
    // authorization code as a query string parameter.
    private void GetAuthorizationCodeFromLiveServers(String callback)
    {
      string requestUrl = String.Format("{0}?client_id={1}&scope=wl.basic&response_type=code&redirect_uri={2}",
        WLIDConstants.OAuthAuthUrl,
        HttpUtility.UrlEncode(ConfigurationManager.AppSettings["LiveId:ClientId"]),
        HttpUtility.UrlEncode(callback));
      this.HttpContext.Response.Redirect(requestUrl, true);
    }

    // Use the Access token in a GET request to the Live servers for the
    // user data. Copy the user data into a WLIDProfile object.
    private WLIDProfile GetUserData(OAuthToken token)
    {
      WLIDError error = null;
      WLIDProfile user = null;

      string getUrl = String.Format("{0}{1}?access_token={2}",
        WLIDConstants.ApisUrl,
        "me",
        HttpUtility.UrlEncode(token.AccessToken));

      HttpWebRequest request = WebRequest.Create(getUrl) as HttpWebRequest;
      request.Method = "GET";
      try
      {
        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        if (response != null)
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(WLIDProfile));
          user = serializer.ReadObject(response.GetResponseStream()) as WLIDProfile;
        }
      }
      catch (WebException e)
      {
        HttpWebResponse response = e.Response as HttpWebResponse;
        if (response != null)
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(WLIDError));
          error = serializer.ReadObject(response.GetResponseStream()) as WLIDError;
          SaveWLIDErrorToCookie(error);
        }
      }
      catch (IOException)
      {
      }

      return user;
    }

    private void RequestAccessTokenByAuthorizationCode(String callback, String authorizationCode, out OAuthToken token, out OAuthError error)
    {
      string content = String.Format("client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&grant_type=authorization_code",
          HttpUtility.UrlEncode(ConfigurationManager.AppSettings["LiveId:ClientId"]),
          HttpUtility.UrlEncode(callback),
          HttpUtility.UrlEncode(ConfigurationManager.AppSettings["LiveId:Secret"]),
          HttpUtility.UrlEncode(authorizationCode));

      RequestAccessToken(content, out token, out error);
    }

    // Send a POST to the Live servers to get an Access token from the
    // Authorization code we retrieved previously.
    private void RequestAccessToken(string postContent, out OAuthToken token, out OAuthError error)
    {
      token = null;
      error = null;

      HttpWebRequest request = WebRequest.Create(WLIDConstants.OAuthTokenUrl) as HttpWebRequest;
      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

      try
      {
        using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
        {
          writer.Write(postContent);
        }

        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        if (response != null)
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OAuthToken));
          token = serializer.ReadObject(response.GetResponseStream()) as OAuthToken;
          if (token != null)
          {
            return;
          }
        }
      }
      catch (WebException e)
      {
        HttpWebResponse response = e.Response as HttpWebResponse;
        if (response != null)
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(OAuthError));
          error = serializer.ReadObject(response.GetResponseStream()) as OAuthError;
        }
      }
      catch (IOException)
      {
      }

      if (error == null)
      {
        error = new OAuthError("request_failed", "Failed to retrieve user access token.");
      }
    }

    private void AddWLIDClaims(WLIDProfile user, ClaimsPrincipal principal)
    {
      if (principal != null && principal.Identity.IsAuthenticated == true)
      {
        var identityprovider = principal.FindFirst("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider");
        if (identityprovider != null && identityprovider.Value == WLIDConstants.IdentityProvider)
        {
          ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
          ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
          ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimTypes.Name, user.Name));
        }
      }
    }
  }
}
