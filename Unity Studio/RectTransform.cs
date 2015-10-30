using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class RectTransform
    {
        public Transform m_Transform;

        public RectTransform(AssetPreloadData preloadData)
        {
            m_Transform = new Transform(preloadData);

            //var sourceFile = preloadData.sourceFile;
            //var a_Stream = preloadData.sourceFile.a_Stream;

            /*
            float[2] AnchorsMin
            float[2] AnchorsMax
            float[2] Pivod
            float Width
            float Height
            float[2] ?
            */

        }
    }
}
