using System.Collections.Generic;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Framework.Tests
{
    public class TestPropertyChangedConsumer : IReactivePropertyNotifiedConsumer
    {
        public string LastPropertyChanged { get; set; }
        public List<string> PropertiesChanged { get; set; }

        public TestPropertyChangedConsumer()
        {
            PropertiesChanged = new List<string>();
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertiesChanged.Add(propertyName);
            LastPropertyChanged = propertyName;
        }
    }
}