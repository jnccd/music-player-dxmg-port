using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicPlayerDXMonoGamePort
{
    public class GigaFloatList
    {
        List<float>[] List;
        const long LIST_LENGTH = 67108863;
        long count;
        public long Count
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Creates a buffer for whole-song wave data. <paramref name="expectedCount"/> is the number of
        /// floats that are going to be added (the caller knows the song length) and sizes the chunks
        /// accordingly, so they do not have to grow by doubling - which copies the buffer over and over
        /// and peaks at almost twice the final size. Without a hint the chunks grow from a minimal
        /// capacity.
        /// </summary>
        public GigaFloatList(long expectedCount = 0)
        {
            List = new List<float>[1000];
            // The first chunk used to be created with a full LIST_LENGTH capacity (67,108,863 floats =
            // 256 MB) and was then immediately replaced by the loop below, so every song allocated and
            // dropped a 256 MB array on the large object heap. Give each chunk the capacity it is
            // actually going to need instead (a song longer than one chunk gets several, all sized).
            long remaining = expectedCount > 0 ? expectedCount : 0;
            for (int i = 0; i < List.Length; i++)
            {
                int capacity = 1;
                if (remaining > 0)
                {
                    capacity = (int)Math.Min(remaining, LIST_LENGTH);
                    remaining -= capacity;
                }
                List[i] = new List<float>(capacity);
            }
            count = 0;
        }

        public void Add(float item)
        {
            lock (List)
            {
                int ListIndex = 0;
                foreach (List<float> L in List)
                {
                    if (L.Count >= LIST_LENGTH)
                        ListIndex++;
                    else
                        break;
                }

                count++;
                List[ListIndex].Add(item);
            }
        }
        public float Get(long index)
        {
            long ListIndex = index / LIST_LENGTH;
            long Index = index % LIST_LENGTH;
            return List[(int)ListIndex][(int)Index];
        }
        public bool Exist(long index)
        {
            long ListIndex = index / LIST_LENGTH;
            long Index = index % LIST_LENGTH;
            return List[(int)ListIndex].Count > Index;
        }
        public float[] GetRange(long startIndex, long count)
        {
            if (Exist(startIndex) && Exist(startIndex + count))
            {
                List<float> Temp = new List<float>();
                for (long i = startIndex; i < startIndex + count; i++)
                    Temp.Add(Get(i));
                return Temp.ToArray();
            }
            else
                return null;
        }

        /// <summary>
        /// Copies <paramref name="count"/> values starting at <paramref name="startIndex"/> into
        /// <paramref name="destination"/> (at <paramref name="destinationOffset"/>), returning false when
        /// the requested range is not available. Same range check as <see cref="GetRange"/>, but without
        /// allocating a temporary list and array - callers that need a window every frame (the
        /// visualization reads one per video frame) would otherwise churn ~128 KB of garbage per frame.
        /// </summary>
        public bool CopyRange(long startIndex, float[] destination, int destinationOffset, int count)
        {
            if (!Exist(startIndex) || !Exist(startIndex + count))
                return false;

            for (int i = 0; i < count; i++)
                destination[destinationOffset + i] = Get(startIndex + i);
            return true;
        }
        public void updateCount()
        {
            count = 0;
            for (int i = 0; i < List.Length; i++)
                count += List[i].Count;
        }
    }
}
