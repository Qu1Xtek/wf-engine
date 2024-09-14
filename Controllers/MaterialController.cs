//using IronBarCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfigurator.Models.Materials;
using PrintNodeNet;
using ZXing;
using Microsoft.VisualBasic.FileIO;
using ZXing.Common;
using WorkflowConfigurator.Repositories;
using SkiaSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WorkflowConfigurator.Services.Materials;
using WorkflowConfigurator.Models.DTO;

namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [Route("Material")]
    public class MaterialController : ControllerBase
    {

        private readonly MaterialService _materialService;
        private readonly WorkflowDefinitionRepository _workflowDefinitionService;
        private readonly IConfiguration _config;

        public MaterialController(MaterialService materialService,
            WorkflowDefinitionRepository workflowDefinitionService,
            IConfiguration config)
        {
            _materialService = materialService;
            _workflowDefinitionService = workflowDefinitionService;
            _config = config;
        }

        [HttpGet]
        public async Task<List<ProductDTO>> GetMaterials([FromQuery] int code)
        {
            return await _materialService.GetCategories(code);
        }

        [HttpPost]
        public async Task<IActionResult> CreateScanType(Material material)
        {
            var res = await _materialService.CreateAsyncIfDoesNotExits(material);
            if (res.IsSuccesful)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res);
            }   
        }

        [HttpPut]
        public async Task UpdateMaterial(Material material)
        {
            await _materialService.UpdateAsync(material);
        }

        [HttpGet]
        [Route("Secret")]
        [AllowAnonymous]
        public async Task<Object> LoginGet()
        {
            PrintNodeConfiguration.ApiKey = _config.GetValue<string>("PRINTNODE_API_KEY");
            var printerId = 0;
            var printer = await PrintNodePrinter.GetAsync(printerId);

            var barcodeString = "6437f3d0aa2d280eaeffd401-2-ivan@arxum.com_Scandemo_0804_1981265896";

            //GeneratedBarcode barcode = BarcodeWriter.CreateBarcode(barcodeString, BarcodeWriterEncoding.QRCode);
            //var fontFace = new IronSoftware.Drawing.Font("Arial");
            //barcode.AddBarcodeValueTextAboveBarcode();
            //barcode.AddAnnotationTextAboveBarcode("6437f3d0aa2d280eaeffd401-2-ivan@arxum.com_Scandemo_0804_1981265896", fontFace, IronSoftware.Drawing.Color.FromArgb(0, 0, 0));
            //var binary = Convert.ToBase64String(barcode.ToPdfBinaryData());
            //var outputStream = barcode.ToStream(ImageFormat.Default);
            //var base64Barcode = Convert.ToBase64String(barcode.ToPdfBinaryData());
            //outputStream.Seek(0, SeekOrigin.Begin);
            //var file =  File(outputStream, "image/jpeg");
            //var raw_base64 = Convert.ToBase64String(test);

            var barcode2 = "c-chip-45-_TestVaseto_2333_943433322";
            var listOfPrintJobs = new List<PrintNodePrintJob>();
            //var printJob = new PrintNodePrintJob
            //{
            //    Title = "Hello, world!",
            //    Content = binary,
            //    ContentType = "pdf_base64"
            //};
            var printJob2 = new PrintNodePrintJob
            {
                Title = "Hello, world!",
                Content = barcode2,
                ContentType = "pdf_base64"
            };

            //listOfPrintJobs.Add(printJob);
            //listOfPrintJobs.Add(printJob);
            //listOfPrintJobs.Add(printJob);
            //BarcodeWriterGeneric barcodeWriter = new BarcodeWriterGeneric<>();
            //barcodeWriter.Encoder = ;
            //listOfPrintJobs.Add(printJob2);
            foreach (var item in listOfPrintJobs)
            {
                var result = await printer.AddPrintJob(item);

            }
            return await Task.FromResult<object>(null);
        }
        [HttpGet]
        [Route("Secret2")]
        [AllowAnonymous]
        public async Task<Object> LoginGett(long printerId)
        {
            var barcodeString = "6437f3d0aa2d280eaeffd401-222-ivan@arxum.com_Scandemo_0804_1981265896";
            var barcodeAnnotation = "ActivityID: 222-6437f3d0aa2d280eaeffd401";
            var pdfAsBase64 = GeneratePDFWithQRCodeFix(barcodeString, barcodeAnnotation, FileSystem.CurrentDirectory + "\\testo2.pdf");
            PrintNodeConfiguration.ApiKey = _config.GetValue<string>("PRINTNODE_API_KEY");

            var printer = await PrintNodePrinter.GetAsync(printerId);
            var listOfPrintJobs = new List<PrintNodePrintJob>();
            PrintNodePrintJob printJob = new PrintNodePrintJob
            {
                Title = "2-up Barcode Printing",
                ContentType = "pdf_base64",
                Content = pdfAsBase64,
                Source = "Configurator"
            };
            listOfPrintJobs.Add(printJob);
            listOfPrintJobs.Add(printJob);
            //// Step 6: Send Print Job to PrintNode
            //var response = await printer.AddPrintJob(printJob);
            foreach (var item in listOfPrintJobs)
            {
                var result = await printer.AddPrintJob(item);

            }
            return new object();
        }
        private string GeneratePDFWithQRCodeFix(string text, string annotation, string pdfFilePath)
        {
            var widthSize = 500;
            var heightSize = 550;
            var skBitmap = GeneratePngFix(text, BarcodeFormat.QR_CODE, widthSize, heightSize, 1);

            if (skBitmap == null || skBitmap.Width <= 0 || skBitmap.Height <= 0)
            {
                throw new InvalidOperationException("Invalid SKBitmap object.");
            }

            using (MemoryStream pdfMemoryStream = new MemoryStream())
            {
                var pdfDoc = new Document();
                var pdfWriter = PdfWriter.GetInstance(pdfDoc, pdfMemoryStream);
                pdfDoc.Open();

                // Convert SKBitmap to byte array
                using (SKImage img = SKImage.FromBitmap(skBitmap))
                using (SKData data = img.Encode())
                {
                    byte[] imageBytes = data.ToArray();

                    // Add QR Code to PDF
                    var qrCodeImage = iTextSharp.text.Image.GetInstance(imageBytes);
                    qrCodeImage.ScaleToFit(widthSize, heightSize);
                    pdfDoc.Add(qrCodeImage);
                }

                // Add Annotation
                iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 42, BaseColor.BLACK);
                var annotationParagraph = new Paragraph(annotation, font);
                pdfDoc.Add(annotationParagraph);

                pdfDoc.Close();

                // Convert PDF MemoryStream to byte array
                byte[] pdfByteArray = pdfMemoryStream.ToArray();

                // Convert byte array to Base64 string
                string base64String = Convert.ToBase64String(pdfByteArray);

                return base64String;
            }
        }


        private SKBitmap GeneratePngFix(string content, BarcodeFormat barcodeformat, int width, int height, int margin)
        {
            var qrWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = barcodeformat,
                Options = new EncodingOptions { Height = height, Width = width, Margin = margin }
            };

            var pixelData = qrWriter.Write(content);
            if (pixelData == null || pixelData.Pixels.Length == 0)
            {
                throw new InvalidOperationException("Invalid pixel data.");
            }

            // Create SKData from the byte array
            var skData = SKData.CreateCopy(pixelData.Pixels);

            // Calculate rowBytes (width * bytes per pixel)
            int rowBytes = pixelData.Width * 4; // 4 bytes for RGBA8888

            // Create SKImage from SKData
            var skImage = SKImage.FromPixels(new SKImageInfo(pixelData.Width, pixelData.Height, SKColorType.Rgba8888), skData, rowBytes);

            // Create SKBitmap from SKImage
            var skBitmap = SKBitmap.FromImage(skImage);

            return skBitmap;
        }
    }
}