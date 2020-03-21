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
			var row = 0;
			foreach (var type in Stage.Children) {
				var inputs = new List<Choreographer.NodeIO>();
				var outputs = new List<Choreographer.NodeIO>();
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
					inputs.Add(new Choreographer.NodeIO(field.FieldType));
					outputs.Add(new Choreographer.NodeIO(field.FieldType));
				}
				nodes.Add(new Choreographer.Node(type, inputs.ToArray(), outputs.ToArray(), row, 0));

				var parents = new HashSet<Type>();
				void _addChildren(Type parentType, int currentRow, int currentColumn) {
					if (parents.Contains(parentType)) return;
					parents.Add(parentType);

					if (!Stage.Relationships.TryGetValue(parentType, out var children)) return;

					foreach (var childType in children) {
						nodes.Add(new Choreographer.Node(childType, Choreographer.NodeIO.Empty, Choreographer.NodeIO.Empty, currentRow++, currentColumn));
						_addChildren(childType, currentRow, currentColumn + 1);
					}
				}
				_addChildren(type, row++, 1);
			}
			Choreographer.Nodes = nodes.ToArray();
		}
	}
}