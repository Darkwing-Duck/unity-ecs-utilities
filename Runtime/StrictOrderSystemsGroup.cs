using Unity.Entities;

namespace Nakuru.Unity.Ecs.Utilities
{

	/// <summary>
	/// Base systems group with disabled sorting
	/// </summary>
	public abstract partial class StrictOrderSystemsGroup : ComponentSystemGroup
	{
		public StrictOrderSystemsGroup() => EnableSystemSorting = false;
	}

}