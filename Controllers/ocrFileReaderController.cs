using IronOcr;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace fileReaderProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ocrFileReaderController : ControllerBase
    {
        //public  Task<IActionResult> get_Scan()
        //{

        //    var ocr = new IronTesseract();
        //    var ocrInput = new OcrInput(@"C:\Users\moves\OneDrive\Desktop\add.pdf");
        //    var contentArea = new Rectangle(x: 215, y: 1250, width: 1335, height: 200);
        //    ocrInput.LoadImage(@"E:\pdf.pdf", contentArea);
        //    var ocrResult = ocr.Read(ocrInput);
        //}


        [HttpPost("defualt-extract-date")] // 
        public async Task<IActionResult> defualtExtractDateFromDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // this is in demo should apply subscription

                //bool result2 = IronOcr.License.IsValidLicense("IRONSUITE.MOVESPEED999.GMAIL.COM.14709-5810B94A09-EWQOKP664JBOXW-N2VDTIVZQDWM-FFX3ZGWYSCC7-YUNQBUPU56DW-72M7P7HX6T2P-XPZYRDGAAIVZ-5IXK4S-TRFA74ICWCOOEA-DEPLOYMENT.TRIAL-XRGN2O.TRIAL.EXPIRES.01.DEC.2024");
                // Read the file into a MemoryStream
                using var stream = new MemoryStream(); // bad approach dont use memeryStream
                await file.CopyToAsync(stream);
                stream.Position = 0; // Reset the stream position to the beginning


                var ocr = new IronTesseract();
                using var input = new OcrInput();

                input.LoadPdf(stream);

                OcrResult result = ocr.Read(input);
                string text = result.Text;

                string datePattern = @"\b((?:January|February|March|April|May|June|July|August|September|October|November|December) \d{1,2}, ?\d{4}|\d{1,2}/\d{1,2}/\d{4})\b";

                var matches = Regex.Matches(text, datePattern);

                if (matches.Count > 0)
                {
                    return Ok(new { Dates = matches.Select(m => m.Value) });
                }
                else
                {
                    return NotFound("No date found in the document.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("single-area-extract-date")]
        public async Task<IActionResult> ExtractDateFromDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;


                var ocr = new IronTesseract();
                using var input = new OcrInput();


                var acceptanceContentArea = new IronSoftware.Drawing.Rectangle()
                {
                    X = 250, // X coordinate
                    Y = 1740, // Y coordinate
                    Width = 300, // Width of the area
                    Height = 40 // Height of the area
                };
                input.AddPdfPage(stream, 0, ContentArea: acceptanceContentArea);

                // uncomment this code will get an error due to MemoryStream

                //var closingContentArea = new IronSoftware.Drawing.Rectangle()
                //{
                //    X = 1050, // X coordinate
                //    Y = 1040, // Y coordinate
                //    Width = 520, // Width of the area
                //    Height = 50 // Height of the area
                //};
                //input.AddPdfPage(stream, 12, ContentArea: closingContentArea);

                //input.LoadPdf(stream, ContentArea: ContentArea);
                //input.LoadImage(stream, dateRegion);
                //input.LoadImage(@"C:/Users/moves/OneDrive/Desktop/3.PNG");
                //input.LoadPdf(@"C:/Users/moves/OneDrive/Desktop/NABOR Sample Contract.pdf");
                OcrResult result = ocr.Read(input);
                string text = result.Text;

                string datePattern = @"\b((?:January|February|March|April|May|June|July|August|September|October|November|December) \d{1,2}, ?\d{4}|\d{1,2}/\d{1,2}/\d{4})\b";

                var matches = Regex.Matches(text, datePattern);

                if (matches.Count > 0)
                {
                    return Ok(new { text, Dates = matches.Select(m => m.Value) });
                }
                else
                {
                    return NotFound(text + " - No date found in the document.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("multiple-areas-extract-date")]
        public async Task<IActionResult> ExtractDatesFromDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // Save the uploaded file to a temporary location
                var tempFilePath = Path.GetTempFileName();
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                var ocr = new IronTesseract();
                using var input = new OcrInput();

                // Define multiple content areas
                var effective_date_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 250, // X coordinate
                    Y = 1740, // Y coordinate
                    Width = 300, // Width of the area
                    Height = 40 // Height of the area
                };
                var closing_date_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 750, // X coordinate
                    Y = 140, // Y coordinate
                    Width = 480, // Width of the area
                    Height = 50 // Height of the area
                };
                var buyer_1_date_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 1050, // X coordinate
                    Y = 940, // Y coordinate
                    Width = 550, // Width of the area
                    Height = 50 // Height of the area
                };
                var buyer_2_date_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 1050, // X coordinate
                    Y = 1000, // Y coordinate
                    Width = 550, // Width of the area
                    Height = 50 // Height of the area
                };
                var seller_1_date_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 1050, // X coordinate
                    Y = 1040, // Y coordinate
                    Width = 550, // Width of the area
                    Height = 50 // Height of the area
                };
                var seller_2_date_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 1050, // X coordinate
                    Y = 1100, // Y coordinate
                    Width = 550, // Width of the area
                    Height = 50 // Height of the area
                };
                var inspection_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 1180, // X coordinate
                    Y = 1475, // Y coordinate
                    Width = 165, // Width of the area
                    Height = 50 // Height of the area
                };
                var buyer_1_name_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 280, // X coordinate
                    Y = 940, // Y coordinate
                    Width = 650, // Width of the area
                    Height = 50 // Height of the area
                };
                var buyer_2_name_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 280, // X coordinate
                    Y = 1000, // Y coordinate
                    Width = 650, // Width of the area
                    Height = 50 // Height of the area
                };
                var seller_1_name_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 280, // X coordinate
                    Y = 1040, // Y coordinate
                    Width = 650, // Width of the area
                    Height = 50 // Height of the area
                };
                var seller_2_name_content_area = new IronSoftware.Drawing.Rectangle()
                {
                    X = 280, // X coordinate
                    Y = 1100, // Y coordinate
                    Width = 650, // Width of the area
                    Height = 50 // Height of the area
                };

                // Add pages with specified content areas
                input.AddPdfPage(tempFilePath, 0, ContentArea: effective_date_content_area);
                input.AddPdfPage(tempFilePath, 1, ContentArea: closing_date_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: buyer_1_date_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: buyer_2_date_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: seller_1_date_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: seller_2_date_content_area);
                input.AddPdfPage(tempFilePath, 4, ContentArea: inspection_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: buyer_1_name_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: buyer_2_name_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: seller_1_name_content_area);
                input.AddPdfPage(tempFilePath, 12, ContentArea: seller_2_name_content_area);

                OcrResult result = ocr.Read(input);
                string text = result.Text;

                string datePattern = @"\b((?:January|February|March|April|May|June|July|August|September|October|November|December) \d{1,2}, ?\d{4}|\d{1,2}/\d{1,2}/\d{4})\b";
                var matches = Regex.Matches(text, datePattern); // checking for date formats
                var splt_data = text.Split("\r\n\r\n\r\n\r\n");
                var effective_date = splt_data[0];
                var closing_date = splt_data[1];
                var buyer_1 = splt_data[2];
                var buyer_2 = splt_data[3];
                var seller_1 = splt_data[4];
                var seller_2 = splt_data[5];
                var data = new extractData();
                data.effectiveData = splt_data[0];
                data.closingDate = splt_data[1];
                data.buyer1Date = splt_data[2];
                data.buyer2Date = splt_data[3];
                data.seller1Date = splt_data[4];
                data.seller2Date = splt_data[5];
                data.inspection = splt_data[6];
                data.buyer1Name = splt_data[7];
                data.buyer2Name = splt_data[8];
                data.seller1Name = splt_data[9];
                data.seller2Name = splt_data[10];
                return Ok(new { data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public class extractData
        {
            public string? effectiveData { get; set; }
            public string? closingDate { get; set; }
            public string? inspection { get; set; }
            public string? buyer1Name { get; set; }
            public string? buyer1Date { get; set; }
            public string? buyer2Name { get; set; }
            public string? buyer2Date { get; set; }
            public string? seller1Name { get; set; }
            public string? seller1Date { get; set; }
            public string? seller2Name { get; set; }
            public string? seller2Date { get; set; }
        }
    }
}
