namespace HardHatCore.HardHatC2Client.Models
{
    public class ReconCenterEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<ReconCenterEntityProperty> Properties { get; set; } = new List<ReconCenterEntityProperty>();

        public class ReconCenterEntityProperty
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Note { get; set; }
        }
    }
}
