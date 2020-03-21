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

			var graph = new List<Choreographer.Node>();
			var parents = new HashSet<Type>();
			(int, Choreographer.Node) _addChildren(IEnumerable<Type> types, Choreographer.Node parent = null, int row = 0, int column = 0) {
				List<Choreographer.Node> nodes = new List<Choreographer.Node>();
				foreach (var type in types) {
					// Parent
					var inputs = new List<Choreographer.NodeIO>();
					var outputs = new List<Choreographer.NodeIO>();
					foreach (var field in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
						inputs.Add(new Choreographer.NodeIO(field.DeclaringType));
						outputs.Add(new Choreographer.NodeIO(field.DeclaringType));
					}
					foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
						inputs.Add(new Choreographer.NodeIO(field.FieldType));
						outputs.Add(new Choreographer.NodeIO(field.FieldType));
					}
					var node = new Choreographer.Node(type, inputs.ToArray(), outputs.ToArray(), row, column);
					graph.Add(node);
					nodes.Add(node);

					// Infinite Recursion
					if (parents.Contains(type)) {
						//node.Connections.Add(new Choreographer.Connection(parent));
						return (row + 1, node);
					}

					// Leaf
					if (!Stage.Relationships.TryGetValue(type, out var children)) return (row + 1, node);

					// Children
					parents.Add(type);
					Choreographer.Node childNode;
					(row, childNode) = _addChildren(children, node, row, column + 1);
					parents.Remove(type);

					// Relationship
					if (childNode == null) continue;
					childNode.Connections.Add(new Choreographer.Connection(node));
				}
				return (row, null);
			}
			_addChildren(Stage.Children);
			Choreographer.Nodes = graph.ToArray();
		}
	}
}