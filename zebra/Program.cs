using System.Net.Sockets;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Text;
using zebra;

Console.WriteLine("== Zebra Label Sender ==");

Console.WriteLine("Select printing method:");
Console.WriteLine("1. Network (IP)");
Console.WriteLine("2. Windows Printer (ZDesigner ZT231-203dpi ZPL)");
Console.Write("Enter choice (1 or 2): ");
string? choice = Console.ReadLine();

const string jsonPath = "data.json";
const string printerName = "ZDesigner ZT231-203dpi ZPL";

if (!File.Exists(jsonPath))
{
    Console.WriteLine("❌ data.json file not found.");
    return;
}

string json = await File.ReadAllTextAsync(jsonPath);
var data = JsonSerializer.Deserialize<PersonInfo>(json);

if (data is null)
{
    Console.WriteLine("❌ Failed to parse JSON.");
    return;
}

string zpl = $"""
    ^XA
    ^FO50,50^A0N,40,40^FDName: {data.Name}^FS
    ^FO50,100^A0N,40,40^FDCPR: {data.Cpr}^FS
    ^FO50,150^A0N,40,40^FDDate: {data.Date}^FS
    ^XZ
    """;
string zpl2 = $$"""
        ^XA
    ^MMT
    ^PW406
    ^LL203
    ^LS0
    ^FT17,46^A0N,31,30^FH\^CI28^FDName:^FS^CI27
    ^FT17,87^A0N,31,30^FH\^CI28^FDCPR:^FS^CI27
    ^FT17,121^A0N,25,25^FH\^CI28^FDDoctor:^FS^CI27
    ^FT17,153^A0N,25,25^FH\^CI28^FDDate:^FS^CI27
    ^FT108,46^A0N,31,30^FH\^CI28^FDHasan Ahmed Hasan^FS^CI27
    ^FT108,121^A0N,25,25^FH\^CI28^FDDr. Smith^FS^CI27
    ^FT108,87^A0N,31,30^FH\^CI28^FD870206630^FS^CI27
    ^SLS,1
    ^FT108,153^A0N,25,25
    ^FC%,{,#
    ^FH\^CI28^FD%d-%b-%Y^FS^CI27
    ^PQ1,0,1,Y
    ^XZ
    """;
switch (choice)
{
    case "1":
        await SendToNetworkPrinterAsync("192.168.100.160", 9100, zpl);
        break;

    case "2":
        bool sent = RawPrinterHelper.SendStringToPrinter(printerName, zpl);
        Console.WriteLine(sent ? "✅ Label sent to Windows printer." : "❌ Failed to send label.");
        break;

    default:
        Console.WriteLine("❌ Invalid choice.");
        break;
}

// === Helper ===

static async Task SendToNetworkPrinterAsync(string ip, int port, string zpl)
{
    try
    {
        using TcpClient client = new();
        await client.ConnectAsync(ip, port);
        using NetworkStream stream = client.GetStream();
        byte[] buffer = Encoding.UTF8.GetBytes(zpl);
        await stream.WriteAsync(buffer);
        Console.WriteLine("✅ Label sent to network printer.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Network error: {ex.Message}");
    }
}

record PersonInfo(string? Name, string? Cpr, string? Date);

// === RawPrinterHelper ===

public static class RawPrinterHelper
{
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
    public struct DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDocName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDataType;
    }

    public static bool SendStringToPrinter(string printerName, string zplCommand)
    {
        IntPtr pBytes;
        int dwCount = Encoding.UTF8.GetByteCount(zplCommand);
        pBytes = Marshal.StringToCoTaskMemAnsi(zplCommand);
        bool success = SendBytesToPrinter(printerName, pBytes, dwCount);
        Marshal.FreeCoTaskMem(pBytes);
        return success;
    }

    private static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, int dwCount)
    {
        DOCINFOA di = new DOCINFOA
        {
            pDocName = "ZPL Label",
            pDataType = "RAW"
        };

        bool success = false;
        if (OpenPrinter(szPrinterName.Normalize(), out IntPtr hPrinter, IntPtr.Zero))
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
}
