using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityRTS;

public class SelectionManager : MonoBehaviour {

    // The camera to use for picking
    public Camera Camera;
    // The marker to use to show selections
    public SelectionMarker SelectionMarker;

    // Parameters for drag selection
    private Vector2? dragStart;
    private Vector2 dragEnd;
    private Rect dragRect {
        get {
            return Rect.MinMaxRect(
                Mathf.Min(dragStart.Value.x, dragEnd.x),
                Mathf.Min(dragStart.Value.y, dragEnd.y),
                Mathf.Max(dragStart.Value.x, dragEnd.x),
                Mathf.Max(dragStart.Value.y, dragEnd.y)
            );
        }
    }
    private bool IsDragging { get { return dragStart != null && dragRect.width >= 4 && dragRect.height >= 4; } }

    // The currently selected objects
    public List<Entity> Selected = new List<Entity>();
    // Markers for the currently selected objects
    public List<SelectionMarker> Markers = new List<SelectionMarker>();

    void Awake() {
        // Make sure we have a valid camera
        if (Camera == null) Camera = camera;
        if (Camera == null) Camera = Camera.main;
    }

    void Update() {
        if (GUIUtility.hotControl == 0) {
            // Begin dragging if they user clicks
            if (dragStart == null && Input.GetMouseButton(0)) dragStart = (Input.mousePosition);
            // Update the drag if they are still clicking
            if (dragStart != null) {
                if (Input.GetMouseButton(0)) dragEnd = (Input.mousePosition);
                UpdateSelection(Input.mousePosition);
                if (Input.GetMouseButtonUp(0)) {
                    EndSelection(Input.mousePosition);
                }
            }
            // Give units orders
            if (Input.GetMouseButtonDown(1)) {
                CommandAtRay(Input.mousePosition);
            }
        }

        // Delete the selection if the user presses delete
        if (Input.GetKeyDown(KeyCode.Delete)) {
            for (int s = Selected.Count - 1; s >= 0; --s) {
                var selected = Selected[s];
                if (selected != null) {
                    var entity = selected;
                    if (entity != null) entity.Simulation.Die();
                }
            }
            ClearSelected();
        }
    }

    // Clear the selection based on user input
    public void PrepareSelect() {
        if (!Input.GetKey(KeyCode.LeftShift)) ClearSelected();
    }
    // Update the drag-selection
    public void UpdateSelection(Vector2 position) {
        if (dragStart != null) {
            dragEnd = position;
        }
        if (IsDragging) {
            PrepareSelect();
            SelectInRange(dragRect);
        }
    }
    // Perform the drag-selection
    public void EndSelection(Vector2 position) {
        if (!IsDragging) {
            PrepareSelect();
            SelectAtRay(Input.mousePosition);
        }
        dragStart = null;
    }

    // Various methods to select objects
    public void SelectInRange(Rect rect) {
        foreach (Entity entity in GameObject.FindObjectsOfType(typeof(Entity))) {
            if (!entity.GetIsSelectable()) continue;
            var spos = Camera.WorldToScreenPoint(entity.transform.position);
            if (rect.Contains(spos)) AddSelectd(entity);
        }
    }
    public Entity GetAtRay(Vector2 mousePos) {
        var ray = Camera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100)) {
            return hit.collider.GetComponentInParents<Entity>();
        }
        return null;
    }
    public void SelectAtRay(Vector2 mousePos) {
        var entity = GetAtRay(mousePos);
        if (entity != null && entity.GetIsSelectable())
            AddSelectd(entity);
    }

    // Send an order to the selected objects
    public void CommandAtRay(Vector2 mousePos) {
        var ray = Camera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100)) {
            var target = hit.collider.GetComponentInParents<Entity>();
            var targetEntity = target != null ? target.Simulation : null;
            Target(targetEntity, new Vector2(hit.point.x, hit.point.z));
        }
    }

    // Clear the selection
    public void ClearSelected() {
        Selected.Clear();
        for (int m = 0; m < Markers.Count; ++m) Destroy(Markers[m].gameObject);
        Markers.Clear();
    }
    // Add a single object to the selection
    public void AddSelectd(Entity selectable) {
        if (Selected.Contains(selectable)) return;
        Selected.Add(selectable);
        if (SelectionMarker != null) {
            var marker = GenerateMarker(selectable);
            Markers.Add(marker);
        }
    }

    // Draw the selection box
    void OnGUI() {
        if (IsDragging) {
            GUI.Box(Transform(dragRect), Selected.Count + " units");
        }
    }

    // Change from Screen Space to GUI Space
    private Rect Transform(Rect rect) {
        return new Rect(rect.x, Screen.height - rect.yMax - 1, rect.width, rect.height);
    }
    private Vector2 Transform(Vector2 vec) {
        return new Vector2(vec.x, Screen.height - vec.y - 1);
    }

    // Notify of a new action request
    private void Target(Simulation.SimEntity entity, Vector2 location) {
        var command = new Simulation.CommandAction() {
            EntityIds = Selected.Select(e => e.Simulation.Id).ToArray(),
            RequestEntityId = entity != null ? entity.Id : -1,
            RequestLocation = location,
        };
        if (SceneManager.Instance != null) {
            SceneManager.Instance.QueueCommand(command);
        }
    }

    private struct MarkerProperties {
        public float Width, Height, Radius;
        public MarkerProperties(float w, float h, float r) { Width = w; Height = h; Radius = r; }
    }
    private Dictionary<MarkerProperties, Mesh> meshCache = new Dictionary<MarkerProperties,Mesh>();
    private SelectionMarker GenerateMarker(Entity entity) {
        var bounds = entity.GetComponent<BoxCollider>();
        var properties = bounds != null ? new MarkerProperties(bounds.size.x * bounds.transform.localScale.x, bounds.size.z * bounds.transform.localScale.z, 0.1f) : new MarkerProperties(0, 0, 0.5f);
        Mesh mesh = null;
        if (!meshCache.TryGetValue(properties, out mesh)) {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            const int Angles = 20;
            const float RadiusExtra = 0.1f;
            for (var a = 0; a < Angles; ++a) {
                var ang = (float)a / Angles * Mathf.PI * 2;
                var sa = Mathf.Sin(ang);
                var ca = Mathf.Cos(ang);
                for (var d = 0; d < 2; ++d) {
                    var rad = properties.Radius + RadiusExtra * d;
                    vertices.Add(new Vector3(
                        sa * rad + properties.Width * (sa < 0 ? -1 : 1) / 2,
                        0,
                        ca * rad + properties.Height * (ca < 0 ? -1 : 1) / 2
                    ));
                }
                var v1 = (a > 0 ? a - 1 : Angles - 1) * 2;
                var v2 = (a) * 2;
                indices.Add((short)(v1 + 0));
                indices.Add((short)(v1 + 1));
                indices.Add((short)(v2 + 1));
                indices.Add((short)(v1 + 0));
                indices.Add((short)(v2 + 1));
                indices.Add((short)(v2 + 0));
            }
            mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            meshCache.Add(properties, mesh);
        }
        var marker = (SelectionMarker)GameObject.Instantiate(SelectionMarker);
        marker.Entity = entity;
        marker.GetComponent<MeshFilter>().mesh = mesh;
        return marker;
        /*
        class SelectionMesh {
            vertices: Standard.IndexedBuffer;
            constructor(public radius: number, public size: vec2) {
                var Angles: number = 20;
                var RadiusExtra: number = 0.1;
                this.vertices = new Standard.IndexedBuffer();
                this.vertices.positions = [];
                this.vertices.indices = [];
                for (var a = 0; a < Angles; ++a) {
                    var ang = a / Angles * Math.PI * 2;
                    var sa = Math.sin(ang);
                    var ca = Math.cos(ang);
                    for (var d = 0; d < 2; ++d) {
                        var rad = radius + RadiusExtra * d;
                        this.vertices.positions.push(
                            sa * rad + size[0] * (sa < 0 ? -1 : 1) / 2,
                            0,
                            ca * rad + size[1] * (ca < 0 ? -1 : 1) / 2
                        );
                    }
                    var v1 = (a > 0 ? a - 1 : Angles - 1) * 2;
                    var v2 = (a) * 2;
                    this.vertices.indices.push(v1 + 0, v1 + 1, v2 + 1, v1 + 0, v2 + 1, v2 + 0);
                }
            }
        }
        */
    }

}
