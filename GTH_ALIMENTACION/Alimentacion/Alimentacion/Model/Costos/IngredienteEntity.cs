using Alimentacion.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alimentacion.Model.Costos
{
    class IngredienteEntity : BaseEntity
    {
        public bool Sobrante { get; set; }
        public string Ran_id { get; set; }
        public DateTime Ali_fecha { get; set; }
        public string Ali_etapa { get; set; }
        public string Art_id { get; set; }
        public string Ali_ing_desc { get; set; }
        public Double? Ali_peso { get; set; }
        public Double? Ali_ms { get; set; }
        public Double? Ali_precio { get; set; }
        public Int32? Inv_ordeña { get; set; }
        public Int32? Inv_j { get; set; }
        public Int32? Days { get; set; }
        public Int32? Inv_d1 { get; set; }
        public Int32? Inv_d2 { get; set; }
        public Int32? Inv_v { get; set; }
        public Int32? Inv_r { get; set; }
        public Int32? Inv_secas { get; set; }
        public Int32? Inv_lec_producida { get; set; }
        public Int32? Inv_lec_vendida { get; set; }
        public Double? Inv_precio { get; set; }
        public Double? Inv_precio_v { get; set; }
        public Double? Inv_establo { get; set; }
        public Double? Art_precio_uni { get; set; }
        public Double? X_VACA { get { return Ali_peso / Inv_ordeña; } }
        public Double? Costo { get { return X_VACA * (Art_precio_uni == 0 ? Ali_precio : Art_precio_uni); } }
        public Double? Precio { get { return Inv_ordeña * Costo; } }
        public Double? Precio_ing { get { return (Art_precio_uni == 0 ? Ali_precio : Art_precio_uni); } }
        public DateTime Fec_ini { get; set; }
        public DateTime Fec_fin { get; set; }
        public DateTime Fec_final { get; set; }
        public Int32? Hour { get; set; }
        public Boolean EmpresaFlag { get; set; }
        public String Empresa { get; set; }
        public String Etapa { get; set; }
        public String EtapaN { get; set; }
        public String EtapaS { get; set; }
        public String RestarH { get; set; }
        public String Establosemp { get; set; }
        public String Establosemp2 { get; set; }
        public String Establosempnum { get; set; }
        public Int32? Config { get; set; }
    }
}
