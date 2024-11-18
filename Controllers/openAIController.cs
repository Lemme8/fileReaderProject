using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;

namespace fileReaderProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class openAIController : ControllerBase
    {
        [HttpPost("chatGPT")]
        public async Task<IActionResult> ExtractDatesFromDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {

                var tempFilePath = Path.GetTempFileName();
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                int pageNumber = 1;

                var area = new PdfRectangle(50, 100, 300, 200); // Adjust these coordinates as needed

                //var extractedText = new StringBuilder();
                var extractedText = "";
                string extracteTextOnPage = "";
                using (var document = PdfDocument.Open(tempFilePath))
                {
                    var page = document.GetPage(pageNumber);
                    extracteTextOnPage = page.Text;
                    //foreach (var word in page.GetWords())
                    //{
                    //    if (IsWithinBounds(word.BoundingBox, area))
                    //    {
                    //        extractedText.Append(word.Text + " ");
                    //    }
                    //}
                }
                var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                IConfiguration config = builder.Build();
                string apiKey = config["openAI:apiKey"] ??
                    throw new InvalidOperationException("API key not found in configuration");
                ChatClient client = new(model: "gpt-4o", apiKey: apiKey);
                ChatCompletion completion = client.CompleteChat($"extract date on this text '{extracteTextOnPage}'");
                Console.WriteLine($"GPT >>{completion.Content[0].Text}");
                return Ok(new { completion.Content[0].Text });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private static string DataExtraction(string tempFilePath, PdfRectangle area, int pageNo)
        {
            var extractedText = new StringBuilder();
            using (var document = PdfDocument.Open(tempFilePath))
            {
                var page = document.GetPage(pageNo);
                foreach (var word in page.GetWords())
                {
                    if (IsWithinBounds(word.BoundingBox, area))
                    {
                        extractedText.Append(word.Text + " ");
                    }
                }
            }
            return extractedText.ToString();
        }
        private static bool IsWithinBounds(PdfRectangle wordBounds, PdfRectangle area)
        {
            return wordBounds.Left >= area.Left && wordBounds.Right <= area.Right
                && wordBounds.Bottom >= area.Bottom && wordBounds.Top <= area.Top;
        }
        public class contractData
        {
            public string? page_1_buyer_name { get; set; }
            public string? page_1_seller_name { get; set; }
            public string? effectiveDate { get; set; }
            public string? closingDate { get; set; }
            public string? inspection { get; set; }
            public string? buyer_name_1 { get; set; }
            public string? buyer_date_1 { get; set; }
            public string? buyer_name_2 { get; set; }
            public string? buyer_date_2 { get; set; }
            public string? seller_name_1 { get; set; }
            public string? seller_date_1 { get; set; }
            public string? seller_name_2 { get; set; }
            public string? seller_date_2 { get; set; }
        }
        [HttpPost("pdf")]
        public async Task<IActionResult> pdfExtractDatesFromDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var tempFilePath = Path.GetTempFileName();
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }


                // Define multiple areas and extract text for each
                //var areas = new List<PdfRectangle>
                //    {
                //        new PdfRectangle(50, 100, 300, 200), // Adjust these coordinates as needed
                //        new PdfRectangle(100, 150, 350, 250), // Another area
                //        // Add more areas as needed
                //    };

                //var extractedTextList = new List<string>();
                //int pageNumber = 1; // Page number to extract text from

                //foreach (var area in areas)
                //{
                //    var extractedText = DataExtraction(tempFilePath, area, pageNumber);
                //    extractedTextList.Add(extractedText);
                //}
                var contact = new contractData();

                var buyerNameArea = new PdfRectangle(80, 700, 500, 680);
                contact.page_1_buyer_name = DataExtraction(tempFilePath, buyerNameArea, 1);
                var sellerNameArea = new PdfRectangle(80, 720, 500, 690);
                contact.page_1_seller_name = DataExtraction(tempFilePath, sellerNameArea, 1);
                var effectiveDateArea = new PdfRectangle(80, 170, 200, 150);
                contact.effectiveDate = DataExtraction(tempFilePath, effectiveDateArea, 1);
                var closingDateArea = new PdfRectangle(260, 750, 450, 730);
                contact.closingDate = DataExtraction(tempFilePath, closingDateArea, 2);
                var inspectionArea = new PdfRectangle(660, 260, 100, 245);
                contact.inspection = DataExtraction(tempFilePath, inspectionArea, 5);

                var buyer1NameArea = new PdfRectangle(80, 460, 400, 440);
                contact.buyer_name_1 = DataExtraction(tempFilePath, buyer1NameArea, 13);
                var buyer1DateArea = new PdfRectangle(850, 460, 200, 445);
                contact.buyer_date_1 = DataExtraction(tempFilePath, buyer1DateArea, 13);

                var buyer2NameArea = new PdfRectangle(80, 430, 400, 400);
                contact.buyer_name_2 = DataExtraction(tempFilePath, buyer2NameArea, 13);
                var buyer2DateArea = new PdfRectangle(1300, 435, 250, 422);
                contact.buyer_date_2 = DataExtraction(tempFilePath, buyer2DateArea, 13);


                var seller1NameArea = new PdfRectangle(80, 410, 300, 390);
                contact.seller_name_1 = DataExtraction(tempFilePath, seller1NameArea, 13);
                var seller1DateArea = new PdfRectangle(1300, 410, 250, 398);
                contact.seller_date_1 = DataExtraction(tempFilePath, seller1DateArea, 13);

                var seller2NameArea = new PdfRectangle(80, 390, 300, 370);
                contact.seller_name_2 = DataExtraction(tempFilePath, seller2NameArea, 13);
                var seller2DateArea = new PdfRectangle(1300, 390, 250, 375);
                contact.seller_date_2 = DataExtraction(tempFilePath, seller2DateArea, 13);

                //var openAiApiKey = "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";
                //var client = new ChatClient(apiKey: openAiApiKey);

                //var response = await client.CompleteChatAsync(new ChatCompletionRequest
                //{
                //    Model = "gpt-4o",
                //    Messages = new List<ChatMessage>
                //    {
                //        new ChatMessage("user", $"Extract date from the following text: '{string.Join(" ", extractedTextList)}'")
                //    }
                //});

                //var chatResponse = response.Choices[0].Message.Content;

                return Ok(new { contact });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
