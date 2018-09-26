using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public class GameObjectTreeNode : TreeNode
    {
        public GameObject gameObject;

        public GameObjectTreeNode(GameObject gameObject)
        {
            if (gameObject != null)
            {
                this.gameObject = gameObject;
                Text = gameObject.m_Name;
            }
        }
    }
}
