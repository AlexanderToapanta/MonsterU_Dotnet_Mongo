using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CapaModelo
{
    public class Personal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        [BsonElement("codigo")]
        public string codigo { get; set; }

        [BsonElement("peperTipo")]
        public string peperTipo { get; set; }

        [BsonElement("documento")]
        public string documento { get; set; }

        [BsonElement("nombres")]
        public string nombres { get; set; }

        [BsonElement("apellidos")]
        public string apellidos { get; set; }

        [BsonElement("email")]
        public string email { get; set; }

        [BsonElement("celular")]
        public string celular { get; set; }

        [BsonElement("fecha_nacimiento")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? fecha_nacimiento { get; set; }

        [BsonElement("sexo")]
        public string sexo { get; set; }

        [BsonElement("estado_civil")]
        public string estado_civil { get; set; }

        [BsonElement("username")]
        public string username { get; set; }

        [BsonElement("password_hash")]
        public string password_hash { get; set; }
        [BsonElement("imagen_perfil")]
        public string imagen_perfil { get; set; }

        [BsonElement("rol")]
        public object rol { get; set; }

        [BsonElement("fecha_ingreso")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime fecha_ingreso { get; set; }

        [BsonElement("estado")]
        public string estado { get; set; }
    }
}