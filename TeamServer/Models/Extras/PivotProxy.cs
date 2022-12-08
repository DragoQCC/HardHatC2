namespace TeamServer.Models.Extras
{
    public class PivotProxy
    {
        public string EngineerId { get; set; }
        public string BindPort { get; set; }
        public string FwdHost { get; set; }
        public string FwdPort { get; set; }
        public PivotProxyType pivotType { get; set; }
        public ProxyDirection pivotDirection { get; set; }

        public enum PivotProxyType
        {
            R_PORT_FWD,
            PORT_FWD,
            SOCKS4a
        }
        public enum ProxyDirection
        {
            Reverse,
            Bind
        }
    }
}
