using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace AssetStudio
{
    public static class SpriteHelper
    {
        public static Bitmap GetImageFromSprite(Sprite m_Sprite)
        {
            if (m_Sprite.m_SpriteAtlas != null && m_Sprite.m_SpriteAtlas.TryGet(out var m_SpriteAtlas))
            {
                if (m_SpriteAtlas.m_RenderDataMap.TryGetValue(m_Sprite.m_RenderDataKey, out var spriteAtlasData) && spriteAtlasData.texture.TryGet(out var m_Texture2D))
                {
                    return CutImage(m_Texture2D, spriteAtlasData.textureRect, m_Sprite, spriteAtlasData.settingsRaw);
                }
            }
            else
            {
                if (m_Sprite.m_RD.texture.TryGet(out var m_Texture2D))
                {
                    return CutImage(m_Texture2D, m_Sprite.m_RD.textureRect, m_Sprite, m_Sprite.m_RD.settingsRaw);
                }
            }
            return null;
        }

        private static Bitmap CutImage(Texture2D m_Texture2D, RectangleF textureRect, Sprite m_Sprite, SpriteSettings settingsRaw)
        {
            var texture2D = new Texture2DConverter(m_Texture2D);
            var originalImage = texture2D.ConvertToBitmap(false);
            if (originalImage != null)
            {
                using (originalImage)
                {
                    var spriteImage = originalImage.Clone(textureRect, PixelFormat.Format32bppArgb);

                    //RotateAndFlip
                    switch (settingsRaw.packingRotation)
                    {
                        case SpritePackingRotation.kSPRFlipHorizontal:
                            spriteImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case SpritePackingRotation.kSPRFlipVertical:
                            spriteImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            break;
                        case SpritePackingRotation.kSPRRotate180:
                            spriteImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case SpritePackingRotation.kSPRRotate90:
                            spriteImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }

                    /* TODO Tight
                     * 2017之前没有PhysicsShape
                     * 5.6之前使用vertices
                     * 5.6需要使用VertexData
                     */
                    if (settingsRaw.packingMode == SpritePackingMode.kSPMTight && m_Sprite.m_PhysicsShape?.Length > 0) //Tight
                    {
                        try
                        {
                            using (var brush = new TextureBrush(spriteImage))
                            {
                                using (var path = new GraphicsPath())
                                {
                                    var points = m_Sprite.m_PhysicsShape.Select(x => x.Select(y => new PointF(y.X, y.Y)).ToArray());
                                    foreach (var p in points)
                                    {
                                        path.AddPolygon(p);
                                    }
                                    using (var matr = new Matrix())
                                    {
                                        matr.Translate(m_Sprite.m_Rect.Width * m_Sprite.m_Pivot.X, m_Sprite.m_Rect.Height * m_Sprite.m_Pivot.Y);
                                        matr.Scale(m_Sprite.m_PixelsToUnits, m_Sprite.m_PixelsToUnits);
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
                        catch
                        {
                            spriteImage = originalImage.Clone(textureRect, PixelFormat.Format32bppArgb);
                            spriteImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            return spriteImage;
                        }
                    }

                    //Rectangle
                    spriteImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    return spriteImage;
                }
            }

            return null;
        }
    }
}
