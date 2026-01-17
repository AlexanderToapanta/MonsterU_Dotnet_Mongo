using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CapaModelo
{
    public class Carrera
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("codigo")]
        public string codigo { get; set; }

        [BsonElement("nombre")]
        public string nombre { get; set; }

        [BsonElement("creditosMaximos")]
        public int creditosMaximos { get; set; }

        [BsonElement("creditosMinimos")]
        public int creditosMinimos { get; set; }
    }
}