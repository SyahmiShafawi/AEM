using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AEMWebApplication.Models
{
    public class PlatformWellActual
    {
        public int id { get; set; }
        public string uniqueName { get; set; } = string.Empty;
        public double latitude { get; set; } = 0.0;
        public double longitude { get; set; } = 0.0;
        public DateTime createdAt { get; set; } =  DateTime.MinValue;
        public DateTime updatedAt { get; set; } = DateTime.MinValue;
        public List<Well> well { get; set; }


        public class Well
        {
            public int id { get; set; }
            public string platformId { get; set; } = string.Empty;
            public string uniqueName { get; set; } = string.Empty;
            public double latitude { get; set; } = 0.0;
            public double longitude { get; set; } = 0.0;
            public DateTime createdAt { get; set; } = DateTime.MinValue;
            public DateTime updatedAt { get; set; } = DateTime.MinValue;
        }
    }
}
