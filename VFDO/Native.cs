
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace VirtualFiles
{
    internal static class NatConstants
    {
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        public const uint FD_CREATETIME = 0x00000008;
        public const uint FD_WRITESTIME = 0x00000020;
        public const uint FD_FILESIZE = 0x00000040;
        public const uint FD_PROGRESSUI = 0x00004000;
        public const uint FD_ATTRIBUTES = 0x00000004;
        public const uint FD_UNICODE = 0x80000000;

        public const int S_OK = 0;
        public const int S_FALSE = 1;
        public const int VARIANT_FALSE = 0;
        public const int VARIANT_TRUE = -1;

        public const int E_FAIL = -2147467259;
        public const int OLE_E_ADVISENOTSUPPORTED = -2147221501;

        public const int DRAGDROP_S_DROP = 0x00040100;
        public const int DRAGDROP_S_CANCEL = 0x00040101;
        public const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
        public const int DV_E_DVASPECT = -2147221397;
        public const int DV_E_FORMATETC = -2147221404;
        public const int DV_E_TYMED = -2147221399;

        public const short CF_BITMAP = 2;
        public const short CF_DIBV5 = 17;
    }

    internal class ClipboardFormatID
    {
        public static readonly short FILECONTENTS = (short)(DataFormats.GetFormat("FileContents").Id);
        public static readonly short FILEDESCRIPTORW = (short)(DataFormats.GetFormat("FileGroupDescriptorW").Id);
        public static readonly short UNTRUSTEDDRAGDROP = (short)(DataFormats.GetFormat("UntrustedDragDrop").Id);

        public static readonly short DISABLEDRAGTEXT = (short)(DataFormats.GetFormat("DisableDragText").Id);
        public static readonly short DROPDESCRIPTION = (short)(DataFormats.GetFormat("DropDescription").Id);
    }

    namespace NativeTypes
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FILEGROUPDESCRIPTOR
        {
            public UInt32 cItems;
            // Followed by 0 or more FILEDESCRIPTORs
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FILEDESCRIPTOR
        {
            public UInt32 dwFlags;
            public Guid clsid;
            public Int32 sizelcx;
            public Int32 sizelcy;
            public Int32 pointlx;
            public Int32 pointly;
            public UInt32 dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public UInt32 nFileSizeHigh;
            public UInt32 nFileSizeLow;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public Int32 cx;
            public Int32 cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHDRAGIMAGE
        {
            public SIZE size;
            public POINT offset;
            public nint hBitmap;
            public UInt32 colorref;
        }


        [Guid("00000121-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public partial interface IDropSource
        {
            [PreserveSig]
            int QueryContinueDrag(int fEscapePressed, uint grfKeyState);
            [PreserveSig]
            int GiveFeedback(uint dwEffect);
        }

        [ComImport]
        [Guid("3D8B0590-F691-11d2-8EA9-006097DF5BD4")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDataObjectAsyncCapability
        {
            void SetAsyncMode([In] Int32 fDoOpAsync);
            void GetAsyncMode([Out] out Int32 pfIsOpAsync);
            void StartOperation([In] IBindCtx pbcReserved);
            void InOperation([Out] out Int32 pfInAsyncOp);
            void EndOperation([In] Int32 hResult, [In] IBindCtx pbcReserved, [In] UInt32 dwEffects);
        }

        [ComImport]
        [Guid("DE5BF786-477A-11D2-839D-00C04FD918D0")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDragSourceHelper
        {
            void InitializeFromBitmap([In] ref SHDRAGIMAGE dragImage, [In] System.Runtime.InteropServices.ComTypes.IDataObject dataObject);

            void InitializeFromWindow([In] nint hWnd, [In] ref POINT pt, [In] System.Runtime.InteropServices.ComTypes.IDataObject dataObject);
        }

        [ComImport]
        [Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
        public class DragSoourceHelper
        {
        }
    }

    public static class NatMethods
    {
        public static bool SUCCEEDED(int hr)
        {
            return hr >= 0;
        }

        public static bool FAILED(int hr)
        {
            return hr < 0;
        }

        [DllImport("shell32.dll")]
        public static extern int SHCreateStdEnumFmtEtc(uint cfmt, FORMATETC[] afmt, out IEnumFORMATETC ppenumFormatEtc);

        [DllImport("ole32.dll", CharSet = CharSet.Auto, ExactSpelling = true, PreserveSig = false)]
        public static extern void DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, NativeTypes.IDropSource dropSource, int allowedEffects, int[] finalEffect);

        [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = true)]
        public static extern void ReleaseStgMedium(ref STGMEDIUM medium);

        [DllImport("Kernel32.dll")]
        public static extern nint GlobalSize(nint hGlobal);

    }

    public class NatHelpers
    {
        public static byte[] MarshalStruct<T>(T obj) where T : struct
        {
            var size = Marshal.SizeOf(obj);
            var buffer = new byte[size];

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            return buffer;
        }

        public static FILETIME MakeFileTime(DateTime dateTime)
        {
            var mtime = dateTime.ToLocalTime().ToFileTime(); // not UTC here?
            return new FILETIME
            {
                dwLowDateTime = (int)(mtime & 0xffffffff),
                dwHighDateTime = (int)(mtime >> 32),
            };
        }

        public static nint ToHGLOBAL(byte[] data)
        {
            var ptr = Marshal.AllocHGlobal(data.Length);
            if(ptr != 0)
                Marshal.Copy(data, 0, ptr, data.Length);
            return ptr;
        }
    }



}
