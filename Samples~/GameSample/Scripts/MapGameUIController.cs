// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using Niantic.Lightship.Maps.SampleAssets.Player;
using TMPro;
using UnityEngine;

namespace Niantic.Lightship.Maps.Samples.GameSample
{
    /// <summary>
    /// Simple UI controller for the MapGame, it switches between a couple of screens and reacts to
    /// various button presses and keeps resources UI updated
    /// </summary>
    internal class MapGameUIController : MonoBehaviour
    {
        [SerializeField]
        private MapGameMapInteractions _mapInteractibles;

        [SerializeField]
        private PlayerLocationController _player;

        [SerializeField]
        private GameObject _introScreen;

        [SerializeField]
        private GameObject _errorScreen;

        [SerializeField]
        private GameObject _gamePlayScreen;

        [SerializeField]
        private GameObject _buildMenu;

        [SerializeField]
        private GameObject _placeStructureScreen;

        [SerializeField]
        private GameObject _buildMenuButton;

        [SerializeField]
        private GameObject _gameOverScreen;

        [SerializeField]
        private TMP_Text _woodText;

        [SerializeField]
        private TMP_Text _planksText;

        [SerializeField]
        private TMP_Text _stoneText;

        [SerializeField]
        private TMP_Text _bricksText;

        [SerializeField]
        private TMP_Text _errorText;

        // keeps track if the player has already won or not
        private bool _hasPlayerWon;

        private void Start()
        {
            _introScreen.SetActive(true);
            _gamePlayScreen.SetActive(false);
            _buildMenu.SetActive(false);
            _placeStructureScreen.SetActive(false);
            _buildMenuButton.SetActive(true);
            _gameOverScreen.SetActive(false);

            MapGameState.Instance.OnStructureBuilt += OnStructurePlaced;
            MapGameState.Instance.OnResourceUpdated += OnResourceUpdated;
            _player.OnGpsError += OnGpsError;
        }

        private void OnGpsError(string errorMessage)
        {
            _errorText.text = errorMessage;
            _errorScreen.SetActive(true);
        }

        private void OnDestroy()
        {
            if (MapGameState.Instance != null)
            {
                MapGameState.Instance.OnStructureBuilt -= OnStructurePlaced;
                MapGameState.Instance.OnResourceUpdated -= OnResourceUpdated;
            }

            if (_player != null)
            {
                _player.OnGpsError -= OnGpsError;
            }
        }

        private void OnResourceUpdated(MapGameState.ResourceType resourceType, int amount)
        {
            switch (resourceType)
            {
                case MapGameState.ResourceType.Wood:
                    _woodText.text = amount.ToString();
                    break;
                case MapGameState.ResourceType.Planks:
                    _planksText.text = amount.ToString();
                    break;
                case MapGameState.ResourceType.Stone:
                    _stoneText.text = amount.ToString();
                    break;
                case MapGameState.ResourceType.Bricks:
                    _bricksText.text = amount.ToString();
                    break;
            }
        }

        public void OnIntroContinuePressed()
        {
            _introScreen.SetActive(false);
            _gamePlayScreen.SetActive(true);
        }

        public void OnBuildButtonPressed()
        {
            // toggle the build menu
            _buildMenu.SetActive(!_buildMenu.activeInHierarchy);
        }

        public void OnBuildMenuClosePressed()
        {
            _buildMenu.SetActive(false);
        }

        public void OnBuildStructureItemPressed(int structureIndex)
        {
            _buildMenu.SetActive(false);
            _placeStructureScreen.SetActive(true);
            _buildMenuButton.SetActive(false);
            _mapInteractibles.StartPlacingStructure((MapGameState.StructureType)structureIndex);
        }

        private void OnStructurePlaced(MapGameState.StructureType structureType)
        {
            _placeStructureScreen.SetActive(false);
            _buildMenuButton.SetActive(true);

            // only show winning screen on first placement of stronghold
            if (!_hasPlayerWon && structureType == MapGameState.StructureType.Stronghold)
            {
                _hasPlayerWon = true;
                _gameOverScreen.SetActive(true);
                _gamePlayScreen.SetActive(false);
            }
        }

        public void OnGameOverContinuePressed()
        {
            _gameOverScreen.SetActive(false);
            _gamePlayScreen.SetActive(true);
        }
    }
}
