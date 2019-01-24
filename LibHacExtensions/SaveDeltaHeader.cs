﻿using System.IO;
using LibHac.IO;

namespace nsZip.LibHacExtensions
{
	public static class SaveDeltaHeader
	{
		public static void Save(string fragmentInput)
		{
			var delta = File.Open($"{fragmentInput}", FileMode.Open).AsStorage();
			if (delta.Length < 0x40)
			{
				throw new InvalidDataException("Delta file is too small.");
			}

			var Header = new DeltaFragmentHeader(new StorageFile(delta, OpenMode.Read));

			if (Header.Magic != DeltaTools.Ndv0Magic)
			{
				throw new InvalidDataException("NDV0 magic value is missing.");
			}

			var fragmentSize = Header.FragmentHeaderSize + Header.FragmentBodySize;
			if (delta.Length < fragmentSize)
			{
				throw new InvalidDataException(
					$"Delta file is smaller than the header indicates. (0x{fragmentSize} bytes)");
			}

			var reader = new FileReader(new StorageFile(delta, OpenMode.Read));
			var writer = File.Open("fragment_meta", FileMode.Create);

			reader.Position = 0;
			writer.Write(reader.ReadBytes((int) Header.FragmentHeaderSize), 0, (int) Header.FragmentHeaderSize);

			long offset = 0;

			while (offset < Header.NewSize)
			{
				ReadSegmentHeader(reader, writer, out var size, out var seek);
				if (seek > 0)
				{
					offset += seek;
				}

				if (size > 0)
				{
					offset += size;
				}

				reader.Position += size;
			}

			delta.Dispose();
			writer.Dispose();
		}

		private static void ReadSegmentHeader(FileReader reader, FileStream writer, out int size, out int seek)
		{
			var pos = reader.Position;
			var type = reader.ReadUInt8();
			var seekBytes = (type & 3) + 1;
			var sizeBytes = ((type >> 3) & 3) + 1;

			size = DeltaTools.ReadInt(reader, sizeBytes);
			seek = DeltaTools.ReadInt(reader, seekBytes);

			reader.Position = pos;
			var len = 1 + sizeBytes + seekBytes;
			writer.Write(reader.ReadBytes(len), 0, len);
		}
	}
}