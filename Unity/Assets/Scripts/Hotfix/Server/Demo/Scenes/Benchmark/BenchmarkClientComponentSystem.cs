﻿using System.Collections.Generic;
using System.Net.Sockets;
using ET.Client;

namespace ET.Server
{
    public static partial class BenchmarkClientComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BenchmarkClientComponent self)
        {
            for (int i = 0; i < 50; ++i)
            {
                self.Start().Coroutine();
            }
        }

        private static async ETTask Start(this BenchmarkClientComponent self)
        {
            await TimerComponent.Instance.WaitAsync(1000);

            Scene scene = await SceneFactory.CreateServerScene(self, VProcess.Instance.IdGenerater.GenerateId(), VProcess.Instance.IdGenerater.GenerateInstanceId(),
                self.DomainZone(), "bechmark", SceneType.Benchmark);
            
            Client.NetClientComponent netClientComponent = scene.AddComponent<Client.NetClientComponent, AddressFamily>(AddressFamily.InterNetwork);

            using Session session = netClientComponent.Create(StartSceneConfigCategory.Instance.BenchmarkServer.OuterIPPort);
            List<ETTask> list = new List<ETTask>(100000);

            async ETTask Call(Session s)
            {
                using G2C_Benchmark benchmark = await s.Call(C2G_Benchmark.Create(true)) as G2C_Benchmark;
            }
            
            for (int j = 0; j < 100000000; ++j)
            {
                list.Clear();
                for (int i = 0; i < list.Capacity; ++i)
                {
                    list.Add(Call(session));
                }
                await ETTaskHelper.WaitAll(list);
            }
        }
    }
}