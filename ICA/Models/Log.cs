//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ICA.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Log
    {
        public int LOGId { get; set; }
        public string Modulo { get; set; }
        public string Accion { get; set; }
        public string Detalle { get; set; }
        public System.DateTime MFechaHora { get; set; }
        public string MUsuarioId { get; set; }
    }
}