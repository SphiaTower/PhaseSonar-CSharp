using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTIR.Analyzers;
using FTIR.Correctors;
using FTIR.Maths;
using FTIR.Utils;

namespace Shokouki.Controllers {
    public class DiskCamera:Camera
    {
        private int _fileIndex = -1;
        protected override void Consume(ISpectrum dequeue)
        {
            const string directory = @"C:\buffer\captured\";
            const string filename = @"captured-aver-sq-";
            const string basePath = directory + filename;
            if (_fileIndex==-1)
            {
                _fileIndex = GetMaxIndex(directory)+1; // todo buggy
            }
            Toolbox.WriteData(basePath+"n"+dequeue.PulseCount+"-"+_fileIndex+".txt",
                dequeue.Amplitudes.Select(d => (d*d/dequeue.PulseCount/dequeue.PulseCount)).ToArray());
            _fileIndex++;
        }

        private static int GetMaxIndex(string directory)
        {
            var files = Directory.GetFiles(directory);
            int maxIndex = -1;
            foreach (var filename in files)
            {
                var preIndex = filename.LastIndexOf("-");
                var postIndex = filename.LastIndexOf(".txt");
                int index;
                if (!int.TryParse(filename.Substring(preIndex + 1, postIndex - preIndex - 1), out index)) {
                    index = -1;
                }
                maxIndex = Math.Max(maxIndex, index);
            }
            return maxIndex;
        }
    }
}
