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

namespace BetterNightSky
{
    public class NightConfig : ModConfig
    {
        public static NightConfig Config;
     
        /*
        [ReloadRequired]
        [DefaultValue(1200)]
        [Range(500, 3000)]
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

        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override bool Equals(object obj)
        {
            if (obj is NightConfig other)
                return AetherStarOffset == other.AetherStarOffset && AetherStarVelocity == other.AetherStarVelocity && MoonScale == other.MoonScale;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return GetType().GetFields().Select(testby => testby.GetValue(this)).GetHashCode();
        }

        public override void OnChanged()
        {

        }
    }
}
