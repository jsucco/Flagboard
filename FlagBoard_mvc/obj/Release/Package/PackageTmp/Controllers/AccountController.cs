using System;
using System.Web.Mvc;
using System.Web.Security;

namespace FlagBoard_mvc.Controllers
{
    public class AccountController : Controller
    {
        [HttpPost]
        public ActionResult LogOff()
        {
            string queryvar = "";
            string cookieval = checkCookie("maintenance_location", "CID");
            if (cookieval.Length > 0)
                queryvar = "?CID=" + checkCookie("maintenance_location", "CID");

            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Cookies["APRKeepMeIn"].Expires = DateTime.Now.AddDays(-1);
            return Redirect("http://apr.standardtextile.com/Login.aspx?returnUrl=maintenance.standardtextile.com" + queryvar);
        }


        #region Helpers

        private string checkCookie(string CookieName, string key = "")
        {
            if (Request.Cookies[CookieName] != null && Request.Cookies[CookieName].HasKeys)
                if (Request.Cookies[CookieName][key] != null)
                    return Request.Cookies[CookieName][key];
            return "";
        }

        #endregion
    }
}
