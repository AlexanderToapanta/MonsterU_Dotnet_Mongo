using System;

namespace CapaModelo
{
    public class OpcionRol
    {
        public string XEROL_ID { get; set; }
        public string XEROL_NOMBRE { get; set; }
        public string XEOPC_ID { get; set; }
        public string XEOPC_NOMBRE { get; set; }
        public bool ASIGNADO { get; set; }
        public DateTime? XROP_FECHA_ASIG { get; set; }
        public DateTime? XROP_FECHA_RETIRO { get; set; }
    }
}