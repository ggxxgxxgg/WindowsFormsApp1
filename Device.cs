using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Config
    {
        public UInt16 OSCF { get; set; }
        public UInt16 PRTRM { get; set; }
        public UInt16 VTST { get; set; }
        public UInt16 BGST { get; set; }
        public UInt16 CBRG { get; set; }
        public UInt16 CZRO { get; set; }
        public UInt16 CVGA { get; set; }
        public UInt16 FVGA { get; set; }
        public UInt16 LFBW { get; set; }
        public UInt16 TCMP { get; set; }

        public UInt16 OPHS { get; set; }
        public UInt16 SOEN { get; set; }
        public UInt16 TICACEN { get; set; }
        public UInt16 TP2IEN { get; set; }
        public UInt16 TC2VOEN { get; set; }
        public UInt16 TPGA { get; set; }
        public UInt16 TLPFIEN { get; set; }

        public string ORDER_S { get; set; }
        public string ORDER_A { get; set; }
        public UInt32 YMW { get; set; }

    }
    internal class Device
    {
        public enum OPT_BLOCK
        {
            B_NOW,
            B_00_7F,
            B_80_FF
        }
        public Int16 Opt_b = (short)OPT_BLOCK.B_00_7F;

        public Byte[] reg = new Byte[31];
        public UInt16 OSCF;//0~15
        public UInt16 PRTRM;//0~15
        public UInt16 VTST;//0~63
        public UInt16 BGST;
        public UInt16 CBRG;
        public UInt16 CZRO;
        public UInt16 CVGA;
        public UInt16 FVGA;
        public UInt16 LFBW;
        public UInt16 TCMP;

        public UInt16 OPHS;
        public UInt16 SOEN;
        public UInt16 TICACEN;
        public UInt16 TP2IEN;
        public UInt16 TC2VOEN;
        public UInt16 TPGA;
        public UInt16 TLPFIEN;

    }
}
