using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using static AssetStudio.Studio;

namespace AssetStudio
{
    static class SpriteHelper
    {
        public static Bitmap GetImageFromSprite(Sprite m_Sprite)
        {
            if (assetsfileList.TryGetPD(m_Sprite.m_SpriteAtlas, out var assetPreloadData))
            {
                var m_SpriteAtlas = new SpriteAtlas(assetPreloadData);
                var index = m_SpriteAtlas.guids.FindIndex(x => x == m_Sprite.first);

                if (index >= 0 && assetsfileList.TryGetPD(m_SpriteAtlas.textures[index], out assetPreloadData))
                {
                    try
                    {
                        if (m_Sprite.m_PhysicsShape.Length > 0)
                        {
                            return CutImage(assetPreloadData, m_SpriteAtlas.textureRects[index], m_Sprite);
                        }
                        return CutImage(assetPreloadData, m_SpriteAtlas.textureRects[index]);
                    }
                    catch
                    {
                        return CutImage(assetPreloadData, m_SpriteAtlas.textureRects[index]);
                    }
                }
            }
            else
            {
                if (assetsfileList.TryGetPD(m_Sprite.texture, out assetPreloadData))
                {
                    return CutImage(assetPreloadData, m_Sprite.textureRect);
                }
            }

            return null;
        }

        private static Bitmap CutImage(AssetPreloadData texture2DAsset, RectangleF textureRect)
        {
            var texture2D = new Texture2DConverter(new Texture2D(texture2DAsset, true));
            using (var originalImage = texture2D.ConvertToBitmap(false))
            {
                if (originalImage != null)
                {
                    var spriteImage = originalImage.Clone(textureRect, PixelFormat.Format32bppArgb);
                    spriteImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    return spriteImage;
                }
            }

            return null;
        }

        private static Bitmap CutImage(AssetPreloadData texture2DAsset, RectangleF textureRect, Sprite sprite)
        {
            var texture2D = new Texture2DConverter(new Texture2D(texture2DAsset, true));
            using (var originalImage = texture2D.ConvertToBitmap(false))
            {
                if (originalImage != null)
                {
                    var spriteImage = originalImage.Clone(textureRect, PixelFormat.Format32bppArgb);
                    using (var brush = new TextureBrush(spriteImage))
                    {
                        using (var path = new GraphicsPath())
                        {
                            foreach (var p in sprite.m_PhysicsShape)
                                path.AddPolygon(p);
                            using (var matr = new Matrix())
                            {
                                matr.Translate(sprite.m_Rect.Width * sprite.m_Pivot.X, sprite.m_Rect.Height * sprite.m_Pivot.Y);
                                matr.Scale(sprite.m_PixelsToUnits, sprite.m_PixelsToUnits);
                                path.Transform(matr);
                                var bitmap = new Bitmap((int)textureRect.Width, (int)textureRect.Height);
                                using (var graphic = Graphics.FromImage(bitmap))
                                {
                                    graphic.FillPath(brush, path);
                                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                                    return bitmap;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
