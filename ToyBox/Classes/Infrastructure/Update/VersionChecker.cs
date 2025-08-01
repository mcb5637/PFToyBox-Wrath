﻿using Kingmaker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using UnityModManagerNet;

namespace ToyBox {
    public static class VersionChecker {
        // Find first entry where Mod Version of entry >= current mod version
        // Using that entry, the mod is compatible if the current game version is < than the game version in the entry
        // Meaning, an entry [x, y] will, for every mod with version <= x mark every version >= y as incompatible
        public static bool IsGameVersionSupported(Version modVersion, UnityModManager.ModEntry.ModLogger logger, string linkToIncompatibilitiesFile) {
            try {
                using var web = new WebClient();
                var raw = web.DownloadString(linkToIncompatibilitiesFile);
                var definition = new[] { new[] { "", "" } };
                var versions = JsonConvert.DeserializeAnonymousType(raw, definition);
                var currentOrNewer = versions.FirstOrDefault(v => new Version(v[0]) >= modVersion);
                if (currentOrNewer == null) return true;
                return IsVersionGreaterThan(GetNumifiedVersion(logger, currentOrNewer[1]), GetNumifiedVersion(logger, GameVersion.GetVersion()));
            } catch (Exception ex) {
                logger.Log(ex.ToString());
            }
            return true;
        }
        public static bool IsVersionGreaterThan(List<BigInteger> a, List<BigInteger> b) {
            int maxLen = Math.Max(a.Count, b.Count);
            for (int i = 0; i < maxLen; i++) {
                BigInteger t = (i < a.Count) ? a[i] : 0;
                BigInteger g = (i < b.Count) ? b[i] : 0;
                if (t > g) {
                    return true;
                }
                if (t < g) {
                    return false;
                }
            }
            return false;
        }
        public static List<BigInteger> GetNumifiedVersion(UnityModManager.ModEntry.ModLogger logger, string version) {
            var comps = version.Split('.');
            var newComps = new List<BigInteger>();
            foreach (var comp in comps) {
                BigInteger num = 0;
                foreach (var c in comp) {
                    try {
                        if (uint.TryParse(c.ToString(), out var n)) {
                            num = num * 10u + n;
                        } else {
                            int signedCharNumber = char.ToUpper(c) - ' ';
                            uint unsignedCharNumber = (uint)Math.Max(0, Math.Min(signedCharNumber, 99));
                            num = num * 100u + unsignedCharNumber;
                        }
                    } catch (Exception ex) {
                        logger.Log($"Error while trying to numify version component {comp}, continuing with {num}.\n{ex}");
                        break;
                    }
                }
                newComps.Add(num);
            }
            return newComps;
        }
    }
}
