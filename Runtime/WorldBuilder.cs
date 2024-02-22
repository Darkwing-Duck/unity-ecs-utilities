using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace Nakuru.Unity.Ecs.Utilities
{
	
	public class WorldBuilder
	{
		// Unity built-in namespace filters
		private struct UnityNamespace
		{
			public const string Core = "Unity.Entities";
			public const string Transforms = "Unity.Transforms";
			public const string Scenes = "Unity.Scenes";
		}
		
		public readonly World World;
		
		// indicates whether need to create root groups or not
		private bool _withInitializeRootGroup;
		private bool _withSimulationRootGroup;
		private bool _withPresentationRootGroup;
		
		// collections of custom system types to be added to corresponding root group
		private readonly List<Type> _initializePhaseSystemTypes;
		private readonly List<Type> _presentationPhaseSystemTypes;
		private readonly List<Type> _simulationPhaseSystemTypes;
		
		private readonly HashSet<string> _namespaceFilters;
		
		private readonly HashSet<string> _excludingNameFilters;
		private readonly HashSet<Type> _excludedSystemTypes;

		private WorldBuilder(string name, WorldFlags flags, bool isDefault)
		{
			_namespaceFilters = new HashSet<string>();
			
			_excludingNameFilters = new HashSet<string>();
			_excludedSystemTypes = new HashSet<Type>();
			
			_initializePhaseSystemTypes = new List<Type>();
			_simulationPhaseSystemTypes = new List<Type>();
			_presentationPhaseSystemTypes = new List<Type>();
			
			World = new World(name, flags);

			if (isDefault)
				World.DefaultGameObjectInjectionWorld = World;
		}
		
		/// <summary>
		/// Entry point to create new world.
		/// </summary>
		/// <param name="name">Name of the world</param>
		/// <param name="flags">Flags of the world</param>
		/// <param name="isDefault">If true then the world will be assigned to <see cref="World.DefaultGameObjectInjectionWorld"/></param>
		/// <returns>WorldBuilder instance using which you can configure the world</returns>
		public static WorldBuilder NewWorld(string name, WorldFlags flags, bool isDefault = false) => new(name, flags, isDefault);

		/// <summary>
		/// Configures default world like <see cref="DefaultWorldInitialization"/> does.
		/// The resulting world will contain only Unity related systems but not yours.
		/// <list type="bullet">
		///		<item>
		///			To add custom system manually to specific root group you have to other builder methods like <see cref="WithSimulationPhaseSystem{TSystem}"/>
		///		</item>
		///		<item>
		///			To add custom system automatically based on attribute like <see cref="UpdateInGroupAttribute"/> call method <see cref="WithNamespaceFilter"/>
		///			to specify your namespace filter and all systems of the namespace will be added to the world in corresponding root groups by Unity rules.
		///		</item>
		/// </list>
		/// </summary>
		public WorldBuilder WithAllUnityDefaultSystems()
		{
			_withInitializeRootGroup = true;
			_withSimulationRootGroup = true;
			_withPresentationRootGroup = true;

			WithUnityCoreSystems();
			WithUnityTransformSystems();
			WithUnityScenesSystems();

			return this;
		}
		
		/// <summary>
		/// Indicates that <see cref="InitializationSystemGroup"/> will be created in the world
		/// </summary>
		public WorldBuilder WithInitializePhase()
		{
			_withInitializeRootGroup = true;
			return this;
		}
		
		/// <summary>
		/// Indicates that <see cref="SimulationSystemGroup"/> will be created in the world
		/// </summary>
		public WorldBuilder WithSimulationPhase()
		{
			_withSimulationRootGroup = true;
			return this;
		}
		
		/// <summary>
		/// Indicates that <see cref="PresentationSystemGroup"/> will be created in the world
		/// </summary>
		public WorldBuilder WithPresentationPhase()
		{
			_withPresentationRootGroup = true;
			return this;
		}
		
		/// <summary>
		/// Adds all systems from the namespace <c>Unity.Entities</c>
		/// </summary>
		public WorldBuilder WithUnityCoreSystems() => WithNamespaceFilter(UnityNamespace.Core);
		
		/// <summary>
		/// Adds all systems from the namespace <c>Unity.Transforms</c>
		/// </summary>
		public WorldBuilder WithUnityTransformSystems() => WithNamespaceFilter(UnityNamespace.Transforms);
		
		/// <summary>
		/// Adds all systems from the namespace <c>Unity.Scenes</c>
		/// </summary>
		public WorldBuilder WithUnityScenesSystems() => WithNamespaceFilter(UnityNamespace.Scenes);
		
		/// <summary>
		/// Adds excluding name filter.
		/// All the systems falling under the filter will be excluded from the world.
		/// </summary>
		public WorldBuilder WithExcludingNameFilter(string value)
		{
			_excludingNameFilters.Add(value);
			return this;
		}

		/// <summary>
		/// Excludes the specified system from being added to the world
		/// </summary>
		public WorldBuilder WithExcludingSystem<TSystem>()
		{
			_excludedSystemTypes.Add(typeof(TSystem));
			return this;
		}
		
		/// <summary>
		/// Excludes the list of systems from being added to the world
		/// </summary>
		public WorldBuilder WithExcludingSystems(IEnumerable<Type> systems)
		{
			_excludedSystemTypes.UnionWith(systems);
			return this;
		}
		
		/// <summary>
		/// Adds namespace filter.
		/// All the systems of the namespace will be added to the world.
		/// </summary>
		public WorldBuilder WithNamespaceFilter(string value)
		{
			_namespaceFilters.Add(value);
			return this;
		}

		/// <summary>
		/// Adds managed system to the <see cref="InitializationSystemGroup"/>.
		/// </summary>
		public WorldBuilder WithInitializePhaseManagedSystem<TSystem>() where TSystem : ComponentSystemBase
		{
			_initializePhaseSystemTypes.Add(typeof(TSystem));
			return this;
		}
		
		/// <summary>
		/// Adds managed system to the <see cref="SimulationSystemGroup"/>.
		/// </summary>
		public WorldBuilder WithSimulationPhaseManagedSystem<TSystem>() where TSystem : ComponentSystemBase
		{
			_simulationPhaseSystemTypes.Add(typeof(TSystem));
			return this;
		}
		
		/// <summary>
		/// Adds managed system to the <see cref="PresentationSystemGroup"/>.
		/// </summary>
		public WorldBuilder WithPresentationPhaseManagedSystem<TSystem>() where TSystem : ComponentSystemBase
		{
			_presentationPhaseSystemTypes.Add(typeof(TSystem));
			return this;
		}
		
		/// <summary>
		/// Adds unmanaged system to the <see cref="InitializationSystemGroup"/>.
		/// </summary>
		public WorldBuilder WithInitializePhaseSystem<TSystem>() where TSystem : unmanaged, ISystem
		{
			_initializePhaseSystemTypes.Add(typeof(TSystem));
			return this;
		}
		
		/// <summary>
		/// Adds unmanaged system to the <see cref="SimulationSystemGroup"/>.
		/// </summary>
		public WorldBuilder WithSimulationPhaseSystem<TSystem>() where TSystem : unmanaged, ISystem
		{
			_simulationPhaseSystemTypes.Add(typeof(TSystem));
			return this;
		}
		
		/// <summary>
		/// Adds unmanaged system to the <see cref="PresentationSystemGroup"/>.
		/// </summary>
		public WorldBuilder WithPresentationPhaseSystem<TSystem>() where TSystem : unmanaged, ISystem
		{
			_presentationPhaseSystemTypes.Add(typeof(TSystem));
			return this;
		}

		/// <summary>
		/// The final point of the world building process.
		/// Creates the world with specified configurations.
		/// If you going to update the world manually then pass <c>'appendToPlayerLoop'</c> as <c>false</c>
		/// </summary>
		/// <param name="appendToPlayerLoop">If <c>true</c> then the world will be added to Unity player loop. By default it's <c>false</c></param>
		public World Build(bool appendToPlayerLoop = false)
		{
			var allSystems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default).ToList();
			allSystems.RemoveAll(t => _excludedSystemTypes.Contains(t));
			allSystems.RemoveAll(t => !_namespaceFilters.Contains(t.Namespace));
			allSystems.RemoveAll(t => _excludingNameFilters.Contains(t.Name));
			
			DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World, allSystems);
			
			ProcessInitializePhase();
			ProcessSimulationPhase();
			ProcessPresentationPhase();

			if (appendToPlayerLoop) {
				ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World);
			}

			return World;
		}

		private void ProcessInitializePhase()
		{
			if (!_withInitializeRootGroup) {
				World.DestroySystem(World.GetExistingSystem<InitializationSystemGroup>());
				return;
			}
		
			var initializePhase = World.GetOrCreateSystemManaged<InitializationSystemGroup>();
			
			_initializePhaseSystemTypes.ForEach(s => {
				AddSystemToUpdateList(initializePhase, s);
			});
		}

		private void ProcessSimulationPhase()
		{
			if (!_withSimulationRootGroup) {
				World.DestroySystem(World.GetExistingSystem<SimulationSystemGroup>());
				return;
			}
		
			var simulationPhase = World.GetOrCreateSystemManaged<SimulationSystemGroup>();
			
			_simulationPhaseSystemTypes.ForEach(s => {
				AddSystemToUpdateList(simulationPhase, s);
			});
		}

		private void ProcessPresentationPhase()
		{
			if (!_withPresentationRootGroup) {
				World.DestroySystem(World.GetExistingSystem<PresentationSystemGroup>());
				return;
			}
		
			var presentationPhase = World.GetOrCreateSystemManaged<PresentationSystemGroup>();
			
			_presentationPhaseSystemTypes.ForEach(s => {
				AddSystemToUpdateList(presentationPhase, s);
			});
		}
		
		private void AddSystemToUpdateList(ComponentSystemGroup rootGroup, Type systemType)
		{
			if (systemType.IsSubclassOf(typeof(ComponentSystemBase))) {
				rootGroup.AddSystemToUpdateList(World.GetOrCreateSystemManaged(systemType));
			} else {
				rootGroup.AddSystemToUpdateList(World.GetOrCreateSystem(systemType));
			}
		}
	}

}