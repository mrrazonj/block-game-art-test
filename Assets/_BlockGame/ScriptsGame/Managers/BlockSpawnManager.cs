using System;
using System.Collections.Generic;
using ArtTest.Models;
using UnityEngine;

namespace ArtTest.Game
{
    public class BlockSpawnManager : MonoBehaviour
    {
        [HideInInspector]
        public List<Block> ActiveBlocks { get; private set; }

        [SerializeField]
        private Transform[] spawnPoints;

        [SerializeField]
        private GameObject blockPrefab;

        private BlockData[] blockData;
        private bool isInitialized = false;

        private int gridWidth;
        private int gridHeight;

        public Action<Block> OnBlockSpawned;

        private void Start()
        {
            SpawnBlocks();
        }

        public void Initialize(GameSettings gameSettings)
        {
            if (isInitialized)
            {
                return;
            }

            print($"BlockSpawnManager.Initialize: {gameSettings.BlockData.Length} blocks available for spawning.");
            blockData = gameSettings.BlockData;

            ActiveBlocks = new List<Block>();
            isInitialized = true;
        }

        public void SpawnBlocks()
        {
            if (!isInitialized)
            {
                Debug.LogError("BlockSpawnManager is not initialized.");
                return;
            }

            print($"SpawnBlocks: {blockData.Length} blocks available for spawning.");

            foreach (var spawnPoint in spawnPoints)
            {
                var randomIndex = UnityEngine.Random.Range(0, blockData.Length);
                var data = blockData[randomIndex];

                var blockObject = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);

                var block = blockObject.GetComponent<Block>();
                var draggableComponent = blockObject.GetComponent<Draggable>();
                if (!draggableComponent)
                {
                    draggableComponent = blockObject.AddComponent<Draggable>();
                }

                block.Initialize(data.Cells, data.BlockColor);
                ActiveBlocks.Add(block);

                OnBlockSpawned?.Invoke(block);
            }
        }

        public bool CanPlaceAnyBlock(GridManager gridManager, GameSettings gameSettings)
        {
            foreach (var block in ActiveBlocks)
            {
                for (int x = 0; x < gameSettings.GridWidth; x++)
                {
                    for (int y = 0; y < gameSettings.GridHeight; y++)
                    {
                        var gridPosition = new Vector2Int(x, y);
                        if (gridManager.CheckValidPlacement(block, gridPosition))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
