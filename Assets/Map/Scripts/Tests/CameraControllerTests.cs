using Nashet.MapMeshes;
using Nashet.Map.GameplayControllers;
using NUnit.Framework;
using System.Reflection;
using System;
using UnityEngine;
using System.Linq;


namespace NashetMapTests
{
	public class CameraControllerTests
	{
		[Test]
		public void CameraController_HasExpectedFieldsAndMethods()
		{
			// Arrange
			var cameraControllerType = typeof(CameraController);
			var expectedMethods = new ValueTuple<string, int, Type[]>[]
			{
				("Move", 2, new[] { typeof(float), typeof(float) }),
				("Zoom", 1, new[] { typeof(float) }),
				("FocusOnPoint", 1, new[] { typeof(Vector3) }),
				("FocusOnProvince", 2, new[] { typeof(ProvinceMesh), typeof(bool) })
			};

			// Act
			var actualFields = cameraControllerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			var actualFieldNamesAndTypes = Array.ConvertAll(actualFields, f => (f.Name, f.FieldType));
			var actualMethods = cameraControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
			var actualMethodSignaturesAndTypes = Array.ConvertAll(actualMethods, m => (m.Name, m.GetParameters().Length, GetParameterTypes(m)));

			// Assert
			foreach (var expectedMethod in expectedMethods)
			{
				Func<ValueTuple<string, int, Type[]>, ValueTuple<string, int, Type[]>, bool> comparison = (x, y) => x.Item1 == y.Item1 && x.Item2 == y.Item2 && x.Item3.SequenceEqual(y.Item3);
				Assert.That(actualMethodSignaturesAndTypes, Contains.Item(expectedMethod)
									.Using(comparison),
									$"{expectedMethod.Item1} signature does not match: {expectedMethod.Item2},{string.Concat(expectedMethod.Item3.Select(x => x + ", ")).TrimEnd(',', ' ')}");
			}
		}

		private Type[] GetParameterTypes(MethodInfo methodInfo)
		{
			var parameterInfos = methodInfo.GetParameters();
			var parameterTypes = new Type[parameterInfos.Length];
			for (int i = 0; i < parameterInfos.Length; i++)
			{
				parameterTypes[i] = parameterInfos[i].ParameterType;
			}
			return parameterTypes;
		}
	}
}