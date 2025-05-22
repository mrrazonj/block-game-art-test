using ArtTest.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtTest.Game
{
    public class BlockSpawnManager : MonoBehaviour
    {
        [HideInInspector]
        public List<Block> ActiveBlocks { get; private set; }

        private Vector3[] rotations = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 90),
            new Vector3(0, 0, 180),
            new Vector3(0, 0, 270) 
        };

        [SerializeField]
        private Transform[] spawnPoints;

        [SerializeField]
        private GameObject blockPrefab;

        private BlockData[] blockData;
        private bool isInitialized = false;

        public event Action<Block> OnBlockSpawned;
        public event Action OnSpawnFinished;

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

                draggableComponent.OnDragEnd += HandleBlockDragEnd;

                blockObject.transform.localEulerAngles += rotations[UnityEngine.Random.Range(0, rotations.Length)];
                block.Initialize(data.Cells, data.BlockColor);
                ActiveBlocks.Add(block);

                OnBlockSpawned?.Invoke(block);
            }

            OnSpawnFinished?.Invoke();
        }

        private void HandleBlockDragEnd(Block block)
        {
            ActiveBlocks.Remove(block);
            if (ActiveBlocks.Count == 0)
            {
                SpawnBlocks();
            }

            block.GetComponent<Draggable>().OnDragEnd -= HandleBlockDragEnd;
        }
    }
}
