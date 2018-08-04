﻿using System.Collections.Generic;
using System.IO;
using Assets.System;
using UnityEngine;

namespace Assets.Fileparsers
{
    public class Gpw
    {
        public AudioClip Clip { get; set; }
        public short AudioRange { get; set; }
    }

    public class GpwParser
    {
        private static readonly Dictionary<string, Gpw> GpwCache = new Dictionary<string, Gpw>();

        public static Gpw ParseGpw(string fileName)
        {
            Gpw gpw;
            if (GpwCache.TryGetValue(fileName, out gpw))
            {
                return gpw;
            }

            using (BinaryReader br = new BinaryReader(VirtualFilesystem.Instance.GetFileStream(fileName)))
            {
                gpw = new Gpw();
                string header = br.ReadCString(4); // Always GAS0
                gpw.AudioRange = br.ReadInt16();
                short unk2 = br.ReadInt16();
                int unk3 = br.ReadInt32();
                int unk4 = br.ReadInt32();
                int unk5 = br.ReadInt32();
                int unk6 = br.ReadInt32();
                int unk7 = br.ReadInt32();
                br.BaseStream.Seek(4, SeekOrigin.Current); // Skip RIFF header
                int waveFileSize = br.ReadInt32(); // Read total size of audio data from RIFF header
                br.BaseStream.Seek(-8, SeekOrigin.Current); // Go back to RIFF header

                byte[] audioData = br.ReadBytes(waveFileSize);
                gpw.Clip = WavUtility.ToAudioClip(audioData, 0, fileName);

                float unk8 = br.ReadSingle();
                float unk9 = br.ReadSingle();

                GpwCache.Add(fileName, gpw);
                return gpw;
            }
        }
    }
}
