using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Utilities {
    public static class Logger {
        public static ConcurrentQueue<string> Queue { get; } = new ConcurrentQueue<string>();

        public static void WriteLine([NotNull] string text) {
            Write(text);
        }

        public static void WriteLine([NotNull] string title, [NotNull] string text) {
            Write(title + @": " + text);
        }

        private static void Write(string text) {
            //            Queue.Enqueue(text);
            Console.WriteLine(text);
        }

        private static void Write() {
            Queue.Enqueue("\n");
        }

        public static void WriteLine() {
            Write();
        }

        public static void WriteSeparator() {
            Write(@"-----------");
        }

        public static void WriteSeparator([NotNull] string title) {
            Write(@"-----" + title + @"-----");
        }
    }
}