﻿using Assets.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Fileparsers
{
    public class WheelLoc
    {
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 Position { get; set; }
    }

    public class VLoc
    {
        public uint Number { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 Position { get; set; }
    }

    public class HLoc
    {
        public string Label { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 Position { get; set; }
        public float Unk { get; set; }
        public uint Num1 { get; set; }
        public uint Num2 { get; set; }
        public uint Num3 { get; set; }
    }

    public class Vdf
    {
        public float LODDistance2;
        public float LODDistance3;
        public float LODDistance4;
        public float LODDistance5;
        public float Mass;
        public float CollisionMultiplier;
        public float DragCoefficient;
        public uint Unknown;
        public string Name { get; set; }
        public uint VehicleType { get; set; }
        public uint VehicleSize { get; set; }
        public float LODDistance1 { get; set; }
        public uint Unk4 { get; set; }
        public List<SdfPart[]> PartsThirdPerson { get; set; }
        public SdfPart[] PartsFirstPerson { get; set; }
        public Bounds BoundsInner { get; internal set; }
        public Bounds BoundsOuter { get; internal set; }
        public WheelLoc[] WheelLoc { get; set; }
        public List<VLoc> VLocs { get; set; }
        public string SOBJGeoName { get; set; }
        public List<HLoc> HLocs { get; set; }
    }

    public class VdfParser
    {
        public static Vdf ParseVdf(string filename)
        {
            using (var br = new Bwd2Reader(filename))
            {
                var vdf = new Vdf();

                br.FindNext("VDFC");

                
                vdf.Name = br.ReadCString(20);
                vdf.VehicleType = br.ReadUInt32();
                vdf.VehicleSize = br.ReadUInt32();
                vdf.LODDistance1 = br.ReadSingle();
                vdf.LODDistance2 = br.ReadSingle();
                vdf.LODDistance3 = br.ReadSingle();
                vdf.LODDistance4 = br.ReadSingle();
                vdf.LODDistance5 = br.ReadSingle();
                vdf.Mass = br.ReadSingle();
                vdf.CollisionMultiplier = br.ReadSingle();
                vdf.DragCoefficient = br.ReadSingle();
                vdf.Unknown = br.ReadUInt32();

                br.FindNext("SOBJ");
                vdf.SOBJGeoName = br.ReadCString(8);

                vdf.VLocs = new List<VLoc>();
                br.FindNext("VLOC");
                while (br.Current != null && br.Current.Name != "EXIT")
                {
                    var vloc = new VLoc
                    {
                        Number = br.ReadUInt32(),
                        Right = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        Up = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        Forward = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
                    };
                    vdf.VLocs.Add(vloc);

                    br.Next();
                }
                

                br.FindNext("VGEO");
                var numParts = br.ReadUInt32();
                vdf.PartsThirdPerson = new List<SdfPart[]>(4);
                for (int damageState = 0; damageState < 4; damageState++)
                {
                    var parts = new SdfPart[numParts];
                    for (int i = 0; i < numParts; i++)
                    {
                        var sdfPart = new SdfPart();
                        sdfPart.Name = br.ReadCString(8);
                        sdfPart.Right = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        sdfPart.Up = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        sdfPart.Forward = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        sdfPart.Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        sdfPart.ParentName = br.ReadCString(8);
                        br.Position += 36;

                        parts[i] = sdfPart;
                    }
                    vdf.PartsThirdPerson.Add(parts);
                }
                br.Position += 100 * numParts * 12;

                vdf.PartsFirstPerson = new SdfPart[numParts];
                for (int i = 0; i < numParts; i++)
                {
                    var sdfPart = new SdfPart();
                    sdfPart.Name = br.ReadCString(8);
                    sdfPart.Right = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    sdfPart.Up = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    sdfPart.Forward = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    sdfPart.Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    sdfPart.ParentName = br.ReadCString(8);
                    br.Position += 36;

                    vdf.PartsFirstPerson[i] = sdfPart;
                }

                br.FindNext("COLP");

                var zMaxOuter = br.ReadSingle();
                var zMaxInner = br.ReadSingle();
                var zMinInner = br.ReadSingle();
                var zMinOuter = br.ReadSingle();

                var xMaxOuter = br.ReadSingle();
                var xMaxInner = br.ReadSingle();
                var xMinInner = br.ReadSingle();
                var xMinOuter = br.ReadSingle();

                var yMaxOuter = br.ReadSingle();
                var yMaxInner = br.ReadSingle();
                var yMinInner = br.ReadSingle();
                var yMinOuter = br.ReadSingle();


                var innerBounds = new Bounds();
                innerBounds.SetMinMax(new Vector3(xMinInner, yMinInner, zMinInner), new Vector3(xMaxInner, yMaxInner, zMaxInner));
                vdf.BoundsInner = innerBounds;

                var outerBounds = new Bounds();
                outerBounds.SetMinMax(new Vector3(xMinOuter, yMinOuter, zMinOuter), new Vector3(xMaxOuter, yMaxOuter, zMaxOuter));
                vdf.BoundsOuter = outerBounds;

                br.FindNext("WLOC");
                vdf.WheelLoc = new WheelLoc[6];
                for (int i = 0; i < 6; i++)
                {
                    var wheelLoc = vdf.WheelLoc[i] = new WheelLoc();
                    var unk1 = br.ReadUInt32();
                    wheelLoc.Right = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    wheelLoc.Up = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    wheelLoc.Forward = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    wheelLoc.Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    var unk2 = br.ReadSingle();
                }

                vdf.HLocs = new List<HLoc>();
                br.FindNext("HLOC");
                while (br.Current != null && br.Current.Name != "EXIT")
                {
                    var hloc = new HLoc
                    {
                        Label = br.ReadCString(16),
                        Num1 = br.ReadUInt32(),
                        Num2 = br.ReadUInt32(),
                        Num3 = br.ReadUInt32(),
                        Right = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        Up = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        Forward = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                        Unk = br.ReadSingle()
                    };
                    vdf.HLocs.Add(hloc);

                    br.Next();
                }

                return vdf;
            }
        }
    }
}