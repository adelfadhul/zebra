using System.Runtime.InteropServices;
using System.Text;
using zebra;


namespace Zebra.Core.Senders
{
    public class WireSender
    {
        private readonly string _printerName;

        public WireSender(string printerName)
        {
            _printerName = printerName ?? throw new ArgumentNullException(nameof(printerName));
        }

        public bool SendCardToPrinter(string name, string cpr)
        {
            var personInfo = new PersonInfo(name, cpr,DateTime.Now.ToShortDateString());
            return SendStringToPrinter(new ZplCard(personInfo).Text);
        }
        private bool SendStringToPrinter(string zplCommand)
        {
            nint pBytes;
            int dwCount = Encoding.UTF8.GetByteCount(zplCommand);
            pBytes = Marshal.StringToCoTaskMemAnsi(zplCommand);
            bool success = SendBytesToPrinter(pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return success;
        }

        private bool SendBytesToPrinter(nint pBytes, int dwCount)
        {
            DOCINFOA di = new DOCINFOA
            {
                pDocName = "ZPL Label",
                pDataType = "RAW"
            };

            bool success = false;
            if (OpenPrinter(_printerName.Normalize(), out nint hPrinter, nint.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, ref di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        success = WritePrinter(hPrinter, pBytes, dwCount, out _);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            return success;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] ref DOCINFOA di);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", SetLastError = true)]
        static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential)]
        private struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDataType;
        }
    }
}
