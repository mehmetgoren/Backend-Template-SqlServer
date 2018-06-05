namespace Server.Models
{
    using System.Collections.Generic;

    public sealed class TreeNode : ITreeObject<TreeNode>
    {
        public string label { get; set; }
        public object data { get; set; }
        public string icon { get; set; }
        public string expandedIcon { get; set; }
        public string collapsedIcon { get; set; }
        public List<TreeNode> children { get; set; }
        public bool? leaf { get; set; }
        public bool? expanded { get; set; }
        public string type { get; set; }
        public TreeNode parent { get; set; }
        public bool? partialSelected { get; set; }
        // public string style { get; set; }
        // public string styleClass { get; set; }

        //extended for checkBox
        public bool @checked { get; set; }

        int ITreeObject.Id => ((ITreeObject)this.data).Id;

        int? ITreeObject.ParentId => ((ITreeObject)this.data).ParentId;

        ICollection<TreeNode> ITreeObject<TreeNode>.Children
        {
            get { return this.children; }
            set
            {
                if (null == value)
                    this.children = null;
                else
                {
                    if (null == this.children)
                        this.children = new List<TreeNode>();
                    else
                        this.children.Clear();
                    this.children.AddRange(value);
                }
            }
        }
    }
}
