using HardHatCore.TeamServer.Models.Extras;
using SQLite;
using HardHatCore.TeamServer.Models;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("PivotProxy")]
    public class PivotProxy_DAO
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
       
        [Column("EngineerId")]
        public string EngineerId { get; set; }
      
        [Column("BindPort")]
        public string BindPort { get; set; }
       
        [Column("FwdHost")]
        public string FwdHost { get; set; }
       
        [Column("FwdPort")]
        public string FwdPort { get; set; }
        
        [Column("pivotType")]
        public PivotProxyType pivotType { get; set; }
       
        [Column("pivotDirection")]
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

        //create a implicit conversion from the DAO to the Model
        public static implicit operator PivotProxy(PivotProxy_DAO dao)
        {
            return new PivotProxy
            {
                EngineerId = dao.EngineerId,
                BindPort = dao.BindPort,
                FwdHost = dao.FwdHost,
                FwdPort = dao.FwdPort,
                pivotType = (PivotProxy.PivotProxyType)dao.pivotType,
                pivotDirection = (PivotProxy.ProxyDirection)dao.pivotDirection
            };
        }

        //create a implicit conversion from the Model to the DAO
        public static implicit operator PivotProxy_DAO(PivotProxy model)
        {
            return new PivotProxy_DAO
            {
                EngineerId = model.EngineerId,
                BindPort = model.BindPort,
                FwdHost = model.FwdHost,
                FwdPort = model.FwdPort,
                pivotType = (PivotProxyType)model.pivotType,
                pivotDirection = (ProxyDirection)model.pivotDirection
            };
        }

    }
}
