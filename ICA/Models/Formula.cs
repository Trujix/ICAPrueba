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
    
    public partial class Formula
    {
        public int FORId { get; set; }
        public string Descripcion { get; set; }
        public string ProcAlmacenado { get; set; }
        public string Elemento { get; set; }
        public string ClasifiCA { get; set; }
        public string SubClasificacion { get; set; }
        public string OperacionGlobal { get; set; }
        public string Estatus { get; set; }
        public System.DateTime MFechaHora { get; set; }
        public string MUsuarioId { get; set; }
        public Nullable<bool> IncluirInformeProv { get; set; }
    }
}