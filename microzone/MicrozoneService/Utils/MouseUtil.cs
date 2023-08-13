using System.Runtime.InteropServices;

namespace MicrozoneService
{
    public static class MouseUtil
    {
        private const uint SPI_GETMOUSE = 0x0003;
        private const uint SPI_SETMOUSE = 0x0004;
        private const uint SPIF_SENDCHANGE = 0x0002;

        public static void SetAcceleration(uint acceleration)
        {
            MOUSEKEYS mouseKeys = GetMouseInfo();
            if (mouseKeys.iMaxSpeed == acceleration)
                return;

            mouseKeys.iMaxSpeed = acceleration;
            SystemParametersInfo(SPI_SETMOUSE, 0, ref mouseKeys, SPIF_SENDCHANGE);
        }

        private static MOUSEKEYS GetMouseInfo()
        {
            MOUSEKEYS mouseKeys = new MOUSEKEYS();
            mouseKeys.cbSize = (uint)Marshal.SizeOf(typeof(MOUSEKEYS));

            SystemParametersInfo(SPI_GETMOUSE, 0, ref mouseKeys, 0);
            return mouseKeys;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEKEYS
        {
            public uint cbSize;
            public uint dwFlags;
            public uint iMaxSpeed;
            public uint iTimeToMaxSpeed; 
            public uint iCtrlSpeed;
            public uint dwReserved1;
            public uint dwReserved2;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref MOUSEKEYS pvParam, uint fWinIni);
    }
}