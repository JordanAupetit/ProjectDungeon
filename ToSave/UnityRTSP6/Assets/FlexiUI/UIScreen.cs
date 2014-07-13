using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/* First try to Keep everything uniformly scaled
 * If still need larger, try collapsing the ones going backward
 * Preffer collapse lower collapseId elements
 * Then collapse anything
 * Then Expand elements
 * 
 * Parallel - choose highest scoring
 * Serial - add to range
 * Special case - two parallel sets of series, with middle rails joined
 * 
 * Keep a list of constraints impacting the rail
 * When list already exists, comparing new list:
 * - get maximum of minimums
 * - ignore all values less than that value
 * - use one with largest length
 * After finding critical path
 * - Ignore anything less or equal to smallest collapseId
 * - Calculate minimum size
 * - If still not enough space, repeat.
 * OR
 * - If items can be ignored, ignore them
 * - Otherwise, temporarily change Width of items to be average
 * - Check if any others are blocking
 * - Need a way to identify any constraints to have percentage reduction - some may not yet be in the critical chain
 */

public class ConstraintChain {
    public List<UIConstraint> Constraints;
    public UIConstraint First { get { return Constraints[0]; } }
    public UIConstraint Last { get { return Constraints[Constraints.Count - 1]; } }

    public UIRail Other(UIRail rail) {
        if (Start == rail) return End;
        if (End == rail) return Start;
        return null;
    }

    public UIRail Start, End;
    public int MinCollapseId;
    public int MaxCollapseId;

    public int _CollapseId = 0;
    public float _CollapsedSize;
    public float _FlexiSize;
    public float _MinimumValue { get { return _FlexiSize < 0 ? _CollapsedSize + _FlexiSize : _CollapsedSize; } }
    public float _MaximumValue { get { return _FlexiSize < 0 ? _CollapsedSize : _CollapsedSize + _FlexiSize; } }

    public ConstraintChain(List<UIConstraint> constraints) {
        Constraints = constraints;
        Start = Constraints[0].Left;
        if (Constraints.Count >= 2)
            Start = Constraints[1].HasEdge(Constraints[0].Left) ? Constraints[0].Right : Constraints[0].Left;
        MinCollapseId = int.MaxValue;
        MaxCollapseId = int.MinValue;
        End = Start;
        for (int c = 0; c < Constraints.Count; ++c) {
            MinCollapseId = Mathf.Min(MinCollapseId, Constraints[c].CollapseId);
            MaxCollapseId = Mathf.Max(MaxCollapseId, Constraints[c].CollapseId);
            End = Constraints[c].Other(End);
        }
    }
    public void CalculateConstraint(int collapseId) {
        _CollapseId = collapseId;
        _CollapsedSize = 0;
        _FlexiSize = 0;
        var rail = Start;
        for (int c = 0; c < Constraints.Count; ++c) {
            var constraint = Constraints[c];
            var sign = (constraint.Left == rail ? 1 : -1);
            var width = constraint.Width * sign;
            if (constraint.CollapseId > collapseId) {
                _CollapsedSize += width;
            } else if (constraint.CollapseId == collapseId) {
                _FlexiSize += width;
            }
            rail = constraint.Other(rail);
        }
        /*for (int c = 0; c < Constraints.Count; ++c) {
            var constraint = Constraints[c];
            var sign = (constraint.Left == rail ? 1 : -1);
            if (collapseId < constraint.CollapseId) _MinimumValue += sign * constraint.Width;
            if (collapseId <= constraint.CollapseId) _MaximumValue += sign * constraint.Width;
            rail = constraint.Other(rail);
        }*/
    }
    public override string ToString() {
        List<UIRail> rails = new List<UIRail>();
        rails.Add(Start);
        for (int c = 0; c < Constraints.Count; ++c) {
            rails.Add(Constraints[c].Other(rails[rails.Count - 1]));
        }
        if(rails.Count == 0) return "";
        return rails.Select(r => r.name).Aggregate((r1, r2) => r1 + ", " + r2);
    }
}

[ExecuteInEditMode]
public class UIScreen : MonoBehaviour {

    public static readonly Vector2[] Directions = new[] { new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), };
    public static readonly Vector2[] Corners = new[] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), };
    public static readonly string[] DirectionNames = new[] { "Left", "Top", "Right", "Bottom", };
    public static readonly string[] CornerNames = new[] { "BottomLeft", "TopLeft", "TopRight", "BottomRight", };
    public static readonly UIRail.Directions[] RailDirections = new[] { UIRail.Directions.Vertical, UIRail.Directions.Horizontal, UIRail.Directions.Vertical, UIRail.Directions.Horizontal, };

    public UIRail[] Edges;
    public UIElement[] Elements;
    public UIConstraint[] HConstraints;

    [System.NonSerialized]
    private List<ConstraintChain> constraintChains;

    public UIRail NewRail(string name, UIRail.Directions dir) {
        var rail = new GameObject().AddComponent<UIRail>();
        rail.name = name;
        rail.transform.parent = transform;
        rail.Direction = dir;
        return rail;
    }
    public UIConstraint NewConstraint(UIRail left, UIRail right, float width, int collapseId) {
        var constraint = new GameObject().AddComponent<UIConstraint>();
        constraint.Left = left;
        constraint.Right = right;
        constraint.Width = width;
        constraint.CollapseId = collapseId;
        constraint.name = "_C" + left.name + "-" + right.name;
        constraint.transform.parent = transform;
        return constraint;
    }

    public void Awake() {
        if (Edges == null || Edges.Length < 4) {
            Edges = new UIRail[DirectionNames.Length];
            var edgesNode = transform.Find("ScreenEdges");
            if (edgesNode == null) (edgesNode = new GameObject().transform).parent = transform;
            for (int e = 0; e < DirectionNames.Length; ++e) {
                Edges[e] = edgesNode.Find(DirectionNames[e]).GetComponent<UIRail>();
                if (Edges[e] == null) {
                    Edges[e] = NewRail(DirectionNames[e], RailDirections[e]);
                    Edges[e].transform.parent = edgesNode;
                }
            }
        }
    }
    public void Start() {
        if (Application.isPlaying) {
            var rail1 = NewRail("R1", UIRail.Directions.Vertical);
            var rail2 = NewRail("R2", UIRail.Directions.Vertical);
            var rail3 = NewRail("R3", UIRail.Directions.Vertical);

            var railM1 = NewRail("M1", UIRail.Directions.Vertical);
            var railM2 = NewRail("M2", UIRail.Directions.Vertical);
            HConstraints = new[] {
                /*NewConstraint(Edges[0], rail1, 500, 1),
                NewConstraint(Edges[0], rail1, 500, 0),
                NewConstraint(Edges[0], rail2, 300, 0),
                NewConstraint(rail1, rail2, 300, 0),
                NewConstraint(rail2, rail3, 300, 1),
                NewConstraint(rail3, Edges[2], 300, 0),*/

                NewConstraint(Edges[0], rail1, 500, 1),
                NewConstraint(rail2, rail1, 300, 0),
                NewConstraint(rail2, rail3, 500, 1),
                NewConstraint(Edges[0], rail3, 700, 1),
                NewConstraint(rail3, Edges[2], 400, 2),

                NewConstraint(railM1, rail3, 300, 0),
                NewConstraint(railM1, railM2, 200, -1),
                NewConstraint(rail3, railM2, 300, 0),
            };
            GenerateConstraintChains();
        }
    }


    //[System.NonSerialized]
    public List<UIRail> rails = new List<UIRail>();
    public void Arrange(Vector2 size) {
        GenerateConstraintChains();
        for (int r = 0; r < rails.Count; r++) rails[r]._value = float.NaN;
        Edges[0]._value = 0;
        Edges[1]._value = 0;
        Edges[2]._value = size.x;
        Edges[3]._value = size.y;
        Arrange(Edges[0], Edges[2]);
    }
    private void GenerateConstraintChains() {
        rails.Clear();
        for (int l = 0; l < HConstraints.Length; ++l) {
            var constraint = HConstraints[l];
            if (!rails.Contains(constraint.Left)) rails.Add(constraint.Left);
            if (!rails.Contains(constraint.Right)) rails.Add(constraint.Right);
        }
        for (int e = 0; e < Edges.Length; ++e) {
            var edge = Edges[e];
            if (!rails.Contains(edge)) rails.Add(edge);
        }
        for (int r = 0; r < rails.Count; ++r) {
            rails[r]._chains.Clear();
        }
        for (int t = 0; t < 10; ++t) {
            bool changed = false;
            for (int c = 0; c < HConstraints.Length; ++c) {
                var constraint = HConstraints[c];
                if (constraint._CanFlex) {
                    for (int d = 0; d < 2; ++d) {
                        var rail = d == 0 ? constraint.Left : constraint.Right;
                        var destR = constraint.Other(rail);
                        if (!float.IsNaN(destR._value)) continue;
                        // Prevent constraint chains from looping back on themselves
                        // Ignore constraints that arent anchored to fixed
                        // No such chain can be created?!
                        // Only one chain per source? So two pathways always uses the most forceful one?
                        // If two sources, then obviously itll include both sources
                        // hmmm

                        // Propagate from rail to other

                        if (float.IsNaN(rail._value)) {
                            // If rail is completely unknown, ignore for now
                            if (rail._chains.Count == 0) continue;

                            // NOTE: Not possible for a current chain to be the same
                            // as the new chain but longer
                            for (int r = 0; r < rail._chains.Count; ++r) {
                                var rchain = rail._chains[r];

                                bool hasChain = false;
                                // If the current constraint chain already passes the rail `destR`
                                for (int rc = 0; rc < rchain.Constraints.Count; ++rc) {
                                    if (rchain.Constraints[rc].Constraint.HasEdge(destR)) hasChain = true;
                                }
                                if (!hasChain) {
                                    // Check if `other` has any chains that are the same as the one we're about to add
                                    for (int o = 0; o < destR._chains.Count; ++o) {
                                        var dchain = destR._chains[o];
                                        if (dchain.Rail == rchain.Rail &&
                                            dchain.Constraints.Count == rchain.Constraints.Count + 1 &&
                                            dchain.OverlapLength(rchain) == rchain.Constraints.Count &&
                                            dchain.Constraints[rchain.Constraints.Count].Constraint == constraint) {
                                            hasChain = true;
                                            break;
                                        }
                                    }
                                }
                                // If a similar chain was not found, add it
                                if (!hasChain) {
                                    var newChain = new InfluenceChain(rchain.Rail);
                                    for (int rc = 0; rc < rchain.Constraints.Count; ++rc)
                                        newChain.Constraints.Add(rchain.Constraints[rc]);
                                    newChain.Constraints.Add(new InfluenceValue(constraint, constraint.Right == rail));
                                    destR._chains.Add(newChain);
                                    changed = true;
                                }
                            }
                        } else {
                            bool hasValue = false;
                            for (int o = 0; o < destR._chains.Count; ++o) {
                                var dchain = destR._chains[o];
                                for (int cc = 0; cc < dchain.Constraints.Count; ++cc) {
                                    if (dchain.Constraints[cc].Constraint == constraint) { hasValue = true; break; }
                                }
                            }
                            if (!hasValue) {
                                var newChain = new InfluenceChain(rail);
                                newChain.Constraints.Add(new InfluenceValue(constraint, constraint.Right == rail));
                                destR._chains.Add(newChain);
                                changed = true;
                            }
                        }
                    }
                }
            }
            if (!changed) break;
            if (t == 9) {
                Debug.LogError("Spun out!");
            }
        }
    }
    private void Arrange(UIRail left, UIRail right) {
        GenerateConstraintChains();
        bool[] offMask = new bool[100];
        //var minColId = activeChains.SelectMany(c => c.Constraints).Min(c => c.CollapseId);
        //var maxColId = activeChains.SelectMany(c => c.Constraints).Max(c => c.CollapseId);
        int maxColId = 2, minColId = -2;
        for (int i = maxColId; i >= minColId; --i) {
            for (int r = 0; r < rails.Count; ++r) {
                var rail = rails[r];
                if (rail._chains.Count == 0 || !float.IsNaN(rail._value)) continue;
                var activeChains = rail._chains;
                // If `i` collapse id would result in no overlap, continue down (include more constraints)
                if (i > minColId) {
                    float minLimit, maxLimit;
                    GetLimitRange(rail, i, out minLimit, out maxLimit);
                    //Debug.Log(i + ": " + minLimit + " -- " + maxLimit);
                    if (maxLimit >= minLimit) {
                        // If point is fixed (and valid) with this collapseId
                        // force the rail position
                        if (minLimit == maxLimit) {
                            rail._value = (minLimit + maxLimit) / 2;
                            //if (rail.name == "M2") {
                                //Debug.Log(rail.name + " @" + i);
                            //}
                        }
                        continue;
                    }
                }
                for (int c1 = 0; c1 < activeChains.Count; ++c1) offMask[c1] = false;
                var value = float.NaN;
                for (int t = 0; t < 10; ++t) {
                    float sumValue = 0;
                    float valueTotal = 0;
                    float divisor = 0;
                    // Need to remove constraints that negatively impact value
                    // (ie. constraints that pull it toward its own limit)
                    for (int c1 = 0; c1 < activeChains.Count; ++c1) {
                        var chain = activeChains[c1];
                        if (offMask[c1]) continue;
                        valueTotal += chain.GetLength(i);
                    }
                    for (int c1 = 0; c1 < activeChains.Count; ++c1) {
                        var chain = activeChains[c1];
                        if (offMask[c1]) continue;
                        //Debug.Log(chain.Constraints[0].Constraint.name + ": " + chain.GetEndValue(i) + " -- " + chain.LeftCollapseId + " - " + chain.RightCollapseId);
                        var amount = (valueTotal - chain.GetLength(i) + 0.001f);
                        sumValue += chain.GetEndValue(i) * amount;
                        divisor += amount;
                    }
                    value = sumValue / divisor;
                    //if (rail.name == "M2") {
                        //Debug.Log(rail.name + "=" + value + " @" + i);
                    //}
                    bool changed = false;
                    for (int c1 = 0; c1 < activeChains.Count; ++c1) {
                        var chain = activeChains[c1];
                        if (offMask[c1]) continue;
                        float minLimit, maxLimit;
                        chain.GetLimits(i, out minLimit, out maxLimit);
                        if (value > minLimit && value < maxLimit) {
                            offMask[c1] = true;
                            changed = true;
                        }
                    }
                    if (!changed) break;
                }
                {
                    float minLimit, maxLimit;
                    GetLimitRange(rail, i + 1, out minLimit, out maxLimit);
                    if (maxLimit < minLimit) Debug.LogError("Limit range exception!");
                    value = Mathf.Clamp(value, minLimit, maxLimit);
                }
                rail._value = value;
            }
        }
    }
    void GetLimitRange(UIRail rail, int collapseId, out float minLimit, out float maxLimit) {
        var chains = rail._chains;
        minLimit = float.MinValue;
        maxLimit = float.MaxValue;
        for (int c1 = 0; c1 < chains.Count; ++c1) {
            var chain = chains[c1];
            float cMinLimit, cMaxLimit;
            chain.GetLimits(collapseId, out cMinLimit, out cMaxLimit);
            minLimit = Mathf.Max(minLimit, cMinLimit);
            maxLimit = Mathf.Min(maxLimit, cMaxLimit);
        }
        /*if (rail.name == "M2") {
            Debug.Log("@" + collapseId + " " + minLimit + " -- " + maxLimit + " with " + chains.Count + " C:" + chains.Select(c => c.ToString()).Aggregate((c1, c2) => c1 + ", " + c2));
        }*/
    }

    [System.NonSerialized]
    private float oldWidth = -1;
    public void OnGUI() {
        if (Screen.width != oldWidth) {
            Arrange(new Vector2(oldWidth = Screen.width, Screen.height));
        }
        for (int r = 0; r < rails.Count; ++r) {
            var rail = rails[r];
            if (!float.IsNaN(rail._value))
                GUI.Label(new Rect(rail._value - 3, r * 20, 200, 20), "#" + rail.name);
        }
        for (int c = 0; c < HConstraints.Length; ++c) {
            var constraint = HConstraints[c];
            if (!constraint._CanFlex) {
                var y = 200 + c * 20;
                var left = constraint.Left._value;
                var right = constraint.Right._value;
                GUI.Box(Rect.MinMaxRect(Mathf.Min(left, right), y, Mathf.Max(left, right), y + 20), constraint.name + " +" + constraint.Width + ": " + constraint.CollapseId);
            }
        }
    }

}
