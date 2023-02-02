using System;
using System.Collections.Generic;

namespace TeamServer.Models.Extras
{
    public class ReconCenterEntity
    {
        public static List<ReconCenterEntity> ReconCenterEntityList = new();


        public string Name { get; set; }
        public string Description { get; set; }

        public List<ReconCenterEntityProperty> Properties { get; set; } = new List<ReconCenterEntityProperty>();

        [Serializable]
        public class ReconCenterEntityProperty
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Note { get; set; }
        }
    }
}
