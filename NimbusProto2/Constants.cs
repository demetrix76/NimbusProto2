using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NimbusProto2
{
    internal static class Constants
    {
        public const string DiskAPIUrl = "https://cloud-api.yandex.net/v1/disk/";

        public static class MIME
        {
            public const string JSON = "application/json";
        }

        public static class StockImageKeys
        {
            public const string Folder = "STOCK:FOLDER";
            public const string File = "STOCK:FILE";
        }

    }
}
