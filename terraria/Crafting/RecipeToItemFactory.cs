using System;

namespace WPFTerraria
{
    public static class RecipeToItemFactory
    {
        private static readonly Dictionary<string, Type> _nameToType;

        static RecipeToItemFactory()
        {
            //create dictionary for name to type mapping
            _nameToType = new Dictionary<string,
Type>(StringComparer.OrdinalIgnoreCase);

            //get all item subclasses
            var itemType = typeof(Item);
            var types = itemType.Assembly.GetTypes()
                .Where(thisType => !thisType.IsAbstract &&
itemType.IsAssignableFrom(thisType));

            //fill dictionary with each item name and type
            foreach (var thisType in types)
            {
                try
                {
                    var instance = (Item)Activator.CreateInstance(thisType);
                    if (!string.IsNullOrWhiteSpace(instance.Name) &&
!_nameToType.ContainsKey(instance.Name))
                        _nameToType[instance.Name] = thisType;
                }
                catch { Console.WriteLine($"error creating instance type in
RecipeToItemFactory & {thisType}"); }
            }
        }

        public static Item CreateByName(string name)
        {
            //return null if invalid input
            if (string.IsNullOrWhiteSpace(name)) return null;

            //create new instance if found in dictionary
            if (_nameToType.TryGetValue(name, out var type))
            {
                return (Item)Activator.CreateInstance(type);
            }

            //return null if not found
            return null;
        }
    }
}
