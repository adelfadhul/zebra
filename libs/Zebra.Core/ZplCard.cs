using zebra;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Zebra.Core
{
    public class ZplCard
    {
        private readonly PersonInfo personInfo;
        public ZplCard(PersonInfo data)
        {
            personInfo = data;
        }

        public string Text => $"""
    ^XA
    ^FO50,50^A0N,30,30^FD{personInfo.Name}^FS
    ^FO50,100^A0N,30,30^FDCPR: {personInfo.Cpr}^FS
    ^FO50,150^A0N,30,30^FDDate: {personInfo.Date}^FS
    ^XZ
    """;
    }
}
