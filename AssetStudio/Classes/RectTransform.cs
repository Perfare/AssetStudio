using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public struct Rect
    {
        private float m_XMin;
        private float m_YMin;
        private float m_Width;
        private float m_Height;
        public Rect(float x, float y, float width, float height)
        {
            this.m_XMin = x;
            this.m_YMin = y;
            this.m_Width = width;
            this.m_Height = height;
        }

        public Rect(Vector2 position, Vector2 size)
        {
            this.m_XMin = position.X;
            this.m_YMin = position.Y;
            this.m_Width = size.X;
            this.m_Height = size.Y;
        }

        public Rect(Rect source)
        {
            this.m_XMin = source.m_XMin;
            this.m_YMin = source.m_YMin;
            this.m_Width = source.m_Width;
            this.m_Height = source.m_Height;
        }

        public static Rect zero =>
            new Rect(0f, 0f, 0f, 0f);
        public static Rect MinMaxRect(float xmin, float ymin, float xmax, float ymax) =>
            new Rect(xmin, ymin, xmax - xmin, ymax - ymin);

        public void Set(float x, float y, float width, float height)
        {
            this.m_XMin = x;
            this.m_YMin = y;
            this.m_Width = width;
            this.m_Height = height;
        }

    }


    public sealed class RectTransform : Transform
    {
        public Vector2 anchorMin { get; set; }
        public Vector2 anchorMax { get; set; }
        public Vector2 anchoredPosition { get; set; }
        public Vector2 sizeDelta { get; set; }
        public Vector2 pivot { get; set; }

        public RectTransform(ObjectReader reader) : base(reader)
        {
            anchorMin = reader.ReadVector2();
            anchorMax = reader.ReadVector2();
            anchoredPosition = reader.ReadVector2();
            sizeDelta = reader.ReadVector2();
            pivot = reader.ReadVector2();
        }
    }
}
