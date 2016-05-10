﻿/*
 * Copyright 2013-2016 Guardtime, Inc.
 *
 * This file is part of the Guardtime client SDK.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *     http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES, CONDITIONS, OR OTHER LICENSES OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 * "Guardtime" and "KSI" are trademarks or registered trademarks of
 * Guardtime, Inc., and no license to trademarks is granted; Guardtime
 * reserves and retains all trademark rights.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Guardtime.KSI.Hashing;
using Guardtime.KSI.Signature;
using Guardtime.KSI.Utils;

namespace Guardtime.KSI.Service
{
    public partial class BlockSigner
    {
        /// <summary>
        /// Merkle tree node. Used to build Merkle tree during local aggregation.
        /// </summary>
        private class TreeNode
        {
            /// <summary>
            /// Create aggregation tree node instance.
            /// </summary>
            /// <param name="level">Node level in the tree</param>
            public TreeNode(uint level)
            {
                Level = level;
            }

            /// <summary>
            /// Create aggregation tree node instance.
            /// </summary>
            /// <param name="documentHash">Hash of a document to be signed</param>
            /// <param name="metaData">Metadata to be added to hash chain</param>
            public TreeNode(DataHash documentHash, AggregationHashChain.MetaData metaData = null)
            {
                if (documentHash == null)
                {
                    throw new ArgumentNullException(nameof(documentHash));
                }
                DocumentHash = NodeHash = documentHash;
                MetaData = metaData;
                Level = 0;
            }

            /// <summary>
            /// Create aggregation tree node instance.
            /// </summary>
            /// <param name="metaData">Metadata to be added to hash chain</param>
            public TreeNode(AggregationHashChain.MetaData metaData)
            {
                if (metaData == null)
                {
                    throw new ArgumentNullException(nameof(metaData));
                }
                MetaData = metaData;
                Level = 0;
            }

            /// <summary>
            /// Document hash value
            /// </summary>
            public DataHash DocumentHash { get; }

            /// <summary>
            /// Metadata to be added to aggregation hash chain
            /// </summary>
            public AggregationHashChain.MetaData MetaData { get; }

            /// <summary>
            /// Hash value of the node.
            /// </summary>
            public DataHash NodeHash { get; set; }

            /// <summary>
            /// Parent node
            /// </summary>
            public TreeNode Parent { get; set; }

            /// <summary>
            /// Left child node
            /// </summary>
            public TreeNode Left { get; set; }

            /// <summary>
            /// Right child node
            /// </summary>
            public TreeNode Right { get; set; }

            /// <summary>
            /// If true then current node is left child, otherwise it is right node
            /// </summary>
            public bool IsLeftNode { get; set; }

            /// <summary>
            /// Node level in the tree. Level for leaves is 0.
            /// </summary>
            public uint Level { get; }

            /// <summary>
            /// String representation of the node.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                string value;
                if (NodeHash == null)
                {
                    if (MetaData == null)
                    {
                        return nameof(TreeNode);
                    }

                    value = "M:" + Base16.Encode(MetaData.EncodeValue());
                }
                else
                {
                    value = Base16.Encode((DocumentHash ?? NodeHash).Value);
                }
                return string.Format("{0}{1}:{2}", Level, IsLeftNode ? "L" : "R", value);
            }

            /// <summary>
            /// Short version of the string representation of the node.
            /// </summary>
            /// <returns></returns>
            private string ToShortString()
            {
                return ToString().Substring(0, 10);
            }

            /// <summary>
            /// Validates child nodes in Merkle tree.
            /// 1) Checks if each node's children's parents are correct.
            /// 2) Checks if left child is marked as left node and right child is marked as right node.
            /// </summary>
            /// <param name="node"></param>
            /// <returns>All nodes that do not have children.</returns>
            private static List<TreeNode> ValidateChildNodes(TreeNode node)
            {
                List<TreeNode> children = new List<TreeNode>();

                if (node.Left != null)
                {
                    if (node != node.Left.Parent)
                    {
                        throw new Exception("Left child parent does not match current node.");
                    }

                    if (!node.Left.IsLeftNode)
                    {
                        throw new Exception("Left child is not marked as left.");
                    }

                    children.AddRange(ValidateChildNodes(node.Left));
                }

                if (node.Right != null)
                {
                    if (node != node.Right.Parent)
                    {
                        throw new Exception("Left child parent does not match current node.");
                    }

                    if (node.Right.IsLeftNode)
                    {
                        throw new Exception("Right child is not marked as right.");
                    }

                    children.AddRange(ValidateChildNodes(node.Right));
                }

                if (children.Count == 0)
                {
                    children.Add(node);
                }

                return children;
            }

            /// <summary>
            /// Validates Merkle tree.
            /// 1) Checks if parent-child relationships are in compliance with each other 
            /// 2) Checks that all lowest level nodes have level value 0.
            /// </summary>
            /// <param name="root">Root node of the tree</param>
            /// <param name="lowestLevelNodes">Lowest level nodes (nodes that do not have children)</param>
            private static void ValidateTree(TreeNode root, List<TreeNode> lowestLevelNodes)
            {
                List<TreeNode> children = ValidateChildNodes(root);

                if (children.Count != lowestLevelNodes.Count)
                {
                    throw new Exception("Invalid tree. Leaf count does not match.");
                }

                for (int i = 0; i < lowestLevelNodes.Count; i++)
                {
                    if (lowestLevelNodes[i].Level != 0)
                    {
                        throw new Exception(string.Format("Invalid tree. Leaf must have level value 0. Position: {0}", i));
                    }
                    if (lowestLevelNodes[i] != children[i])
                    {
                        throw new Exception(string.Format("Invalid tree. Leaves at position {0} do not match.", i));
                    }
                }
            }

            /// <summary>
            /// Validates and prints Merkle tree.
            /// </summary>
            /// <param name="root">Root node of the tree</param>
            /// <returns></returns>
            public static string PrintTree(TreeNode root)
            {
                if (root == null)
                {
                    return null;
                }

                ValidateTree(root, GetLowestLevelNodes(root));

                return PrintTreeLevel(new List<TreeNode>() { root }, root.Level);
            }

            /// <summary>
            /// Returns lowest level nodes (nodes that do not have children) of a tree
            /// </summary>
            /// <param name="node">Sub-tree root node</param>
            /// <returns></returns>
            private static List<TreeNode> GetLowestLevelNodes(TreeNode node)
            {
                List<TreeNode> result = new List<TreeNode>();

                if (node.Left == null && node.Right == null)
                {
                    result.Add(node);
                    return result;
                }

                if (node.Left != null)
                {
                    result.AddRange(GetLowestLevelNodes(node.Left));
                }

                if (node.Right != null)
                {
                    result.AddRange(GetLowestLevelNodes(node.Right));
                }

                return result;
            }

            /// <summary>
            /// Print given nodes as one tree level
            /// </summary>
            /// <param name="nodes">Nodes in the tree level</param>
            /// <param name="level">Level value to be printed</param>
            /// <returns></returns>
            private static string PrintTreeLevel(List<TreeNode> nodes, uint level)
            {
                if (nodes == null || nodes.Count == 0)
                {
                    return null;
                }

                string prefix = string.Empty;
                string space = string.Empty;

                // calculate spaces
                // 2^n - 1, where n = level
                double spacesCount = (1 << (int)level) - 1;
                string twelveSpaces = "            ";

                for (int i = 0; i < spacesCount; i++)
                {
                    prefix += "      ";
                    space += twelveSpaces;
                }

                StringBuilder sb = new StringBuilder();

                sb.Append(prefix);

                // print spaces and tree branches
                if (nodes.Count > 1)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TreeNode firstNode = nodes[i];
                            TreeNode secondNode = i + 1 < nodes.Count ? nodes[i + 1] : null;

                            if (firstNode != null && (firstNode.IsLeftNode || secondNode != null))
                            {
                                sb.Append("        / ");
                            }
                            else if (firstNode == null || firstNode.IsLeftNode)
                            {
                                sb.Append("          ");
                            }

                            if (level > 0 && secondNode != null)
                            {
                                sb.Append((space + "  ").Replace(" ", "‾"));
                            }
                            else
                            {
                                sb.Append((space + "  "));
                            }

                            if (secondNode != null || (firstNode != null && !firstNode.IsLeftNode && firstNode.NodeHash != null))
                            {
                                sb.Append(" \\        ");
                            }
                            else
                            {
                                sb.Append("          ");
                            }
                        }
                        else if (i + 2 < nodes.Count)
                        {
                            sb.Append(space + "  ");
                        }
                    }
                }

                sb.AppendLine();
                sb.Append(prefix);

                bool isFirst = true;

                // print nodes
                foreach (TreeNode node in nodes)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append(space);
                    }

                    if (node == null || (node.NodeHash == null && node.MetaData == null))
                    {
                        sb.Append(twelveSpaces);
                    }
                    else
                    {
                        sb.Append(node.ToShortString() + "  ");
                    }
                }

                sb.AppendLine();

                if (level > 0)
                {
                    sb.Append(PrintTreeLevel(GetChildNodes(nodes), level - 1));
                }

                return sb.ToString();
            }

            /// <summary>
            /// Returns child nodes for each given node. 
            /// 2 nodes are returned per each given node (event if given node is null).
            /// If left or right child is null then null is returned for those children. If given node is null then null is returned as left and right node.
            /// </summary>
            /// <param name="nodes"></param>
            /// <returns>Child nodes, containing 2*nodes.Length elements.</returns>
            private static List<TreeNode> GetChildNodes(List<TreeNode> nodes)
            {
                List<TreeNode> result = new List<TreeNode>();

                for (int index = 0; index < nodes.Count; index++)
                {
                    TreeNode node = nodes[index];

                    if (node == null)
                    {
                        result.Add(null);
                        result.Add(null);
                    }
                    else
                    {
                        if (node.Left != null)
                        {
                            if (node.Level - 1 > node.Left.Level)
                            {
                                TreeNode newNode = new TreeNode(node.Level - 1)
                                {
                                    Left = node.Left
                                };
                                result.Add(newNode);
                            }
                            else
                            {
                                result.Add(node.Left);
                            }
                        }
                        else if (index + 1 < nodes.Count)
                        {
                            result.Add(null);
                        }

                        if (node.Right != null)
                        {
                            if (node.Level - 1 > node.Right.Level)
                            {
                                TreeNode newNode = new TreeNode(node.Level - 1)
                                {
                                    Right = node.Right
                                };
                                result.Add(newNode);
                            }
                            else
                            {
                                result.Add(node.Right);
                            }
                        }
                        else if (index + 1 < nodes.Count)
                        {
                            result.Add(null);
                        }
                    }
                }

                return result;
            }
        }
    }
}