using System;
using System.Collections.Generic;
using System.Reflection;
using Plugins.Stagehand;
using UnityEditor;
using UnityEngine;

namespace Stagehand {
	public class Stageographer : MonoBehaviour {
		private void Awake() {
			if (!EditorApplication.isPlaying) return;

			var nodes = new List<Choreographer.Node>();
			foreach (var type in Stage.Children) {
				var inputs = new List<Choreographer.NodeIO>();
				var outputs = new List<Choreographer.NodeIO>();
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
					var parents = new HashSet<Type>();

					void _AddChildren(Type parentType) {
						if (parents.Contains(parentType)) return;
						parents.Add(parentType);
						
						if (!Stage.Relationships.TryGetValue(parentType, out var children)) return;
						foreach (var childType in children) {
							nodes.Add(new Choreographer.Node(childType, inputs.ToArray(), outputs.ToArray()));
							_AddChildren(childType);
						}
					}
					_AddChildren(type);

					inputs.Add(new Choreographer.NodeIO(field.FieldType));
					outputs.Add(new Choreographer.NodeIO(field.FieldType));
				}
				nodes.Add(new Choreographer.Node(type, inputs.ToArray(), outputs.ToArray()));
			}
			Choreographer.Nodes = nodes.ToArray();
		}
	}
}