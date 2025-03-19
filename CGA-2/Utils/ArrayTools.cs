namespace CGA2.Utils
{
    public class ArrayTools
    {
        public static int BinarySearch(List<Range> meshes, int item)
        {
            int low = 0;
            int high = meshes.Count - 1;

            while (low <= high)
            {
                int mid = low + ((high - low) >> 1);

                if (item > meshes[mid].End.Value)
                    low = mid + 1;
                else if (item < meshes[mid].Start.Value)
                    high = mid - 1;
                else
                    return mid;
            }

            return -1;
        }
    }
}
