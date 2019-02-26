using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryHelper
{
    public class RegistryHelper
    {
        const string REGISTRY_PATH = @"SOFTWARE\WOW6432Node\C4Security\C4PSS Server";

        public static string GetRegistryValue(string key)
        {
            RegistryKey _key;
            _key = Registry.LocalMachine.CreateSubKey(REGISTRY_PATH);

            var value = _key.GetValue(key);
            if (value == null)
            {
                _key.Close();
                return null;
            }

            _key.Close();
            return value.ToString();
        }

        public static string[] GetRegistrySubKeyNames(string regpath)
        {
            RegistryKey _key;
            _key = Registry.LocalMachine.CreateSubKey(regpath);

            var value = _key.GetSubKeyNames();
            if (value == null)
            {
                _key.Close();
                return null;
            }

            _key.Close();
            return value;
        }

        public static void SetRegistryKey(string key, string data)
        {
            using (var registryKey = Registry.LocalMachine.CreateSubKey(REGISTRY_PATH))
            {
                if (registryKey == null)
                    throw new ArgumentNullException("registryKey is null.");

                if (registryKey.GetValue(key) == null || registryKey.GetValue(key).ToString() != data)
                    registryKey.SetValue(key, data);
            }

        }
    }
}
 
