using UnityEngine;

namespace Nashet.EconomicSimulation
{
	public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T instance;

		public static T Instance
		{
			get
			{
				// If the instance is already set, return it
				if (instance != null)
					return instance;

				// Search for an existing instance in the scene
				instance = FindObjectOfType<T>();

				// If no instance exists, create a new GameObject and add the component
				if (instance == null)
				{
					GameObject singletonObject = new GameObject(typeof(T).Name);
					instance = singletonObject.AddComponent<T>();
				}

				// Make sure the instance persists across scenes
				//DontDestroyOnLoad(instance.gameObject);

				return instance;
			}
		}

		protected virtual void Awake()
		{
			// If an instance already exists and it is not this instance, destroy this instance
			if (instance != null && instance != this)
			{
				Destroy(gameObject);
				return;
			}

			// Set the instance to this instance
			instance = this as T;

			// Make sure the instance persists across scenes
			//DontDestroyOnLoad(gameObject);
		}
	}
}