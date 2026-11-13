using UnityEngine;
using UnityEngine.Tilemaps;

// Goes on the Ground tilemap. When the game starts it makes the water
// tiles solid and the land tiles walkable, so the player can't walk on water.
public class WaterCollision : MonoBehaviour
{
    [SerializeField] private TileBase waterTile;

    private void Awake()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(cell);

            if (tile == null)
                continue;

            if (tile == waterTile)
                tilemap.SetColliderType(cell, Tile.ColliderType.Grid);
            else
                tilemap.SetColliderType(cell, Tile.ColliderType.None);
        }

        // merge all the water into one big collider so it's quicker
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        gameObject.AddComponent<CompositeCollider2D>();

        TilemapCollider2D tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
    }
}
