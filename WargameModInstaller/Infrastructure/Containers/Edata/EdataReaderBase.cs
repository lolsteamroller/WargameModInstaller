﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Containers;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public abstract class EdataReaderBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual EdataHeader ReadHeader(Stream stream, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            EdataHeader header;

            var buffer = new byte[Marshal.SizeOf(typeof(EdataHeader))];

            stream.Read(buffer, 0, buffer.Length);

            header = MiscUtilities.ByteArrayToStructure<EdataHeader>(buffer);

            if (header.Version > 2)
            {
                throw new NotSupportedException(string.Format("Edata version {0} not supported", header.Version));
            }

            return header;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="header"></param>
        /// <param name="loadContent"></param>
        /// <returns>A Collection of the Files found in the Dictionary.</returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// "The only tricky part about that algorythm is that you have to skip one byte if the length of the File/Dir name PLUS nullbyte is an odd number."
        /// </remarks>
        protected virtual IEnumerable<EdataContentFile> ReadEdatV2Dictionary(
            Stream stream,
            EdataHeader header,
            bool loadContent,
            CancellationToken token)
        {
            var files = new List<EdataContentFile>();
            var dirs = new List<EdataContentDirectory>();
            var endings = new List<long>();

            stream.Seek(header.DictOffset, SeekOrigin.Begin);

            long dirEnd = header.DictOffset + header.DictLength;
            uint id = 0;

            while (stream.Position < dirEnd)
            {
                //Cancel if requested;
                token.ThrowIfCancellationRequested();


                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    var file = new EdataContentFile();
                    stream.Read(buffer, 0, 4);
                    file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    buffer = new byte[8];
                    stream.Read(buffer, 0, buffer.Length);
                    file.Offset = BitConverter.ToInt64(buffer, 0);
                    file.TotalOffset = file.Offset + header.FileOffset;

                    stream.Read(buffer, 0, buffer.Length);
                    file.Size = BitConverter.ToInt64(buffer, 0);

                    var checkSum = new byte[16];
                    stream.Read(checkSum, 0, checkSum.Length);
                    file.Checksum = checkSum;

                    file.Name = MiscUtilities.ReadString(stream);
                    file.Path = MergePath(dirs, file.Name);

                    if (file.Name.Length % 2 == 0)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    //to Id służy do identyfikacji plików, oparte na kolejności odczytu, nie pochodzi z danych edata.
                    file.Id = id;
                    id++;

                    ResolveFileType(stream, file, header);

                    if (loadContent)
                    {
                        long currentStreamPosition = stream.Position;

                        file.Content = ReadContent(stream, file.TotalOffset, file.Size);
                        //file.Content = ReadContent(stream, header.FileOffset + file.Offset, file.Size);
                        //file.Size = file.Content.Length;

                        stream.Seek(currentStreamPosition, SeekOrigin.Begin);
                    }

                    files.Add(file);

                    while (endings.Count > 0 && stream.Position == endings.Last())
                    {
                        dirs.Remove(dirs.Last());
                        endings.Remove(endings.Last());
                    }
                }
                else if (fileGroupId > 0)
                {
                    var dir = new EdataContentDirectory();

                    stream.Read(buffer, 0, 4);
                    dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    if (dir.FileEntrySize != 0)
                    {
                        endings.Add(dir.FileEntrySize + stream.Position - 8);
                    }
                    else if (endings.Count > 0)
                    {
                        endings.Add(endings.Last());
                    }

                    dir.Name = MiscUtilities.ReadString(stream);
                    if (dir.Name.Length % 2 == 0)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    dirs.Add(dir);
                }
            }

            return files;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="header"></param>
        /// <param name="loadContent"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual IEnumerable<EdataContentFile> ReadEdatV1Dictionary(
            Stream stream,
            EdataHeader header,
            bool loadContent,
            CancellationToken token )
        {
            var files = new List<EdataContentFile>();
            var dirs = new List<EdataContentDirectory>();
            var endings = new List<long>();

            stream.Seek(header.DictOffset, SeekOrigin.Begin);

            long dirEnd = header.DictOffset + header.DictLength;
            uint id = 0;

            while (stream.Position < dirEnd)
            {
                //Cancel if requested;
                token.ThrowIfCancellationRequested();


                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    var file = new EdataContentFile();
                    stream.Read(buffer, 0, 4);
                    file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    //buffer = new byte[8];  - it's [4] now, so no need to change
                    stream.Read(buffer, 0, 4);
                    file.Offset = BitConverter.ToInt32(buffer, 0);
                    file.TotalOffset = file.Offset + header.FileOffset;

                    stream.Read(buffer, 0, 4);
                    file.Size = BitConverter.ToInt32(buffer, 0);

                    //var checkSum = new byte[16];
                    //fileStream.Read(checkSum, 0, checkSum.Length);
                    //file.Checksum = checkSum;
                    stream.Seek(1, SeekOrigin.Current);  //instead, skip 1 byte - as in WEE DAT unpacker

                    file.Name = MiscUtilities.ReadString(stream);
                    file.Path = MergePath(dirs, file.Name);

                    if ((file.Name.Length + 1) % 2 == 0)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    file.Id = id;
                    id++;

                    ResolveFileType(stream, file, header);

                    if (loadContent)
                    {
                        long currentStreamPosition = stream.Position;

                        file.Content = ReadContent(stream, file.TotalOffset, file.Size);
                        file.Size = file.Content.Length; ////dodane

                        stream.Seek(currentStreamPosition, SeekOrigin.Begin);
                    }

                    files.Add(file);

                    while (endings.Count > 0 && stream.Position == endings.Last())
                    {
                        dirs.Remove(dirs.Last());
                        endings.Remove(endings.Last());
                    }
                }
                else if (fileGroupId > 0)
                {
                    var dir = new EdataContentDirectory();

                    stream.Read(buffer, 0, 4);
                    dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    if (dir.FileEntrySize != 0)
                    {
                        endings.Add(dir.FileEntrySize + stream.Position - 8);
                    }
                    else if (endings.Count > 0)
                    {
                        endings.Add(endings.Last());
                    }

                    dir.Name = MiscUtilities.ReadString(stream);
                    if ((dir.Name.Length + 1) % 2 == 1)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    dirs.Add(dir);
                }
            }
            return files;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="contentFiles"></param>
        protected void LoadContentFiles(Stream source, IEnumerable<EdataContentFile> contentFiles)
        {
            foreach (var file in contentFiles)
            {
                file.Content = ReadContent(source, file.TotalOffset, file.Size);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        protected byte[] ReadContent(Stream stream, long offset, long size)
        {
            byte[] contentBuffer = new byte[size];

            stream.Seek(offset, SeekOrigin.Begin);
            stream.Read(contentBuffer, 0, contentBuffer.Length);

            return contentBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void ResolveFileType(Stream stream, EdataContentFile file, EdataHeader header)
        {
            // save original offset
            long origOffset = stream.Position;

            stream.Seek(file.Offset + header.FileOffset, SeekOrigin.Begin);

            var headerBuffer = new byte[12];
            stream.Read(headerBuffer, 0, headerBuffer.Length);

            file.FileType = GetFileTypeFromHeaderData(headerBuffer);

            // set offset back to original
            stream.Seek(origOffset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Merges a filename in its dictionary tree.
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="fileName"></param>
        /// <returns>The valid Path inside the package.</returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual String MergePath(IEnumerable<EdataContentDirectory> dirs, String fileName)
        {
            var b = new StringBuilder();
            foreach (EdataContentDirectory dir in dirs)
            {
                b.Append(dir.Name);
            }
            b.Append(fileName);

            return b.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerData"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static ContentFileType GetFileTypeFromHeaderData(byte[] headerData)
        {
            var knownHeaders = new List<KeyValuePair<ContentFileType, byte[]>>();
            knownHeaders.Add(new KeyValuePair<ContentFileType, byte[]>(ContentFileType.Ndfbin, ContentFileType.Ndfbin.MagicBytes));
            knownHeaders.Add(new KeyValuePair<ContentFileType, byte[]>(ContentFileType.Edata, ContentFileType.Edata.MagicBytes));
            knownHeaders.Add(new KeyValuePair<ContentFileType, byte[]>(ContentFileType.Trad, ContentFileType.Trad.MagicBytes));
            knownHeaders.Add(new KeyValuePair<ContentFileType, byte[]>(ContentFileType.Save, ContentFileType.Save.MagicBytes));
            knownHeaders.Add(new KeyValuePair<ContentFileType, byte[]>(ContentFileType.Prxypcpc, ContentFileType.Prxypcpc.MagicBytes));
            knownHeaders.Add(new KeyValuePair<ContentFileType, byte[]>(ContentFileType.Image, ContentFileType.Image.MagicBytes));

            foreach (var knownHeader in knownHeaders)
            {
                if (knownHeader.Value.Length < headerData.Length)
                {
                    headerData = headerData.Take(knownHeader.Value.Length).ToArray();
                }

                if (MiscUtilities.ComparerByteArrays(headerData, knownHeader.Value))
                {
                    return knownHeader.Key;
                }
            }

            return ContentFileType.Unknown;
        }

        #region Helpers

        //protected void WriteContentFiles(String path, IEnumerable<EdataContentFile> files)
        //{
        //    var paths = files
        //        .Select(f => f.Path)
        //        .OrderBy(x => x, new EdataDictStringComparer())
        //        .ToList();

        //    using (var stream = File.CreateText(path))
        //    {
        //        foreach (var p in paths)
        //        {
        //            stream.WriteLine(p);
        //        }
        //    }
        //}

        //protected void ReadAndWriteDictionaryStats(
        //    Stream readStream,
        //    EdataHeader header,
        //    String filePath)
        //{
        //    readStream.Seek(header.DictOffset, SeekOrigin.Begin);
        //    readStream.Seek(10, SeekOrigin.Current);

        //    long dirEnd = header.DictOffset + header.DictLength;

        //    using (var writeStream = File.CreateText(filePath))
        //    {
        //        while (readStream.Position < dirEnd)
        //        {
        //            var buffer = new byte[4];
        //            readStream.Read(buffer, 0, 4);
        //            int fileGroupId = BitConverter.ToInt32(buffer, 0);

        //            if (fileGroupId == 0)
        //            {
        //                readStream.Read(buffer, 0, 4);
        //                int entrySize = BitConverter.ToInt32(buffer, 0);

        //                //skip ofsset 8, size 8, checsum 16
        //                readStream.Seek(32, SeekOrigin.Current);

        //                String name = MiscUtilities.ReadString(readStream);

        //                if (name.Length % 2 == 0)
        //                {
        //                    readStream.Seek(1, SeekOrigin.Current);
        //                }

        //                writeStream.WriteLine(String.Format("File, {0}, {1}, ", name, entrySize));
        //            }
        //            else
        //            {
        //                int entrySize = fileGroupId;

        //                readStream.Read(buffer, 0, 4);
        //                int relevance = BitConverter.ToInt32(buffer, 0);

        //                String name = MiscUtilities.ReadString(readStream);

        //                if (name.Length % 2 == 0)
        //                {
        //                    readStream.Seek(1, SeekOrigin.Current);
        //                }

        //                writeStream.WriteLine(String.Format("Dir, {0}, {1}, {2}", name, entrySize, relevance));
        //            }
        //        }
        //    }
        //} 
        #endregion

    }

}
