using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using System;
using System.Drawing;
using System.Windows.Forms;


namespace Resizable
{
    public class ImagePreview : GH_Param<GH_String>
    {
        public Image Image;
        public double ImageRatio;
        public ImagePreview() : base(new GH_InstanceDescription("Resizable",
            "Resizable", "Resizable Component", "Category", "subcategory"))
        {
            Image = Image.FromFile("C:\\Users\\MRHE\\OneDrive - Ramboll\\Desktop\\ada.jpg");
            ImageRatio = Image.Height / Image.Width;
        }
        public override void CreateAttributes()
        {
            m_attributes = new ResizableComponentAttributes(this);
        }
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("6b5273e5-e6b6-4a08-a5b8-4cbf5c7f7cb1");
    }
    public class ResizableComponentAttributes : GH_ResizableAttributes<ImagePreview>
    {
        public ResizableComponentAttributes(ImagePreview owner) : base(owner)
        {
            Bounds = new Rectangle(0, 0, 150, 150);
        }
        public override bool HasOutputGrip => true;
        protected override Size MinimumSize => new Size(50, 50);
        protected override Padding SizingBorders => new Padding(6);
        protected override void Layout()
        {
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

                graphics.DrawImage(Owner.Image, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
            }
        }
    }
}