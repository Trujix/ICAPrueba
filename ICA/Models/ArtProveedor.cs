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
    
    public partial class ArtProveedor
    {
        public int EVAId { get; set; }
        public int ARTId { get; set; }
        public int PRVId { get; set; }
        public System.DateTime MFechaHora { get; set; }
        public string MUsuarioId { get; set; }
    
        public virtual Articulo Articulo { get; set; }
        public virtual Evaluacion Evaluacion { get; set; }
        public virtual Proveedor Proveedor { get; set; }
    }
}