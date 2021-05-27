﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using BeatSaberPlaylistsLib;
using PlaylistManager.Configuration;
using UnityEngine;

namespace PlaylistManager.Utilities
{
    public class PlaylistLibUtils
    {
        public static BeatSaberPlaylistsLib.PlaylistManager playlistManager
        {
            get
            {
                return BeatSaberPlaylistsLib.PlaylistManager.DefaultManager;
            }
        }

        public static BeatSaberPlaylistsLib.Types.IPlaylist CreatePlaylist(string playlistName, string playlistAuthorName, BeatSaberPlaylistsLib.PlaylistManager playlistManager, bool defaultCover = true)
        {
            string playlistFolderPath = Path.Combine(Environment.CurrentDirectory, "Playlists");
            string playlistFileName = string.Join("_", playlistName.Replace("/", "").Replace("\\", "").Replace(".", "").Split(' '));
            if (string.IsNullOrEmpty(playlistFileName))
            {
                playlistFileName = "playlist";
            }
            string playlistPath = Path.Combine(playlistFolderPath, playlistFileName + ".blist");
            string originalPlaylistPath = Path.Combine(playlistFolderPath, playlistFileName);
            int dupNum = 0;
            while (File.Exists(playlistPath))
            {
                dupNum++;
                playlistPath = originalPlaylistPath + string.Format("({0}).blist", dupNum);
                playlistFileName = playlistFileName + string.Format("({0})", dupNum);
            }

            BeatSaberPlaylistsLib.Types.IPlaylist playlist = playlistManager.CreatePlaylist(playlistFileName, playlistName, playlistAuthorName, "");

            if (defaultCover)
            {
                using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.Logo.png"))
                {
                    playlist.SetCover(imageStream);
                }
            }

            if (!PluginConfig.Instance.DefaultAllowDuplicates)
            {
                playlist.AllowDuplicates = false;
            }

            playlistManager.StorePlaylist(playlist);
            return playlist;
        }

        #region Image

        private static DrawSettings defaultDrawSettings = new DrawSettings
        {
            Color = System.Drawing.Color.White,
            DrawStyle = DrawStyle.Normal,
            Font = new Font("teko", 80, FontStyle.Regular),
            StringFormat = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            },
            MinTextSize = 80,
            MaxTextSize = 140,
            WrapWidth = 10
        };

        private static Stream GetFolderImageStream() =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream("PlaylistManager.Icons.FolderIcon.png");

        internal static Sprite DrawFolderIcon(string str)
        {
            if (str.Length > 15)
            {
                str = str.Substring(0, 15) + "...";
            }
            Image img = ImageUtilities.DrawString("\n"+str, Image.FromStream(GetFolderImageStream()), defaultDrawSettings);
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            return BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(ms.ToArray());
        }

        internal static Sprite GeneratePlaylistIcon(BeatSaberPlaylistsLib.Types.IPlaylist playlist) => BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(BeatSaberPlaylistsLib.Utilities.GenerateCoverForPlaylist(playlist).ToArray());

        #endregion
    }
}
