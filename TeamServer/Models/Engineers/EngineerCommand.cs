using System;
using System.Threading.Tasks;


namespace TeamServer.Models.Engineers;

[Serializable]
public abstract class EngineerCommand 
{
    public abstract string Name { get; }

    public abstract Task Execute(EngineerTask task);
}