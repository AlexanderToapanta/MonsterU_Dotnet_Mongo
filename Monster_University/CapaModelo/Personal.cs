using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaModelo
{
    public class Persona
    {
        public string id { get; set; }
        public string codigo { get; set; }
        public string peperTipo { get; set; }
        public string documento { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string email { get; set; }
        public string celular { get; set; }
        public DateTime? fecha_nacimiento { get; set; }
        public string sexo { get; set; }
        public string estado_civil { get; set; }
        public string username { get; set; }
        public string password_hash { get; set; }
        public object rol { get; set; }
        public DateTime fecha_ingreso { get; set; }
        public string estado { get; set; }
    }
}