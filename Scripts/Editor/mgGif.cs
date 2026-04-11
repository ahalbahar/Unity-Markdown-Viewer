// ============================================================
// File:    mgGif.cs
// Purpose: A lightweight GIF decoder for Unity.
// Author:  Ahmad Albahar
// ============================================================

//#define mgGIF_UNSAFE

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace AB.MDV.GIF
{
    /// <summary>
    /// Represents a single frame of a GIF image.
    /// </summary>
    public class Image : ICloneable
    {
        /// <summary>
        /// Width of the image frame.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the image frame.
        /// </summary>
        public int Height;

        /// <summary>
        /// Delay before the next frame in milliseconds.
        /// </summary>
        public int Delay;

        /// <summary>
        /// Raw pixel data for the image frame.
        /// </summary>
        public Color32[] RawImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        public Image()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class by cloning another image.
        /// </summary>
        /// <param name="img">The image to clone.</param>
        public Image(Image img)
        {
            Width = img.Width;
            Height = img.Height;
            Delay = img.Delay;
            RawImage = img.RawImage != null ? (Color32[])img.RawImage.Clone() : null;
        }

        /// <summary>
        /// Creates a deep copy of the image.
        /// </summary>
        /// <returns>A new <see cref="Image"/> instance.</returns>
        public object Clone()
        {
            return new Image(this);
        }

        /// <summary>
        /// Creates a Unity <see cref="Texture2D"/> from the raw image data.
        /// </summary>
        /// <returns>A new <see cref="Texture2D"/>.</returns>
        public Texture2D CreateTexture()
        {
            var tex = new Texture2D(Width, Height, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            tex.SetPixels32(RawImage);
            tex.Apply();

            return tex;
        }
    }

    /// <summary>
    /// Decodes GIF data from a byte array.
    /// Supports multi-frame animations and transparency.
    /// </summary>
#if mgGIF_UNSAFE
    unsafe
#endif
    /// <summary>
    /// Decodes GIF data from a byte array and exposes successive frames as <see cref="Image"/> instances.
    /// </summary>
    public class Decoder
    {
        /// <summary>
        /// The GIF version string (e.g., GIF89a).
        /// </summary>
        public string Version;

        /// <summary>
        /// Global width of the GIF.
        /// </summary>
        public ushort Width;

        /// <summary>
        /// Global height of the GIF.
        /// </summary>
        public ushort Height;

        /// <summary>
        /// Global background color.
        /// </summary>
        public Color32 BackgroundColour;

        //------------------------------------------------------------------------------
        // GIF format enums

        [Flags]
        private enum ImageFlag
        {
            Interlaced = 0x40,
            ColourTable = 0x80,
            TableSizeMask = 0x07,
            BitDepthMask = 0x70,
        }

        private enum Block
        {
            Image = 0x2C,
            Extension = 0x21,
            End = 0x3B
        }

        private enum Extension
        {
            GraphicControl = 0xF9,
            Comments = 0xFE,
            PlainText = 0x01,
            ApplicationData = 0xFF
        }

        private enum Disposal
        {
            None = 0x00,
            DoNotDispose = 0x04,
            RestoreBackground = 0x08,
            ReturnToPrevious = 0x0C
        }

        [Flags]
        private enum ControlFlags
        {
            HasTransparency = 0x01,
            DisposalMask = 0x0C
        }

        //------------------------------------------------------------------------------

        private const uint NoCode = 0xFFFF;
        private const ushort NoTransparency = 0xFFFF;

        // input stream to decode
        private byte[] Input;
        private int D;

        // colour table
        private Color32[] GlobalColourTable;
        private Color32[] LocalColourTable;
        private Color32[] ActiveColourTable;
        private ushort TransparentIndex;

        // current image
        private Image Image = new Image();
        private ushort ImageLeft;
        private ushort ImageTop;
        private ushort ImageWidth;
        private ushort ImageHeight;

        private Color32[] Output;
        private Color32[] PreviousImage;

        private readonly int[] Pow2 = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };

#if !mgGIF_UNSAFE
        private Decoder() { }
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="Decoder"/> class and loads the GIF data.
        /// </summary>
        /// <param name="data">The byte array containing the GIF data.</param>
        public Decoder(byte[] data)
            : this()
        {
            Load(data);
        }

        /// <summary>
        /// Loads GIF data from a byte array.
        /// </summary>
        /// <param name="data">The GIF data.</param>
        /// <returns>The decoder instance.</returns>
        public Decoder Load(byte[] data)
        {
            Input = data;
            D = 0;

            GlobalColourTable = new Color32[256];
            LocalColourTable = new Color32[256];
            TransparentIndex = NoTransparency;
            Output = null;
            PreviousImage = null;

            Image.Delay = 0;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadByte()
        {
            return Input[D++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort ReadUInt16()
        {
            return (ushort)(Input[D++] | Input[D++] << 8);
        }

        private void ReadHeader()
        {
            if (Input == null || Input.Length <= 12)
            {
                throw new Exception("Invalid data");
            }

            Version = Encoding.ASCII.GetString(Input, 0, 6);
            D = 6;

            if (Version != "GIF87a" && Version != "GIF89a")
            {
                throw new Exception("Unsupported GIF version");
            }

            Width = ReadUInt16();
            Height = ReadUInt16();

            Image.Width = Width;
            Image.Height = Height;

            var flags = (ImageFlag)ReadByte();
            var bgIndex = ReadByte();

            ReadByte(); // aspect ratio

            if (flags.HasFlag(ImageFlag.ColourTable))
            {
                ReadColourTable(GlobalColourTable, flags);
            }

            BackgroundColour = GlobalColourTable[bgIndex];
        }

        /// <summary>
        /// Decodes the next image frame from the GIF stream.
        /// </summary>
        /// <returns>The decoded <see cref="Image"/>, or null if the end of the stream is reached.</returns>
        public Image NextImage()
        {
            if (D == 0)
            {
                ReadHeader();
            }

            while (true)
            {
                var block = (Block)ReadByte();

                switch (block)
                {
                    case Block.Image:
                        {
                            var img = ReadImageBlock();

                            if (img != null)
                            {
                                return img;
                            }
                        }
                        break;

                    case Block.Extension:
                        {
                            var ext = (Extension)ReadByte();

                            if (ext == Extension.GraphicControl)
                            {
                                ReadControlBlock();
                            }
                            else
                            {
                                SkipBlocks();
                            }
                        }
                        break;

                    case Block.End:
                        return null;

                    default:
                        throw new Exception("Unexpected block type");
                }
            }
        }

        private Color32[] ReadColourTable(Color32[] colourTable, ImageFlag flags)
        {
            var tableSize = Pow2[(int)(flags & ImageFlag.TableSizeMask) + 1];

            for (var i = 0; i < tableSize; i++)
            {
                colourTable[i] = new Color32(
                    Input[D++],
                    Input[D++],
                    Input[D++],
                    0xFF
                );
            }

            return colourTable;
        }

        private void SkipBlocks()
        {
            var blockSize = Input[D++];

            while (blockSize != 0x00)
            {
                D += blockSize;
                blockSize = Input[D++];
            }
        }

        private void ReadControlBlock()
        {
            ReadByte();                             // block size (0x04)
            var flags = (ControlFlags)ReadByte();  // flags
            Image.Delay = ReadUInt16() * 10;        // delay (1/100th -> milliseconds)
            var transparentColour = ReadByte();     // transparent colour
            ReadByte();                             // terminator (0x00)

            if (flags.HasFlag(ControlFlags.HasTransparency))
            {
                TransparentIndex = transparentColour;
            }
            else
            {
                TransparentIndex = NoTransparency;
            }

            switch ((Disposal)(flags & ControlFlags.DisposalMask))
            {
                default:
                case Disposal.None:
                case Disposal.DoNotDispose:
                    PreviousImage = Output;
                    break;

                case Disposal.RestoreBackground:
                    Output = new Color32[Width * Height];
                    break;

                case Disposal.ReturnToPrevious:
                    Output = new Color32[Width * Height];

                    if (PreviousImage != null)
                    {
                        Array.Copy(PreviousImage, Output, Output.Length);
                    }
                    break;
            }
        }

        private Image ReadImageBlock()
        {
            ImageLeft = ReadUInt16();
            ImageTop = ReadUInt16();
            ImageWidth = ReadUInt16();
            ImageHeight = ReadUInt16();
            var flags = (ImageFlag)ReadByte();

            if (ImageWidth == 0 || ImageHeight == 0)
            {
                return null;
            }

            if (flags.HasFlag(ImageFlag.ColourTable))
            {
                ActiveColourTable = ReadColourTable(LocalColourTable, flags);
            }
            else
            {
                ActiveColourTable = GlobalColourTable;
            }

            if (Output == null)
            {
                Output = new Color32[Width * Height];
                PreviousImage = Output;
            }

            DecompressLZW();

            if (flags.HasFlag(ImageFlag.Interlaced))
            {
                Deinterlace();
            }

            Image.RawImage = Output;
            return Image;
        }

        private void Deinterlace()
        {
            var numRows = Output.Length / Width;
            var writePos = Output.Length - Width;
            var input = Output;

            Output = new Color32[Output.Length];

            for (var row = 0; row < numRows; row++)
            {
                int copyRow;

                if (row % 8 == 0)
                {
                    copyRow = row / 8;
                }
                else if ((row + 4) % 8 == 0)
                {
                    var o = numRows / 8;
                    copyRow = o + (row - 4) / 8;
                }
                else if ((row + 2) % 4 == 0)
                {
                    var o = numRows / 4;
                    copyRow = o + (row - 2) / 4;
                }
                else
                {
                    var o = numRows / 2;
                    copyRow = o + (row - 1) / 2;
                }

                Array.Copy(input, (numRows - copyRow - 1) * Width, Output, writePos, Width);
                writePos -= Width;
            }
        }

#if mgGIF_UNSAFE

        private bool Disposed = false;
        private int CodesLength;
        private IntPtr CodesHandle;
        private ushort* pCodes;
        private IntPtr CurBlock;
        private uint* pCurBlock;
        private const int MaxCodes = 4096;
        private IntPtr Indices;
        private ushort** pIndicies;

        /// <summary>
        /// Initializes a new instance of the <see cref="Decoder"/> class with unmanaged resources.
        /// </summary>
        public Decoder()
        {
            CodesLength = 128 * 1024;
            CodesHandle = Marshal.AllocHGlobal(CodesLength * sizeof(ushort));
            pCodes = (ushort*)CodesHandle.ToPointer();

            CurBlock = Marshal.AllocHGlobal(64 * sizeof(uint));
            pCurBlock = (uint*)CurBlock.ToPointer();

            Indices = Marshal.AllocHGlobal(MaxCodes * sizeof(ushort*));
            pIndicies = (ushort**)Indices.ToPointer();
        }

        /// <summary>
        /// Protected disposal implementation.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            Marshal.FreeHGlobal(CodesHandle);
            Marshal.FreeHGlobal(CurBlock);
            Marshal.FreeHGlobal(Indices);

            Disposed = true;
        }

        /// <summary>
        /// Finalizer for the <see cref="Decoder"/> class.
        /// </summary>
        ~Decoder()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Decoder"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void DecompressLZW()
        {
            var pCodeBufferEnd = pCodes + CodesLength;

            fixed (byte* pData = Input)
            {
                fixed (Color32* pOutput = Output, pColourTable = ActiveColourTable)
                {
                    var row = (Height - ImageTop - 1) * Width;
                    var safeWidth = ImageLeft + ImageWidth > Width ? Width - ImageLeft : ImageWidth;

                    var pWrite = &pOutput[row + ImageLeft];
                    var pRow = pWrite;
                    var pRowEnd = pWrite + ImageWidth;
                    var pImageEnd = pWrite + safeWidth;

                    int minimumCodeSize = Input[D++];

                    if (minimumCodeSize > 11)
                    {
                        minimumCodeSize = 11;
                    }

                    var codeSize = minimumCodeSize + 1;
                    var nextSize = Pow2[codeSize];
                    var maximumCodeSize = Pow2[minimumCodeSize];
                    var clearCode = (uint)maximumCodeSize;
                    var endCode = (uint)maximumCodeSize + 1;

                    var numCodes = (uint)maximumCodeSize + 2;
                    var pCodesEnd = pCodes;

                    for (ushort i = 0; i < numCodes; i++)
                    {
                        pIndicies[i] = pCodesEnd;
                        *pCodesEnd++ = 1;
                        *pCodesEnd++ = i;
                    }

                    uint previousCode = NoCode;
                    uint mask = (uint)(nextSize - 1);
                    uint shiftRegister = 0;

                    int bitsAvailable = 0;
                    int bytesAvailable = 0;

                    uint* pD = pCurBlock;

                    while (true)
                    {
                        uint curCode = shiftRegister & mask;

                        if (bitsAvailable >= codeSize)
                        {
                            bitsAvailable -= codeSize;
                            shiftRegister >>= codeSize;
                        }
                        else
                        {
                            if (bytesAvailable <= 0)
                            {
                                var pBlock = &pData[D++];
                                bytesAvailable = *pBlock++;
                                D += bytesAvailable;

                                if (bytesAvailable == 0)
                                {
                                    return;
                                }

                                pCurBlock[(bytesAvailable - 1) / 4] = 0;
                                Buffer.MemoryCopy(pBlock, pCurBlock, 256, bytesAvailable);
                                pD = pCurBlock;
                            }

                            shiftRegister = *pD++;
                            int newBits = bytesAvailable >= 4 ? 32 : bytesAvailable * 8;
                            bytesAvailable -= 4;

                            if (bitsAvailable > 0)
                            {
                                var bitsRemaining = codeSize - bitsAvailable;
                                curCode |= (shiftRegister << bitsAvailable) & mask;
                                shiftRegister >>= bitsRemaining;
                                bitsAvailable = newBits - bitsRemaining;
                            }
                            else
                            {
                                curCode = shiftRegister & mask;
                                shiftRegister >>= codeSize;
                                bitsAvailable = newBits - codeSize;
                            }
                        }

                        if (curCode == clearCode)
                        {
                            codeSize = minimumCodeSize + 1;
                            nextSize = Pow2[codeSize];
                            numCodes = (uint)maximumCodeSize + 2;
                            pCodesEnd = &pCodes[numCodes * 2];
                            previousCode = NoCode;
                            mask = (uint)(nextSize - 1);
                            continue;
                        }
                        else if (curCode == endCode)
                        {
                            break;
                        }

                        bool plusOne = false;
                        ushort* pCodePos = null;

                        if (curCode < numCodes)
                        {
                            pCodePos = pIndicies[curCode];
                        }
                        else if (previousCode != NoCode)
                        {
                            pCodePos = pIndicies[previousCode];
                            plusOne = true;
                        }
                        else
                        {
                            continue;
                        }

                        var codeLength = *pCodePos++;
                        var newCode = *pCodePos;
                        var pEnd = pCodePos + codeLength;

                        do
                        {
                            var code = *pCodePos++;

                            if (code != TransparentIndex && pWrite < pImageEnd)
                            {
                                *pWrite = pColourTable[code];
                            }

                            if (++pWrite == pRowEnd)
                            {
                                pRow -= Width;
                                pWrite = pRow;
                                pRowEnd = pRow + ImageWidth;
                                pImageEnd = pRow + safeWidth;

                                if (pWrite < pOutput)
                                {
                                    SkipBlocks();
                                    return;
                                }
                            }
                        }
                        while (pCodePos < pEnd);

                        if (plusOne)
                        {
                            if (newCode != TransparentIndex && pWrite < pImageEnd)
                            {
                                *pWrite = pColourTable[newCode];
                            }

                            if (++pWrite == pRowEnd)
                            {
                                pRow -= Width;
                                pWrite = pRow;
                                pRowEnd = pRow + ImageWidth;
                                pImageEnd = pRow + safeWidth;

                                if (pWrite < pOutput)
                                {
                                    break;
                                }
                            }
                        }

                        if (previousCode != NoCode && numCodes != MaxCodes)
                        {
                            pCodePos = pIndicies[previousCode];
                            codeLength = *pCodePos++;

                            if (pCodesEnd + codeLength + 1 >= pCodeBufferEnd)
                            {
                                var pBase = pCodes;
                                CodesLength *= 2;
                                CodesHandle = Marshal.ReAllocHGlobal(CodesHandle, (IntPtr)(CodesLength * sizeof(ushort)));
                                pCodes = (ushort*)CodesHandle.ToPointer();
                                pCodeBufferEnd = pCodes + CodesLength;
                                pCodesEnd = pCodes + (pCodesEnd - pBase);

                                for (int i = 0; i < numCodes; i++)
                                {
                                    pIndicies[i] = pCodes + (pIndicies[i] - pBase);
                                }

                                pCodePos = pIndicies[previousCode];
                                pCodePos++;
                            }

                            pIndicies[numCodes++] = pCodesEnd;
                            *pCodesEnd++ = (ushort)(codeLength + 1);
                            Buffer.MemoryCopy(pCodePos, pCodesEnd, codeLength * sizeof(ushort), codeLength * sizeof(ushort));
                            pCodesEnd += codeLength;
                            *pCodesEnd++ = newCode;
                        }

                        if (numCodes >= nextSize && codeSize < 12)
                        {
                            nextSize = Pow2[++codeSize];
                            mask = (uint)(nextSize - 1);
                        }

                        previousCode = curCode;
                    }

                    SkipBlocks();
                }
            }
        }

#else

        int[]    Indices  = new int[ 4096 ];
        ushort[] Codes    = new ushort[ 128 * 1024 ];
        uint[]   CurBlock = new uint[ 64 ];

        void DecompressLZW()
        {
            // output write position

            int row       = ( Height - ImageTop - 1 ) * Width; // reverse rows for unity texture coords
            int col       = ImageLeft;
            int rightEdge = ImageLeft + ImageWidth;

            // setup codes

            int minimumCodeSize = Input[ D++ ];

            if( minimumCodeSize > 11 )
            {
                minimumCodeSize = 11;
            }

            var codeSize        = minimumCodeSize + 1;
            var nextSize        = Pow2[ codeSize ];
            var maximumCodeSize = Pow2[ minimumCodeSize ];
            var clearCode       = maximumCodeSize;
            var endCode         = maximumCodeSize + 1;

            // initialise buffers

            var codesEnd = 0;
            var numCodes = maximumCodeSize + 2;

            for( ushort i = 0; i < numCodes; i++ )
            {
                Indices[ i ] = codesEnd;
                Codes[ codesEnd++ ] = 1; // length
                Codes[ codesEnd++ ] = i; // code
            }

            // LZW decode loop

            uint previousCode   = NoCode; // last code processed
            uint mask           = (uint) ( nextSize - 1 ); // mask out code bits
            uint shiftRegister  = 0; // shift register holds the bytes coming in from the input stream, we shift down by the number of bits

            int  bitsAvailable  = 0; // number of bits available to read in the shift register
            int  bytesAvailable = 0; // number of bytes left in current block

            int  blockPos       = 0;

            while( true )
            {
                // get next code

                uint curCode = shiftRegister & mask;

                if( bitsAvailable >= codeSize )
                {
                    bitsAvailable -= codeSize;
                    shiftRegister >>= codeSize;
                }
                else
                {
                    // reload shift register


                    // if start of new block

                    if( bytesAvailable <= 0 )
                    {
                        // read blocksize
                        bytesAvailable = Input[ D++ ];

                        // exit if end of stream
                        if( bytesAvailable == 0 )
                        {
                            return;
                        }

                        // read block
                        CurBlock[ ( bytesAvailable - 1 ) / 4 ] = 0; // zero last entry
                        Buffer.BlockCopy( Input, D, CurBlock, 0, bytesAvailable );
                        blockPos = 0;
                        D += bytesAvailable;
                    }

                    // load shift register

                    shiftRegister = CurBlock[ blockPos++ ];
                    int newBits = bytesAvailable >= 4 ? 32 : bytesAvailable * 8;
                    bytesAvailable -= 4;

                    // read remaining bits

                    if( bitsAvailable > 0 )
                    {
                        var bitsRemaining = codeSize - bitsAvailable;
                        curCode |= ( shiftRegister << bitsAvailable ) & mask;
                        shiftRegister >>= bitsRemaining;
                        bitsAvailable = newBits - bitsRemaining;
                    }
                    else
                    {
                        curCode = shiftRegister & mask;
                        shiftRegister >>= codeSize;
                        bitsAvailable = newBits - codeSize;
                    }
                }

                // process code

                if( curCode == clearCode )
                {
                    // reset codes
                    codeSize = minimumCodeSize + 1;
                    nextSize = Pow2[ codeSize ];
                    numCodes = maximumCodeSize + 2;

                    // reset buffer write pos
                    codesEnd = numCodes * 2;

                    // clear previous code
                    previousCode = NoCode;
                    mask = (uint) ( nextSize - 1 );

                    continue;
                }
                else if( curCode == endCode )
                {
                    // stop
                    break;
                }

                bool plusOne = false;
                int  codePos = 0;

                if( curCode < numCodes )
                {
                    // write existing code
                    codePos = Indices[ curCode ];
                }
                else if( previousCode != NoCode )
                {
                    // write previous code
                    codePos = Indices[ previousCode ];
                    plusOne = true;
                }
                else
                {
                    continue;
                }


                // output colours

                var codeLength = Codes[ codePos++ ];
                var newCode    = Codes[ codePos ];

                for( int i = 0; i < codeLength; i++ )
                {
                    var code = Codes[ codePos++ ];

                    if( code != TransparentIndex && col < Width )
                    {
                        Output[ row + col ] = ActiveColourTable[ code ];
                    }

                    if( ++col == rightEdge )
                    {
                        col = ImageLeft;
                        row -= Width;

                        if( row < 0 )
                        {
                            SkipBlocks();
                            return;
                        }
                    }
                }

                if( plusOne )
                {
                    if( newCode != TransparentIndex && col < Width )
                    {
                        Output[ row + col ] = ActiveColourTable[ newCode ];
                    }

                    if( ++col == rightEdge )
                    {
                        col = ImageLeft;
                        row -= Width;

                        if( row < 0 )
                        {
                            break;
                        }
                    }
                }

                // create new code

                if( previousCode != NoCode && numCodes != Indices.Length )
                {
                    // get previous code from buffer

                    codePos = Indices[ previousCode ];
                    codeLength = Codes[ codePos++ ];

                    // resize buffer if required (should be rare)

                    if( codesEnd + codeLength + 1 >= Codes.Length )
                    {
                        Array.Resize( ref Codes, Codes.Length * 2 );
                    }

                    // add new code

                    Indices[ numCodes++ ] = codesEnd;
                    Codes[ codesEnd++ ] = (ushort) ( codeLength + 1 );

                    // copy previous code sequence

                    var stop = codesEnd + codeLength;

                    while( codesEnd < stop )
                    {
                        Codes[ codesEnd++ ] = Codes[ codePos++ ];
                    }

                    // append new code

                    Codes[ codesEnd++ ] = newCode;
                }

                // increase code size?

                if( numCodes >= nextSize && codeSize < 12 )
                {
                    nextSize = Pow2[ ++codeSize ];
                    mask = (uint) ( nextSize - 1 );
                }

                // remember last code processed
                previousCode = curCode;
            }

            // skip any remaining blocks
            SkipBlocks();
        }
#endif // mgGIF_UNSAFE

        /// <summary>
        /// Returns a short identifier describing the decoder build configuration.
        /// </summary>
        /// <returns>A compact string containing version, endianness, safety mode, backend, and runtime profile.</returns>
        public static string Ident()
        {
            var v = "1.1";
            var e = BitConverter.IsLittleEndian ? "L" : "B";

#if ENABLE_IL2CPP
            var b = "N";
#else
            var b = "M";
#endif

#if mgGIF_UNSAFE
            var s = "U";
#else
            var s = "S";
#endif

#if NET_4_6
            var n = "4.x";
#else
            var n = "2.0";
#endif

            return $"{v} {e}{s}{b} {n}";
        }
    }
}
