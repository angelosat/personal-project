using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public static class Generator
    {
        private static int length;
        public static double[,] noise;
        public static double[,,] Noise3D;

        private static Random rand;

        private static double Interpolate4(double a, double b, double t)
        {
            double r = t * Math.PI;
            t = (1 - Math.Cos(r)) * .5;
            return a + t * (b - a);
        }
        

        public static void InitNoise(int w, int h)
        {
            rand = new Random();
            noise = new double[w, h];
            length = w;
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    noise[i, j] = (double)rand.Next(100) / 100;
                }
            }
        }

        public static void InitNoise3D(int w, int h, int d)
        {
            Noise3D = new double[w,h,d];
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    for (int k = 0; k < d; k++)
                        Noise3D[i, j, k] = Engine.Map.Random.NextDouble() * 2 - 1;
        }

        private static readonly int[] perm = new int[512];
        public static void InitGradient3()
        {
            for (int i = 0; i < 512; i++) 
                perm[i] = p[i & 255];
        }

        private static readonly Vector3[] grad3 = new Vector3[] {new Vector3(1,1,0),new Vector3 (-1,1,0),new Vector3(1,-1,0),new Vector3(-1,-1,0),
                                        new Vector3(1,0,1),new Vector3(-1,0,1),new Vector3(1,0,-1),new Vector3(-1,0,-1),
                                        new Vector3(0,1,1),new Vector3(0,-1,1),new Vector3(0,1,-1),new Vector3(0,-1,-1)};

        private static int[] p = {151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180};


        public static byte Perm(byte seed)
        {
            return (byte)perm[(int)seed];
        }
        public static byte[] Shuffle(byte[] seedArray, int times)
        {
            byte[] newseed = new byte[seedArray.Length];
            seedArray.CopyTo(newseed, 0);
            for (int i = 0; i < times; i++)
                for (int k = 0; k < newseed.Length; k++)
                    newseed[k] = (byte)perm[(int)newseed[k]];
            return newseed;
        }

        

        public static double Perlin3Db(int x, int y, int z, int frequency, byte[] seedarray)
        {
            int f = frequency;
            //int i0 = x / f, j0 = y / f, k0 = z / f;
            //int i1 = i0 + 1, j1 = j0 + 1, k1 = k0 + 1;
            Vector3 d000, d001, d010, d011, d100, d101, d110, d111;

            int _x = x % f;
            int _y = y % f;
            int _z = z % f;
            if (_x < 0)
                _x += f;
            if (_y < 0)
                _y += f;
            if (_z < 0)
                _z += f;

            double tii = _x / (float)f;
            double tjj = _y / (float)f;
            double tkk = _z / (float)f;


            d000 = new Vector3((float)tii, (float)tjj, (float)tkk);
            d001 = new Vector3((float)tii, (float)tjj, (float)tkk - 1);
            d010 = new Vector3((float)tii, (float)tjj - 1, (float)tkk);
            d011 = new Vector3((float)tii, (float)tjj - 1, (float)tkk - 1);
            d100 = new Vector3((float)tii - 1, (float)tjj, (float)tkk);
            d101 = new Vector3((float)tii - 1, (float)tjj, (float)tkk - 1);
            d110 = new Vector3((float)tii - 1, (float)tjj - 1, (float)tkk);
            d111 = new Vector3((float)tii - 1, (float)tjj - 1, (float)tkk - 1);

            int xx = seedarray[0] + (int)Math.Floor(x / (float)f) & 255;
            int yy = seedarray[1] + (int)Math.Floor(y / (float)f) & 255;
            int zz = seedarray[2] + (int)Math.Floor(z / (float)f) & 255;

            int gi000 = perm[xx + perm[yy + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi001 = perm[xx + perm[yy + perm[zz + 1 + perm[seedarray[3]]]]] % 12;
            int gi010 = perm[xx + perm[yy + 1 + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi011 = perm[xx + perm[yy + 1 + perm[zz + 1 + perm[seedarray[3]]]]] % 12;
            int gi100 = perm[xx + 1 + perm[yy + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi101 = perm[xx + 1 + perm[yy + perm[zz + 1 + perm[seedarray[3]]]]] % 12;
            int gi110 = perm[xx + 1 + perm[yy + 1 + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi111 = perm[xx + 1 + perm[yy + 1 + perm[zz + 1 + perm[seedarray[3]]]]] % 12;

            float n000 = Vector3.Dot(grad3[gi000], d000);
            float n001 = Vector3.Dot(grad3[gi001], d001);
            float n010 = Vector3.Dot(grad3[gi010], d010);
            float n011 = Vector3.Dot(grad3[gi011], d011);
            float n100 = Vector3.Dot(grad3[gi100], d100);
            float n101 = Vector3.Dot(grad3[gi101], d101);
            float n110 = Vector3.Dot(grad3[gi110], d110);
            float n111 = Vector3.Dot(grad3[gi111], d111);

            double nx00, nx01, nx10, nx11, nxy0, nxy1, nxyz;

            double ti = 3 * Math.Pow(tii, 2) - 2 * Math.Pow(tii, 3);
            double tj = 3 * Math.Pow(tjj, 2) - 2 * Math.Pow(tjj, 3);
            double tk = 3 * Math.Pow(tkk, 2) - 2 * Math.Pow(tkk, 3);

            nx00 = Mix(n000, n100, ti);
            nx01 = Mix(n001, n101, ti);
            nx10 = Mix(n010, n110, ti);
            nx11 = Mix(n011, n111, ti);

            nxy0 = Mix(nx00, nx10, tj);
            nxy1 = Mix(nx01, nx11, tj);

            nxyz = Mix(nxy0, nxy1, tk);

            return nxyz;
        }

        /// <summary>
        /// returns a noise value between -1 and 1
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="frequency"></param>
        /// <param name="seedarray"></param>
        /// <returns></returns>
        public static double Perlin3D(int x, int y, int z, int frequency, byte[] seedarray)
        {
            int f = frequency;
            //int i0 = x / f, j0 = y / f, k0 = z / f;
            //int i1 = i0 + 1, j1 = j0 + 1, k1 = k0 + 1;
            float[] d000, d001, d010, d011, d100, d101, d110, d111;

            int _x = x % f;
            int _y = y % f;
            int _z = z % f;
            if (_x < 0)
                _x += f;
            if (_y < 0)
                _y += f;
            if (_z < 0)
                _z += f;

            double tii = _x / (float)f;
            double tjj = _y / (float)f;
            double tkk = _z / (float)f;

  
            d000 = new float[3]{(float)tii, (float)tjj, (float)tkk};
            d001 = new float[3]{(float)tii, (float)tjj, (float)tkk - 1};
            d010 = new float[3]{(float)tii, (float)tjj - 1, (float)tkk};
            d011 = new float[3]{(float)tii, (float)tjj - 1, (float)tkk - 1};
            d100 = new float[3]{(float)tii - 1, (float)tjj, (float)tkk};
            d101 = new float[3]{(float)tii - 1, (float)tjj, (float)tkk - 1};
            d110 = new float[3]{(float)tii - 1, (float)tjj - 1, (float)tkk};
            d111 = new float[3]{(float)tii - 1, (float)tjj - 1, (float)tkk - 1};

            int xx = seedarray[0] + (int)Math.Floor(x / (float)f) & 255;
            int yy = seedarray[1] + (int)Math.Floor(y / (float)f) & 255;
            int zz = seedarray[2] + (int)Math.Floor(z / (float)f) & 255;


            int gi000 = perm[xx + perm[yy + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi001 = perm[xx + perm[yy + perm[zz + 1 + perm[seedarray[3]]]]] % 12;
            int gi010 = perm[xx + perm[yy + 1 + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi011 = perm[xx + perm[yy + 1 + perm[zz + 1 + perm[seedarray[3]]]]] % 12;
            int gi100 = perm[xx + 1 + perm[yy + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi101 = perm[xx + 1 + perm[yy + perm[zz + 1 + perm[seedarray[3]]]]] % 12;
            int gi110 = perm[xx + 1 + perm[yy + 1 + perm[zz + perm[seedarray[3]]]]] % 12;
            int gi111 = perm[xx + 1 + perm[yy + 1 + perm[zz + 1 + perm[seedarray[3]]]]] % 12;


            float n000 = Dot(grad3[gi000], d000);
            float n001 = Dot(grad3[gi001], d001);
            float n010 = Dot(grad3[gi010], d010);
            float n011 = Dot(grad3[gi011], d011);
            float n100 = Dot(grad3[gi100], d100);
            float n101 = Dot(grad3[gi101], d101);
            float n110 = Dot(grad3[gi110], d110);
            float n111 = Dot(grad3[gi111], d111);

            double nx00, nx01, nx10, nx11, nxy0, nxy1, nxyz;


            double ti = 3 * Math.Pow(tii, 2) - 2 * Math.Pow(tii, 3);
            double tj = 3 * Math.Pow(tjj, 2) - 2 * Math.Pow(tjj, 3);
            double tk = 3 * Math.Pow(tkk, 2) - 2 * Math.Pow(tkk, 3);

            nx00 = Mix(n000, n100, ti);
            nx01 = Mix(n001, n101, ti);
            nx10 = Mix(n010, n110, ti);
            nx11 = Mix(n011, n111, ti);

            nxy0 = Mix(nx00, nx10, tj);
            nxy1 = Mix(nx01, nx11, tj);

            nxyz = Mix(nxy0, nxy1, tk);

            return nxyz;
        }

        static double Mix(double a, double b, double t)
        {
            return b * t + a * (1 - t);
        }

        static float Dot(Vector3 a, float[] b)
        {
            return a.X * b[0] + a.Y * b[1] + a.Z * b[2];
        }

        

        

        public static double Perlin(int i, int j, int octave_count, int dim, double persistence)
        {
            //octave_count = (int)Math.Log(dim, 2);
            double ti, tj, tl, tr, bl, br;
            double color = 0, amplitude, frequency;
            int o = octave_count, wavelength, sample_i0, sample_j0, sample_i1, sample_j1;
            length = dim;
            amplitude = 1;
            double total_amp = 0;
            for(int k=0; k<o; k++)
            {
                wavelength = 1 << k;
                frequency = 1 / wavelength;
                amplitude = Math.Pow(persistence, o-k);
                total_amp += amplitude;

                sample_i0 = (i / wavelength) * wavelength; //topleft corner of sample square
                //sample_i1 = (sample_i0 + wavelength) % length; // use this for edge warping
                sample_i1 = Math.Min(length - 1, (sample_i0 + wavelength));

                sample_j0 = (j / wavelength) * wavelength;
                //sample_j1 = (sample_j0 + wavelength) % length; // use this for edge warping
                sample_j1 = Math.Min(length - 1, (sample_j0 + wavelength));

                ti = (i - sample_i0) / (double)wavelength; //normalized distance from sample
                tj = (j - sample_j0) / (double)wavelength;

                tl = noise[sample_i0, sample_j0];
                tr = noise[sample_i1, sample_j0];
                bl = noise[sample_i0, sample_j1];
                br = noise[sample_i1, sample_j1];

                color += Interpolate4(Interpolate4(tl, tr, ti), Interpolate4(bl, br, ti), tj) * amplitude;
            }

            //normalisation
            for (int k = 0; k < o; k++)
            {
                color /= total_amp;
            }

            return color;
        }

        //private static double smoothNoise(int i, int j)
        //{
        //    int x1, y1, x2, y2;
        //    x1 = Math.Max(0, i - 1);
        //    x2 = Math.Min(i + 1, noise.GetUpperBound(0));
        //    y1 = Math.Max(0, j - 1);
        //    y2 = Math.Min(j + 1, noise.GetUpperBound(1));

        //    double corners = (noise[x1, y1] + noise[x2, y1] + noise[x1, y2] + noise[x2, y2]) / 16;
        //    double sides = (noise[x1, j] + noise[x2, j] + noise[i, y1] + noise[i, y2]) / 8;
        //    double center = noise[i, j] / 4;

        //    return corners + sides + center;
        //}
    }
}
