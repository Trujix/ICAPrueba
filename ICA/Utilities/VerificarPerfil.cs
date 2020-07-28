using ICA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ICA.Utilities
{
    public class VerificarPerfil : ActionFilterAttribute
    {
        int MODId;
        string Accion;

        public VerificarPerfil(int MODId, string accion)
        {
            this.MODId = MODId;
            this.Accion = accion;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            bool isAuthorised = Helpers.TieneAcceso(MODId, Accion);
            if (!isAuthorised)
                filterContext.Result = new HttpUnauthorizedResult();
        }

    }
}