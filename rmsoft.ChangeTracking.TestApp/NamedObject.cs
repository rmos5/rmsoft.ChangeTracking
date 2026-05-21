namespace rmsoft.ChangeTracking.TestApp
{
    public partial class NamedObject : TrackedObjectBase
    {
        private string? name;

        [PropertyChangeTracker]
        public string? Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;

                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string? description;

        [PropertyChangeTracker]
        public string? Description
        {
            get => description;
            set
            {
                if (description == value)
                    return;

                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public NamedObject()
        {
        }

        public NamedObject(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
