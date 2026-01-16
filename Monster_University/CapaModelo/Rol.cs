using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CapaModelo
{
    [BsonIgnoreExtraElements]
    public class Rol
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("codigo")]
        public string Codigo { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("opciones_permitidas")]
        public List<string> OpcionesPermitidas { get; set; } = new List<string>();

        [BsonElement("estado")]
        public string Estado { get; set; } = "activo";

       
    }
}