using System;
using System.Collections.Generic;

namespace CapaModelo
{
    public class Configuracion
    {
        public string Id { get; set; } // MongoDB _id
        public string Tipo { get; set; } // "sexo", "estado_civil", etc.
        public List<ValorConfiguracion> Valores { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaActualizacion { get; set; }
    }

    public class ValorConfiguracion
    {
        public string Codigo { get; set; } // "M", "F", "S", "C", etc.
        public string Nombre { get; set; } // "Masculino", "Femenino", "Soltero/a", "Casado/a"
        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } = 0;
    }
}