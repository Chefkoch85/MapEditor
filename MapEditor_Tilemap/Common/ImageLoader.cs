using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Common
{
	public class ImageLoader
	{
		public static BitmapImage load(string file)
		{
			FileInfo fi = new FileInfo(file);
			if (!fi.Exists)
				return null;

			var uri = new Uri(fi.FullName, UriKind.Absolute);
			var bitmap = new BitmapImage(uri);
			return bitmap;
		}

		public static BitmapImage[] loadDirectory(string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);
			if (!di.Exists || di.GetFiles("*", SearchOption.AllDirectories).Length <= 0)
				return null;

			FileInfo[] fis = di.GetFiles("*", SearchOption.AllDirectories);
			BitmapImage[] bitmaps = new BitmapImage[fis.Length];

			for(int i = 0; i < fis.Length; i++)
			{
				FileInfo fi = new FileInfo(fis[i].FullName);
				var uri = new Uri(fi.FullName, UriKind.Absolute);
				bitmaps[i] = new BitmapImage(uri);

			}

			return bitmaps;
		}
	}
}
