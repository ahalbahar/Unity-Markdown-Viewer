// ============================================================
// File:    MarkdownDiagramEncoding.cs
// Purpose: Shared encoding helpers for remote diagram renderers.
// Author:  Ahmad Albahar
// Created: 2026-07-08
// Notes:   Keeps Mermaid and PlantUML request payload encoding consistent.
// ============================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AB.MDV
{
    internal static class MarkdownDiagramEncoding
    {
        private const string PlantUmlAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        internal static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length + 32);

            foreach (char character in value)
            {
                switch (character)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(character))
                        {
                            builder.AppendFormat("\\u{0:x4}", (int)character);
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        internal static byte[] CompressZlib(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                output.WriteByte(0x78);
                output.WriteByte(0xDA);

                byte[] compressed = CompressRawDeflate(data);
                output.Write(compressed, 0, compressed.Length);

                uint checksum = ComputeAdler32(data);
                output.WriteByte((byte)((checksum >> 24) & 0xFF));
                output.WriteByte((byte)((checksum >> 16) & 0xFF));
                output.WriteByte((byte)((checksum >> 8) & 0xFF));
                output.WriteByte((byte)(checksum & 0xFF));

                return output.ToArray();
            }
        }

        internal static string ToBase64Url(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        internal static string ToPlantUmlEncoded(string source)
        {
            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
            byte[] compressed = CompressRawDeflate(sourceBytes);
            var builder = new StringBuilder(((compressed.Length + 2) / 3) * 4);

            for (int i = 0; i < compressed.Length; i += 3)
            {
                if (i + 2 < compressed.Length)
                {
                    AppendPlantUmlEncodedBytes(builder, compressed[i], compressed[i + 1], compressed[i + 2]);
                }
                else if (i + 1 < compressed.Length)
                {
                    AppendPlantUmlEncodedBytes(builder, compressed[i], compressed[i + 1], 0);
                }
                else
                {
                    AppendPlantUmlEncodedBytes(builder, compressed[i], 0, 0);
                }
            }

            return builder.ToString();
        }

        internal static string ComputeSha256(string value)
        {
            using (var hash = SHA256.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(value);
                byte[] digest = hash.ComputeHash(data);
                var builder = new StringBuilder(digest.Length * 2);

                for (int i = 0; i < digest.Length; i++)
                {
                    builder.Append(digest[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static byte[] CompressRawDeflate(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, true))
                {
                    deflate.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        private static uint ComputeAdler32(byte[] data)
        {
            const uint modulo = 65521;
            uint a = 1;
            uint b = 0;

            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % modulo;
                b = (b + a) % modulo;
            }

            return (b << 16) | a;
        }

        private static void AppendPlantUmlEncodedBytes(StringBuilder builder, byte b1, byte b2, byte b3)
        {
            int c1 = b1 >> 2;
            int c2 = ((b1 & 0x3) << 4) | (b2 >> 4);
            int c3 = ((b2 & 0xF) << 2) | (b3 >> 6);
            int c4 = b3 & 0x3F;

            builder.Append(EncodePlantUml6Bit(c1));
            builder.Append(EncodePlantUml6Bit(c2));
            builder.Append(EncodePlantUml6Bit(c3));
            builder.Append(EncodePlantUml6Bit(c4));
        }

        private static char EncodePlantUml6Bit(int value)
        {
            return PlantUmlAlphabet[value & 0x3F];
        }
    }
}
