using ArtTest.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

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

        private Vector3 spawnZOffset = new Vector3(0, 0, -2);

        [SerializeField]
        private Transform[] spawnPoints;

        [SerializeField]
        private GameObject blockPrefab;
        [SerializeField]
        private GameObject blockVisualPrefab;

        public ObjectPool<Block> BlockPool;
        public ObjectPool<GameObject> BlockVisualPool;

        private BlockData[] blockData;
        private Sprite cachedSprite;
        private bool isInitialized = false;

        public event Action<Block> OnBlockSpawned;
        public event Action OnSpawnFinished;

        private void Start()
        {
            SpawnBlocks();
        }

        public void Initialize(GameSettings gameSettings, GameTheme gameTheme)
        {
            if (isInitialized)
            {
                return;
            }

            BlockPool = new ObjectPool<Block>(
                createFunc: () => Instantiate(blockPrefab).GetComponent<Block>(),
                actionOnGet: block =>
                {
                    if (block.transform.childCount > 0)
                    {
                        foreach (Transform child in block.transform)
                        {
                            BlockVisualPool.Release(child.gameObject);
                        }
                    }
                    block.gameObject.SetActive(true);
                },
                actionOnRelease: block => block.gameObject.SetActive(false),
                actionOnDestroy: block => Destroy(block.gameObject),
                collectionCheck: true,
                defaultCapacity: 10
            );

            BlockVisualPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(blockVisualPrefab),
                actionOnGet: visual => visual.SetActive(true),
                actionOnRelease: visual =>
                {
                    if (visual == null)
                    {
                        return;
                    }
                    visual.transform.SetParent(this.transform);
                    visual.SetActive(false);
                },
                actionOnDestroy: visual => Destroy(visual),
                collectionCheck: true,
                defaultCapacity: 100
            );

            print($"BlockSpawnManager.Initialize: {gameSettings.BlockData.Length} blocks available for spawning.");
            cachedSprite = gameTheme.BlockSprite;
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

                var blockObject = BlockPool.Get(); 

                var block = blockObject.GetComponent<Block>();
                var draggableComponent = blockObject.GetComponent<DragComponent>();
                if (!draggableComponent)
                {
                    draggableComponent = blockObject.gameObject.AddComponent<DragComponent>();
                }

                blockObject.GetComponent<DragComponent>().SetStartPosition(spawnPoint.position + spawnZOffset);
                blockObject.transform.rotation = Quaternion.identity;
                blockObject.transform.localEulerAngles += rotations[UnityEngine.Random.Range(0, rotations.Length)];

                draggableComponent.OnDragEnd += HandleBlockDragEnd;

                block.Initialize(cachedSprite, data.Cells, data.BlockColor, ref BlockVisualPool);
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

            block.GetComponent<DragComponent>().OnDragEnd -= HandleBlockDragEnd;
        }
    }
}
