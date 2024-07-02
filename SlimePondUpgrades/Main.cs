using HarmonyLib;
using SRML;
using SRML.Console;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SRML.SR;
using SRML.Utils.Enum;
using System.Linq;

namespace PondUpgrades
{
    public class Main : ModEntryPoint
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{System.Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
        }
        public override void Load()
        {
            var PlotPurchaseUI = Resources.FindObjectsOfTypeAll<EmptyPlotUI>().First((x) => !x.name.EndsWith("(Clone)"));
            LandPlotUpgradeRegistry.RegisterPurchasableUpgrade<PondUI>(new LandPlotUpgradeRegistry.UpgradeShopEntry()
            {
                cost = PlotPurchaseUI.pond.cost * 2,
                icon = PlotPurchaseUI.pond.icon,
                isUnlocked = (x) => true,
                LandPlotName = "pond",
                landplotPediaId = PediaDirector.Id.POND,
                mainImg = PlotPurchaseUI.pond.img,
                upgrade = Ids.POND_SLIME_CAPACITY,
                isAvailable = (x) => !x.HasUpgrade(Ids.POND_SLIME_CAPACITY)
            });
            LandPlotUpgradeRegistry.RegisterPurchasableUpgrade<PondUI>(new LandPlotUpgradeRegistry.UpgradeShopEntry()
            {
                cost = PlotPurchaseUI.pond.cost * 2,
                icon = PlotPurchaseUI.pond.icon,
                isUnlocked = (x) => true,
                LandPlotName = "pond",
                landplotPediaId = PediaDirector.Id.POND,
                mainImg = PlotPurchaseUI.pond.img,
                upgrade = Ids.POND_PLORT_CAPACITY,
                isAvailable = (x) => !x.HasUpgrade(Ids.POND_PLORT_CAPACITY)
            });
            var ancientWater = GameContext.Instance.LookupDirector.GetIcon(Identifiable.Id.MAGIC_WATER_LIQUID);
            LandPlotUpgradeRegistry.RegisterPurchasableUpgrade<PondUI>(new LandPlotUpgradeRegistry.UpgradeShopEntry()
            {
                cost = PlotPurchaseUI.pond.cost * 10,
                icon = ancientWater,
                isUnlocked = (x) => x.HasUpgrade(Ids.POND_PLORT_CAPACITY) && x.HasUpgrade(Ids.POND_SLIME_CAPACITY),
                LandPlotName = "pond",
                landplotPediaId = PediaDirector.Id.POND,
                mainImg = ancientWater,
                upgrade = Ids.POND_ANCIENT_BLESSING,
                isAvailable = (x) => !x.HasUpgrade(Ids.POND_ANCIENT_BLESSING)
            });
            LandPlotUpgradeRegistry.RegisterPlotUpgrader<AncientWaterUpgrader>(LandPlot.Id.POND);
            if (SRModLoader.IsModPresent("slimeponds"))
                LandPlotUpgradeRegistry.RegisterPlotUpgrader<AncientWaterUpgrader>((LandPlot.Id)System.Enum.Parse(typeof(LandPlot.Id), "POND_SLIME"));
        }

        class AncientWaterUpgrader : PlotUpgrader
        {
            public override void Apply(LandPlot.Upgrade upgrade)
            {
                if (upgrade == Ids.POND_ANCIENT_BLESSING)
                    transform.Find("Water/Water Scaler/Surface").GetComponent<MeshRenderer>().material = Resources.FindObjectsOfTypeAll<Material>().First((x) => x.name == "Depth Magic Water Ball");
            }
        }
    }

    [EnumHolder]
    static class Ids
    {
        public static LandPlot.Upgrade POND_SLIME_CAPACITY;
        public static LandPlot.Upgrade POND_PLORT_CAPACITY;
        public static LandPlot.Upgrade POND_ANCIENT_BLESSING;
    }

    [HarmonyPatch(typeof(ResourceBundle), "LoadFromText")]
    class Patch_LoadResources
    {
        static void Postfix(string path, Dictionary<string, string> __result)
        {
            if (path == "pedia")
            {
                var lang = GameContext.Instance.MessageDirector.GetCultureLang();
                if (lang == MessageDirector.Lang.RU)
                {
                    __result.Add(Ids.POND_SLIME_CAPACITY, "Количество слаймов", "Удваивает количество слаймов, которых может содержать пруд");
                    __result.Add(Ids.POND_PLORT_CAPACITY, "Количество плортов", "Удваивает количество плортов, которые может содержать пруд");
                    __result.Add(Ids.POND_ANCIENT_BLESSING, "Древнее Благословение", "Благословляет воду древней силой, увеличивая возможности содержания прудом слаймов и плортов в три раза");
                }
                else
                {
                    __result.Add(Ids.POND_SLIME_CAPACITY, "Slime Capacity", "Doubles the number of slimes that the pond can contain");
                    __result.Add(Ids.POND_PLORT_CAPACITY, "Plort Capacity", "Doubles the number of plorts that the pond can contain");
                    __result.Add(Ids.POND_ANCIENT_BLESSING, "Ancient Blessing", "Blesses the water with an ancient power, increasing the slime and plort capacity by 3 times");
                }
            }
        }
    }

    [HarmonyPatch(typeof(SlimeEatWater), "CalcMaximumSlimeDensity")]
    static class Patch_CalcMaximumSlimeDensity
    {
        static void Postfix(SlimeEatWater __instance, ref int __result)
        {
            var mult = 1;
            foreach (var w in __instance.waters)
                if (w)
                {
                    var l = w.GetComponentInParent<LandPlot>();
                    if (l)
                    {
                        if (mult < 2 && l.HasUpgrade(Ids.POND_SLIME_CAPACITY))
                            mult = 2;
                        if (mult < 6 && l.HasUpgrade(Ids.POND_ANCIENT_BLESSING))
                            mult = 6;
                    }
                }
            __result *= mult;
        }
    }

    [HarmonyPatch(typeof(SlimeEatWater), "CalcMaximumPlortDensity")]
    static class Patch_CalcMaximumPlortDensity
    {
        static void Postfix(SlimeEatWater __instance, ref int __result)
        {
            var mult = 1;
            foreach (var w in __instance.waters)
                if (w)
                {
                    var l = w.GetComponentInParent<LandPlot>();
                    if (l)
                    {
                        if (mult < 2 && l.HasUpgrade(Ids.POND_PLORT_CAPACITY))
                            mult = 2;
                        if (mult < 6 && l.HasUpgrade(Ids.POND_ANCIENT_BLESSING))
                            mult = 6;
                    }
                }
            __result *= mult;
        }
    }

    public static class ExtentionMethods
    {

        public static void Add(this Dictionary<string, string> col, LandPlot.Upgrade id, string name, string desc, LandPlot.Id plot = LandPlot.Id.POND)
        {
            col[$"m.upgrade.name.{plot.ToString().ToLowerInvariant()}.{id.ToString().ToLowerInvariant()}"] = name;
            col[$"m.upgrade.desc.{plot.ToString().ToLowerInvariant()}.{id.ToString().ToLowerInvariant()}"] = desc;
        }
    }
}