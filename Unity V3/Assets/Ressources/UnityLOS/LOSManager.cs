using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LOSManager : MonoBehaviour {

    public enum OnDiscover { None, RevealOnFirstDiscover, RevealOnAnyDiscover, StayRevealed, };

    public bool PreviewInEditor = false;

    // The parameters used to determine texture size and quality
    [Serializable]
    public class SizeParameters {
        public Terrain Terrain;
        public int Width = -1, Height = -1;
        public float Scale = 1;
        public bool HighDetailTexture = false;
    }
    public SizeParameters Size;
    public Terrain Terrain { get { return Size.Terrain; } }
    public int Width { get { return Size.Width; } }
    public int Height { get { return Size.Height; } }
    public float Scale { get { return Size.Scale; } }
    public bool HighDetailTexture { get { return Size.HighDetailTexture; } }

    // The parameters used for visual features
    [Serializable]
    public class VisualParameters {
        [Range(0, 255)]
        public int AOIntensity = 128;
        [Range(0, 1024)]
        public int InterpolationRate = 512;
        public float GrayscaleDecayDuration = 300;
        public float RevealedDecayDuration = float.PositiveInfinity;
        public float VisibleDecayDuration = 0;
        public OnDiscover EntityDiscoverMode = OnDiscover.StayRevealed;
        [Range(0, 4)]
        public float FringeSize = 1;
    }
    public VisualParameters Visual;
    public int AOIntensity { get { return Visual.AOIntensity; } }
    public int InterpolationRate { get { return Visual.InterpolationRate; } }
    public float GrayscaleDecayDuration { get { return Visual.GrayscaleDecayDuration; } }
    public float RevealedDecayDuration { get { return Visual.RevealedDecayDuration; } }
    public float VisibleDecayDuration { get { return Visual.VisibleDecayDuration; } }
    public OnDiscover EntityDiscoverMode { get { return Visual.EntityDiscoverMode; } }
    public float FadeStart { get { return Visual.FringeSize; } }

    // Parameters used to enable height blockers
    [Serializable]
    public class HeightBlockerParameters {
        public bool Enable = true;
        public bool AllowOwnTeamBlockers = false;
    }
    public HeightBlockerParameters HeightBlockers;
    public bool EnableHeightBlockers { get { return HeightBlockers.Enable; } }
    public bool AllowOwnTeamHeightBlockers { get { return HeightBlockers.AllowOwnTeamBlockers; } }

    // List of entities that interact with LOS
    [HideInInspector]
    public List<LOSEntity> Entities = new List<LOSEntity>();
    // List of entities currently animating their LOS
    [HideInInspector]
    public List<LOSEntity> AnimatingEntities = new List<LOSEntity>();

    public int ActualWidth {
        get { return SizeFromParams(Width, Terrain != null ? Terrain.terrainData.size.x : 0, Scale); }
    }
    public int ActualHeight {
        get { return SizeFromParams(Height, Terrain != null ? Terrain.terrainData.size.z : 0, Scale); }
    }
    public float MapWidth { get { return MapSizeFromSize(ActualWidth, Scale); } }
    public float MapHeight { get { return MapSizeFromSize(ActualHeight, Scale); } }

    // Some internal data
    int frameId = 0;
    float oldTimer = 0, timer = 0;
    Color32[] pixels;
    Color32[] lerpPixels;
    float[,] blockHeights;
    float[,] terrainHeightsCache;
    Texture2D losTexture;

    // Used to determine when the user changes a field that requires
    // the texture to be recreated
    private int previewParameterHash = 0;
    private int GenerateParameterHash() { return (Width + Height * 1024) + Scale.GetHashCode() + HighDetailTexture.GetHashCode(); }

    // Get a size from the provided properties
    private int SizeFromParams(int desired, float terrainSize, float scale) {
        int size = 128;
        if (desired > 0) size = Mathf.CeilToInt(desired * scale);
        else if (terrainSize > 0) size = Mathf.CeilToInt(terrainSize * scale);
        return Mathf.Clamp(size, 4, 512);
    }
    private float MapSizeFromSize(int size, float scale) {
        return size / scale;
    }
    // Create a texture matching the required properties
    void InitializeTexture() {
        int width = ActualWidth;
        int height = ActualHeight;
        TextureFormat texFormat = HighDetailTexture ?
            (AOIntensity > 0 ? TextureFormat.ARGB32 : TextureFormat.RGB24) :
            (AOIntensity > 0 ? TextureFormat.ARGB4444 : TextureFormat.RGB565);

        if (losTexture == null || losTexture.format != texFormat || losTexture.width != width || losTexture.height != height) {
            if (losTexture != null) DestroyImmediate(losTexture);
            losTexture = new Texture2D(width, height, texFormat, false);
            losTexture.hideFlags = HideFlags.HideAndDontSave;
        }
        pixels = losTexture.GetPixels32();
        for (int p = 0; p < pixels.Length; ++p) pixels[p] = Color.black;
        losTexture.SetPixels32(pixels);
        blockHeights = null;
        lerpPixels = null;
        if (Terrain != null) {
            Shader.SetGlobalTexture("_FOWTex", losTexture);
            Shader.SetGlobalVector("_FOWTex_ST",
                new Vector4(
                    Scale / width, Scale / height,
                    0, 0
                )
            );
        }
        //Debug.Log("FOW Texture created, " + width + " x" + height);
    }

    public void Update() {
        if (Application.isPlaying || PreviewInEditor) {
            // Make sure we have a valid texture
            if (losTexture == null || previewParameterHash != GenerateParameterHash()) {
                InitializeTexture();
                previewParameterHash = GenerateParameterHash();
            }
#if UNITY_EDITOR
        } else {
            // Or just use a white texture as placeholder
            Shader.SetGlobalTexture("_FOWTex", UnityEditor.EditorGUIUtility.whiteTexture);
            if (losTexture != null) DestroyImmediate(losTexture);
            losTexture = null;
#endif
        }

        if (losTexture != null) {
            // Update timers
            timer += Time.deltaTime;
            ++frameId;

            // Update any animating entities (update their FOW color)
            for (int e = 0; e < AnimatingEntities.Count; ++e) {
                if (AnimatingEntities[e].UpdateFOWColor())
                    AnimatingEntities.RemoveAt(e--);
            }
            // If in editor mode
            if (!Application.isPlaying) {
                // Refresh the map each frame
                for (int p = 0; p < pixels.Length; ++p) {
                    pixels[p] = new Color32(0, 255, 0, 255);
                }
                // Add LOS and AO for all entities
                foreach (var entity in Entities) {
                    RevealLOS(entity, entity.IsRevealing ? 255 : 0, 255, 255);
                    if (entity.EnableAO && (AOIntensity > 0 || EnableHeightBlockers)) {
                        var bounds = entity.Bounds;
                        AddAO(bounds, entity.Height);
                    }
                }
            } else {
                bool forceFullUpdate = Time.frameCount == 1;
                // Reset all entities to be invisible
                if (forceFullUpdate) {
                    int revealerCount = 0;
                    foreach (var entity in Entities) {
                        entity.RevealState = LOSEntity.RevealStates.Hidden;
                        if (entity.IsRevealing) revealerCount++;
                    }
                    if (revealerCount == 0) {
                        Debug.LogError("No LOSEntity items were marked as revealers! Tick the 'Is Revealed' checkbox for at least 1 item.");
                    }
                }
                // Ensure we have space to store blocking heights (if enabled)
                if ((blockHeights == null || blockHeights.GetLength(0) != losTexture.height || blockHeights.GetLength(1) != losTexture.width) && EnableHeightBlockers) {
                    blockHeights = new float[losTexture.height, losTexture.width];
                    forceFullUpdate = true;
                }
                // Reset AO and LOS
                bool updateAo = (frameId % 2) == 0;
                if (updateAo || forceFullUpdate) {
                    // Handle decays
                    int visibleDecayCount = GetDecayAmount(VisibleDecayDuration);
                    int revealedDecayCount = GetDecayAmount(RevealedDecayDuration);
                    int grayDecayCount = GetDecayAmount(GrayscaleDecayDuration);
                    if (visibleDecayCount != 0 || revealedDecayCount != 0 || grayDecayCount != 0 || AOIntensity > 0) {
                        for (int p = 0; p < pixels.Length; ++p) {
                            pixels[p].r = (byte)Mathf.Max(pixels[p].r - visibleDecayCount, 0);
                            pixels[p].g = (byte)Mathf.Max(pixels[p].g - revealedDecayCount, 0);
                            pixels[p].b = (byte)Mathf.Max(pixels[p].b - grayDecayCount, 0);
                            pixels[p].a = 255;
                        }
                    }

                    // Update non-LOS features
                    if (AOIntensity > 0 || EnableHeightBlockers) {
                        if (Terrain != null && EnableHeightBlockers && blockHeights != null) {
                            // If terrainHeightsCache is invalid, recreate it
                            if (terrainHeightsCache == null || terrainHeightsCache.GetLength(0) != blockHeights.GetLength(0) || terrainHeightsCache.GetLength(1) != blockHeights.GetLength(1)) {
                                terrainHeightsCache = (float[,])blockHeights.Clone();
                                for (int y = 0; y < blockHeights.GetLength(0); ++y) {
                                    for (int x = 0; x < blockHeights.GetLength(1); ++x) {
                                        var terrainData = Terrain.terrainData;
                                        int tx = Mathf.RoundToInt(x * terrainData.heightmapWidth / terrainData.size.x / Scale);
                                        int ty = Mathf.RoundToInt(y * terrainData.heightmapHeight / terrainData.size.z / Scale);
                                        terrainHeightsCache[y, x] = terrainData.GetHeight(tx, ty);
                                    }
                                }
                            }
                            for (int y = 0; y < blockHeights.GetLength(0); ++y) {
                                for (int x = 0; x < blockHeights.GetLength(1); ++x) {
                                    blockHeights[y, x] = terrainHeightsCache[y, x];
                                }
                            }
                        }
                        foreach (var entity in Entities) {
                            var bounds = entity.Bounds;
                            if (entity.EnableAO && AOIntensity > 0) AddAO(bounds, entity.Height);
                            if (EnableHeightBlockers && (AllowOwnTeamHeightBlockers || !entity.IsRevealing))
                                AddHeightBlocker(bounds, entity.transform.position.y + entity.Height);
                        }
                    }
                }
                // Reveal LOS from all entities
                foreach (var entity in Entities) {
                    if (entity.IsRevealing) RevealLOS(entity, 255, 255, 330);
                }
                int count = 0;
                foreach (var entity in Entities) {
                    ++count;
                    var rect = entity.Bounds;
                    var fowColor = GetFOWColor(rect);
                    var visible = GetRevealFromFOW(fowColor);
                    bool revealTerrain = false;
                    if (visible == LOSEntity.RevealStates.Unfogged) {
                        switch (EntityDiscoverMode) {
                            case OnDiscover.RevealOnFirstDiscover: revealTerrain = entity.RevealState == LOSEntity.RevealStates.Hidden; break;
                            case OnDiscover.RevealOnAnyDiscover: revealTerrain = entity.RevealState != visible; break;
                            case OnDiscover.StayRevealed: revealTerrain = true; break;
                        }
                    }
                    if (revealTerrain) {
                        RevealLOS(rect, 0, entity.Height + entity.transform.position.y, 0, 255, 255);
                    }
                    if (entity.RevealState != visible || RevealedDecayDuration == 0) {
                        //(entity.RevealState != LOSEntity.RevealStates.Hidden || visible != LOSEntity.RevealStates.Fogged)
                        entity.RevealState = visible;
                    }
                    //if (visible != LOSEntity.RevealStates.Hidden || forceFullUpdate)
                    {
                        entity.SetFOWColor(GetQuantizedFOW(fowColor), !forceFullUpdate);
                        // Queue the item for FOW animation
                        if (entity.RequiresFOWUpdate && !AnimatingEntities.Contains(entity))
                            AnimatingEntities.Add(entity);
                    }
                }
            }
            bool isChanged = true;
            if (InterpolationRate > 0 && Application.isPlaying) {
                if (lerpPixels == null) lerpPixels = pixels.ToArray();
                else {
                    isChanged = false;
                    // `rate` will never be less than this
                    int interpGranularity = HighDetailTexture ? 2 : 4;
                    int quantInterpRate = InterpolationRate / interpGranularity;
                    int rate = ((int)(quantInterpRate * timer) - (int)(quantInterpRate * oldTimer)) * interpGranularity;
                    for (int p = 0; p < lerpPixels.Length; ++p) {
                        byte r = EaseToward(lerpPixels[p].r, pixels[p].r, rate),
                            g = EaseToward(lerpPixels[p].g, pixels[p].g, rate),
                            b = EaseToward(lerpPixels[p].b, pixels[p].b, rate),
                            a = EaseToward(lerpPixels[p].a, pixels[p].a, rate);
                        if (isChanged || lerpPixels[p].a != a || lerpPixels[p].r != r || lerpPixels[p].g != g || lerpPixels[p].b != b) {
                            isChanged = true;
                            lerpPixels[p] = new Color32(r, g, b, a);
                        }
                    }
                }
            } else lerpPixels = null;

            if (isChanged) {
                losTexture.SetPixels32(lerpPixels ?? pixels);
                losTexture.Apply();
            }
            oldTimer = timer;
        }
    }
    private byte EaseToward(byte from, byte to, int amount) {
        if (Mathf.Abs(from - to) < amount) return to;
        return (byte)(from + (to > from ? amount : -amount));
    }
    private int GetDecayAmount(float duration, int granularity = 8) {
        if (float.IsInfinity(duration)) return 0;
        if (duration == 0) return 255;
        int oldGrayDecay = (int)(256 / granularity * oldTimer / duration) * granularity;
        int newGrayDecay = (int)(256 / granularity * timer / duration) * granularity;
        int grayDecayCount = newGrayDecay - oldGrayDecay;
        return grayDecayCount;
    }

    // Get the extents of a point/rectangle
    private void GetExtents(Vector2 pos, int inflateRange, out int xMin, out int yMin, out int xMax, out int yMax) {
        xMin = Mathf.RoundToInt(pos.x - inflateRange);
        xMax = Mathf.RoundToInt(pos.x + inflateRange);
        yMin = Mathf.RoundToInt(pos.y - inflateRange);
        yMax = Mathf.RoundToInt(pos.y + inflateRange);
        if (xMin < 0) xMin = 0; else if (xMax >= losTexture.width) xMax = losTexture.width - 1;
        if (yMin < 0) yMin = 0; else if (yMax >= losTexture.height) yMax = losTexture.height - 1;
    }
    private void GetExtents(Rect rect, float inflateRange, out int xMin, out int yMin, out int xMax, out int yMax) {
        xMin = Mathf.CeilToInt((rect.xMin - inflateRange) * Scale - 0.5f);
        xMax = Mathf.FloorToInt((rect.xMax + inflateRange) * Scale - 0.5f);
        yMin = Mathf.CeilToInt((rect.yMin - inflateRange) * Scale - 0.5f);
        yMax = Mathf.FloorToInt((rect.yMax + inflateRange) * Scale - 0.5f);
        if (xMin < 0) xMin = 0; else if (xMax >= losTexture.width) xMax = losTexture.width - 1;
        if (yMin < 0) yMin = 0; else if (yMax >= losTexture.height) yMax = losTexture.height - 1;
        if (xMax < xMin) xMax = xMin;
        if (yMax < yMin) yMax = yMin;
    }

    // Add a height blocker
    private void AddHeightBlocker(Rect rect, float height) {
        int xMin, yMin, xMax, yMax;
        GetExtents(rect, 0, out xMin, out yMin, out xMax, out yMax);
        for (int y = yMin; y <= yMax; ++y) {
            for (int x = xMin; x <= xMax; ++x) {
                blockHeights[y, x] = Mathf.Max(blockHeights[y, x], height);
            }
        }
    }

    private float DoAxisRange(float x, float center, float width, float range) {
        var value = (Mathf.Abs(x - center) - (width / 2)) / range;
        if (value < 0) value = 0;
        return value;
    }
    // Add ambient occlusion around an eara
    private void AddAO(Rect rect, float height) {
        byte aoAmount = (byte)AOIntensity;
        byte nonAOAmount = (byte)(255 - aoAmount);
        float spreadRange = height / 2 + 0.2f;
        int xMin, yMin, xMax, yMax;
        GetExtents(rect, spreadRange, out xMin, out yMin, out xMax, out yMax);
        for (int y = yMin; y <= yMax; ++y) {
            float fy = (y + 0.5f) / Scale;
            float yIntl = Mathf.Clamp(fy, rect.yMin, rect.yMax);
            for (int x = xMin; x <= xMax; ++x) {
                float fx = (x + 0.5f) / Scale;
                var nodePos = new Vector2(fx, fy);
                var intlPos = new Vector2(Mathf.Clamp(fx, rect.xMin, rect.xMax), yIntl);

                var axisClearance =
                    DoAxisRange(nodePos.x, rect.center.x, rect.width, spreadRange) +
                    DoAxisRange(nodePos.y, rect.center.y, rect.height, spreadRange);
                if (axisClearance > 1) continue;

                //float dst2 = (intlPos - nodePos).sqrMagnitude;
                //if (dst2 >= spreadRange * spreadRange) continue;
                int p = x + y * losTexture.width;
                //var aoValue = 2 * dst2 / (dst2 * 1 + spreadRange * spreadRange + 1);
                var aoValue = 1 - (1 - axisClearance) * (1 - axisClearance);
                byte value = (byte)(nonAOAmount + aoAmount * aoValue);
                if (pixels[p].a > value) pixels[p].a = value;
            }
        }
    }

    // Reveal an area
    private void RevealLOS(LOSEntity sight, float los, float fow, float grayscale) {
        Rect rect = sight.Bounds;
        RevealLOS(rect, sight.Range, sight.Height + sight.transform.position.y, los, fow, grayscale);
    }
    struct Node {
        public short X, Y;
        public float StartAngle, EndAngle;
        public short Distance;
        public byte Overlap;
        public override string ToString() {
            return StartAngle + " - " + EndAngle + " +" + Overlap;
        }
    }
    private List<Node> allNodes = new List<Node>();
    private List<int> activeNodes = new List<int>();
    private List<int> nodeOverlaps = new List<int>();
    private float GetStartAngle(int lx, int ly, Vector2 loc) {
        var dx = (lx + (ly + (lx > 0 ? 1 : 0) > 0 ? -0.0f : 1.0f) - loc.x);
        var dy = (ly + (lx + (ly > 0 ? 0 : 1) > 0 ? 1.0f : -0.0f) - loc.y);
        return AngFromDXY(dx, dy);
    }
    private float GetEndAngle(int lx, int ly, Vector2 loc) {
        var dx = (lx + (ly + (lx > 0 ? 0 : 1) > 0 ? 1.0f : -0.0f) - loc.x);
        var dy = (ly + (lx + (ly > 0 ? 1 : 0) > 0 ? -0.0f : 1.0f) - loc.y);
        return AngFromDXY(dx, dy);
    }
    private float AngFromDXY(float dx, float dy) {
        var ady = Mathf.Abs(dy);
        if (dx < -ady) return 6 - dy / dx;
        if (dx > ady)return 2 - dy / dx;
        if (dy < 0) return 4 + dx / dy;
        if (ady < 0.001f) return 0;
        if (dx < 0) return 8 + dx / dy;
        return dx / dy;
    }
    [NonSerialized]
    int[] columns = new int[128];
    [NonSerialized]
    float[] colAngCache = new float[128];
    [NonSerialized]
    int[] columnsMax = new int[128];
    [NonSerialized]
    int[] colClearXMin = new int[256];
    [NonSerialized]
    int[] colClearXMax = new int[256];
    private void RevealLOS(Rect rect, float range, float height, float los, float fow, float grayscale) {
        var mapCtr = rect.center * Scale;
        int cx = Mathf.FloorToInt(mapCtr.x), cy = Mathf.FloorToInt(mapCtr.y);
        if (range * Scale >= 1 && blockHeights != null && EnableHeightBlockers &&
            (cx >= 0 && cy >= 0 && cx < losTexture.width && cy < losTexture.height))
        {
            int rangeI = Mathf.CeilToInt((range + Mathf.Max(rect.width, rect.height)) * Scale);
            int xiMin, yiMin, xiMax, yiMax;
            GetExtents(rect, 0, out xiMin, out yiMin, out xiMax, out yiMax);

            var locCtr = mapCtr - new Vector2(cx, cy);

            int minX = -cx, maxX = losTexture.width - cx - 1;
            int minY = -cy, maxY = losTexture.height - cy - 1;

            allNodes.Clear();
            activeNodes.Clear();
            nodeOverlaps.Clear();
            for (int c = -rangeI; c < rangeI; ++c) {
                colClearXMin[c + 128] = -100000;
                colClearXMax[c + 128] = 100000;
            }
            // Find all blocking nodes, update clear column bounds
            for (int d = -1; d <= 1; d += 2) {
                activeNodes.Clear();
                for (int c = 0; c < rangeI; ++c) {
                    columnsMax[c] = GetColumnSize(c, rangeI);
                    columns[c] = Mathf.Clamp(-columnsMax[c] * d, minX, maxX) * d;
                    columnsMax[c] = Mathf.Clamp(columnsMax[c] * d, minX, maxX) * d;
                }
                columns[0] = 1;
                for (int c = 0; c < rangeI; ++c) UpdateColumnCache(c, d, locCtr);
                int rangeY = Mathf.Clamp(rangeI * d, minY - 1, maxY + 1) * d;
                for (int t = 0; t < 4096; ++t) {
                    int col = GetNextColumn(rangeY, d, locCtr);
                    if (col == -1) break;
                    int row = columns[col]++;
                    UpdateColumnCache(col, d, locCtr);

                    int lx = row * d, ly = col * d;
                    int wx = cx + lx, wy = cy + ly;
                    if (blockHeights[wy, wx] <= height) continue;
                    var lStartXPerY = GetStartAngle(lx, ly, locCtr);
                    var lEndXPerY = GetEndAngle(lx, ly, locCtr);
                    if (lEndXPerY < lStartXPerY) lEndXPerY += 8;
                    bool isVisible = true;
                    int dst = lx * lx + ly * ly;
                    for (int n = 0; n < activeNodes.Count; ++n) {
                        var node = allNodes[activeNodes[n]];
                        if (DeltaAngleS1(node.StartAngle, node.EndAngle, lStartXPerY) >= -0.00001f) {
                            activeNodes.RemoveAt(n--);
                        }
                        if (dst > node.Distance && DeltaAngleS2(node.StartAngle, lEndXPerY, node.EndAngle) >= 0) {
                            isVisible = false; break;
                        }
                    }
                    if (isVisible) {
                        if (lx <= 0) colClearXMin[ly + 128] = Mathf.Max(lx, colClearXMin[ly + 128]);
                        if (lx >= 0) colClearXMax[ly + 128] = Mathf.Min(lx, colClearXMax[ly + 128]);

                        var node = new Node() {
                            X = (short)lx,
                            Y = (short)ly,
                            Overlap = (byte)allNodes.Count,
                            StartAngle = lStartXPerY,
                            EndAngle = lEndXPerY,
                            Distance = (short)dst,
                        };
                        activeNodes.Add(allNodes.Count);
                        allNodes.Add(node);
                    }
                }
            }
            // If no nodes were found, defer to faster non-height routine below
            if (allNodes.Count > 0) {
                activeNodes.Clear();
                // Clean up invalid nodes
                for (int n1 = 0; n1 < allNodes.Count; ++n1) {
                    var node = allNodes[n1];
                    float cover = node.StartAngle;
                    for (int a = 0; a < activeNodes.Count; ++a) {
                        int n2 = activeNodes[a];
                        var other = allNodes[n2];
                        if (DeltaAngleS1(other.StartAngle, other.EndAngle, node.StartAngle) >= -0.00001f) {
                            activeNodes.RemoveAt(a--);
                            continue;
                        }
                        if (other.Distance >= node.Distance) continue;
                        var otherEnd = ForwardAngle(node.StartAngle, 1, other.EndAngle);
                        cover = Mathf.Max(cover, otherEnd);
                    }
                    for (int n2 = n1 + 1; n2 < allNodes.Count; ++n2) {
                        var other = allNodes[n2];
                        if (other.Distance >= node.Distance) continue;
                        if (DeltaAngleS1(node.StartAngle, cover, other.StartAngle) > 0.0001f) break;
                        var otherEnd = ForwardAngle(node.StartAngle, 1, other.EndAngle);
                        cover = Mathf.Max(cover, otherEnd);
                    }
                    if (IsAngleGreater(node.StartAngle, cover, node.EndAngle)) {
                        allNodes.RemoveAt(n1--);
                    } else {
                        activeNodes.Add(n1);
                        node.Overlap = (byte)activeNodes.Count;
                        allNodes[n1] = node;
                    }
                }
                // Validate clear bounds
                for (int d = -1; d <= 1; d += 2) {
                    int minClearX = -10000, maxClearX = 10000;
                    for (int c = 0; c < rangeI; ++c) {
                        if (minClearX < -c) minClearX--;
                        if (maxClearX > c) maxClearX++;
                        colClearXMin[c * d + 128] = minClearX = Mathf.Max(colClearXMin[c * d + 128], minClearX);
                        colClearXMax[c * d + 128] = maxClearX = Mathf.Min(colClearXMax[c * d + 128], maxClearX);
                    }
                }
                // TODO: Fix Overlap for first items, then adjust startBlock
                int startBlock = 0;
                // Perform occlusion queries and apply visibility
                for (int d = -1; d <= 1; d += 2) {
                    for (int c = 0; c < rangeI; ++c) {
                        columnsMax[c] = GetColumnSize(c, rangeI);
                        columns[c] = Mathf.Clamp(-columnsMax[c] * d, minX, maxX) * d;
                        columnsMax[c] = Mathf.Clamp(columnsMax[c] * d, minX, maxX) * d;
                    }
                    columns[0] = 1;
                    for (int c = 0; c < rangeI; ++c) UpdateColumnCache(c, d, locCtr);
                    int rangeY = Mathf.Clamp(rangeI * d, minY - 1, maxY + 1) * d;
                    for (int t = 0; t < 4096; ++t) {
                        int col = GetNextColumn(rangeY, d, locCtr);
                        if (col < 0) break;
                        int row = columns[col]++;
                        UpdateColumnCache(col, d, locCtr);

                        int lx = row * d, ly = col * d;
                        int wx = cx + lx, wy = cy + ly;
                        var nodePos = new Vector2(wx + 0.5f, wy + 0.5f) / Scale;
                        var intlPos = new Vector2(
                            Mathf.Clamp(nodePos.x, rect.xMin, rect.xMax),
                            Mathf.Clamp(nodePos.y, rect.yMin, rect.yMax)
                        );
                        float dist2 = (intlPos - nodePos).sqrMagnitude;
                        float range2 = (range * range);
                        if (dist2 > range2) continue;

                        float bright = 1;
                        if (FadeStart > 0) {
                            float innerRange = Mathf.Max(range - FadeStart, 0);
                            float innerRange2 = innerRange * innerRange;
                            if (dist2 > innerRange2)
                                bright *= Mathf.Clamp01((range - Mathf.Sqrt(dist2)) / (range - innerRange));
                        }

                        if (dist2 > 0.001f && (lx <= colClearXMin[ly + 128] || lx >= colClearXMax[ly + 128])) {
                            //bright = 0;
                            int dst = lx * lx + ly * ly;

                            float lStartXPerY = GetStartAngle(lx, ly, locCtr);
                            float lEndXPerY = GetEndAngle(lx, ly, locCtr);
                            if (lEndXPerY < lStartXPerY) lEndXPerY += 8;

                            float revealedFrom = lStartXPerY, revealedTo = lEndXPerY;

                            if (allNodes.Count > 1) {
                                int prevSB = startBlock;
                                for (int ty = 0; ty < 50; ++ty) {
                                    var block = allNodes[startBlock];
                                    // We're intersecting current block
                                    if (IsAngleBetween(revealedFrom, ref block)) break;

                                    int nextId = startBlock + 1;
                                    if (nextId >= allNodes.Count) nextId -= allNodes.Count;
                                    var next = allNodes[nextId];
                                    // Next block is too far forward
                                    if (IsAngleGreater(block.StartAngle, next.StartAngle, revealedFrom)) break;
                                    startBlock = nextId;
                                }
                                /*if (prevSB != startBlock) {
                                    int colStart = 2;
                                    int prevId = startBlock;
                                    for (int ty = 0; ty < 10; ++ty) {
                                        var blockId = startBlock + ty;
                                        if (blockId >= allNodes.Count) blockId -= allNodes.Count;
                                        var block = allNodes[blockId];
                                        if (blockId != prevId) {
                                            var prev = allNodes[prevId];
                                            if (IsAngleGreater(prev.StartAngle, block.StartAngle, prev.EndAngle)) break;
                                        }
                                        colStart = Mathf.Max(colStart, Mathf.Abs(block.Y));
                                        for (int c = colStart; c < rangeI; ++c) {
                                            bool changed = false;
                                            while (columns[c] < columnsMax[c] && AngleDistance(block.EndAngle, GetEndAngle((columns[c] + 1) * d, c * d, locCtr)) < 0) {
                                                ++columns[c];
                                                changed = true;
                                            }
                                            if (changed) UpdateColumnCache(c, d, locCtr);
                                        }
                                        prevId = blockId;
                                    }
                                }*/
                            }
                            int endId = startBlock;
                            for (int b = 0; b < allNodes.Count; ++b) {
                                var node = allNodes[endId];
                                // If intersecting, minimise the reveal amount by the overlap
                                if (dst > node.Distance && IsAngleBetween(revealedFrom, ref node)) {
                                    revealedFrom = ForwardAngle(revealedFrom, 1.0f, node.EndAngle);
                                    if (revealedFrom >= revealedTo) break;
                                }
                                int nextId = endId + 1;
                                if (nextId >= allNodes.Count) nextId -= allNodes.Count;
                                if (nextId == startBlock) break;
                                var next = allNodes[nextId];
                                // Next block too far forward
                                if (IsAngleGreater(node.StartAngle, next.StartAngle, revealedFrom)) break;
                                endId = nextId;
                            }
                            if (revealedTo <= revealedFrom) continue;
                            MoveAngleTo(ref endId, revealedTo);
                            int overlap = 1;
                            for (int tr = 0; tr < 10 && overlap > 0; ++tr) {
                                if (MoveBack(ref endId, ref overlap, revealedTo)) {
                                    var node = allNodes[endId];
                                    if (dst > node.Distance) {
                                        revealedTo = ForwardAngle(revealedTo, -1.0f, node.StartAngle);
                                        if (revealedTo <= revealedFrom) break;
                                    }
                                }
                                if (endId == startBlock) break;
                            }
                            if (revealedTo <= revealedFrom) continue;
                            bright *= (revealedTo - revealedFrom) / (lEndXPerY - lStartXPerY);
                        }
                        int p = wx + wy * losTexture.width;
                        pixels[p].r = (byte)Mathf.Max(pixels[p].r, (byte)(bright * los));
                        pixels[p].g = (byte)Mathf.Max(pixels[p].g, (byte)(bright * fow));
                        pixels[p].b = (byte)Mathf.Max(pixels[p].b, (byte)(Mathf.Clamp(bright * grayscale, 0, 255)));
                    }
                }
                // Reveal the centre cell
                {
                    int p = cx + cy * losTexture.width;
                    pixels[p].r = (byte)Mathf.Max(pixels[p].r, (byte)(los));
                    pixels[p].g = (byte)Mathf.Max(pixels[p].g, (byte)(fow));
                    pixels[p].b = (byte)Mathf.Max(pixels[p].b, (byte)(Mathf.Clamp(grayscale, 0, 255)));
                }
                return;
            }
        }/* else if (EnableHeightBlockers && blockHeights != null) {
            int rangeI = Mathf.CeilToInt(range * Scale);
            int rangeIE = Mathf.CeilToInt((range + Mathf.Max(rect.width, rect.height)) * Scale);
            int xMin, yMin, xMax, yMax;
            int xiMin, yiMin, xiMax, yiMax;
            GetExtents(rect, range, out xMin, out yMin, out xMax, out yMax);
            GetExtents(rect, 0, out xiMin, out yiMin, out xiMax, out yiMax);

            for (int a = 0; a < 4; ++a) {
                int d = (a % 2);
                // Get axis extents, based on direction
                int jMin = d == 0 ? xMin : yMin, jMax = d == 0 ? xMax : yMax;
                int kMin = d == 0 ? yMin : xMin, kMax = d == 0 ? yMax : xMax;
                int jMid = d == 0 ? (xiMin + xiMax) / 2 : (yiMin + yiMax) / 2;
                int kMid = d == 0 ? (yiMin + yiMax) / 2 : (xiMin + xiMax) / 2;
                int prevMax = 0;
                // Iterate one axis
                for (int dj = jMin - jMid; dj <= jMax - jMid; ++dj) {
                    int kStart = (a < 2 ? Mathf.Max(kMin - kMid, 0) : Mathf.Max(kMid - kMax, 0));
                    int kEnd = (a < 2 ? kMax - kMid : kMid - kMin);
                    // Make sure its in bounds
                    if (kEnd <= 0) continue;
                    // Iterate outward
                    int curMax = kEnd;
                    for (int dk = kStart; dk <= kEnd; ++dk) {
                        int wj = jMid + dj * dk / kEnd;
                        int wk = kMid + dk * (a < 2 ? 1 : -1);
                        int wx = d == 0 ? wj : wk;
                        int wy = d == 0 ? wk : wj;
                        if (wx < 0 || wy < 0 || wx >= losTexture.width || wy >= losTexture.height) continue;
                        if (curMax == kEnd && dk >= 0 && (wx < xiMin || wy < yiMin || wx > xiMax || wy > yiMax)) {
                            if (blockHeights[wy, wx] > height) curMax = dk;
                        }
                        {
                            var nodePos = new Vector2(wx, wy) / Scale;
                            var intlPos = new Vector2(
                                Mathf.Clamp(nodePos.x, rect.xMin, rect.xMax - 1),
                                Mathf.Clamp(nodePos.y, rect.yMin, rect.yMax - 1)
                            );
                            float dist2 = (intlPos - nodePos).sqrMagnitude;
                            float range2 = (range * range);
                            if (dist2 > range2) continue;
                            float innerRange = Mathf.Max(range - FadeStart, 0);
                            float innerRange2 = innerRange * innerRange;
                            float bright = 1;
                            if (dist2 > innerRange2) {
                                bright = Mathf.Clamp01((range - Mathf.Sqrt(dist2)) / (range - innerRange));
                            }
                            int p = wx + wy * losTexture.width;
                            if (dk > curMax) bright = bright * Mathf.Clamp01(0.75f - 0.5f * (dk - curMax) / 3);
                            pixels[p].r = (byte)Mathf.Max(pixels[p].r, (byte)(bright * los));
                            pixels[p].g = (byte)Mathf.Max(pixels[p].g, (byte)(bright * fow));
                            pixels[p].b = (byte)Mathf.Max(pixels[p].b, (byte)(Mathf.Clamp(bright * grayscale, 0, 255)));
                        }
                        if (dk > curMax) break;
                    }
                    prevMax = curMax;
                }
            }
            return;
        }*/
        {
            int rangeI = Mathf.CeilToInt(range * Scale);
            int rangeIE = Mathf.CeilToInt((range + Mathf.Max(rect.width, rect.height)) * Scale);
            int xMin, yMin, xMax, yMax;
            GetExtents(rect, range, out xMin, out yMin, out xMax, out yMax);
            for (int y = yMin; y <= yMax; ++y) {
                int dy = Mathf.Max(Mathf.Min(yMax - y, y - yMin), 0);
                float fy = (y + 0.5f) / Scale;
                float yIntl = Mathf.Clamp(fy, rect.yMin, rect.yMax);
                int inset = rangeI - GetColumnSize(rangeI - dy - 1, rangeI) - 1;
                for (int x = xMin + inset; x <= xMax - inset; ++x) {
                    float fx = (x + 0.5f) / Scale;
                    var nodePos = new Vector2(fx, fy);
                    var intlPos = new Vector2(Mathf.Clamp(fx, rect.xMin, rect.xMax), yIntl);
                    float dist2 = (intlPos - nodePos).sqrMagnitude;
                    float range2 = (range * range);
                    if (dist2 > range2) continue;
                    float innerRange = Mathf.Max(range - FadeStart, 0);
                    float innerRange2 = innerRange * innerRange;
                    float bright = 1;
                    if (dist2 > innerRange2) {
                        bright = Mathf.Clamp01((range - Mathf.Sqrt(dist2)) / (range - innerRange));
                    }
                    int p = x + y * losTexture.width;
                    pixels[p].r = (byte)Mathf.Max(pixels[p].r, bright * los);
                    pixels[p].g = (byte)Mathf.Max(pixels[p].g, bright * fow);
                    pixels[p].b = (byte)Mathf.Max(pixels[p].b, Mathf.Clamp(bright * grayscale, 0, 255));
                }
            }
        }
    }

    private bool MoveBack(ref int blockId, ref int overlap, float ang) {
        var block = allNodes[blockId];
        --blockId;
        if (blockId < 0) blockId += allNodes.Count;
        var prev = allNodes[blockId];
        --overlap;

        if (IsAngleBetween(ang, ref prev)) {
            overlap = Mathf.Max(overlap, prev.Overlap);
            return true;
        }
        return false;
    }
    // Only called when blockId is to the left of ang
    // (ie. moving from revealedFrom to revealedTo)
    // Go until blockId is past ang
    private void MoveAngleTo(ref int blockId, float ang) {
        if (ang > 8) ang -= 8;
        List<Node> nodes = allNodes;
        int startId = blockId;
        int count = 0;
        while (true) {
            count++;
            var prev = allNodes[blockId];
            ++blockId;
            if (blockId >= nodes.Count) blockId -= nodes.Count;
            if (startId == blockId) break;
            var next = allNodes[blockId];
            var nextAng = next.StartAngle;
            if (IsAngleGreater(prev.StartAngle, nextAng, ang)) break;
        }
    }

    // First angle is sanitised
    private float DeltaAngleS1(float epoch, float a1, float a2) {
        if (a2 < epoch) a2 += 8;
        return a2 - a1;
    }
    // Second angle is sanitised
    private float DeltaAngleS2(float epoch, float a1, float a2) {
        if (a1 < epoch) a1 += 8;
        return a2 - a1;
    }
    private float DeltaAngle(float epoch, float a1, float a2) {
        if (a1 < epoch) a1 += 8;
        if (a2 < epoch) a2 += 8;
        return a2 - a1;
    }
    private bool IsAngleGreater(float epoch, float a1, float a2) {
        return DeltaAngle(epoch, a1, a2) <= 0;
    }

    private float ForwardAngle(float from, float dir, float to) {
        if (to * dir > (from) * dir + 8) to -= 8 * dir;
        else if (to * dir < from * dir) to += 8 * dir;
        return to;
    }

    private float AngleDistance(float from, float to) {
        if (from < to - 4) from += 8;
        else if (from > to + 4) from -= 8;
        return to - from;
    }

    private bool IsAngleBetween(float ang, ref Node node) {
        if (ang < node.StartAngle) ang += 8;
        return ang >= node.StartAngle && ang <= node.EndAngle;
    }

    private int GetColumnSize(int colId, int range) {
        int x = Mathf.Min(range, range * 3 / 2 - colId);
        while (x > -range && x * x + colId * colId > range * range) --x;
        return x;
    }
    private void UpdateColumnCache(int c, int dir, Vector2 locCtr) {
        if (columns[c] > columnsMax[c]) colAngCache[c] = float.NaN;
        else {
            var reqXPerY = GetStartAngle(columns[c] * dir, c * dir, locCtr);
            if (dir > 0 && reqXPerY >= 4) reqXPerY -= 8;
            colAngCache[c] = reqXPerY;
        }
    }
    private int GetNextColumn(int rangeI, int dir, Vector2 locCtr) {
        float nextXPY = float.PositiveInfinity;
        int nextCol = -1;
        for (int c = 0; c < rangeI; ++c) {
            if (colAngCache[c] < nextXPY) {
                nextXPY = colAngCache[c];
                nextCol = c;
            }
        }
        return nextCol;
    }

    // Notify that the terrain heights are no longer valid
    public void InvalidateTerrainHeightsCache() {
        terrainHeightsCache = null;
        blockHeights = null;
    }


    // Get states and colours for entities
    public Color32 GetFOWColor(Vector2 pos) {
        int x = Mathf.RoundToInt(pos.x * Scale),
            y = Mathf.RoundToInt(pos.x * Scale);
        int p = x + y * losTexture.width;
        return pixels[p];
    }
    public Color32 GetFOWColor(Rect rect) {
        int xMin, yMin, xMax, yMax;
        GetExtents(rect, 0, out xMin, out yMin, out xMax, out yMax);
        Color32 color = new Color32(0, 0, 0, 0);
        for (int y = yMin; y <= yMax; ++y) {
            for (int x = xMin; x <= xMax; ++x) {
                int p = x + y * losTexture.width;
                color.r = (byte)Mathf.Max(color.r, pixels[p].r);
                color.g = (byte)Mathf.Max(color.g, pixels[p].g);
                color.b = (byte)Mathf.Max(color.b, pixels[p].b);
                color.a = (byte)Mathf.Max(color.a, pixels[p].a);
            }
        }
        return color;
    }
    public LOSEntity.RevealStates GetRevealFromFOW(Color32 px) {
        if (px.r >= 128) return LOSEntity.RevealStates.Unfogged;
        if (px.g >= 128) return LOSEntity.RevealStates.Fogged;
        return LOSEntity.RevealStates.Hidden;
    }
    public LOSEntity.RevealStates IsVisible(Vector2 pos) {
        return GetRevealFromFOW(GetFOWColor(pos));
    }
    public LOSEntity.RevealStates IsVisible(Rect rect) {
        return GetRevealFromFOW(GetFOWColor(rect));
    }
    public Color32 GetQuantizedFOW(Color32 px) {
        if (px.r >= 128) {
            px.r = px.g = px.b = 255;
        } else {
            px.r = 0;
            px.g = px.g < 128 ? (byte)0 : (byte)255;
        }
        return px;
    }


    // Allow entities to tell us when theyre added
    public static void AddEntity(LOSEntity entity) {
        if (Instance != null && !Instance.Entities.Contains(entity)) Instance.Entities.Add(entity);
    }
    public static void RemoveEntity(LOSEntity entity) {
        if (Instance != null) Instance.Entities.Remove(entity);
    }


    // A singleton instance of this class
    private static LOSManager _instance;
    public static LOSManager Instance {
        get {
            if (_instance == null) _instance = GameObject.FindObjectOfType<LOSManager>();
            return _instance;
        }
    }

}
