using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using Esoteric.DAL.Interface;

namespace Lynx.DAL
{
    [Export]
    [Export(typeof(IRandomAccessRepository<FileInfo, string>))]
    public class FileRepository : RandomAccessRepository<FileInfo, string>
    {
        #region Properties
        //-----------------------------------------------------------------------------
        public Collection<DirectoryInfo> Directories
        {
            get
            {
                return _directories ?? (_directories = new Collection<DirectoryInfo>() );
            }
        }
        Collection<DirectoryInfo> _directories;

        public string SearchPattern
        {
            get
            {
                return _searchPattern ?? (_searchPattern = "*.*");
            }
            set
            {
                _searchPattern = value;
            }
        }
        string _searchPattern;
	
        #endregion

        #region Private methods
        //-----------------------------------------------------------------------------	
        // Depth-first search
        private IEnumerable<FileInfo> ForAll(DirectoryInfo dirInfo)
        {
            foreach (DirectoryInfo di in dirInfo.GetDirectories())
                foreach (FileInfo fi in ForAll(di))
                    yield return fi;

            foreach (FileInfo fi in dirInfo.GetFiles(SearchPattern))
                yield return fi;
        }

        #endregion

        #region Other Public Methods
        //-----------------------------------------------------------------------------	
        public void AddDirectories(string[] directories)
        {
            foreach (string s in directories)
                AddDirectory(s);
        }

        public void AddDirectory(string directory)
        {
            if (directory.LastIndexOf(Path.DirectorySeparatorChar) != directory.Length - 1)
                directory += Path.DirectorySeparatorChar;
            AddDirectory(new DirectoryInfo(directory));
        }

        public void AddDirectories(DirectoryInfo[] directories)
        {
            foreach (DirectoryInfo di in directories)
                AddDirectory(di);
        }

        public void AddDirectory(DirectoryInfo di)
        {
            Directories.Add(di);
        }
        #endregion

        #region RandomAccessRepository Methods
        //-----------------------------------------------------------------------------	
        public override IEnumerator<FileInfo> GetEnumerator()
        {
            if (Directories.Count == 0)
                throw new InvalidOperationException("There must be at least one root directory assigned before calling this method");

            foreach (DirectoryInfo di in Directories)
                foreach (FileInfo fi in ForAll(di))
                    yield return fi;
        }

        public override FileInfo Get(string fileName)
        {
            return new FileInfo(fileName);
        }

        public override string GetKey(FileInfo item)
        {
            return item.FullName;
        }

        public override FileInfo Add(FileInfo item)
        {
            throw new NotImplementedException();
        }

        public override FileInfo Set(FileInfo item, string fileName)
        {
            throw new NotImplementedException();
        }

        public override bool Delete(string fileName)
        {
            try
            {
                FileInfo fi = new FileInfo(fileName);
                fi.Delete();
            }
            catch (System.ArgumentNullException) { return false; }
            catch (System.Security.SecurityException) { return false; }
            catch (System.ArgumentException) { return false; }
            catch (System.UnauthorizedAccessException) { return false; }
            catch (System.IO.PathTooLongException) { return false; }
            catch (System.NotSupportedException) { return false; }

            return true;
        }
        #endregion
    }
}
