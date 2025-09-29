using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace Louvre.Shared.Repository
{
	public interface IMediaRepository
	{
		Task<ImagesSaveViewModel> SaveMedia(int? mediaId, IFormFile file, string folderName = "", string fileName = "", IDbTransaction tran = null);
		Task SaveProfilePic(int personalInfoId, IFormFile profilePhoto, int? profileImageMediaID, IDbTransaction tran = null);
		Task<MediaSavePostViewModel> GetMedia(int? meidaId);
		Task<MediaFileOnlyPostViewModel> GetMediaFileOnly(int? mediaId);
		string GetQRImage(string qrcode);
		Task DeleteExistingFileAsync(int? mediaId, IDbTransaction tran = null);
		
	}

	public class MediaRepository : IMediaRepository
	{
		private readonly IDbContext _dbContext;
		private readonly IHostingEnvironment _env;
		int size = 1400;
		int width, height;

		public MediaRepository(IDbContext entity, IHostingEnvironment env)
		{
			_dbContext = entity;
			_env = env;
		}

        private const long maxFileSize = 10 * 1024 * 1024;

        public async Task<ImagesSaveViewModel> SaveMedia(int? mediaId, IFormFile file, string folderName = "", string fileName = "", IDbTransaction tran = null)
		{
			var result = new ImagesSaveViewModel();
			folderName = "gallery/" + folderName;

			//try
			//{
				if (file.Length > 0)
				{
                if (file.Length > maxFileSize)
                    throw new PreDefinedException(fileName + " exceeds maximum allowed size of 10 MB.");

                await DeleteExistingFileAsync(mediaId, tran);

					if (!Directory.Exists(Path.Combine("wwwroot", folderName)))
					{
						Directory.CreateDirectory(Path.Combine("wwwroot", folderName));
					}
					if (string.IsNullOrEmpty(fileName))
					{
						fileName = Guid.NewGuid().ToString("N");
					}
					else
					{
						Random random = new Random();
						fileName += "_" + random.Next(1000);
					}
					string extension = Path.GetExtension(file.FileName).Substring(1);
					fileName = $"{fileName}.{extension}";
					string path = Path.Combine(_env.ContentRootPath, "wwwroot", folderName);

					switch (file.ContentType)
					{
						case "image/jpeg":
						case "image/jpg":
						case "image/png":
							var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream());

							if (image.Width > size)
							{
								width = size;
								height = image.Height * size / image.Width;
							}
							else if (image.Height > size)
							{
								height = size;
								width = image.Width * size / image.Height;
							}

							if (image.Width > size || image.Height > size)
								image.Mutate(x => x.Resize(width, height));
							image.Save(Path.Combine(path, fileName));
							break;
						case "application/pdf":
							using (var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create))
							{
								await file.CopyToAsync(fs);
							}
							break;
						default:
							//using (var fs = new FileStream(Path.Combine(path, fileName), FileMode.Create))
							//{
							//	await file.CopyToAsync(fs);
							//}
							//break;
							throw new PreDefinedException("Unsupported File Type", "Supported file types are .jpg, .jpeg, .png, .pdf");
					}

					Media media = new Media()
					{
						ContentType = file.ContentType,
						FileName = "/" + folderName + "/" + fileName,
						ContentLength = file.Length,
						Extension = extension,
						MediaID = mediaId
					};
					mediaId = await _dbContext.SaveAsync(media, tran);
					result.IsSuccess = true;
					result.MediaID = mediaId;
				}
				else
				{
					result.IsSuccess = false;
					result.ErrorMessage = "No file found";
				}
			//}
			//catch (Exception err)
			//{
			//	result.IsSuccess = false;
			//	result.ErrorMessage = err.Message;
			//}
			return result;
		}

		
		public async Task SaveProfilePic(int personalInfoId, IFormFile profilePhoto, int? profileImageMediaID, IDbTransaction tran = null)
		{
			if (profilePhoto != null)
			{
				var mediaResult = await SaveMedia(profileImageMediaID, profilePhoto, "profile", personalInfoId.ToString(), tran);
				if (mediaResult.IsSuccess && profileImageMediaID is null)
				{
					await _dbContext.ExecuteAsync($"Update PersonalInfos Set ProfileImageMediaID={mediaResult.MediaID} where PersonalInfoID={personalInfoId}", null, tran);
				}
			}
		}

		public async Task<MediaSavePostViewModel> GetMedia(int? mediaId)
		{
			return await _dbContext.GetAsync<MediaSavePostViewModel>($@"Select MediaID,IsUrl,Case when IsURL=1 then FileName end as MediaURL,Case when IsURL<>1 then FileName end as FileURL
            From Medias
            Where MediaID=@MediaID and ISNULL(IsDeleted,0)=@IsDeleted", new { MediaID = Convert.ToInt32(mediaId), IsDeleted = false });
		}

		public async Task<MediaFileOnlyPostViewModel> GetMediaFileOnly(int? mediaId)
		{
			var r = await _dbContext.GetAsync<MediaFileOnlyPostViewModel>($"Select MediaID,FileName,ContentType From Medias Where MediaID=@MediaID", new { MediaID = Convert.ToInt32(mediaId) });
			if (r == null)
				r = new MediaFileOnlyPostViewModel();
			return r;
		}

		public async Task DeleteExistingFileAsync(int? mediaId, IDbTransaction tran = null)
		{
			if (mediaId != null)
			{
				try
				{
					var m = await _dbContext.GetAsync<Media>(Convert.ToInt32(mediaId), tran);
					string deleteFilePath = Path.Combine(_env.ContentRootPath, "wwwroot");
					deleteFilePath += m.FileName;
					File.Delete(deleteFilePath);
				}
				catch { }
			}
		}

		public string? GetQRImage(string qrcode)
		{
			if (qrcode != null)
			{
				QRCodeGenerator qrGenerator = new QRCodeGenerator();
				QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrcode, QRCodeGenerator.ECCLevel.Q);
				BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
				byte[] qrCodeImage = qrCode.GetGraphic(20);
				return "data:image/png;base64," + Convert.ToBase64String(qrCodeImage);
			}
			else
			{
				return qrcode;
			}
		}

	}
}
