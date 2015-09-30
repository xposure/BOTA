using System.Collections.Generic;
using Microsoft.Xna.Framework;
public static class GridRayTracer
{
    public static IEnumerable<Vector3> Trace(Vector3 start, Vector3 end)
    {
        return Trace(start.X, start.Y, start.Z, end.X, end.Y, end.Z);
    }

    public static IEnumerable<Vector3> Trace(double x0, double y0, double z0, double x1, double y1, double z1)
    {
        double dx = System.Math.Abs(x1 - x0);
        double dy = System.Math.Abs(y1 - y0);
        double dz = System.Math.Abs(z1 - z0);

        int x = (int)x0;
        int y = (int)y0;
        int z = (int)z0;

        double dt_dx = 1.0 / dx;
        double dt_dy = 1.0 / dy;
        double dt_dz = 1.0 / dz;

        double t = 0;

        int n = 1;
        int x_inc, y_inc, z_inc;
        double t_next_y, t_next_x, t_next_z;

        if (dx == 0)
        {
            x_inc = 0;
            t_next_x = dt_dx; // infinity
        }
        else if (x1 > x0)
        {
            x_inc = 1;
            n += (int)x1 - x;
            t_next_x = ((int)x0 + 1 - x0) * dt_dx;
        }
        else
        {
            x_inc = -1;
            n += x - (int)x1;
            t_next_x = (x0 - (int)x0) * dt_dx;
        }

        if (dy == 0)
        {
            y_inc = 0;
            t_next_y = dt_dy; // infinity
        }
        else if (y1 > y0)
        {
            y_inc = 1;
            n += (int)y1 - y;
            t_next_y = ((int)y0 + 1 - y0) * dt_dy;
        }
        else
        {
            y_inc = -1;
            n += y - (int)y1;
            t_next_y = (y0 - (int)y0) * dt_dy;
        }

        if (dz == 0)
        {
            z_inc = 0;
            t_next_z = dt_dz; // infinity
        }
        else if (z1 > z0)
        {
            z_inc = 1;
            n += (int)z1 - z;
            t_next_z = ((int)z0 + 1 - z0) * dt_dz;
        }
        else
        {
            z_inc = -1;
            n += z - (int)z1;
            t_next_z = (z0 - (int)z0) * dt_dz;
        }

        for (; n > 0; --n)
        {
            yield return new Vector3(x, y, z);
            //visit(x, y, z);

            if (t_next_x <= t_next_y && t_next_x <= t_next_z) // t_next_x is smallest
            {
                x += x_inc;
                t = t_next_x;
                t_next_x += dt_dx;
            }
            else if (t_next_y <= t_next_x && t_next_y <= t_next_z) // t_next_y is smallest
            {
                y += y_inc;
                t = t_next_y;
                t_next_y += dt_dy;
            }
            else // t_next_z is smallest
            {
                z += z_inc;
                t = t_next_z;
                t_next_z += dt_dz;
            }
        }
    }
}
