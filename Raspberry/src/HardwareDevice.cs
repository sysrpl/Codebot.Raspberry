namespace Raspberry
{
    public class HardwareDevice
    {
        string Info(string ident)
        {
            var items = (DeviceAttribute[])GetType().GetCustomAttributes(typeof(DeviceAttribute), true);
            if (items.Length == 0) 
                return string.Empty;
            switch (ident)
            {
                case nameof(Model):
                    return items[0].Model;
                case nameof(Name):
                    return items[0].Name;
                case nameof(Category):
                    return items[0].Category;
                case nameof(Description):
                    return items[0].Description;
                case nameof(Remarks):
                    return items[0].Remarks;
                default:
                    return string.Empty;
            }
        }

        public string Model { get => Info(nameof(Model)); }
        public string Name { get => Info(nameof(Name)); }
        public string Category { get => Info(nameof(Category)); }
        public string Description { get => Info(nameof(Description)); }
        public string Remarks { get => Info(nameof(Remarks)); }
        public virtual void Update() { }
    }
}
