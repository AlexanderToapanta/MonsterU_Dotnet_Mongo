using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo
{
    public class Personal
    {
        public string PEPER_ID { get; set; }
        public string PEPER_NOMBRE { get; set; }
        public string PEPER_APELLIDO { get; set; }
        public string PEPER_EMAIL { get; set; }
        public string PEPER_CEDULA { get; set; }
        public string PEPER_CELULAR { get; set; }
        public string PEPER_TIPO { get; set; }
        public DateTime? PEPEPER_FECH_INGR { get; set; }
        public DateTime? PEPER_FECHA_NAC { get; set; }
        public string PEPER_FOTO { get; set; }

        // Llave foránea a Estado Civil
        public string PEESC_ID { get; set; }

        // Propiedad de navegación (opcional)
        public PEESC_ESTCIV EstadoCivil { get; set; }

        // Llave foránea a Sexo
        public string PESEX_ID { get; set; }

        // Propiedad de navegación (opcional)
        public PESEX_SEXO Sexo { get; set; }

        public string XEUSU_ID { get; set; }
    }
}