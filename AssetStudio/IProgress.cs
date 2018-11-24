using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public interface IProgress
    {
        void Report(int value);
    }

    public sealed class DummyProgress : IProgress
    {
        public void Report(int value) { }
    }
}
