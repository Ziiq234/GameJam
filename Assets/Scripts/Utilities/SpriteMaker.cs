using UnityEngine;

// Draws a few small pixel sprites in code, so we don't need extra image files.
public static class SpriteMaker
{
    // a 16x16 sword pointing up
    public static Sprite MakeSword(Color blade)
    {
        Texture2D texture = NewTexture(16, 16);
        Color edge = blade * 0.7f;
        edge.a = 1f;
        Color guard = new Color(0.85f, 0.65f, 0.2f);
        Color handle = new Color(0.4f, 0.25f, 0.1f);

        // blade
        for (int y = 5; y < 15; y++)
        {
            texture.SetPixel(7, y, blade);
            texture.SetPixel(8, y, edge);
        }
        texture.SetPixel(7, 15, blade);

        // guard
        for (int x = 4; x < 12; x++)
        {
            texture.SetPixel(x, 4, guard);
        }

        // handle and pommel
        for (int y = 1; y < 4; y++)
        {
            texture.SetPixel(7, y, handle);
            texture.SetPixel(8, y, handle);
        }
        texture.SetPixel(7, 0, guard);
        texture.SetPixel(8, 0, guard);

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.2f), 32);
    }

    // a small glowing ball for magic bolts
    public static Sprite MakeOrb(Color colour)
    {
        Texture2D texture = NewTexture(12, 12);

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 12; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(5.5f, 5.5f));

                if (distance < 2.5f)
                    texture.SetPixel(x, y, Color.white);
                else if (distance < 5.5f)
                    texture.SetPixel(x, y, colour);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 12, 12), new Vector2(0.5f, 0.5f), 32);
    }

    // a little festival lantern on a post (16x24)
    public static Sprite MakeLantern(Color glow)
    {
        Texture2D texture = NewTexture(16, 24);
        Color wood = new Color(0.35f, 0.22f, 0.12f);
        Color frame = new Color(0.2f, 0.15f, 0.1f);

        // post
        for (int y = 0; y < 14; y++)
        {
            texture.SetPixel(7, y, wood);
            texture.SetPixel(8, y, wood);
        }

        // lantern box with a glowing middle
        for (int x = 4; x < 12; x++)
        {
            for (int y = 14; y < 22; y++)
            {
                bool edge = x == 4 || x == 11 || y == 14 || y == 21;
                texture.SetPixel(x, y, edge ? frame : glow);
            }
        }

        // little roof
        for (int x = 5; x < 11; x++)
        {
            texture.SetPixel(x, 22, frame);
        }
        texture.SetPixel(7, 23, frame);
        texture.SetPixel(8, 23, frame);

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 24), new Vector2(0.5f, 0.1f), 24);
    }

    // a wooden signpost (16x20)
    public static Sprite MakeSignpost()
    {
        Texture2D texture = NewTexture(16, 20);
        Color wood = new Color(0.45f, 0.3f, 0.15f);
        Color dark = new Color(0.3f, 0.2f, 0.1f);

        for (int y = 0; y < 12; y++)
        {
            texture.SetPixel(7, y, dark);
            texture.SetPixel(8, y, dark);
        }

        for (int x = 1; x < 15; x++)
        {
            for (int y = 11; y < 18; y++)
            {
                bool edge = x == 1 || x == 14 || y == 11 || y == 17;
                texture.SetPixel(x, y, edge ? dark : wood);
            }
        }

        // pretend writing
        for (int x = 3; x < 12; x += 2)
        {
            texture.SetPixel(x, 14, dark);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 20), new Vector2(0.5f, 0.1f), 24);
    }

    private static Texture2D NewTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        // start fully see-through
        Color[] clear = new Color[width * height];
        texture.SetPixels(clear);
        return texture;
    }
}
