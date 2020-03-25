using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Stagehand {
	public class Stageographer : MonoBehaviour {
		private static IEnumerator _vanilla() {
			yield break;
		}
		private static readonly IEnumerator __vanilla = _vanilla();
		private static readonly int _skipVanillaFields = __vanilla.GetType().GetFields().Length;

		private void Awake() {
			if (!EditorApplication.isPlaying) return;

			var graph = new List<Choreographer.Node>();
			var parents = new HashSet<Type>();
			(int, List<Choreographer.Node> children) _addChildren(IEnumerable<Type> types, Choreographer.Node parent = null, int row = 0, int column = 0) {
				var prevParent = parent;
				foreach (var type in types) {
					// Parent
					var inputs = new List<Choreographer.NodeIO>();
					/*foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
						string methodName;
						if (methodInfo.ContainsGenericParameters) {
							methodName = string.Join(", ", methodInfo.GetGenericArguments().Select(argType => (argType.DeclaringType)));
						} else {
							methodName = $"{methodInfo.Name} ({(methodInfo.ReturnType)})";
						}
						outputs.Add(new Choreographer.NodeIO(methodInfo.ReturnType, methodName));
					}*/
					
					var outputs = new List<Choreographer.NodeIO>();
					foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
						outputs.Add(new Choreographer.NodeIO(fieldInfo.FieldType, fieldInfo.Name));
					}
					
					var node = new Choreographer.Node(type, inputs.ToArray(), outputs.ToArray(), row, column);
					graph.Add(node);

					// Relationship
					if (parent != null) node.Connections.Add(new Choreographer.Connection(Choreographer.Connection.ConnectionType.Inherited, parent));

					// Infinite Recursion
					if (parents.Contains(type)) {
						node.Connections.Add(new Choreographer.Connection(Choreographer.Connection.ConnectionType.Recursive, parent));
						++row;
						continue;
					}

					// Stagehand Queues
					parent = node;
					var actionPairs = Stage._GetQueue(type);
					foreach (var actionPair in actionPairs) {
						foreach (var action in actionPair.Value) {
							// Inputs
							var actionInputs = new List<Choreographer.NodeIO>();
							var actionType = action.GetType();
							var fields = actionType.GetFields();
							for (var i = _skipVanillaFields; i < fields.Length; ++i) {
								actionInputs.Add(new Choreographer.NodeIO(fields[i].FieldType, fields[i].Name));
							}

							// Outputs
							var actionOutputs = new List<Choreographer.NodeIO>();

							// Child
							var actionNode = new Choreographer.Node(actionType, actionInputs.ToArray(), actionOutputs.ToArray(), row, ++column);
							graph.Add(actionNode);

							// Relationship
							actionNode.Connections.Add(new Choreographer.Connection(Choreographer.Connection.ConnectionType.Inherited, parent));

							// "Parent" of Next Child
							parent = actionNode;
						}

						// Carriage Return
						column -= actionPair.Value.Count;
					}

					// Next!
					++row;
					parent = prevParent;

					// Leaf
					if (!Stage.Relationships.TryGetValue(type, out var children)) {
						continue;
					}

					// Children
					parents.Add(type);
					List<Choreographer.Node> childNodes;
					(row, childNodes) = _addChildren(children, node, row, column + 1);
					parents.Remove(type);
				}
				return (row, null);
			}
			_addChildren(Stage.Children);
			Choreographer.Nodes = graph.ToArray();
		}
	}
}