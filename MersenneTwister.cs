namespace RandomNumberApp;

public class MersenneTwister
{
    const int N = 624;
    const int M = 397;
    const uint MATRIX_A = 0x9908b0dfU;
    const uint UPPER_MASK = 0x80000000U;
    const uint LOWER_MASK = 0x7fffffffU;

    private readonly uint[] mt = new uint[N];
    private int mti = N + 1;

    public MersenneTwister(uint seed)
    {
        mt[0] = seed;

        for (mti = 1; mti < N; mti++)
        {
            mt[mti] =
                (uint)(
                    1812433253U *
                    (mt[mti - 1] ^
                    (mt[mti - 1] >> 30))
                    + mti);
        }
    }

    public uint NextUInt()
    {
        uint y;

        uint[] mag01 = { 0x0U, MATRIX_A };

        if (mti >= N)
        {
            int kk;

            for (kk = 0; kk < N - M; kk++)
            {
                y =
                    (mt[kk] & UPPER_MASK) |
                    (mt[kk + 1] & LOWER_MASK);

                mt[kk] =
                    mt[kk + M] ^
                    (y >> 1) ^
                    mag01[y & 1];
            }

            for (; kk < N - 1; kk++)
            {
                y =
                    (mt[kk] & UPPER_MASK) |
                    (mt[kk + 1] & LOWER_MASK);

                mt[kk] =
                    mt[kk + (M - N)] ^
                    (y >> 1) ^
                    mag01[y & 1];
            }

            y =
                (mt[N - 1] & UPPER_MASK) |
                (mt[0] & LOWER_MASK);

            mt[N - 1] =
                mt[M - 1] ^
                (y >> 1) ^
                mag01[y & 1];

            mti = 0;
        }

        y = mt[mti++];

        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;
        y ^= y >> 18;

        return y;
    }

    public int Next(int maxValue)
    {
        return (int)(NextUInt() % maxValue);
    }
}
