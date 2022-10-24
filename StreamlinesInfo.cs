using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Streamlines
{
    public class StreamlinesInfo : GH_AssemblyInfo
    {
        public override string Name => "Streamlines";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("A28FBB74-6F2A-4B0A-AC24-AB419A76D827");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}