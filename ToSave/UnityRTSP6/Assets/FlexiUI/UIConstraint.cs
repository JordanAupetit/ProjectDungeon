using UnityEngine;
using System.Collections;

public class UIConstraint : MonoBehaviour {

    public int CollapseId = 0;
    public float Width = 100;

    public UIRail Left;
    public UIRail Right;

    public UIRail.Directions Direction {
        get { return Left.Direction; }
    }

    public bool _CanFlex { get { return float.IsNaN(Left._value) || float.IsNaN(Right._value); } }

    public bool HasEdge(UIRail rail) {
        return Left == rail || Right == rail;
    }

    public UIRail Other(UIRail rail) {
        if (Left == rail) return Right;
        if (Right == rail) return Left;
        return null;
    }

}
