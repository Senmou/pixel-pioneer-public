using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Pathfinding
{
    public class Grid
    {
        public int Rows => _nodes.GetLength(1);
        public int Columns => _nodes.GetLength(0);
        public Vector3Int Offset => _offset;

        private Vector3Int _offset;
        private Node[,] _nodes;

        public Grid(Vector3Int offset, int width, int height)
        {
            _offset = offset;
            _nodes = new Node[width, height];
        }

        public void AddNode(Node node)
        {
            _nodes[node.LocalPosition.x, node.LocalPosition.y] = node;
        }

        public Node GetNode(Vector3Int localPos)
        {
            return _nodes[localPos.x, localPos.y];
        }

        public void SetWalkable(Vector3Int localPos, bool isWalkable)
        {
            _nodes[localPos.x, localPos.y].Walkable = isWalkable;
        }
    }

    public class Node
    {
        public Node Parent;
        public Vector3Int LocalPosition;
        public Vector3Int WorldPosition => LocalPosition + _grid.Offset;
        public Vector3 WorldCenterPos => WorldPosition + new Vector3(0.5f, 0.5f);
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        private Grid _grid;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;

        public Node(Grid grid, Vector3Int worldPos, bool walkable, float weight = 1)
        {
            _grid = grid;
            Parent = null;
            LocalPosition = worldPos - grid.Offset;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
            Walkable = walkable;
        }
    }

    public class Astar
    {
        private Grid _grid;

        private int GridRows => _grid.Rows;
        private int GridCols => _grid.Columns;

        public Astar(Grid grid)
        {
            _grid = grid;
        }

        public Stack<Node> FindPath(Vector3Int startPos, Vector3Int endPos)
        {
            Node start = _grid.GetNode(startPos);
            Node end = _grid.GetNode(endPos);

            Stack<Node> Path = new Stack<Node>();
            List<Node> OpenList = new List<Node>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Add(start);

            while (OpenList.Count != 0 && !ClosedList.Exists(x => x.LocalPosition == end.LocalPosition))
            {
                current = OpenList.OrderBy(e => e.F).First();
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current);

                foreach (Node n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        bool isFound = false;
                        foreach (var oLNode in OpenList)
                        {
                            if (oLNode == n)
                            {
                                isFound = true;
                            }
                        }
                        if (!isFound)
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.LocalPosition.x - end.LocalPosition.x) + Math.Abs(n.LocalPosition.y - end.LocalPosition.y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Add(n);
                        }
                    }
                }
            }

            // construct path, if end was not closed return null
            if (!ClosedList.Exists(x => x.LocalPosition == end.LocalPosition))
            {
                return null;
            }

            // if all good, return path
            Node temp = current; // ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null);
            return Path;
        }

        private List<Node> GetAdjacentNodes(Node n)
        {
            List<Node> temp = new List<Node>();

            int row = n.LocalPosition.y;
            int col = n.LocalPosition.x;

            if (row + 1 < GridRows)
            {
                temp.Add(_grid.GetNode(new Vector3Int(col, row + 1)));
            }
            if (row - 1 >= 0)
            {
                temp.Add(_grid.GetNode(new Vector3Int(col, row - 1)));
            }
            if (col - 1 >= 0)
            {
                temp.Add(_grid.GetNode(new Vector3Int(col - 1, row)));
            }
            if (col + 1 < GridCols)
            {
                temp.Add(_grid.GetNode(new Vector3Int(col + 1, row)));
            }

            return temp;
        }
    }
}