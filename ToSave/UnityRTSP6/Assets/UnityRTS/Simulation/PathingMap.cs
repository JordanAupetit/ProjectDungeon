using UnityEngine;
using System.Collections;
using UnityRTS;

namespace Simulation {
    public class PathingChunk {
        public readonly int Size;
        public byte[,] Costs;
        public Point GridPosition;
        public PathingChunk(int size) {
            Size = size;
            Costs = new byte[size, size];
        }
    }

    public class PathingMap : MonoBehaviour {

        public int ChunkSize = 8;

        [HideInInspector]
        public Expandable2DGrid<PathingChunk> Chunks = new Expandable2DGrid<PathingChunk>();

        public Point WorldToMap(Vector2 pos) {
            return new Point(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
        }

        public Point WorldToChunk(Point world) {
            return new Point(
                (world.X + (world.X < 0 ? -ChunkSize + 1 : 0)) / ChunkSize,
                (world.Y + (world.Y < 0 ? -ChunkSize + 1 : 0)) / ChunkSize
            );
        }
        public Point WorldToChunk(Point world, out Point loc) {
            var grid = WorldToChunk(world);
            loc = world - grid * ChunkSize;
            return grid;
        }
        public Point WorldToChunk(Vector2 world) {
            return WorldToChunk(WorldToMap(world));
        }
        public Point WorldToChunk(Vector2 world, out Point loc) {
            return WorldToChunk(WorldToMap(world), out loc);
        }

        public PathingChunk RequireChunk(Point grid) {
            var chunk = Chunks.Get(grid);
            if (chunk == null) {
                chunk = new PathingChunk(ChunkSize) {
                    GridPosition = grid,
                };
                Chunks.Add(grid, chunk);
            }
            return chunk;
        }

        public void AddCost(Polygon polygon, Vector2 position) {
            int count = DeltaCost(polygon, position, 1);
            if (count == 0) Debug.LogWarning("Unable to add cost! " + polygon);
            //else Debug.Log("Adding cost " + rect + " added " + count);
        }
        public void RemoveCost(Polygon polygon, Vector2 position) {
            DeltaCost(polygon, position, -1);
        }
        public bool IsClear(Polygon polygon, Vector2 position) {
            return DeltaCost(polygon, position, 0) > 0;
        }

        public int DeltaCost(Polygon polygon, Vector2 position, int amount) {
            Point tlg, tll, brg, brl;
            float xMin, yMin, xMax, yMax;
            polygon.GetExtents(out xMin, out yMin, out xMax, out yMax);
            xMin += position.x; yMin += position.y;
            xMax += position.x; yMax += position.y;
            tlg = WorldToChunk(new Vector2(xMin, yMin), out tll);
            brg = WorldToChunk(new Vector2(xMax - 1, yMax - 1), out brl);

            int sum = 0;

            for (int cx = tlg.X; cx <= brg.X; ++cx) {
                for (int cy = tlg.Y; cy <= brg.Y; ++cy) {
                    var chunk = RequireChunk(new Point(cx, cy));
                    var tl = new Point(cx == tlg.X ? tll.X : 0, cy == tlg.Y ? tll.Y : 0);
                    var br = new Point(cx == brg.X ? brl.X : ChunkSize - 1, cy == brg.Y ? brl.Y : ChunkSize - 1);
                    for (int lx = tl.X; lx <= br.X; ++lx) {
                        for (int ly = tl.Y; ly <= br.Y; ++ly) {
                            var newValue = (byte)(chunk.Costs[lx, ly] + amount);
                            chunk.Costs[lx, ly] = newValue;
                            sum += newValue;
                        }
                    }
                }
            }

            return sum;
        }

    }

}
