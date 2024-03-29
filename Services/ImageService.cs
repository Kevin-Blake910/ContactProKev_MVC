﻿using ContactProKev_MVC.Services.Interfaces;

namespace ContactProKev_MVC.Services
{
    public class ImageService : IImageService
    {
        private readonly string defaultImage = "/img/superdog200bg.jpg";


        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if(fileData == null)
            {
                return defaultImage;
            }

            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData);
                string imageSrcString = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageSrcString;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
