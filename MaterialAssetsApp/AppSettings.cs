using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterialAssetsApp
{
    public static class AppSettings
    {
        private static readonly string _path =
            System.IO.Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData),
                "MaterialAssetsApp", "settings.txt");

        public static int LastEmployeeID
        {
            get
            {
                try
                {
                    System.IO.Directory.CreateDirectory(
                        System.IO.Path.GetDirectoryName(_path));
                    var text = System.IO.File.ReadAllText(_path);
                    return int.Parse(text.Trim());
                }
                catch { return 0; }
            }
            set
            {
                try
                {
                    System.IO.Directory.CreateDirectory(
                        System.IO.Path.GetDirectoryName(_path));
                    System.IO.File.WriteAllText(_path, value.ToString());
                }
                catch { }
            }
        }
    }
}
