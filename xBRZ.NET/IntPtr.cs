namespace xBRZ.NET
{
    public class IntPtr
    {
        private int[] _arr;
        private int _ptr;

        public IntPtr(int[] intArray)
        {
            _arr = intArray;
        }

        public void Position(int position)
        {
            _ptr = position;
        }

        public int Get()
        {
            return _arr[_ptr];
        }

        public void Set(int val)
        {
            _arr[_ptr] = val;
        }
    }
}
