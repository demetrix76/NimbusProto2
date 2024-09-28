using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NimbusProto2.Properties
{
    internal sealed partial class Settings : IDisposable
    {
        public void Dispose()
        {
            Save();
        }

        public string DeviceId
        {
            get
            {
                var deviceId = DeviceIdImpl;
                if (deviceId == null || deviceId == "")
                {
                    deviceId = Guid.NewGuid().ToString();
                    DeviceIdImpl = deviceId;
                }
                return deviceId;
            }
        }

        public string DeviceName { get { return System.Environment.MachineName; } }
        public string ConfirmationURI { get { return "nimbuskeeper://confirmation"; } }
        public string ClientID { get { return "c622984c846e4b1eb0713067cb51ac23"; } }
        public string ClientSecret { get { return "593003631bcb45dca8fae5381bf2989a"; } }
    }
}