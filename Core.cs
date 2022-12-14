using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;
using Sylvan.Data.Csv;

namespace Streamlines
{
    internal sealed class Core
    {
        public static PointCloud CreateProbePoints(int size)
        {
            PointCloud probes = new PointCloud();

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    for (int z = 0; z < size; z++)
                        probes.Add(new Point3d(x, y, z));

            return probes;
        }

        public static Vector3d[] CreateVelocityVectors(int size)
        {
            Vector3d[] velocityVectors = new Vector3d[size * size * size];

            int i = 0;
            Random random = new Random();

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    for (int z = 0; z < size; z++)
                        velocityVectors[i++] = new Vector3d(1 + random.NextDouble() - 0.7, random.NextDouble() - 0.5, random.NextDouble() - 0.5);

            return velocityVectors;
        }

        public static PointCloud ReadProbeLocations(string filePath)
        {
            PointCloud probes = new PointCloud();
            var csv = CsvDataReader.Create(filePath);
            using (csv)
            {
                while (csv.Read())
                {
                    var x = csv.GetDouble(0);
                    var y = csv.GetDouble(1);
                    var z = csv.GetDouble(2);
                    probes.Add(new Point3d(x, y, z));
                }
            }
            return probes;
        }

    }
}
