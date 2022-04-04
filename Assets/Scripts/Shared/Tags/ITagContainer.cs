using System.Collections.Generic;

namespace Assets.Scripts.Shared.Tags
{
    public interface ITagContainer
    {
        List<ITag> Tags { get; }      
    }
}