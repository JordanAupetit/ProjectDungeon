using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class InfluenceValue {
    public UIConstraint Constraint;
    public int CollapseId;
    public float Value;
    // The id at which this can collapse/expand freely on each side
    public int LeftCollapseId, RightCollapseId;
    public InfluenceValue(UIConstraint constraint, bool reverse) {
        Constraint = constraint;
        CollapseId = constraint.CollapseId;
        Value = constraint.Width * (reverse ? -1 : 1);
        //if (Value == 0) Debug.LogError("Zero value! " + constraint.name);
        LeftCollapseId = reverse ? CollapseId : int.MaxValue;
        RightCollapseId = reverse ? int.MaxValue : CollapseId;
    }
}
public class InfluenceChain {
    public UIRail Rail;
    public List<InfluenceValue> Constraints = new List<InfluenceValue>();
    public int LeftCollapseId {
        get {
            int start = 0;
            GetLastRail(out start);
            return Constraints.Skip(start).Min(c => c.LeftCollapseId);
        }
    }
    public int RightCollapseId {
        get {
            int start = 0;
            GetLastRail(out start);
            return Constraints.Skip(start).Min(c => c.RightCollapseId);
        }
    }

    public InfluenceChain(UIRail rail) { Rail = rail; }

    public int OverlapLength(InfluenceChain other) {
        int len = Mathf.Min(Constraints.Count, other.Constraints.Count);
        for (int c = 0; c < len; ++c) {
            if (Constraints[c].Constraint != other.Constraints[c].Constraint) return c;
        }
        return len;
    }

    private UIRail GetLastRail(out int index) {
        var outRail = Rail;
        var rail = Rail;
        index = 0;
        for (int c = 0; c < Constraints.Count; ++c) {
            var other = Constraints[c].Constraint.Other(rail);
            if (!float.IsNaN(other._value)) { outRail = other; index = c + 1; }
            rail = other;
        }
        return outRail;
    }

    // Return the length of everything >= collapseId
    public float GetEndValue(int collapseId) {
        int start;
        var rail = GetLastRail(out start);
        float value = rail._value;
        for (int c = start; c < Constraints.Count; ++c) {
            if (Constraints[c].CollapseId >= collapseId) value += Constraints[c].Value;
        }
        return value;
    }
    // Get the limit including everything >= collapseId; allowing everything else
    // to collapse or expand to infinity
    public void GetLimits(int collapseId, out float minLimit, out float maxLimit) {
        int start;
        var rail = GetLastRail(out start);
        minLimit = maxLimit = rail._value;
        for (int c = start; c < Constraints.Count; ++c) {
            var constraint = Constraints[c];
            if (constraint.CollapseId >= collapseId) {
                minLimit += constraint.Value;
                maxLimit += constraint.Value;
            } else {
                if (constraint.Value < 0) minLimit = float.MinValue;
                else maxLimit = float.MaxValue;
            }
        }
    }
    public float GetLength(int collapseId) {
        int start;
        GetLastRail(out start);
        float value = 0;
        if (Constraints.Count == 0) Debug.LogError("No constraints!");
        for (int c = start; c < Constraints.Count; ++c) {
            if (Constraints[c].CollapseId == collapseId) {
                value += Mathf.Abs(Constraints[c].Value);
            }
        }
        return value;
    }
    public bool IsValidValue(int collapseId, float value) {
        var endValue = GetEndValue(collapseId);
        int leftColId = LeftCollapseId;
        int rightColId = RightCollapseId;
        if (value < endValue - 0.001f) return collapseId > leftColId;
        if (value > endValue + 0.001f) return collapseId > rightColId;
        return true;
    }

    public override string ToString() {
        var rail = Rail;
        string str = rail.name;
        for (int c = 0; c < Constraints.Count; ++c) {
            var other = Constraints[c].Constraint.Other(rail);
            str += "-" + other.name;
            rail = other;
        }
        return str;
    }

}

public class UIRail : MonoBehaviour {

    public enum Directions { Horizontal, Vertical, };

    public Directions Direction = Directions.Horizontal;

    [NonSerialized]
    public List<InfluenceChain> _chains = new List<InfluenceChain>();

    [NonSerialized]
    public float _value;

}
