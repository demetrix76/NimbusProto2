namespace VirtualFiles
{
    public class DefaultDropSource : NativeTypes.IDropSource
    {
        public int GiveFeedback(uint dwEffect)
        {
            return NatConstants.DRAGDROP_S_USEDEFAULTCURSORS;
        }

        public int QueryContinueDrag(int fEscapePressed, uint grfKeyState)
        {
            var escapePressed = (0 != fEscapePressed);

            if (escapePressed)
            {
                return NatConstants.DRAGDROP_S_CANCEL;
            }
            else if (0 == (grfKeyState & 1))
            {
                return NatConstants.DRAGDROP_S_DROP;
            }

            return NatConstants.S_OK;
        }

        public static int DoDragDrop(VFDO dataObject, DragDropEffects preferredEffects)
        {
            var finalEffects = new int[1];
            NatMethods.DoDragDrop(dataObject, new DefaultDropSource(), (int)preferredEffects, finalEffects);
            return finalEffects[0];
        }
    }
}
