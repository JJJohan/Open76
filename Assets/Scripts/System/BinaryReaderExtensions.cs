﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Assets.System
{
    public static class BinaryReaderExtensions
    {
        public static string ReadCString(this BinaryReader br, int maxLength)
        {
            var chrs = br.ReadBytes(maxLength);
            for (int i = 0; i < chrs.Length; i++)
            {
                chrs[i] = (byte)(chrs[i] & 0x7F);   // Skip high byte (old skool ascii)
            }
            for (int i = 0; i < chrs.Length; i++)
            {
                if (chrs[i] == 0)
                    return Encoding.ASCII.GetString(chrs, 0, i);
            }
            return Encoding.ASCII.GetString(chrs);
        }
    }
}
