// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace WindowsPdbReader
{
    internal enum SYM
    {
        S_END = 0x0006,  // Block, procedure, "with" or thunk end
        S_OEM = 0x0404,  // OEM defined symbol

        S_REGISTER_ST = 0x1001,  // Register variable
        S_CONSTANT_ST = 0x1002,  // constant symbol
        S_UDT_ST = 0x1003,  // User defined type
        S_COBOLUDT_ST = 0x1004,  // special UDT for cobol that does not symbol pack
        S_MANYREG_ST = 0x1005,  // multiple register variable
        S_BPREL32_ST = 0x1006,  // BP-relative
        S_LDATA32_ST = 0x1007,  // Module-local symbol
        S_GDATA32_ST = 0x1008,  // Global data symbol
        S_PUB32_ST = 0x1009,  // a internal symbol (CV internal reserved)
        S_LPROC32_ST = 0x100a,  // Local procedure start
        S_GPROC32_ST = 0x100b,  // Global procedure start
        S_VFTABLE32 = 0x100c,  // address of virtual function table
        S_REGREL32_ST = 0x100d,  // register relative address
        S_LTHREAD32_ST = 0x100e,  // local thread storage
        S_GTHREAD32_ST = 0x100f,  // global thread storage

        S_LPROCMIPS_ST = 0x1010,  // Local procedure start
        S_GPROCMIPS_ST = 0x1011,  // Global procedure start

        // new symbol records for edit and continue information

        S_FRAMEPROC = 0x1012,  // extra frame and proc information
        S_COMPILE2_ST = 0x1013,  // extended compile flags and info

        // new symbols necessary for 16-bit enumerates of IA64 registers
        // and IA64 specific symbols

        S_MANYREG2_ST = 0x1014,  // multiple register variable
        S_LPROCIA64_ST = 0x1015,  // Local procedure start (IA64)
        S_GPROCIA64_ST = 0x1016,  // Global procedure start (IA64)

        // Local symbols for IL
        S_LOCALSLOT_ST = 0x1017,  // local IL sym with field for local slot index
        S_PARAMSLOT_ST = 0x1018,  // local IL sym with field for parameter slot index

        S_ANNOTATION = 0x1019,  // Annotation string literals

        // symbols to support managed code debugging
        S_GMANPROC_ST = 0x101a,  // Global proc
        S_LMANPROC_ST = 0x101b,  // Local proc
        S_RESERVED1 = 0x101c,  // reserved
        S_RESERVED2 = 0x101d,  // reserved
        S_RESERVED3 = 0x101e,  // reserved
        S_RESERVED4 = 0x101f,  // reserved
        S_LMANDATA_ST = 0x1020,
        S_GMANDATA_ST = 0x1021,
        S_MANFRAMEREL_ST = 0x1022,
        S_MANREGISTER_ST = 0x1023,
        S_MANSLOT_ST = 0x1024,
        S_MANMANYREG_ST = 0x1025,
        S_MANREGREL_ST = 0x1026,
        S_MANMANYREG2_ST = 0x1027,
        S_MANTYPREF = 0x1028,  // Index for type referenced by name from metadata
        S_UNAMESPACE_ST = 0x1029,  // Using namespace

        // Symbols w/ SZ name fields. All name fields contain utf8 encoded strings.
        S_ST_MAX = 0x1100,  // starting point for SZ name symbols

        S_OBJNAME = 0x1101,  // path to object file name
        S_THUNK32 = 0x1102,  // Thunk Start
        S_BLOCK32 = 0x1103,  // block start
        S_WITH32 = 0x1104,  // with start
        S_LABEL32 = 0x1105,  // code label
        S_REGISTER = 0x1106,  // Register variable
        S_CONSTANT = 0x1107,  // constant symbol
        S_UDT = 0x1108,  // User defined type
        S_COBOLUDT = 0x1109,  // special UDT for cobol that does not symbol pack
        S_MANYREG = 0x110a,  // multiple register variable
        S_BPREL32 = 0x110b,  // BP-relative
        S_LDATA32 = 0x110c,  // Module-local symbol
        S_GDATA32 = 0x110d,  // Global data symbol
        S_PUB32 = 0x110e,  // a internal symbol (CV internal reserved)
        S_LPROC32 = 0x110f,  // Local procedure start
        S_GPROC32 = 0x1110,  // Global procedure start
        S_REGREL32 = 0x1111,  // register relative address
        S_LTHREAD32 = 0x1112,  // local thread storage
        S_GTHREAD32 = 0x1113,  // global thread storage

        S_LPROCMIPS = 0x1114,  // Local procedure start
        S_GPROCMIPS = 0x1115,  // Global procedure start
        S_COMPILE2 = 0x1116,  // extended compile flags and info
        S_MANYREG2 = 0x1117,  // multiple register variable
        S_LPROCIA64 = 0x1118,  // Local procedure start (IA64)
        S_GPROCIA64 = 0x1119,  // Global procedure start (IA64)
        S_LOCALSLOT = 0x111a,  // local IL sym with field for local slot index
        S_SLOT = S_LOCALSLOT,  // alias for LOCALSLOT
        S_PARAMSLOT = 0x111b,  // local IL sym with field for parameter slot index

        // symbols to support managed code debugging
        S_LMANDATA = 0x111c,
        S_GMANDATA = 0x111d,
        S_MANFRAMEREL = 0x111e,
        S_MANREGISTER = 0x111f,
        S_MANSLOT = 0x1120,
        S_MANMANYREG = 0x1121,
        S_MANREGREL = 0x1122,
        S_MANMANYREG2 = 0x1123,
        S_UNAMESPACE = 0x1124,  // Using namespace

        // ref symbols with name fields
        S_PROCREF = 0x1125,  // Reference to a procedure
        S_DATAREF = 0x1126,  // Reference to data
        S_LPROCREF = 0x1127,  // Local Reference to a procedure
        S_ANNOTATIONREF = 0x1128,  // Reference to an S_ANNOTATION symbol
        S_TOKENREF = 0x1129,  // Reference to one of the many MANPROCSYM's

        // continuation of managed symbols
        S_GMANPROC = 0x112a,  // Global proc
        S_LMANPROC = 0x112b,  // Local proc

        // short, light-weight thunks
        S_TRAMPOLINE = 0x112c,  // trampoline thunks
        S_MANCONSTANT = 0x112d,  // constants with metadata type info

        // native attributed local/parms
        S_ATTR_FRAMEREL = 0x112e,  // relative to virtual frame ptr
        S_ATTR_REGISTER = 0x112f,  // stored in a register
        S_ATTR_REGREL = 0x1130,  // relative to register (alternate frame ptr)
        S_ATTR_MANYREG = 0x1131,  // stored in >1 register

        // Separated code (from the compiler) support
        S_SEPCODE = 0x1132,

        S_LOCAL = 0x1133,  // defines a local symbol in optimized code
        S_DEFRANGE = 0x1134,  // defines a single range of addresses in which symbol can be evaluated
        S_DEFRANGE2 = 0x1135,  // defines ranges of addresses in which symbol can be evaluated

        S_SECTION = 0x1136,  // A COFF section in a PE executable
        S_COFFGROUP = 0x1137,  // A COFF group
        S_EXPORT = 0x1138,  // A export

        S_CALLSITEINFO = 0x1139,  // Indirect call site information
        S_FRAMECOOKIE = 0x113a,  // Security cookie information

        S_DISCARDED = 0x113b,  // Discarded by LINK /OPT:REF (experimental, see richards)

        S_RECTYPE_MAX,              // one greater than last
        S_RECTYPE_LAST = S_RECTYPE_MAX - 1,

    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct ManProcSym
    {
        // internal ushort reclen;    // Record length [SYMTYPE]
        // internal ushort rectyp;    // S_GMANPROC, S_LMANPROC, S_GMANPROCIA64 or S_LMANPROCIA64
        internal uint parent;     // pointer to the parent
        internal uint end;        // pointer to this blocks end
        internal uint next;       // pointer to next symbol
        internal uint len;        // Proc length
        internal uint dbgStart;   // Debug start offset
        internal uint dbgEnd;     // Debug end offset
        internal uint token;      // COM+ metadata token for method
        internal uint off;
        internal ushort seg;
        internal byte flags;      // (CV_PROCFLAGS) Proc flags
        internal ushort retReg;     // Register return value is in (may not be used for all archs)
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct CV_LineSection
    {
        internal uint off;
        internal ushort sec;
        internal ushort flags;
        internal uint cod;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CV_SourceFile
    {
        internal uint index;          // Index to file in checksum section.
        internal uint count;          // Number of CV_Line records.
        internal uint linsiz;         // Size of CV_Line recods.
    }

    [Flags]
    internal enum CV_Line_Flags : uint
    {
        linenumStart = 0x00ffffff,   // line where statement/expression starts
        deltaLineEnd = 0x7f000000,   // delta to line where statement ends (optional)
        fStatement = 0x80000000,   // true if a statement linenumber, else an expression line num
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct CV_Line
    {
        internal uint offset;         // Offset to start of code bytes for line number
        internal uint flags;          // (CV_Line_Flags)
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct CV_Column
    {
        internal ushort offColumnStart;
        internal ushort offColumnEnd;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct CV_FileCheckSum
    {
        internal uint name;           // Index of name in name table.
        internal byte len;            // Hash length
        internal byte type;           // Hash type
    }

    internal enum DEBUG_S_SUBSECTION
    {
        SYMBOLS = 0xF1,
        LINES = 0xF2,
        STRINGTABLE = 0xF3,
        FILECHKSMS = 0xF4,
        FRAMEDATA = 0xF5,
    }
}