﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class ICAEntities : DbContext
    {
        public ICAEntities()
            : base("name=ICAEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Articulo> Articulo { get; set; }
        public virtual DbSet<ArticuloCriterio> ArticuloCriterio { get; set; }
        public virtual DbSet<Criterio> Criterio { get; set; }
        public virtual DbSet<Equipo> Equipo { get; set; }
        public virtual DbSet<EquipoCriterio> EquipoCriterio { get; set; }
        public virtual DbSet<Evaluacion> Evaluacion { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Modulo> Modulo { get; set; }
        public virtual DbSet<ModuloPerfil> ModuloPerfil { get; set; }
        public virtual DbSet<Perfil> Perfil { get; set; }
        public virtual DbSet<Skus> Skus { get; set; }
        public virtual DbSet<Tienda> Tienda { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<UsuarioPerfil> UsuarioPerfil { get; set; }
        public virtual DbSet<ValorReferencia> ValorReferencia { get; set; }
        public virtual DbSet<VARValor> VARValor { get; set; }
        public virtual DbSet<ParametroMuestra> ParametroMuestra { get; set; }
        public virtual DbSet<Programacion> Programacion { get; set; }
        public virtual DbSet<VariacionPermitida> VariacionPermitida { get; set; }
        public virtual DbSet<ProgramacionZona> ProgramacionZona { get; set; }
        public virtual DbSet<TipoProducto> TipoProducto { get; set; }
        public virtual DbSet<Formula> Formula { get; set; }
        public virtual DbSet<EvArticulo> EvArticulo { get; set; }
        public virtual DbSet<EvEquipo> EvEquipo { get; set; }
        public virtual DbSet<EvidenciaArticulo> EvidenciaArticulo { get; set; }
        public virtual DbSet<EvidenciaEquipo> EvidenciaEquipo { get; set; }
        public virtual DbSet<Prueba> Prueba { get; set; }
        public virtual DbSet<ArtProveedor> ArtProveedor { get; set; }
        public virtual DbSet<Proveedor> Proveedor { get; set; }
        public virtual DbSet<ProveedorContacto> ProveedorContacto { get; set; }
        public virtual DbSet<RangoTorqueVacio> RangoTorqueVacio { get; set; }
        public virtual DbSet<RangoCriterio> RangoCriterio { get; set; }
    
        public virtual ObjectResult<sp_ValorPreArticulo_Result> sp_ValorPreArticulo()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_ValorPreArticulo_Result>("sp_ValorPreArticulo");
        }
    
        public virtual ObjectResult<sp_ValorPreEquipo_Result> sp_ValorPreEquipo()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_ValorPreEquipo_Result>("sp_ValorPreEquipo");
        }
    }
}
