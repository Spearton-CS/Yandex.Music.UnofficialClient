using System.Globalization;
using System.Runtime.InteropServices;

namespace Yandex.Music.UnofficialClient
{
    public static class DwmApi
    {
        private static string ToBgr(Color c) => $"{c.B:X2}{c.G:X2}{c.R:X2}";
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attribute, int[] attributeValue, int attributeSize);
        private const int DWWMA_BORDER_COLOR = 34, DWWMA_CAPTION_COLOR = 35, DWMWA_TEXT_COLOR = 36;
        public static void CustomWindow(Color captionColor, Color fontColor, Color borderColor, IntPtr handle)
        {
            _ = DwmSetWindowAttribute(handle, DWWMA_CAPTION_COLOR, new int[] { int.Parse(ToBgr(captionColor), NumberStyles.HexNumber) }, 4);
            _ = DwmSetWindowAttribute(handle, DWMWA_TEXT_COLOR, new int[] { int.Parse(ToBgr(fontColor), NumberStyles.HexNumber) }, 4);
            _ = DwmSetWindowAttribute(handle, DWWMA_BORDER_COLOR, new int[] { int.Parse(ToBgr(borderColor), NumberStyles.HexNumber) }, 4);
        }
    }
}