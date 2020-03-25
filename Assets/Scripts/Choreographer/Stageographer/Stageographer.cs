using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Stagehand {
	public class Stageographer : MonoBehaviour {
		// By defining, instantiating, and referencing the size of the "vanilla" IEnumerator, we're able to skip the
		// language level implemented fields and only show the end user what they care about.
		// TODO: Should this be configurable through a toggle?
		private static IEnumerator _vanilla() {
			yield break;
		}
		private static readonly IEnumerator __vanilla = _vanilla();
		private static readonly int _skipVanillaFields = __vanilla.GetType().GetFields().Length;

		private void Awake() {
			// Graph is flat: top to bottom, left to right.
			// TODO: HACK: Replace with Stage<Node> syntax!
			var graph = new List<Choreographer.Node>();

			// Parent trap! Reference of lineage to prevent recursion.
			var parents = new HashSet<Type>();

			// Recursively add children to a parent.
			int _addChildren(Choreographer.Node parent, IEnumerable<Type> childTypes, int row = 0, int column = 0) {
				foreach (var childType in childTypes) {
					// Inputs
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
					
					// Outputs
					var outputs = new List<Choreographer.NodeIO>();
					foreach (var fieldInfo in childType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)) {
						outputs.Add(new Choreographer.NodeIO(fieldInfo.FieldType, fieldInfo.Name));
					}

					// Parent
					var node = new Choreographer.Node(childType, inputs.ToArray(), outputs.ToArray(), row, column);
					graph.Add(node);
					//Stage<Choreographer.Node>.Hand(ref node);

					// Relationship
					if (parent != null) node.Connections.Add(new Choreographer.Connection(Choreographer.Connection.ConnectionType.Inherited, parent));

					// Infinite Recursion
					if (parents.Contains(childType)) {
						node.Connections.Add(new Choreographer.Connection(Choreographer.Connection.ConnectionType.Recursive, parent));
						++row;
						continue;
					}

					// Stagehand Queues
					var prevNode = node;
					var actionPairs = Stage._GetQueue(childType);
					foreach (var actionPair in actionPairs) {
						foreach (var action in actionPair.Value) {
							// Inputs
							var actionInputs = new List<Choreographer.NodeIO>();
							var actionType = action.GetType();
							var actionFields = actionType.GetFields();
							for (var i = _skipVanillaFields; i < actionFields.Length; ++i) {
								actionInputs.Add(new Choreographer.NodeIO(actionFields[i].FieldType, actionFields[i].Name));
							}

							// Outputs
							var actionOutputs = new List<Choreographer.NodeIO>();

							// Child
							var actionNode = new Choreographer.Node(actionType, actionInputs.ToArray(), actionOutputs.ToArray(), row, ++column);
							graph.Add(actionNode);
							//Stage<Choreographer.Node>.Hand(ref actionNode);

							// Relationship
							actionNode.Connections.Add(new Choreographer.Connection(Choreographer.Connection.ConnectionType.Inherited, prevNode));

							// "Parent" of Next Child
							prevNode = actionNode;
						}

						// Carriage Return
						column -= actionPair.Value.Count;
					}

					// Next!
					++row;

					// Leaf
					if (!Stage.Relationships.TryGetValue(childType, out var children)) {
						continue;
					}

					// Children
					parents.Add(childType);
					row = _addChildren(node, children, row, column + 1);
					parents.Remove(childType);
				}

				// Finished!
				return row;
			}

			// Build the Graph!
			_addChildren(null, Stage.Children);

			// TODO: HACK: How should we communicate with Choreographer?
			Choreographer.Nodes = graph.ToArray();
		}
	}
}