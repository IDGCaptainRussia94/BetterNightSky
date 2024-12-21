using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System.Security.Cryptography.X509Certificates;
using Terraria.Utilities;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Net.Http.Headers;
using System.Reflection;
using static Terraria.Main;

namespace BetterNightSky
{
	public enum CelestialObject
	{
		Mars = 8, Saturn, Jupiter, Mercury, CrabNebula, Andromeda, CatsEyeNebula, CarinaNebula, Triangulum,
		LargeMagellanicCloud, SmallMagellanicCloud, HelixNebula, Uranus
	}

	class BetterNightSky : Mod
	{
        public static readonly BindingFlags UniversalBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static Asset<Texture2D>[] starTextures;
		public static Asset<Texture2D>[] StarTexturesReference => Terraria.GameContent.TextureAssets.Star;

		public static Asset<Texture2D>[] moonTextures;
		public static Asset<Texture2D>[] MoonTexturesReference => Terraria.GameContent.TextureAssets.Moon;

		public static Dictionary<CelestialObject, int> CelestialIndex = new Dictionary<CelestialObject, int>();
		public static int NewArraySize = 21;
		public static int NewStarCount = 1200;

        public static Mod realisticSkies;

        public static (int, int, int) normalStarCount;

		public static Vector2[] starVelocityForAether;
		public static int starIndex = 0;

		public static bool SpecialStarType(Star star) => star.type > 7;

		//we use a new random object because Main.rand isn't initialized yet in Load()
		static FastRandom fastRandom = FastRandom.CreateWithRandomSeed();

		public BetterNightSky()
		{

		}

		public override void Load()
		{
			if (Main.dedServ)
				return;

			starTextures = Terraria.GameContent.TextureAssets.Star;
			moonTextures = Terraria.GameContent.TextureAssets.Moon;
        }

		public static void ApplyPatches(bool load)
		{
			if (load)
			{
                On_Star.Fall += On_Star_Fall;
                On_Main.DrawStar += On_Main_DrawStar;
                On_Main.DoUpdate += On_Main_DoUpdate;
                On_Main.DrawStarsInBackground += On_Main_DrawStarsInBackground;
                IL_Star.SpawnStars += IL_Star_SpawnStars;
                IL_Main.DrawStar += IL_Main_DrawStar;
                IL_Main.DrawSunAndMoon += ScaleMoonSizePatch;
                return;
			}
            On_Star.Fall -= On_Star_Fall;
            On_Main.DrawStar -= On_Main_DrawStar;
            On_Main.DoUpdate -= On_Main_DoUpdate;
            IL_Star.SpawnStars -= IL_Star_SpawnStars;
            IL_Main.DrawStar -= IL_Main_DrawStar;
            IL_Main.DrawSunAndMoon -= ScaleMoonSizePatch;
        }

        private static void On_Main_DoUpdate(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
        {
            NightWorld.DoUpdates();
            orig(self, ref gameTime);
        }

        private static void On_Main_DrawStarsInBackground(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
        {
            CountStars();
            orig(self,sceneArea,artificial);
        }
		public static void CountStars() => starIndex = 0;

        private static void On_Main_DrawStar(On_Main.orig_DrawStar orig, Main self, ref Main.SceneArea sceneArea, float starOpacity, Color bgColorForStars, int i, Star theStar, bool artificial, bool foreground)
        {
            ChangeDrawStar(orig,self, ref sceneArea,starOpacity,bgColorForStars,i,theStar,artificial,foreground);
        }

        private static void On_Star_Fall(On_Star.orig_Fall orig, Star self) => StarFallCheck(orig, self);
        public static void StarFallCheck(On_Star.orig_Fall orig, Star self)
        {
            if (!SpecialStarType(self))
                orig(self);
        }

        public static void ChangeDrawStar(On_Main.orig_DrawStar orig, Main self, ref Main.SceneArea sceneArea, float starOpacity, Color bgColorForStars, int i, Star theStar, bool artificial, bool foreground)
        {
            int bgTopY = sceneArea.bgTopY;

            if (!artificial && realisticSkies != null)
            {
                sceneArea.bgTopY = 0;
            }

            bool uniqueStar = SpecialStarType(theStar);

            if (!uniqueStar || !artificial)
            {
                //if (uniqueStar || NightConfig.Config.StarCount < starIndex)
                    orig(self, ref sceneArea, starOpacity, bgColorForStars, i, theStar, artificial, foreground);
            }
            sceneArea.bgTopY = bgTopY;
            starIndex++;
        }

        private delegate bool AdjustYPositionOfStarDelegate(Star theStar,bool artifical, ref Main.SceneArea sceneArea, ref Vector2 starY);
        private static bool AdjustYPositionOfStarMethod(Star theStar, bool artifical, ref Main.SceneArea sceneArea, ref Vector2 starY)
        {
			return AdjustYPositionOfStarMethodLocal(theStar, artifical,ref sceneArea, ref starY);
        }
		public static bool AdjustYPositionOfStarMethodLocal(Star theStar, bool artifical, ref Main.SceneArea sceneArea, ref Vector2 starY)
		{
			//Main.NewText(theStar.type);
			if (SpecialStarType(theStar))
			{
				return true;
			}
			else
			{
				if (artifical && !theStar.falling)
				{
                    //UnifiedRandom rando = new UnifiedRandom(theStar.position.GetHashCode());
                    starY += starVelocityForAether[starIndex] * Main.GlobalTimeWrappedHourly * NightConfig.Config.AetherStarVelocity;
                    starY.X += (Main.screenPosition.X * -((starIndex / 4000f) + 0.025f))*NightConfig.Config.AetherStarOffset;
                    starY.X %= 1921;
                    starY.Y %= 1081;

                }
            }
			return false;
		}

        private static void IL_Main_DrawStar(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (c.TryGotoNext(MoveType.After, i => i.MatchBrfalse(out _)))
            {
				//int ldargs = (c.Prev.Operand as System.Reflection.ParameterInfo).Position;

				if (c.TryGotoNext(MoveType.Before, i => i.MatchLdfld<Star>("falling")))
				{
					c.Index--;
					int index = c.Index;
                    ILLabel branchingLabel = default;

					if (c.TryGotoNext(MoveType.Before, i => i.MatchBrtrue(out branchingLabel)))

						c.Index = index;

                    c.Emit(OpCodes.Ldarg, 5);
                    c.Emit(OpCodes.Ldarg, 6);
                    c.Emit(OpCodes.Ldarga, 1);
                    c.Emit(OpCodes.Ldloca, 6);
					c.EmitDelegate<AdjustYPositionOfStarDelegate>(AdjustYPositionOfStarMethod);
                    c.Emit(OpCodes.Brtrue, branchingLabel);
					return;	
				}
				throw new Exception("Couldn't find Ldloc 6 and/or Vector 'Y'");
			}
            throw new Exception("Failed to find BR.False");
        }

        private static Action<int> addExtraStarData = delegate (int starID)
        {
            if (Main.rand.NextBool(25))
            {
                Main.star[starID].type = 5;
            }
            if (Main.rand.NextBool(15))
            {
                Main.star[starID].type = 6;
            }
            if (Main.rand.NextBool(13))
            {
                Main.star[starID].type = 7;
            }
        };
        private static void IL_Star_SpawnStars(MonoMod.Cil.ILContext il)
		{
			ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, i => i.MatchStloc(1)))
            {

                c.EmitLdcI4(NewStarCount);
                c.Emit(OpCodes.Stloc, 1);

                if (c.TryGotoNext(MoveType.After, i => i.MatchStfld<Star>("twinkleSpeed")))
				{
					c.Emit(OpCodes.Ldloc, 4);
					c.EmitDelegate(addExtraStarData);

					return;
				}
                throw new Exception("Failed to find the start of the star num values");
            }
			throw new Exception("Failed to find the end of the star assignment code");
		}

        private delegate void AdjustMoonScaleDelegate(ref float moonSize);
        private static void AdjustMoonScaleMethod(ref float moonSize)
        {
            moonSize *= NightConfig.Config.MoonScale;
        }

        private static void ScaleMoonSizePatch(ILContext il)
        {

            var c = new ILCursor(il);

            FieldInfo starsInfo = typeof(Main).GetField("starsHit", UniversalBindingFlags);
            FieldInfo dayTime = typeof(Main).GetField("dayTime", UniversalBindingFlags);


            if (c.TryGotoNext(MoveType.After, i => i.MatchStsfld(starsInfo)))
            {
                ILLabel endBranch = default;

                if (c.TryGotoNext(MoveType.After, i => i.MatchLdsfld(dayTime), i => i.MatchBrtrue(out endBranch)))
                {
                    c.Emit(OpCodes.Ldloca, 10);
                    c.EmitDelegate(AdjustMoonScaleMethod);
                    return;
                }
                throw new Exception("Failed to find 'dayTime' field or jump to Branch True");
            }
            throw new Exception("Failed to find 'starsHit' field");
        }


        public class BetterNightSkySystem : ModSystem
		{

            public static void SpawnNewStars()
            {
                //NewStarCount = Math.Max(NightConfig.Config.StarCount, 500);
                Array.Resize<Star>(ref Main.star, NewStarCount);
                Array.Resize<Vector2>(ref starVelocityForAether, NewStarCount);

				UnifiedRandom rando = new UnifiedRandom();

                Main.numStars = NewStarCount;
                for (int i = 0; i < NewStarCount; i++)
                {
					starVelocityForAether[i] = new Vector2(rando.NextFloat(0.1f, 1f), rando.NextFloat(-0.2f, 0.2f)) * 25f;
                    Main.star[i] = new Star();
                }
                for (int i = 0; i < NewStarCount; i++)
                {
                    Star.SpawnStars(i);
                }
            }

            public override void OnModLoad()
			{

				if (!Main.dedServ)
				{
					NightConfig.Config = ModContent.GetInstance<NightConfig>();

                    if (ModLoader.TryGetMod("RealisticSky", out Mod skies))
                        realisticSkies = skies;

                    normalStarCount = (Main.numStars, Main.star.Length, starTextures.Length);

                    ApplyPatches(true);

                    Array.Resize<Asset<Texture2D>>(ref Terraria.GameContent.TextureAssets.Star, NewArraySize);

					SpawnNewStars();

					for (int i = 0; i < Terraria.GameContent.TextureAssets.Star.Length; i++)
					{
						StarTexturesReference[i] = ModContent.Request<Texture2D>("BetterNightSky/Textures/Star_" + i);
					}

					for (int i = 0; i < MoonTexturesReference.Length; i++)
					{
						MoonTexturesReference[i] = ModContent.Request<Texture2D>("BetterNightSky/Textures/Moon1");
					}

					int startIndex = NewStarCount;

                    for (int i = 8; i < NewArraySize; i++)
					{
						Main.star[startIndex - (i - 7)].type = i;
						CelestialIndex.Add((CelestialObject)i, startIndex - (i - 7));
					}
				}
			}

			private TaskCompletionSource<bool> unloadTcs;

			public void DoUnloads()
			{
                //if (unloadTcs != null)
                //{
                UnifiedRandom rando = new UnifiedRandom();

                for (int i = 0; i < Main.star.Length; i++)
                {
                    if (Main.star[i].type > 4)
                    {
                        Main.star[i].type = rando.Next(5);
                    }
                }

                Array.Resize<Asset<Texture2D>>(ref Terraria.GameContent.TextureAssets.Star, normalStarCount.Item3);
					Array.Resize<Star>(ref Main.star, normalStarCount.Item2);
					starVelocityForAether = null;

                    Main.numStars = normalStarCount.Item1;

					for (int i = 0; i < normalStarCount.Item3; i++)
					{
						StarTexturesReference[i] = ModContent.Request<Texture2D>("Terraria/Images/Star_" + i);
					}

					for (int i = 0; i < moonTextures.Length; i++)
					{
						MoonTexturesReference[i] = ModContent.Request<Texture2D>("Terraria/Images/Moon_" + i);
					}

                Star.SpawnStars();
                //unloadTcs.SetResult(true);
                //unloadTcs = null;
                //}
            }

			public override void Unload()
			{

				if (!Main.dedServ)
				{
                    ApplyPatches(false);

                    DoUnloads();

                    CelestialIndex.Clear();
                    Main.numStars = normalStarCount.Item1;
                }
			}
		}
    }
}