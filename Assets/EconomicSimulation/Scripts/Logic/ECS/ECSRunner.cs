using Leopotam.EcsLite;
using UnityEngine;

namespace Nashet.EconomicSimulation.ECS
{
	public class ECSRunner : MonoBehaviour
	{
		public bool IsReady { get; private set; }

		private static EcsWorld _world;
		public static EcsWorld EcsWorld
		{
			get
			{
				if (_world == null)
				{
					_world = new EcsWorld();
				}
				return _world;
			}
		}
		private IEcsSystems initSystems;
		private static IEcsSystems updateSystems;
		private IEcsSystems fixedUpdateSystems;

		private void Awake()
		{
			initSystems = new EcsSystems(EcsWorld)
				//.Add(new InitSystem())

				;

			initSystems.Init();
			IsReady = true;

			updateSystems = new EcsSystems(EcsWorld)

#if UNITY_EDITOR && DEBUG
		// add debug systems for custom worlds here, for example:
		// .Add (new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem ("events"))
		.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem(entityNameFormat: "000"))
#endif
				//.Add(new BattleSystem())
				.Add(new ScienceSystem())
				.Add(new CountryCoresSystem())
				.Add(new ProduceSystem())
				;

			updateSystems.Init();

			fixedUpdateSystems = new EcsSystems(EcsWorld)// gameData)
				;

			fixedUpdateSystems.Init();
		}

		public static void RunManually()
		{
			updateSystems.Run();
		}

		//private void Update()
		//{
		//	updateSystems.Run();
		//}

		//private void FixedUpdate()
		//{
		//	fixedUpdateSystems.Run();
		//}

		private void OnDestroy()
		{
			initSystems.Destroy();
			updateSystems.Destroy();
			fixedUpdateSystems.Destroy();
			EcsWorld.Destroy();
		}
	}
}