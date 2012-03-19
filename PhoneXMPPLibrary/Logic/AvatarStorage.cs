/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;
#if !MONO
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#endif

using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;

namespace System.Net.XMPP
{
    public class AvatarStorage
    {
        public AvatarStorage(string strAccountFolder)
        {
            AccountFolder = strAccountFolder;
        }

        private string m_strAccountFolder = null;

        public string AccountFolder
        {
          get { return m_strAccountFolder; }
          set { m_strAccountFolder = value; }
        }

        public bool AvatarExist(string strHash)
        {
            bool bRet = false;
            IsolatedStorageFile storage = null;

            
#if WINDOWS_PHONE
            storage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
#endif
            string strFileName = string.Format("{0}/{1}", AccountFolder, strHash);

            bRet = storage.FileExists(strFileName);
            storage.Dispose();

            return bRet;
        }



#if !MONO
        public System.Windows.Media.Imaging.BitmapImage GetAvatarImage(string strHash)
        {
            System.Windows.Media.Imaging.BitmapImage objImage = null;
            IsolatedStorageFile storage = null;

#if WINDOWS_PHONE
            storage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
#endif

            string strFileName = string.Format("{0}/{1}", AccountFolder, strHash);
            if (storage.FileExists(strFileName) == false)
            {
                storage.Dispose();
                return null;
            }

            IsolatedStorageFileStream stream = null;
            try
            {
                stream = new IsolatedStorageFileStream(strFileName, System.IO.FileMode.Open, storage);
                objImage = new System.Windows.Media.Imaging.BitmapImage();
#if WINDOWS_PHONE
                objImage.SetSource(stream);
#else
                objImage.BeginInit();
                objImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                objImage.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
                
                objImage.StreamSource = stream;
                objImage.EndInit();

                double nWith = objImage.Width;
                double nHeight = objImage.Height;
#endif

            }
            catch (Exception)
            {
            }
            finally
            {
                if (stream != null)
                    stream.Close();

                storage.Dispose();
            }

            return objImage;
        }
#else
        public byte [] GetAvatarImage(string strHash)
        {
            byte [] bImage = null;
            IsolatedStorageFile storage = null;

            storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

            string strFileName = string.Format("{0}/{1}", AccountFolder, strHash);
            if (storage.FileExists(strFileName) == false)
            {
                storage.Dispose();
                return null;
            }

            IsolatedStorageFileStream stream = null;
            try
            {
                stream = new IsolatedStorageFileStream(strFileName, System.IO.FileMode.Open, storage);
        
                bImage = new byte[stream.Length];
                stream.Read(bImage, 0, bImage.Length);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (stream != null)
                    stream.Close();

                storage.Dispose();
            }

            return bImage;
        }

#endif



        /// <summary>
        /// Writes the avatar to disk and returns the hash value
        /// </summary>
        /// <param name="bData"></param>
        /// <returns></returns>
        public string WriteAvatar(byte[] bImageData)
        {
            SHA1Managed sha = new SHA1Managed();
            string strHash = "";
            IsolatedStorageFile storage = null;

            strHash = SocketServer.TLS.ByteHelper.HexStringFromByte(sha.ComputeHash(bImageData), false, int.MaxValue);
#if WINDOWS_PHONE
            storage = IsolatedStorageFile.GetUserStoreForApplication();
#else
            storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);
#endif

            string strFileName = string.Format("{0}/{1}", AccountFolder, strHash);

            if (storage.FileExists(strFileName) == true)
            {
                storage.Dispose();
                return strHash;
            }

            
            if (storage.DirectoryExists(AccountFolder) == false)
                storage.CreateDirectory(AccountFolder);

            // Load from storage
            IsolatedStorageFileStream location = null;
            try
            {
                location = new IsolatedStorageFileStream(strFileName, System.IO.FileMode.Create, storage);
                location.Write(bImageData, 0, bImageData.Length);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (location != null)
                    location.Close();

                storage.Dispose();
            }

            return strHash;
        }


        public void SaveFileToCameraRoll()
        {
            //  MediaLibrary library = new MediaLibrary();

            //if (radioButtonCameraRoll.IsChecked == true)
            //{
            //    // Save the image to the camera roll album.
            //    Picture pic = library.SavePictureToCameraRoll("SavedPicture.jpg", myFileStream
        }

    }
}
