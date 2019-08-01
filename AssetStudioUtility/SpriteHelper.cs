using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
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
                    return CutImage(m_Texture2D, m_Sprite, spriteAtlasData.textureRect, spriteAtlasData.textureRectOffset, spriteAtlasData.settingsRaw);
                }
            }
            else
            {
                if (m_Sprite.m_RD.texture.TryGet(out var m_Texture2D))
                {
                    return CutImage(m_Texture2D, m_Sprite, m_Sprite.m_RD.textureRect, m_Sprite.m_RD.textureRectOffset, m_Sprite.m_RD.settingsRaw);
                }
            }
            return null;
        }

        private static Bitmap CutImage(Texture2D m_Texture2D, Sprite m_Sprite, RectangleF textureRect, Vector2 textureRectOffset, SpriteSettings settingsRaw)
        {
            var texture2D = new Texture2DConverter(m_Texture2D);
            var originalImage = texture2D.ConvertToBitmap(false);
            if (originalImage != null)
            {
                using (originalImage)
                {
                    //var spriteImage = originalImage.Clone(textureRect, PixelFormat.Format32bppArgb);
                    var textureRectI = Rectangle.Round(textureRect);
                    var spriteImage = new Bitmap(textureRectI.Width, textureRectI.Height, PixelFormat.Format32bppArgb);
                    var destRect = new Rectangle(0, 0, textureRectI.Width, textureRectI.Height);
                    using (var graphic = Graphics.FromImage(spriteImage))
                    {
                        graphic.DrawImage(originalImage, destRect, textureRectI, GraphicsUnit.Pixel);
                    }
                    if (settingsRaw.packed == 1)
                    {
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

                        //Tight
                        if (settingsRaw.packingMode == SpritePackingMode.kSPMTight)
                        {
                            try
                            {
                                var triangles = GetTriangles(m_Sprite.m_RD);
                                var points = triangles.Select(x => x.Select(y => new PointF(y.X, y.Y)).ToArray());
                                using (var path = new GraphicsPath())
                                {
                                    foreach (var p in points)
                                    {
                                        path.AddPolygon(p);
                                    }
                                    using (var matr = new Matrix())
                                    {
                                        var version = m_Sprite.version;
                                        if (version[0] < 5
                                           || (version[0] == 5 && version[1] < 4)
                                           || (version[0] == 5 && version[1] == 4 && version[2] <= 1)) //5.4.1p3 down
                                        {
                                            matr.Translate(m_Sprite.m_Rect.Width * 0.5f - textureRectOffset.X, m_Sprite.m_Rect.Height * 0.5f - textureRectOffset.Y);
                                        }
                                        else
                                        {
                                            matr.Translate(m_Sprite.m_Rect.Width * m_Sprite.m_Pivot.X - textureRectOffset.X, m_Sprite.m_Rect.Height * m_Sprite.m_Pivot.Y - textureRectOffset.Y);
                                        }
                                        matr.Scale(m_Sprite.m_PixelsToUnits, m_Sprite.m_PixelsToUnits);
                                        path.Transform(matr);
                                        var bitmap = new Bitmap(textureRectI.Width, textureRectI.Height);
                                        using (var graphic = Graphics.FromImage(bitmap))
                                        {
                                            using (var brush = new TextureBrush(spriteImage))
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
                                // ignored
                            }
                        }
                    }

                    //Rectangle
                    spriteImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    return spriteImage;
                }
            }

            return null;
        }

        private static Vector2[][] GetTriangles(SpriteRenderData m_RD)
        {
            if (m_RD.vertices != null) //5.6 down
            {
                var vertices = m_RD.vertices.Select(x => (Vector2)x.pos).ToArray();
                var triangleCount = m_RD.indices.Length / 3;
                var triangles = new Vector2[triangleCount][];
                for (int i = 0; i < triangleCount; i++)
                {
                    var first = m_RD.indices[i * 3];
                    var second = m_RD.indices[i * 3 + 1];
                    var third = m_RD.indices[i * 3 + 2];
                    var triangle = new[] { vertices[first], vertices[second], vertices[third] };
                    triangles[i] = triangle;
                }
                return triangles;
            }

            return GetTriangles(m_RD.m_VertexData, m_RD.m_SubMeshes, m_RD.m_IndexBuffer); //5.6 and up
        }

        private static Vector2[][] GetTriangles(VertexData m_VertexData, SubMesh[] m_SubMeshes, byte[] m_IndexBuffer)
        {
            var triangles = new List<Vector2[]>();
            var m_Channel = m_VertexData.m_Channels[0]; //kShaderChannelVertex
            var m_Stream = m_VertexData.m_Streams[m_Channel.stream];
            using (BinaryReader vertexReader = new BinaryReader(new MemoryStream(m_VertexData.m_DataSize)),
                                indexReader = new BinaryReader(new MemoryStream(m_IndexBuffer)))
            {
                foreach (var subMesh in m_SubMeshes)
                {
                    vertexReader.BaseStream.Position = m_Stream.offset + subMesh.firstVertex * m_Stream.stride + m_Channel.offset;

                    var vertices = new Vector2[subMesh.vertexCount];
                    for (int v = 0; v < subMesh.vertexCount; v++)
                    {
                        vertices[v] = vertexReader.ReadVector3();
                        vertexReader.BaseStream.Position += m_Stream.stride - 12;
                    }

                    indexReader.BaseStream.Position = subMesh.firstByte;

                    var triangleCount = subMesh.indexCount / 3u;
                    for (int i = 0; i < triangleCount; i++)
                    {
                        var first = indexReader.ReadUInt16() - subMesh.firstVertex;
                        var second = indexReader.ReadUInt16() - subMesh.firstVertex;
                        var third = indexReader.ReadUInt16() - subMesh.firstVertex;
                        var triangle = new[] { vertices[first], vertices[second], vertices[third] };
                        triangles.Add(triangle);
                    }
                }
            }
            return triangles.ToArray();
        }
    }
}
