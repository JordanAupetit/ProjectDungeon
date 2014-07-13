using UnityEngine;
using System.Collections;

/// <summary>
/// This represents an entity that interacts with the LOSManager
/// </summary>
[ExecuteInEditMode]
public class LOSEntity : MonoBehaviour {

    public enum RevealStates { Hidden, Fogged, Unfogged, };

    // Revealers reveal the surrounding area of terrain
    public bool IsRevealer = false;
    // Explicitly disable revealing (set when SetIsCurrentTeam(false) is called)
    public bool DisableRevealing { get; private set; }

    // AO only really works on static entities, or very tall entities
    public bool EnableAO = false;

    public float Range = 10;

    // Bounds to be used for AO, height blocking, and LOS reveal
    public BoxCollider BoundCollider;
    // Height is used for AO and peering over walls
    public float Height { get { return BoundCollider != null ? BoundCollider.center.y + BoundCollider.size.y : 0; } }
    public Vector2 BaseSize { get { return BoundCollider != null ? new Vector2(BoundCollider.size.x, BoundCollider.size.z) : Vector2.zero; } }

    // Our bounds on the terrain
    public Rect Bounds {
        get {
            var fwd = transform.forward * transform.localScale.z;
            var rgt = transform.right * transform.localScale.x;
            var aabbSize = new Vector2(
                Mathf.Abs(rgt.x) * BaseSize.x + Mathf.Abs(fwd.x) * BaseSize.y,
                Mathf.Abs(rgt.z) * BaseSize.x + Mathf.Abs(fwd.z) * BaseSize.y
            );
            var bounds = new Rect(
                transform.position.x - aabbSize.x / 2,
                transform.position.z - aabbSize.y / 2,
                aabbSize.x, aabbSize.y);
            return bounds;
        }
    }
    public bool IsRevealing { get { return IsRevealer && !DisableRevealing; } }

    // Current state, if we're visible, fogged, or hidden
    public RevealStates RevealState;

    public void Awake() {
        if (BoundCollider == null) BoundCollider = GetComponent<BoxCollider>();
    }

    public void SetIsCurrentTeam(bool isCurrent) {
        DisableRevealing = !isCurrent;
    }

    // Tell the LOS manager that we're here
    public void OnEnable() {
        LOSManager.AddEntity(this);
    }
    public void OnDisable() {
        LOSManager.RemoveEntity(this);
    }

    // Some cache parameters for FOW animation
    Color _oldfowColor = Color.clear;
    Color _fowColor = Color.clear;
    float _fowInterp = 1;
    // Request a change to the FOW colour
    public void SetFOWColor(Color fowColor, bool interpolate) {
        fowColor.a = 255;
        if (fowColor == _fowColor) return;
        if (!interpolate) {
            _fowColor = fowColor;
            _fowInterp = 1;
            UpdateFOWColor();
            return;
        }
        _oldfowColor = Color.Lerp(_oldfowColor, _fowColor, _fowInterp);
        _fowColor = fowColor;
        _fowInterp = 0;
        UpdateFOWColor();
    }
    // Does this item require fog of war updates
    public bool RequiresFOWUpdate { get { return _fowInterp < 1; } }
    // Returns true when the item has completed transitioning its fog of war colour
    public bool UpdateFOWColor() {
        _fowInterp = Mathf.Clamp01(_fowInterp + Time.deltaTime / 0.4f);
        var fowColor = Color.Lerp(_oldfowColor, _fowColor, _fowInterp);
        foreach (var renderer in GetComponentsInChildren<Renderer>()) {
            if (fowColor.r > 0 || fowColor.g > 0) {
                renderer.enabled = true;
                foreach (var material in renderer.materials) {
                    material.SetColor("_FOWColor", fowColor);
                }
            } else {
                renderer.enabled = false;
            }
        }
        return !RequiresFOWUpdate;
    }

    /*public void OnDrawGizmosSelected() {
        var mat = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(BaseSize.x, Height, BaseSize.y));
        Gizmos.matrix = mat;
    }*/

}
