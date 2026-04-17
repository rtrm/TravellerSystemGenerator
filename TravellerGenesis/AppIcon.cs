using System.Drawing;
using System.IO;

namespace TravellerGenesis
{
    internal static class AppIcon
    {
        private static Icon? _icon;

        public static Icon? Get()
        {
            if (_icon == null)
            {
                string path = Path.Combine(System.AppContext.BaseDirectory, "TravellerGenesis.ico");
                if (File.Exists(path))
                    _icon = new Icon(path);
            }
            return _icon;
        }
    }
}
