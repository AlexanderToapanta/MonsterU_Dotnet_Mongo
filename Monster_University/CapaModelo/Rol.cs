using System;
using System.Collections.Generic;

namespace CapaModelo
{
    public class Rol
    {
        public string codigo { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public List<string> opciones_permitidas { get; set; }
        public string estado { get; set; }
    }
}