using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RegionCombine {
    public class QuadtreeNode {
        public QuadtreeNode(Rect bound) {
            _bound = bound;
        }

        public Rect Bound {
            get {
                return _bound;
            }
        }

        protected Rect _bound;
        public virtual void SetSubNodes(QuadtreeNode[] subNodes) {
            _subNodes = subNodes;
        }

        public virtual void Receive(Renderer render) {
            if (!Bound.Contains(new Vector2(render.bounds.center.x, render.bounds.center.z))) {
                return;
            }

            for (int i = 0; i < _subNodes.Length; i++) {
                _subNodes[i].Receive(render);
            }
        }

        public QuadtreeNode[] SubNodes {
            get {
                return _subNodes;
            }
        }

        public const int SubCount = 4;
        protected QuadtreeNode[] _subNodes;
    }

    public class QuadtreeLeaf : QuadtreeNode {
        public QuadtreeLeaf(Rect bound)
            : base(bound) {
        }

        public override void SetSubNodes(QuadtreeNode[] subNodes) {
            //QuadtreeCore.Assert(false);
        }

        public override void Receive(Renderer render) {
            if (!Bound.Contains(new Vector2(render.bounds.center.x, render.bounds.center.z))) {
                return;
            }

            if (!Contains(render)) {
                m_lstGameObject.Add(render.gameObject);
                Quadtree.nReceivedCount++;
            }
        }

        public bool Contains(Renderer render) {
            if (m_lstGameObject.Contains(render.gameObject)) {
                return true;
            }

            return false;
        }

        public List<GameObject> objects {
            get {
                return m_lstGameObject;
            }
        }

        private List<GameObject> m_lstGameObject = new List<GameObject>();
        public bool bSelect = false;
    }

    public class Quadtree {
        public delegate QuadtreeNode QuadtreeCreateNode(Rect bnd);
        private QuadtreeNode _root;
        private List<QuadtreeLeaf> m_leaves = null;
        
        //public QuadtreeLeaf m_selectLeaf = null;
        private float cellSize = 0.0f;
        public static int nReceivedCount = 0;
        //public float fDebugLine;
        
        public Quadtree(Rect bound, float cellsize) {
            cellSize = cellsize;
            _root = new QuadtreeNode(bound);

            m_leaves = new List<QuadtreeLeaf>();
            nReceivedCount = 0;
            BuildRecursively(_root);
        }

        public void Receive(Renderer render) {
            _root.Receive(render);
        }

        public List<QuadtreeLeaf> GetAllLeaves() {
            m_leaves.Sort((leaf1, leaf2) => {
                if (leaf1.Bound.center.y == leaf2.Bound.center.y) {
                    if (leaf1.Bound.center.x < leaf2.Bound.center.x) {
                        return -1;
                    }
                    else {
                        return 1;
                    }
                }
                else if (leaf1.Bound.center.y < leaf2.Bound.center.y) {
                    return 1;
                }
                else if (leaf1.Bound.center.y > leaf2.Bound.center.y) {
                    return -1;
                }

                return 0;
            });

            return m_leaves;
        }

        public void DrawDebugLine(float fDebugLine) {
            for (int i = 0; i < m_leaves.Count; ++i) {
                QuadtreeLeaf leaf = m_leaves[i];
                if (leaf.bSelect) {
                    DrawRect(leaf.Bound, fDebugLine, Color.red, 0.2f);
                }
                else {
                    DrawRect(leaf.Bound, fDebugLine, Color.white, 0.2f);
                }
                
            }
        }

        private void BuildRecursively(QuadtreeNode node) {
            float subWidth = node.Bound.width * 0.5f;
            float subHeight = node.Bound.height * 0.5f;
            bool isPartible = subWidth > cellSize && subHeight > cellSize;
            // create subnodes
            QuadtreeCreateNode _nodeCreator = (bnd) => {
                return new QuadtreeNode(bnd);
            };

            QuadtreeCreateNode _leafCreator = (bnd) => {
                QuadtreeLeaf leaf = new QuadtreeLeaf(bnd);
                m_leaves.Add(leaf);
                return leaf;
            };

            QuadtreeCreateNode creator = isPartible ? _nodeCreator : _leafCreator;

            node.SetSubNodes(new QuadtreeNode[QuadtreeNode.SubCount]
            {
                creator(new Rect(node.Bound.xMin, node.Bound.yMin, subWidth, subHeight)), 
                creator(new Rect(node.Bound.xMin + subWidth, node.Bound.yMin, subWidth, subHeight)), 
                creator(new Rect(node.Bound.xMin, node.Bound.yMin + subHeight, subWidth, subHeight)), 
                creator(new Rect(node.Bound.xMin + subWidth, node.Bound.yMin + subHeight, subWidth, subHeight)), }

            );

            // do it recursively
            if (isPartible) {
                foreach (var sub in node.SubNodes) {
                    BuildRecursively(sub);
                }
            }
        }

        private void DrawRect(Rect r, float y, Color c, float padding = 0.0f)
        {
            Debug.DrawLine(new Vector3(r.xMin + padding, y, r.yMin + padding), new Vector3(r.xMin + padding, y, r.yMax - padding), c);
            Debug.DrawLine(new Vector3(r.xMin + padding, y, r.yMin + padding), new Vector3(r.xMax - padding, y, r.yMin + padding), c);
            Debug.DrawLine(new Vector3(r.xMax - padding, y, r.yMax - padding), new Vector3(r.xMin + padding, y, r.yMax - padding), c);
            Debug.DrawLine(new Vector3(r.xMax - padding, y, r.yMax - padding), new Vector3(r.xMax - padding, y, r.yMin + padding), c);
        }
        
    }
}
