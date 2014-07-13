using UnityEngine;
using System.Collections;

public class UIElement : MonoBehaviour {

    public UIRail Top;
    public UIRail Right;
    public UIRail Bottom;
    public UIRail Left;


    public Rect GetBounds() {
        return Rect.MinMaxRect(
            Left._value, Top._value,
            Right._value, Bottom._value
        );
    }


    public UIRail GetEdge(int d) {
        switch (d) {
            case 0: return Left;
            case 1: return Top;
            case 2: return Right;
            case 3: return Bottom;
        }
        return null;
    }

    public void SetEdge(int d, UIRail rail) {
        switch (d) {
            case 0: Left = rail; break;
            case 1: Top = rail; break;
            case 2: Right = rail; break;
            case 3: Bottom = rail; break;
        }
    }
}
