using Newtonsoft.Json;
using WorkflowConfiguration.Activities;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using PrintNodeNet;
using iTextSharp.text.pdf;
using iTextSharp.text;
using ZXing.Common;
using ZXing;
using SkiaSharp;

namespace WorkflowConfigurator.Services.Printer
{
    public class PrinterService
    {
        private readonly IConfiguration _config;
        private readonly WorkflowDefinitionRepository _workflowDefinitionService;

        public PrinterService(IConfiguration config, WorkflowDefinitionRepository workflowDefinitionService)
        {
            _config = config;
            _workflowDefinitionService = workflowDefinitionService;
        }

        public List<string> CreateLabelsPdf(WorkflowInstance workflowInstance)
        {
            var definition = _workflowDefinitionService.Get(workflowInstance.WorkflowDefinitionId);
            var activityBarcodes = new List<string>();
            var activities = definition
                .WorkflowDefinitionVersionData[workflowInstance.DefinitionVer]
                .Activities;


            foreach (ActivityDefinition activityDefinition in activities)
            {
                if (activityDefinition.ActivityType.Equals(nameof(ScanActivity))
                    || activityDefinition.ActivityType.Equals(nameof(ScanSplitScreenActivity)))
                {
                    var scanActivity = JsonConvert.DeserializeObject<ScanActivity>(activityDefinition.ActivityMetadata);

                    var activityBarcode = GenerateBarcode(activityDefinition.ActivityId,
                        workflowInstance.InstanceName,
                        workflowInstance.CreatedOn.Value,
                        scanActivity.ScanType);

                    activityBarcodes.Add(activityBarcode);

                }
            }

            workflowInstance.GlobalVariables["LabelsPrinted"] = "true";

            return activityBarcodes;
        }


        private string GenerateBarcode(int activityId, string instanceName,DateTime instanceCreateon, string scanType)
        {
            var barcodeString = GenerateBarcodeString(activityId, instanceName, scanType);

            var barcode = GeneratePDFWithQRCodeFix(barcodeString, $"activityID: {activityId}-{instanceName}", "testooo");

            return barcode;
        }

        public string GenerateBarcodeString(int activityId, string instanceName, string scanType)
        {
            return $"{scanType}-{activityId}-{instanceName}";
        }

        public async Task<object> SendPrintJob(List<string> generatedBarcodes, long printerId = 1)
        {
            PrintNodeConfiguration.ApiKey = _config.GetValue<string>("PRINTNODE_API_KEY");
            if (PrintNodeConfiguration.ApiKey == null)
            {
                throw new ArgumentNullException("PRINTNODE_API_KEY is not set as environment variables.");
            }
            var printer = await PrintNodePrinter.GetAsync(printerId);
            List<long> printJobResults = new List<long>(generatedBarcodes.Count);
            foreach (string barcode in generatedBarcodes)
            {
                var printJob = new PrintNodePrintJob() { Title = "Arxum", Content = barcode, ContentType = "pdf_base64" };
                var printJobResult = await printer.AddPrintJob(printJob);
                printJobResults.Add(printJobResult);
            }


            return printJobResults;
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