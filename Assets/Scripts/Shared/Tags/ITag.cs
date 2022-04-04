using System;
using System.Collections.Generic;

namespace Assets.Scripts.Shared.Tags
{
    public interface ITag
    {
        Guid Id { get; }
        string Name { get; }
        List<string> Groups { get; }
    }
    
}