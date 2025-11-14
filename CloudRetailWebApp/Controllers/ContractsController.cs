// FILE: ContractsController.cs
// DESCRIPTION: Controller for managing contract documents using Azure File Shares.
//              Allows admins to upload, download, and manage contract files.
// SOURCES:
//    - ASP.NET Core MVC Controllers: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
//    - Azure File Shares: https://learn.microsoft.com/en-us/azure/storage/files/storage-dotnet-how-to-use-files

using CloudRetailWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CloudRetailWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ContractsController : Controller
    {
        private readonly IStorageService _storageService;
        private readonly FunctionApiService _functionApiService;

        public ContractsController(IStorageService storageService, FunctionApiService functionApiService)
        {
            _storageService = storageService;
            _functionApiService = functionApiService;
        }

        // GET: Contracts
        public IActionResult Index()
        {
            return View();
        }

        // GET: Contracts/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Contracts/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return View();
            }

            try
            {
                var uploadedViaFunction = await _functionApiService.UploadContractAsync(file);
                if (!uploadedViaFunction)
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    await _storageService.SendFileToFileShareAsync(file.FileName, fileBytes);
                }

                TempData["SuccessMessage"] = $"Contract '{file.FileName}' uploaded successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                return View();
            }
        }

        // GET: Contracts/Download
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            try
            {
                var fileContent = await _functionApiService.DownloadContractAsync(fileName);
                if (fileContent == null)
                {
                    fileContent = await _storageService.GetFileFromFileShareAsync(fileName);
                }

                if (fileContent == null)
                {
                    TempData["ErrorMessage"] = "File not found.";
                    return RedirectToAction(nameof(Index));
                }

                return File(fileContent, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contracts/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }

            try
            {
                var deleted = await _functionApiService.DeleteContractAsync(fileName);
                if (!deleted)
                {
                    deleted = await _storageService.DeleteFileFromFileShareAsync(fileName);
                }

                if (deleted)
                {
                    TempData["SuccessMessage"] = $"Contract '{fileName}' deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "File not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contracts/ListFiles (API endpoint for AJAX)
        [HttpGet]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                var files = await _functionApiService.GetContractFilesAsync();
                if (files == null || !files.Any())
                {
                    files = await _storageService.ListFilesInFileShareAsync();
                }

                return Json(files?.ToList() ?? new List<string>());
            }
            catch
            {
                return Json(new List<string>());
            }
        }
    }
}

