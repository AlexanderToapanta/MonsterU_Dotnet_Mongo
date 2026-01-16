using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CapaModelo
{
    [BsonIgnoreExtraElements]
    public class Configuracion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("tipo")]
        [BsonRequired]
        public string Tipo { get; set; }

        [BsonElement("valores")]
        public List<ValorConfiguracion> Valores { get; set; } = new List<ValorConfiguracion>();


        public class ValorConfiguracion
        {
            [BsonElement("codigo")]
            public string Codigo { get; set; }

            [BsonElement("nombre")]
            public string Nombre { get; set; }

            
        }
    }
}