using System;
using System.IO;
using System.Reflection;
using Duckov.UI;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using UnityEngine;

namespace BigStorage2
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public static int StorageCapacityIncrease { get; private set; } = 300;

        protected override void OnAfterSetup()
        {
            Debug.Log("BigStorage2模组：OnAfterSetup方法被调用");
            LoadConfig();
        }


        void OnEnable()
        {
            TryHookStorage();
        }

        void OnDisable()
        {
            UnhookStorage();
        }

        private void TryHookStorage()
        {
            PlayerStorage.OnRecalculateStorageCapacity -= OnRecalculateStorageCapacity;
            PlayerStorage.OnRecalculateStorageCapacity += OnRecalculateStorageCapacity;
            Debug.Log("BigStorage2模组：成功挂钩 OnRecalculateStorageCapacity 事件");
        }

        private void UnhookStorage()
        {
            PlayerStorage.OnRecalculateStorageCapacity -= OnRecalculateStorageCapacity;
            Debug.Log("BigStorage2模组：取消挂钩 OnRecalculateStorageCapacity 事件");
        }

        private void OnRecalculateStorageCapacity(PlayerStorage.StorageCapacityCalculationHolder calculationHolder)
        {
            Debug.Log($"仓库容量：{PlayerStorage.Inventory.Capacity}，holder容量：{calculationHolder.capacity}");
            calculationHolder.capacity += StorageCapacityIncrease;
            // 检查玩家仓库物品量是否大于容量 PlayerStorage.Inventory.Capacity是最终仓库容量（包括mod修改之后的
            if (PlayerStorage.Inventory.Content.Count > PlayerStorage.Inventory.Capacity)
            {
                // 将超出的部分存入 马蜂自提点
                var addCount = 0;
                // 将超出的部分存入 马蜂自提点
                for (int i = 0; i < PlayerStorage.Inventory.Content.Count; i++)
                {
                    if (i >= PlayerStorage.Inventory.Capacity)
                    {
                        var storageItem = PlayerStorage.Inventory.Content[i];
                        PlayerStorage.IncomingItemBuffer.Add(ItemTreeData.FromItem(storageItem));
                        storageItem.Detach();
                        storageItem.DestroyTree();
                        addCount += 1;
                    }
                }

                if (addCount > 0)
                {
                    try
                    {
                        NotificationText.Push($"检测到仓库容量异常，有{addCount}个物品发送到[马蜂自提点]");
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"BigStorage2模组：错误：{e.Message}");
                    }
                }
            }
        }

        private void LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "info.ini");
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines)
                    {
                        if (line.Trim().StartsWith("StorageCapacity="))
                        {
                            string value = line.Trim().Substring("StorageCapacity=".Length).Trim();
                            if (int.TryParse(value, out int capacity))
                            {
                                StorageCapacityIncrease = capacity;
                                Debug.Log($"BigStorage2模组：已从配置文件读取StorageCapacity值: {capacity}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("BigStorage2模组：未找到info.ini文件，使用默认值");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"BigStorage2模组：读取配置文件时出错：{e.Message}，使用默认值");
            }
        }
    }
}