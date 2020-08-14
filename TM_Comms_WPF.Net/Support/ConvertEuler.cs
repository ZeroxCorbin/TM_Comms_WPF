using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM_Comms_WPF
{
    public class ConvertEuler
    {
        public static CPose QuaternionToEuler(Pose q)
        {
            CPose euler = new CPose();

            // if the input quaternion is normalized, this is exactly one. Otherwise, this acts as a correction factor for the quaternion's not-normalizedness
            double unit = (q.Qx * q.Qx) + (q.Qy * q.Y) + (q.Qz * q.Qz) + (q.Qw * q.Qw);

            // this will have a magnitude of 0.5 or greater if and only if this is a singularity case
            double test = q.Qx * q.Qw - q.Qy * q.Qz;

            if (test > 0.4995f * unit) // singularity at north pole
            {
                euler.X = Math.PI / 2;
                euler.Y = 2f * Math.Atan2(q.Qy, q.Qx);
                euler.Z = 0;
            }
            else if (test < -0.4995f * unit) // singularity at south pole
            {
                euler.X = -Math.PI / 2;
                euler.Y = -2f * Math.Atan2(q.Qy, q.Qx);
                euler.Z = 0;
            }
            else // no singularity - this is the majority of cases
            {
                euler.X = Math.Asin(2f * (q.Qw * q.Qx - q.Qy * q.Qz));
                euler.Y = Math.Atan2(2f * q.Qw * q.Qy + 2f * q.Qz * q.Qx, 1 - 2f * (q.Qx * q.Qx + q.Qy * q.Qy));
                euler.Z = Math.Atan2(2f * q.Qw * q.Qz + 2f * q.Qx * q.Qy, 1 - 2f * (q.Qz * q.Qz + q.Qx * q.Qx));
            }

            // all the math so far has been done in radians. Before returning, we convert to degrees...
            euler.X *= (Math.PI / 180);
            euler.Y *= (Math.PI / 180);
            euler.Z *= (Math.PI / 180);

            //...and then ensure the degree values are between 0 and 360
            euler.X %= 360;
            euler.Y %= 360;
            euler.Z %= 360;

            return euler;
        }

        public static Pose EulerToQuaternion(CPose euler)
        {
            double xOver2 = euler.X * (Math.PI / 180) * 0.5f;
            double yOver2 = euler.Y * (Math.PI / 180) * 0.5f;
            double zOver2 = euler.Z * (Math.PI / 180) * 0.5f;

            double sinXOver2 = Math.Sin(xOver2);
            double cosXOver2 = Math.Cos(xOver2);
            double sinYOver2 = Math.Sin(yOver2);
            double cosYOver2 = Math.Cos(yOver2);
            double sinZOver2 = Math.Sin(zOver2);
            double cosZOver2 = Math.Cos(zOver2);

            return new Pose(euler.X, euler.Y, euler.Z,
                            cosYOver2 * sinXOver2 * cosZOver2 + sinYOver2 * cosXOver2 * sinZOver2,
                            sinYOver2 * cosXOver2 * cosZOver2 - cosYOver2 * sinXOver2 * sinZOver2,
                            cosYOver2 * cosXOver2 * sinZOver2 - sinYOver2 * sinXOver2 * cosZOver2,
                            cosYOver2 * cosXOver2 * cosZOver2 + sinYOver2 * sinXOver2 * sinZOver2,
                            0);
        }

    }

}
