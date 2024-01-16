using System.ComponentModel;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace HardHatCore.ContractorSystem.InvocationPoints
{
    //todo: Add a way to specify which properties to watch for changes on. Can prob use an attribute to filter out properties that don't need to be watched.
    internal class PropertyChangeAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.ImplementInterface(builder.Target, typeof(INotifyPropertyChanged), OverrideStrategy.Ignore);

            foreach (var property in builder.Target.Properties.Where(p =>
                         !p.IsAbstract && p.Writeability == Writeability.All))
            {
                builder.Advice.OverrideAccessors(property, null, nameof(this.OverridePropertySetter));
            }
        }

        [InterfaceMember]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Introduce(WhenExists = OverrideStrategy.Ignore)]
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(meta.This, new PropertyChangedEventArgs(name));

        [Template]
        private dynamic OverridePropertySetter(dynamic value)
        {
            if (value != meta.Target.Property.Value)
            {
                meta.Proceed();
                OnPropertyChanged(meta.Target.Property.Name);
            }

            return value;
        }

    }
}
