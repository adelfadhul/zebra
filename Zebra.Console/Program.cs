using System.Net.Sockets;
using System.Text.Json;


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

var zpl= new Zpl(data).Text;
switch (choice)
{
    case "1":
        await new NetworkSender().SendToNetworkPrinterAsync("192.168.100.160", 9100, zpl);
        break;

    case "2":
        bool sent = new WireSender().SendStringToPrinter(printerName, zpl);
        Console.WriteLine(sent ? "✅ Label sent to Windows printer." : "❌ Failed to send label.");
        break;

    default:
        Console.WriteLine("❌ Invalid choice.");
        break;
}

