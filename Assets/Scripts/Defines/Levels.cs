using System;
using System.Collections.Generic;
using Assets.Scripts.Shared.Tags;

namespace Assets.Scripts.Defines
{
    public class LevelTag : Tag
    {
        public LevelTag(Guid id, string name) : base(id, name, "Level")
        {
        }
    }


    public class AvailableLevels : ITagContainer
    {
        public List<ITag> Tags
        {
            get
            {
                return new List<ITag>() { 
                    new LevelTag(new Guid("17b20e46-025c-4373-be37-b9109ac669f9"), "Location_01"),
                    new LevelTag(new Guid("b47642bf-788b-49e9-89f4-0b5edd75b297"), "Location_02"),
                    new LevelTag(new Guid("735710d4-c738-4925-9cc0-da237b56b1a5"), "Location_03"),
                    new LevelTag(new Guid("d84263a3-8d5f-4273-97c0-4780e85f5d58"), "Location_04"),
                };
            }
        }
    }


}