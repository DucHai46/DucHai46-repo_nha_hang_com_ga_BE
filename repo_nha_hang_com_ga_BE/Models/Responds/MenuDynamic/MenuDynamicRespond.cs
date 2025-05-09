using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Responds.MenuDynamic;

public class MenuDynamicRespond
{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public string? routeLink { get; set; }
        public string? icon { get; set; }
        public string? label { get; set; }
        public bool? isOpen { get; set; } = false;
        public List<MenuDynamicChild>? children { get; set; }
}

