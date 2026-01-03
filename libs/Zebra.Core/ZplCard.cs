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
 ^MMT
 ^PW406
 ^LL203
 ^LS0
 ^FT17,46^A0N,31,30^FH\^CI28^FDName:^FS^CI27
 ^FT17,87^A0N,31,30^FH\^CI28^FDCPR:^FS^CI27
 ^FT17,121^A0N,25,25^FH\^CI28^FDDoctor:^FS^CI27
 ^FT17,153^A0N,25,25^FH\^CI28^FDDate:^FS^CI27
 ^FT108,46^A0N,31,30^FH\^CI28^FD{personInfo.Name}^FS^CI27
 ^FT108,87^A0N,31,30^FH\^CI28^FD{personInfo.Cpr}^FS^CI27
 ^FT108,121^A0N,25,25^FH\^CI28^FD{personInfo.Doctor}^FS^CI27
 ^FT108,153^A0N,25,25^FH\^CI28^FD{personInfo.Date:dd-MMM-yyyy}^FS^CI27
 ^PQ1,0,1,Y
 ^XZ
 """;

        public string Text_old => $"""
    ^XA
    ^FO50,50^A0N,30,30^FD{personInfo.Name}^FS
    ^FO50,100^A0N,30,30^FDCPR: {personInfo.Cpr}^FS
    ^FO50,150^A0N,30,30^FDDate: {personInfo.Date}^FS
    ^XZ
    """;
    }
}
