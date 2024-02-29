﻿using HarmonyLib;
using PlaylistManager.Configuration;
using PlaylistManager.Utilities;

/*
 * This patch removes the download icon for empty beatmaplevelcollections
 * Introduced since 1.18.0
 */

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionCell), nameof(AnnotatedBeatmapLevelCollectionCell.RefreshAvailabilityAsync))]
    internal class AnnotatedBeatmapLevelCollectionCell_RefreshAvailabilityAsync
    {
        private static void Postfix(AnnotatedBeatmapLevelCollectionCell __instance, BeatmapLevelPack ____beatmapLevelPack)
        {
            if (____beatmapLevelPack is BeatSaberPlaylistsLib.Types.IPlaylist playlist)
            {
                __instance.SetDownloadIconVisible(PluginConfig.Instance.ShowDownloadIcon && PlaylistLibUtils.GetMissingSongs(playlist).Count > 0);
            }
        }
    }
}
