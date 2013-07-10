using System.Security.Claims;
using System.Web.Mvc;

namespace WAAD_ACS_Sample.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      ClaimsPrincipal cp = ClaimsPrincipal.Current;
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
  }
}
