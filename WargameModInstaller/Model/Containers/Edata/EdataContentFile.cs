using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Containers.Edata
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///struct dictFileEntry {
    ///     DWORD groupId;
    ///     DWORD fileEntrySize;
    ///     DWORD offset;
    ///     DWORD chunk2;   
    ///     DWORD fileSize;
    ///     DWORD chunk4;
    ///     blob checksum[16];
    ///     zstring name;
    /// };
    /// </remarks>
    public class EdataContentFile : IContentFile
    {
        /// <summary>
        /// Occurs when the content is loaded
        /// </summary>
        public event EventHandler ContentLoaded;

        /// <summary>
        /// Occurs when the content is unloaded
        /// </summary>
        public event EventHandler ContentUnloaded;

        /// <summary>
        /// Gets or sets the content owner container file.
        /// </summary>
        public IContainerFile Owner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path written in the edata dictionary. 
        /// </summary>
        public String Path
        {
            get;
            set; 
        }

        /// <summary>
        /// Gets or sets the file raw content.
        /// </summary>
        public byte[] Content
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the file's content length in bytes.
        /// Returns zero when no content.
        /// 
        /// //Maxymalny rozmiar tablicy to int32.max, wiec to long jest zbędne...
        /// </summary>
        public long ContentSize
        {
            get
            {
                return IsContentLoaded ? Content.Length : 0;
            }
        }

        /// <summary>
        /// Gets the information wheather the file's content is loaded.
        /// False, when content null, true when content set, even if zero bytes long.
        /// </summary>
        public bool IsContentLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the information wheather the file's content is loaded with user's custom data.
        /// </summary>
        public bool IsCustomContentLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the information wheather the file's content is loaded with file's orginal data.
        /// </summary>
        public bool IsOriginalContentLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets an offset of the content file 
        /// from the begining of the content section of conatianer's file.
        /// </summary>
        public long Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an offset of the content file from the begining of the container file.
        /// </summary>
        public long TotalOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a length of the content file read from container file.
        /// </summary>
        public long Length
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content file checksum.
        /// </summary>
        public byte[] Checksum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content file type.
        /// </summary>
        public ContentFileType FileType
        {
            get;
            set;
        }

        /// <summary>
        /// Loads object content with the provided original data.
        /// </summary>
        /// <param name="data"></param>
        public void LoadOrginalContent(byte[] data)
        {
            Content = data;

            IsContentLoaded = true;
            IsOriginalContentLoaded = true;
            IsCustomContentLoaded = false;

            NotifyContentLoaded();
        }

        /// <summary>
        /// Loads object content with the provided user's custom data.
        /// </summary>
        /// <param name="data"></param>
        public void LoadCustomContent(byte[] data)
        {
            Content = data;

            IsContentLoaded = true;
            IsCustomContentLoaded = true;
            IsOriginalContentLoaded = false;

            NotifyContentLoaded();
        }

        /// <summary>
        /// Discards object's current content;
        /// </summary>
        public void UnloadContent()
        {
            Content = null;

            IsContentLoaded = false;
            IsCustomContentLoaded = false;
            IsOriginalContentLoaded = false;

            NotifyContentUnloaded();
        }

        public override string ToString()
        {
            return Path;
        }

        private void NotifyContentLoaded()
        {
            var handler = ContentLoaded;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void NotifyContentUnloaded()
        {
            var handler = ContentUnloaded;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

    }

}
