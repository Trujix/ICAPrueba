using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICA.Models
{
    public class ValorReferenciaApp
    {
        public string VARClave;
        public List<VARValorApp> Valores;
        public byte Modificable;
        public string Descripcion;
    }
}