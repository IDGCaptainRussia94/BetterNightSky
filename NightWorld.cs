using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Terraria.Utilities;

namespace BetterNightSky
{
	public class NightWorld : ModSystem
	{
		public static DateTime date;
		public static Vector2 CelestialAlignment(Vector2 celestialLocation)
		{
			Vector2 location = celestialLocation;
			float maxSize = 1921f;

            float time = date.DayOfYear / 366f;

			float offsetSize = -100 + ((maxSize + 200) * time);

            return new Vector2(((celestialLocation.X)+(maxSize*time)) % maxSize, celestialLocation.Y);

		}
		public override void OnModLoad()
		{
			date = DateTime.Now;
		}
        public static void DoUpdates()
        {
			UnifiedRandom rando = new UnifiedRandom(Main.worldName.GetHashCode());
			bool hasRotation = false;

			foreach (KeyValuePair<CelestialObject, int> pair in BetterNightSky.CelestialIndex)
			{
				switch(pair.Key)
				{
					case CelestialObject.Mars:
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(80, 220));
                        break;

					case CelestialObject.Saturn:
						Main.star[pair.Value].position = CelestialAlignment(new Vector2(900, 90));
						break;

					case CelestialObject.Jupiter:
						Main.star[pair.Value].position = CelestialAlignment(new Vector2(700, 165));
						break;

					case CelestialObject.Mercury:
						Main.star[pair.Value].position = CelestialAlignment(new Vector2(240, 400));
						break;

					case CelestialObject.CrabNebula:
						hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(1660, 370));
						break;

					case CelestialObject.Andromeda:
                        hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(500, 250));
						break;

					case CelestialObject.CatsEyeNebula:
                        hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(1300, 530));
						break;

					case CelestialObject.CarinaNebula:
                        hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(360, 530));
						break;

					case CelestialObject.Triangulum:
                        hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(1700, 700));
						break;

					case CelestialObject.Uranus:
						Main.star[pair.Value].position = CelestialAlignment(new Vector2(1100, 120));
						break;

					case CelestialObject.LargeMagellanicCloud:
                        hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(1400, 190));
						break;

					case CelestialObject.SmallMagellanicCloud:
                        hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(950, 230));
						break;

					case CelestialObject.HelixNebula:
                        hasRotation = true;
                        Main.star[pair.Value].position = CelestialAlignment(new Vector2(920, 450));
                        break;
                }

				bool inAetherNebula = hasRotation && Main.shimmerAlpha > 0;

                Main.star[pair.Value].type = (int)pair.Key;
				Main.star[pair.Value].rotation = inAetherNebula ? (rando.NextFloat(-1f,1f)*0.01f* Main.GlobalTimeWrappedHourly)+rando.NextFloat(MathHelper.TwoPi) : 0f;
				Main.star[pair.Value].scale = 1f;
				Main.star[pair.Value].twinkleSpeed = hasRotation ? 5f : 0f;
                Main.star[pair.Value].twinkle = 1f;
                Main.star[pair.Value].fadeIn = inAetherNebula ? 0.5f : 0f;
                Main.star[pair.Value].hidden = false;
                Main.star[pair.Value].falling = false;
            }
		}
	}
}
