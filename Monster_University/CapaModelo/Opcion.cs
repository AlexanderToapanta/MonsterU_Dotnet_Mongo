using System;

namespace CapaModelo
{
    public class Opcion
    {
        public string XEOPC_ID { get; set; }
        public string XEOPC_NOMBRE { get; set; }
        public DateTime? XROP_FECHA_ASIG { get; set; }
        public DateTime? XROP_FECHA_RETIRO { get; set; }
        public bool ASIGNADO { get; set; } // Para usar en vistas
    }
}