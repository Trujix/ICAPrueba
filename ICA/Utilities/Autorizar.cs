using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace ICA.Utilities
{
    public class Autorizar : AuthorizeAttribute
    {

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Returns HTTP 401 - see comment in HttpUnauthorizedResult.cs.
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary {
                    { "action", "Index" },
                    { "controller", "Login" }
               });
        }
    }
}