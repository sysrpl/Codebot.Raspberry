using System;

namespace Raspberry
{
    public class DeviceAttribute : Attribute
    {
        public DeviceAttribute(string model, string name)
        {
            Model = model;
            Name = name;
        }

        public string Model { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
    }
}
