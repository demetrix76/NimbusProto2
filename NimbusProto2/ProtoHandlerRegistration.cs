using Microsoft.Win32;
using System.Reflection;

namespace NimbusProto2
{
    internal class ProtoHandlerRegistration : IDisposable
    {
        internal ProtoHandlerRegistration() 
        {
            RegisterHandler();
        }
        public void Dispose()
        {
            UnregisterHandler();
        }

        private bool RegisterHandler()
        {
            using var classesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Classes", true);
            if (classesKey == null)
                return false;

            var keyProtoName = classesKey.CreateSubKey("nimbuskeeper");
            if(keyProtoName == null) return false;

            keyProtoName.SetValue(null, "URL: nimbuskeeper");
            keyProtoName.SetValue("URL Protocol", "open");

            var keyShell = keyProtoName.CreateSubKey("shell");
            if (keyShell == null) return false;

            var keyOpen = keyShell.CreateSubKey("open");
            if (keyOpen == null) return false;

            var keyCommand = keyOpen.CreateSubKey("command");
            if (keyCommand == null) return false;

            var exePath = Application.ExecutablePath;
            keyCommand.SetValue(null, $"\"{exePath}\" \"%1\"");

            return true;
        }

        private void UnregisterHandler()
        {
            using var classesKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Classes", true);
            if (classesKey == null)
                return;

            classesKey.DeleteSubKeyTree("nimbuskeeper", false);
        }
    }
}
