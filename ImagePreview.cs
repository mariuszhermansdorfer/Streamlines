using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Streamlines
{
    public class ImagePreview : GH_PersistentParam<GH_String>
    {
        public Image Image;
        public double ImageRatio;
        public string ImagePath;
        public ImagePreview() : base(new GH_InstanceDescription("ImagePreview",
            "ImagePreview", "ImagePreview", "Category", "subcategory"))
        {
            Optional = true;
        }
        public override void CreateAttributes()
        {
            m_attributes = new ResizableComponentAttributes(this);
        }
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("6b5273e5-e6b6-4a08-a5b8-4cbf5c7f7cb1");
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            //base.AppendAdditionalMenuItems(menu);
            ToolStripMenuItem selectFile = new ToolStripMenuItem("Select one existing file", null, new EventHandler(this.Menu_SelectFileClicked));
            ToolStripMenuItem clearFile = new ToolStripMenuItem("Clear image file", null, new EventHandler(this.Menu_ClearFileClicked));
            menu.Items.Add(selectFile);
            menu.Items.Add(clearFile);
        }

        private void Menu_SelectFileClicked(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select one existing file";
            openFileDialog.Filter = "All files|*.*";
            openFileDialog.CheckFileExists = false;
            DialogResult dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                base.RecordPersistentDataEvent("Select one existing file");
                base.PersistentData.Clear();
                base.PersistentData.Append(new GH_String(openFileDialog.FileName));
                base.OnObjectChanged(GH_ObjectEventType.PersistentData);
                this.Sources.Clear();
                this.ExpireSolution(true);
            }
        }
        private void Menu_ClearFileClicked(object sender, EventArgs e)
        {
            Image = null;
            ImagePath = "";
            base.RecordPersistentDataEvent("Clear image file");
            base.PersistentData.Clear();
            base.OnObjectChanged(GH_ObjectEventType.PersistentData);
            RemoveAllSources();
            ExpireSolution(true);
        }
        protected override GH_GetterResult Prompt_Singular(ref GH_String value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GH_String> value)
        {
            return GH_GetterResult.cancel;
        }

        protected override void OnVolatileDataCollected()
        {
            if (VolatileDataCount > 0)
            {
                ImagePath = VolatileData.get_Branch(0)[0].ToString();
                Image = Image.FromFile(ImagePath);
                ImageRatio = (float)Image.Height / (float)Image.Width;
            }
        }
    }
    public class ResizableComponentAttributes : GH_ResizableAttributes<ImagePreview>
    {
        public ResizableComponentAttributes(ImagePreview owner) : base(owner)
        {
            Bounds = new Rectangle(0, 0, 150, 150);
        }
        public override bool HasOutputGrip => true;
        protected override Size MinimumSize => new Size(50, 50);
        protected override Padding SizingBorders => new Padding(2);
        protected override void Layout()
        {
            if (Owner.Image == null)
                Bounds = new RectangleF(Pivot.X, Pivot.Y, 150, 150);
            else
                Bounds = new RectangleF(Pivot.X, Pivot.Y, (float)Bounds.Width, (float)(Bounds.Width * Owner.ImageRatio));
        }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel == GH_CanvasChannel.Wires)
            {
                RenderIncomingWires(canvas.Painter, Owner.Sources, Owner.WireDisplay);
            }
            else if (channel == GH_CanvasChannel.Objects)
            {
                var capsule = GH_Capsule.CreateCapsule(Bounds, GH_Palette.Normal, 0, 0);
                capsule.AddInputGrip(InputGrip.Y);
                capsule.AddOutputGrip(OutputGrip.Y);
                capsule.SetJaggedEdges(false, false);
                capsule.Render(graphics, Selected, Owner.Locked, true);
                capsule.Dispose();

                if (Owner.Locked || Owner.Image == null)
                    return;

                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                graphics.DrawImage(Owner.Image, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
            }
        }
    }
}