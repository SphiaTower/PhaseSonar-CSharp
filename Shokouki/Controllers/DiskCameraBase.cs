using System;
using System.IO;

namespace Shokouki.Controllers
{
    public abstract class DiskCameraBase<T> : Camera<T>
    {
        protected const string Suffix = ".txt";
        private readonly string _directory;
        private int _fileIndex = 0;
        protected string TimeStamp { get; } = DateTime.Now.ToShortTimeString().Remove(2,1);

        protected DiskCameraBase(string directory, string prefix, bool on) : base(on)
        {
            _directory = directory;
            BasePath = Path.Combine(_directory,prefix);
        }

        protected string BasePath { get; }

        protected override void ConsumeElement(T dequeue)
        {
         /*   if (_fileIndex == -1)
            {
                _fileIndex = GetMaxIndex(_directory) + 1; // todo buggy
            }*/
            if (OnConsume(dequeue, BasePath, _fileIndex))
            {
                _fileIndex++;
            }
            
        }

        protected abstract bool OnConsume(T dequeue, string basePath, int fileIndex);

        /*private static int GetMaxIndex(string directory)
        {
            var files = Directory.GetFiles(directory);
            var maxIndex = -1;
            foreach (var filename in files)
            {
                var preIndex = filename.LastIndexOf("-");
                var postIndex = filename.LastIndexOf(".txt");
                int index;
                if (!int.TryParse(filename.Substring(preIndex + 1, postIndex - preIndex - 1), out index))
                {
                    index = -1;
                }
                maxIndex = Math.Max(maxIndex, index);
            }
            return maxIndex;
        }*/
    }
}