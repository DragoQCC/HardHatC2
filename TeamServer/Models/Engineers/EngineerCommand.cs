using System;
using System.Threading.Tasks;

namespace TeamServer.Models.Engineers;

[Serializable]
public class EngineerCommand
{
    public virtual string Name { get; }


    public virtual async Task Execute(EngineerTask task)
    {
        
    }
}