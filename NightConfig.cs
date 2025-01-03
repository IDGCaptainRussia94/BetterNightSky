using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using System.Reflection;
using Humanizer;

namespace BetterNightSky
{
    public class NightConfig : ModConfig
    {
        public static int StarCount => 1200;
        public static NightConfig Config;

        //Still running into Array issues... I should redo the whole idea to correct this problem

        /*
        [ReloadRequired]
        [DefaultValue(1200)]
        [Range(1200, 3000)]
        [Increment(25)]
        [DrawTicks]
        public int StarCount;
        */


        [DefaultValue(1f)]
        [Range(0f, 5f)]
        [Increment(0.05f)]
        [DrawTicks]
        public float MoonScale;

        [DefaultValue(typeof(Vector2), "1, 1")]
        [Range(-10f, 10f)]
        [Increment(0.1f)]
        [DrawTicks]
        public Vector2 AetherStarVelocity;

        [DefaultValue(1f)]
        [Range(-5f, 5f)]
        [Increment(0.05f)]
        [DrawTicks]
        public float AetherStarOffset;

        [DefaultValue(true)]
        public bool AetherCelestialBodies;

        public NightConfigCelestialBodies CelestialBodies;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override bool Equals(object obj)
        {
            if (obj is NightConfig other)
                return AetherStarOffset == other.AetherStarOffset && AetherStarVelocity == other.AetherStarVelocity && MoonScale == other.MoonScale && AetherCelestialBodies == other.AetherCelestialBodies;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return GetType().GetFields().Select(testby => testby.GetValue(this)).GetHashCode();
        }

        public override void OnChanged()
        {
            CelestialBodies?.OnUpdate();
        }

        public override void OnLoaded()
        {
            CelestialBodies?.OnUpdate();
        }
    }
    public class NightConfigCelestialBodies
    {
        public static NightConfigCelestialBodies CelestialBodyConfig => NightConfig.Config.CelestialBodies;
        public static bool[] celestialBodyBools = Enumerable.Repeat(true, BetterNightSky.NewArraySize).ToArray();

        [DefaultValue(1f)]
        [Range(0f, 1f)]
        [Increment(0.01f)]
        [DrawTicks]
        public float AltitudeHeight;
        [DefaultValue(1f)]
        [Range(0f, 5f)]
        [Increment(0.05f)]
        [DrawTicks]
        public float altitudeFadingPercent;

        [Header("$Mods.BetterNightSky.Configs.NightConfigCelestialBodies.HeaderVisibility")]

        [DefaultValue(true)]
        public bool CelestialBodyOn8;
        [DefaultValue(true)]
        public bool CelestialBodyOn9;
        [DefaultValue(true)]
        public bool CelestialBodyOn10;
        [DefaultValue(true)]
        public bool CelestialBodyOn11;
        [DefaultValue(true)]
        public bool CelestialBodyOn12;
        [DefaultValue(true)]
        public bool CelestialBodyOn13;
        [DefaultValue(true)]
        public bool CelestialBodyOn14;
        [DefaultValue(true)]
        public bool CelestialBodyOn15;
        [DefaultValue(true)]
        public bool CelestialBodyOn16;
        [DefaultValue(true)]
        public bool CelestialBodyOn17;
        [DefaultValue(true)]
        public bool CelestialBodyOn18;
        [DefaultValue(true)]
        public bool CelestialBodyOn19;
        [DefaultValue(true)]
        public bool CelestialBodyOn20;

        public void OnUpdate()
        {
            int index = 8;
            foreach (FieldInfo field in typeof(NightConfigCelestialBodies).GetFields())
            {
                if (field.FieldType == typeof(bool))
                {
                    celestialBodyBools[index] = (bool)field.GetValue(this);
                    index++;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is NightConfigCelestialBodies other)
            {
                foreach(FieldInfo field in typeof(NightConfigCelestialBodies).GetFields())
                {
                    if (field.GetValue(other) != field.GetValue(this))
                    {
                        return false;
                    }
                }
                return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return GetType().GetFields().Select(testby => testby.GetValue(this)).GetHashCode();
        }
    }
}
