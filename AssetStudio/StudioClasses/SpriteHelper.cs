using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    static class SpriteHelper
    {
        public static Bitmap GetImageFromSprite(Sprite m_Sprite)
        {
            Bitmap bitmap = null;
            SpriteSettings settingsRaw = null;
            if (m_Sprite.m_SpriteAtlas != null && m_Sprite.m_SpriteAtlas.TryGet(out var objectReader))
            {
                var m_SpriteAtlas = new SpriteAtlas(objectReader);
                var spriteAtlasData = m_SpriteAtlas.m_RenderDataMap.FirstOrDefault(x => x.Key.Item1 == m_Sprite.first).Value;
                if (spriteAtlasData != null && spriteAtlasData.texture.TryGet(out objectReader))
                {
                    settingsRaw = spriteAtlasData.settingsRaw;
                    try
                    {
                        if (settingsRaw.packingMode == SpritePackingMode.kSPMTight && m_Sprite.m_PhysicsShape.Length > 0) //Tight
                        {
                            bitmap = CutTightImage(objectReader, spriteAtlasData.textureRect, m_Sprite);
                        }
                        else
                        {
                            bitmap = CutRectangleImage(objectReader, spriteAtlasData.textureRect);
                        }
                    }
                    catch
                    {
                        bitmap = CutRectangleImage(objectReader, spriteAtlasData.textureRect);
                    }
                }
            }
            else
            {
                //TODO Tight
                if (m_Sprite.texture.TryGet(out objectReader))
                {
                    settingsRaw = m_Sprite.settingsRaw;
                    bitmap = CutRectangleImage(objectReader, m_Sprite.textureRect);
                }
            }
            if (bitmap != null)
            {
                RotateAndFlip(bitmap, settingsRaw.packingRotation);
                return bitmap;
            }
            return null;
        }

        private static Bitmap CutRectangleImage(ObjectReader texture2DAsset, RectangleF textureRect)
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

        private static Bitmap CutTightImage(ObjectReader texture2DAsset, RectangleF textureRect, Sprite sprite)
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

        private static void RotateAndFlip(Bitmap bitmap, SpritePackingRotation packingRotation)
        {
            switch (packingRotation)
            {
                case SpritePackingRotation.kSPRFlipHorizontal:
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
                case SpritePackingRotation.kSPRFlipVertical:
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    break;
                case SpritePackingRotation.kSPRRotate180:
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case SpritePackingRotation.kSPRRotate90:
                    bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
        }
    }
}
