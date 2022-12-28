using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace Aryes
{
    internal static class Program
    {
        private static void Main()
        {
            var root = Environment.CurrentDirectory;
            RecurseDirectory(root);
        }

        private static void RecurseDirectory(string path)
        {
            ProcessDirectory(path);
            foreach (var subdir in Directory.EnumerateDirectories(path))
            {
                RecurseDirectory(subdir);
            }
        }

        static void ProcessDirectory(string path)
        {
            var start = DateTime.Today - new TimeSpan(30, 0, 0, 0);
            foreach (var f in Directory.EnumerateFiles(path, "*.jpg"))
            {
                try
                {
                    DateTime date;
                    using (var img = Image.FromFile(f))
                    {
                        Trace.WriteLine("{0}", f);
                        date = DateTime.MinValue;
                        foreach (var p in img.PropertyItems)
                        {
                            if (p.Id != Exif.PropertyTag.ExifDTOrig) continue;
                            var s = Encoding.ASCII.GetString(p.Value);
                            if (!DateTime.TryParseExact(s, "yyyy:MM:dd HH:mm:ss\0", null, DateTimeStyles.AssumeLocal, out date))
                            {
                                Trace.WriteLine("WARNING: Could not parse Date taken '{0}'", s);
                            }
                        }
                        if (date == DateTime.MinValue)
                        {
                            Trace.WriteLine("WARNING: Falling back to Modification date.");
                            date = new FileInfo(f).LastWriteTime;
                        }
                    }
                    if (date > start)
                        continue;
                    var dir = Path.GetDirectoryName(f);
                    if (dir == null) continue;
                    var dest = Path.Combine(dir, $"{date:yyyyMMdd-HHmmss}.jpg");
                    if (dest == f) continue;
                    Console.Write(dest + "\r");
                    if (File.Exists(dest))
                        continue;   //TODO: handle
                    File.Move(f, dest);
                    Trace.WriteLine(dest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
