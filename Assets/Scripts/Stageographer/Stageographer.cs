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
			var parents = new HashSet<Type>();
			int _addChildren(IEnumerable<Type> types, int row = 0, int column = 0) {
				foreach (var type in types) {
					// Parent
					var inputs = new List<Choreographer.NodeIO>();
					var outputs = new List<Choreographer.NodeIO>();
					/*foreach (var field in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
						inputs.Add(new Choreographer.NodeIO(field.DeclaringType));
						outputs.Add(new Choreographer.NodeIO(field.DeclaringType));
					}
					foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
						inputs.Add(new Choreographer.NodeIO(field.FieldType));
						outputs.Add(new Choreographer.NodeIO(field.FieldType));
					}*/
					nodes.Add(new Choreographer.Node(type, inputs.ToArray(), outputs.ToArray(), row, column));

					// Infinite Recursion
					if (parents.Contains(type)) {
						// TODO: Mark the node with an infinite loop indicator, pointing to the original source of recursion.
						return row + 1;
					}

					// Leaf
					if (!Stage.Relationships.TryGetValue(type, out var children)) return row + 1;

					// Children
					parents.Add(type);
					row = _addChildren(children, row, column + 1);
					parents.Remove(type);
				}
				return row;
			}
			_addChildren(Stage.Children);
			Choreographer.Nodes = nodes.ToArray();
		}
	}
}