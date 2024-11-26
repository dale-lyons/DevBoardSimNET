using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Design;

//using Boards;

namespace Preferences
{
    public abstract class PreferencesBase
    {
        public abstract void Default();

        public static T Load<T>(string key) where T : class, new()
        {
            var serializer = new XmlSerializer(typeof(T));
            string filename = formFilename(key);
            //return new T();

            if (!File.Exists(filename))
                return new T();
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    var ret = serializer.Deserialize(sr) as T;
                    if (ret != null)
                        return ret;
                    return new T();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new T();
            }
        }

        private static string formFilename(string key)
        {
            string configFile = Path.Combine(Path.GetDirectoryName(Application.CommonAppDataPath), key);
            return Path.ChangeExtension(configFile, ".xml");
        }

        public static void Save<T>(PreferencesBase configuration, string key)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StreamWriter(formFilename(key)))
            {
                serializer.Serialize(writer, configuration);
                writer.Close();
            }
        }
    }
}