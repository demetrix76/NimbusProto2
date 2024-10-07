
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using IDataObjectAsyncCapability = VirtualFiles.NativeTypes.IDataObjectAsyncCapability;

namespace VirtualFiles
{
    public partial class VFDO : IDataObject,
        IDataObjectAsyncCapability
    {

        #region IDataObject implementation
        int IDataObject.DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
        {
            Marshal.ThrowExceptionForHR(NatConstants.OLE_E_ADVISENOTSUPPORTED);
            throw new NotImplementedException();
        }

        void IDataObject.DUnadvise(int connection)
        {
            Marshal.ThrowExceptionForHR(NatConstants.OLE_E_ADVISENOTSUPPORTED);
            throw new NotImplementedException();
        }

        int IDataObject.EnumDAdvise(out IEnumSTATDATA? enumAdvise)
        {
            Marshal.ThrowExceptionForHR(NatConstants.OLE_E_ADVISENOTSUPPORTED);
            throw new NotImplementedException();
        }

        IEnumFORMATETC IDataObject.EnumFormatEtc(DATADIR direction)
        {
            if (direction == DATADIR.DATADIR_GET)
            {
                if (_dataDescriptors.Count == 0)
                    throw new InvalidOperationException("EnumFormatEtc called on an empty DataObject");

                if (NatMethods.SUCCEEDED(NatMethods.SHCreateStdEnumFmtEtc(
                        (uint)_dataDescriptors.Count, _dataDescriptors.Select(d => d.FormatDescriptor).ToArray(), out IEnumFORMATETC enumerator)))
                    return enumerator;

                Marshal.ThrowExceptionForHR(NatConstants.E_FAIL);
            }
            throw new NotImplementedException();
        }

        int IDataObject.GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
        {
            throw new NotImplementedException();
        }

        void IDataObject.GetData(ref FORMATETC format, out STGMEDIUM medium)
        {
            medium = new();
            ((IDataObject)this).GetDataHere(ref format, ref medium);
        }

        void IDataObject.GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
        {
            var hr = ((IDataObject)this).QueryGetData(ref format);
            if(NatMethods.FAILED(hr))
                Marshal.ThrowExceptionForHR(hr);

            var formatVal = format;

            // we don't have a FILECONTENTS descriptor ready until right now,
            // so we generate one on the fly;
            // drawback: it won't be reported in EnumFormatEtc; will that confuse Explorer or any other consumer?
            // do we need to generate a fake one there?

            var descriptor = IsFileContentRequested(ref format) ?
                new FileContentsDescriptor(_fileListSource, format.lindex)
             : _dataDescriptors.Where(d => d.Match(formatVal)).FirstOrDefault();

            if (descriptor == null)
                Marshal.ThrowExceptionForHR(NatConstants.DV_E_FORMATETC);

            // NOTE the original example was starting an async action here if the data requested was FILEDESCRIPTORW

            var (ptr, result) = descriptor!.GetData();

            if(NatMethods.FAILED(result))
                Marshal.ThrowExceptionForHR(result);

            medium.tymed = descriptor.FormatDescriptor.tymed;
            medium.unionmember = ptr;
        }

        int IDataObject.QueryGetData(ref FORMATETC format)
        {
            // at this stage there's no FILECONTENTS as the source is yet to be called,
            // so we just pretend it's here - we'll generate it on the fly

            if (IsFileContentRequested(ref format))
                return NatConstants.S_OK;

            var formatVal = format;

            var formatMatches = _dataDescriptors.Where(d => d.FormatDescriptor.cfFormat == formatVal.cfFormat);
            if (!formatMatches.Any())
                return NatConstants.DV_E_FORMATETC;

            var tymedMatches = formatMatches.Where(d => (d.FormatDescriptor.tymed & formatVal.tymed) != 0);
            if (!tymedMatches.Any())
                return NatConstants.DV_E_TYMED;

            var aspectMatches = tymedMatches.Where(d => d.FormatDescriptor.dwAspect == formatVal.dwAspect);
            if (!aspectMatches.Any())
                return NatConstants.DV_E_DVASPECT;

            return NatConstants.S_OK;
        }

        void IDataObject.SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
        {
            // The client reports the actual drop effect through this call,
            // but we don't really need to know it for the task we're working on;
            // Disabled for now as this code needs some refinement:
            // We should make sure we don't create duplicate DataDescriptors for repeated SetData calls
            throw new NotImplementedException();

#if false
            if(formatIn.tymed == TYMED.TYMED_HGLOBAL || formatIn.tymed == TYMED.TYMED_ISTREAM)
            {
                _dataDescriptors.Add(new RawDataDescriptor(formatIn, ref medium));
            }
            else
            {
                Marshal.ThrowExceptionForHR(NatConstants.DV_E_TYMED); 
            }
            if(release)
            {
                NatMethods.ReleaseStgMedium(ref medium);
            }
#endif
        }
        #endregion

        static bool IsFileContentRequested(ref FORMATETC format)
        {
            return format.cfFormat == ClipboardFormatID.FILECONTENTS
                && format.dwAspect == DVASPECT.DVASPECT_CONTENT
                && 0 != (format.tymed & TYMED.TYMED_ISTREAM);
        }

        #region IDataObjectAsyncCapability implementation

        private bool _inOperation;
        void IDataObjectAsyncCapability.SetAsyncMode(int fDoOpAsync)
        {
            IsAsynchronous = fDoOpAsync == NatConstants.VARIANT_TRUE;
        }

        void IDataObjectAsyncCapability.GetAsyncMode(out int pfIsOpAsync)
        {
            pfIsOpAsync = IsAsynchronous ? NatConstants.VARIANT_TRUE : NatConstants.VARIANT_FALSE;
        }

        void IDataObjectAsyncCapability.StartOperation(IBindCtx pbcReserved)
        {
            _inOperation = true;
            AsyncBegin?.Invoke();
        }

        void IDataObjectAsyncCapability.InOperation(out int pfInAsyncOp)
        {
            pfInAsyncOp = _inOperation ? NatConstants.VARIANT_TRUE : NatConstants.VARIANT_FALSE;
        }

        void IDataObjectAsyncCapability.EndOperation(int hResult, IBindCtx pbcReserved, uint dwEffects)
        {
            _inOperation = false;
            AsyncEnd?.Invoke();
        }

        #endregion
    }
}
