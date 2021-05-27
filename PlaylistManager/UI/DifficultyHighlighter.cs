﻿using BeatSaberPlaylistsLib.Types;
using HMUI;
using PlaylistManager.HarmonyPatches;
using PlaylistManager.Interfaces;
using PlaylistManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace PlaylistManager.UI
{
    public class DifficultyHighlighter : IPreviewBeatmapLevelUpdater, IDisposable, IInitializable
    {
        private BeatmapCharacteristicSegmentedControlController beatmapCharacteristicSegmentedControlController;
        private readonly IconSegmentedControl beatmapCharacteristicSegmentedControl;
        private BeatmapDifficultySegmentedControlController beatmapDifficultySegmentedControlController;
        private SegmentedControl beatmapDifficultySegmentedControl;
        private PlaylistSong selectedPlaylistSong;

        internal event Action<bool> selectedDifficultyChanged;

        public DifficultyHighlighter(StandardLevelDetailViewController standardLevelDetailViewController)
        {
            StandardLevelDetailView standardLevelDetailView = Accessors.StandardLevelDetailViewAccessor(ref standardLevelDetailViewController);
            beatmapCharacteristicSegmentedControlController = Accessors.BeatmapCharacteristicSegmentedControlController(ref standardLevelDetailView);
            beatmapCharacteristicSegmentedControl = Accessors.BeatmapCharacteristicsSegmentedControlAccessor(ref beatmapCharacteristicSegmentedControlController);
            beatmapDifficultySegmentedControlController = Accessors.BeatmapDifficultySegmentedControlControllerAccessor(ref standardLevelDetailView);
            beatmapDifficultySegmentedControl = Accessors.BeatmapDifficultySegmentedControlAccessor(ref beatmapDifficultySegmentedControlController);
        }

        public void Initialize()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent += BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            beatmapDifficultySegmentedControl.didSelectCellEvent += BeatmapDifficultySegmentedControl_didSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent += BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void Dispose()
        {
            beatmapCharacteristicSegmentedControl.didSelectCellEvent -= BeatmapCharacteristicSegmentedControl_DidSelectCellEvent;
            beatmapDifficultySegmentedControl.didSelectCellEvent -= BeatmapDifficultySegmentedControl_didSelectCellEvent;
            BeatmapDifficultySegmentedControlController_SetData.CharacteristicsSegmentedControllerDataSetEvent -= BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent;
        }

        public void PreviewBeatmapLevelUpdated(IPreviewBeatmapLevel beatmapLevel)
        {
            if (beatmapLevel is PlaylistSong selectedPlaylistSong)
            {
                this.selectedPlaylistSong = selectedPlaylistSong;
            }
            else
            {
                this.selectedPlaylistSong = null;
            }
        }

        private void HighlightDifficultiesForSelectedCharacteristic()
        {
            if (selectedPlaylistSong != null && selectedPlaylistSong.Difficulties != null && selectedPlaylistSong.Difficulties.Count != 0)
            {
                List<Difficulty> difficultiesToHighlight = selectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.ToUpper() == beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName.ToUpper());
                List<BeatmapDifficulty> availaibleDifficulties = Accessors.DifficultiesAccessor(ref beatmapDifficultySegmentedControlController);
                List<SegmentedControlCell> difficultyCells = Accessors.SegmentedControllerCellsAccessor(ref beatmapDifficultySegmentedControl);

                foreach (var difficulty in difficultiesToHighlight)
                {
                    if (availaibleDifficulties.Contains(difficulty.BeatmapDifficulty))
                    {
                        SegmentedControlCell cellToHighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(difficulty.BeatmapDifficulty)];
                        CurvedTextMeshPro textToHighlight = cellToHighlight.GetComponentInChildren<CurvedTextMeshPro>();
                        textToHighlight.faceColor = new UnityEngine.Color32(255, 255, 0, 255);
                    }
                }
            }
        }

        private void RaiseDifficultyChangedEvent() => selectedDifficultyChanged?.Invoke(IsSelectedDifficultyHighlighted);

        internal void ToggleSelectedDifficultyHighlight()
        {
            if (selectedPlaylistSong != null)
            {
                List<SegmentedControlCell> difficultyCells = Accessors.SegmentedControllerCellsAccessor(ref beatmapDifficultySegmentedControl);
                if (IsSelectedDifficultyHighlighted)
                {
                    selectedPlaylistSong.Difficulties.RemoveAll(d => d.BeatmapDifficulty == beatmapDifficultySegmentedControlController.selectedDifficulty);
                    SegmentedControlCell cellToUnhighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(beatmapDifficultySegmentedControlController.selectedDifficulty)];
                    CurvedTextMeshPro textToUnhighlight = cellToUnhighlight.GetComponentInChildren<CurvedTextMeshPro>();
                    textToUnhighlight.faceColor = new UnityEngine.Color32(255, 255, 255, 255);
                }
                else
                {
                    if (selectedPlaylistSong.Difficulties == null)
                    {
                        selectedPlaylistSong.Difficulties = new List<Difficulty>();
                    }
                    Difficulty difficulty = new Difficulty();
                    difficulty.BeatmapDifficulty = beatmapDifficultySegmentedControlController.selectedDifficulty;
                    difficulty.Characteristic = beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName;
                    selectedPlaylistSong.AddDifficulty(difficulty);

                    SegmentedControlCell cellToHighlight = difficultyCells[beatmapDifficultySegmentedControlController.GetClosestDifficultyIndex(beatmapDifficultySegmentedControlController.selectedDifficulty)];
                    CurvedTextMeshPro textToHighlight = cellToHighlight.GetComponentInChildren<CurvedTextMeshPro>();
                    textToHighlight.faceColor = new UnityEngine.Color32(255, 255, 0, 255);
                }
            }

        }

        private void BeatmapDifficultySegmentedControlController_CharacteristicsSegmentedControllerDataSetEvent()
        {
            HighlightDifficultiesForSelectedCharacteristic();
            RaiseDifficultyChangedEvent();
        }

        private void BeatmapCharacteristicSegmentedControl_DidSelectCellEvent(SegmentedControl _, int __)
        {
            HighlightDifficultiesForSelectedCharacteristic();
        }

        private void BeatmapDifficultySegmentedControl_didSelectCellEvent(SegmentedControl arg1, int arg2)
        {
            RaiseDifficultyChangedEvent();
        }

        private bool IsSelectedDifficultyHighlighted
        {
            get
            {
                if (selectedPlaylistSong != null && selectedPlaylistSong.Difficulties != null && selectedPlaylistSong.Difficulties.Count != 0)
                {
                    List<Difficulty> difficulties = selectedPlaylistSong.Difficulties.FindAll(difficulty => difficulty.Characteristic.ToUpper() == beatmapCharacteristicSegmentedControlController.selectedBeatmapCharacteristic.serializedName.ToUpper());
                    return difficulties.Select(d => d.BeatmapDifficulty).Contains(beatmapDifficultySegmentedControlController.selectedDifficulty);
                }
                return false;
            }
        }
    }
}
