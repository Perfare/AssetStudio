namespace AssetStudio
{
    public static class Progress
    {
        public static IProgress Default = new DummyProgress();
        private static int preValue;

        public static void Reset()
        {
            preValue = 0;
            Default.Report(0);
        }

        public static void Report(int current, int total)
        {
            var value = (int)(current * 100f / total);
            Report(value);
        }

        private static void Report(int value)
        {
            if (value > preValue)
            {
                preValue = value;
                Default.Report(value);
            }
        }
    }
}
