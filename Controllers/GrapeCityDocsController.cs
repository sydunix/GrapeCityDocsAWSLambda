using Microsoft.AspNetCore.Mvc;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Common;
using System.IO.Compression;
using System.Text;
using GrapeCity.Documents.Drawing;
using GrapeCity.Documents.Pdf.Annotations;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
//using Ionic.Zip;

namespace GrapeCityDocsAWSLambda.Controllers;

public class MergePdfs
{
    public string? PdfName1 { get; set; }
    public string? PdfName2 { get; set; }
}

public class WatermarkPdf
{
    public string? SourcePdf { get; set; }
    public string? Watermark { get; set; }
}

public class Convert
{
    public string? Source { get; set; }
    public string? Format { get; set; }
}

[Route("api/[controller]")]
public class GrapeCityDocsController : ControllerBase
{
    // POST api/grapecitydocs/mergepdf
    [HttpPost("mergepdf" )]
    public IActionResult Post( [FromForm]MergePdfs mergepdfs)
    {
        

        if (string.IsNullOrEmpty(mergepdfs.PdfName1) || string.IsNullOrEmpty(mergepdfs.PdfName2))
        {
            return new BadRequestObjectResult("Please pass a name on the query"
                    + " string or in the request body");
        } 
        
        else


        {
            //Console.WriteLine($"{pdfInput.PdfInput1} was COMPRESSED!!.");
            string upload1 = mergepdfs.PdfName1;
            string upload2 = mergepdfs.PdfName2;

            // Create Pdf Document
            GcPdfDocument pdf1 = new GcPdfDocument();
            var fsone = new FileStream(Path.Combine(upload1), FileMode.Open, FileAccess.Read);

            //Load the document
            pdf1.Load(fsone);

            // Create Pdf Document
            GcPdfDocument pdf2 = new GcPdfDocument();
            var fstwo = new FileStream(Path.Combine(upload2), FileMode.Open, FileAccess.Read);
            //Load the document
            pdf2.Load(fstwo);

            pdf1.MergeWithDocument(pdf2, new MergeDocumentOptions());

            // Save PDF Locally, Rootpath
            pdf1.Save("mergedInvoice.pdf");


            string path = @"mergedInvoice.pdf";
            var stream = new MemoryStream(System.IO.File.ReadAllBytes(path));

            return new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = "mergedInvoice.pdf"
            };

        }

        
    }

    // GET api/grapecitydocs/splitpdfbypage
    [HttpGet("splitpdfbypage")]
    public IActionResult Get([FromQuery]string nameOfPdf,int noOfPages)
    {
  

        // Create Pdf Document
        GcPdfDocument pdf = new GcPdfDocument();
        var fs = new FileStream(Path.Combine(nameOfPdf), FileMode.Open, FileAccess.Read);

        // Load the document
        pdf.Load(fs);

        GcPdfDocument pdf1 = new GcPdfDocument();
        GcPdfDocument pdf2 = new GcPdfDocument();
        GcPdfDocument pdf3 = new GcPdfDocument();

        // Assign variable for the number of pages
            

        for (int i = 0, pageNumber = i + 1; i < noOfPages; i++, pageNumber++)
        {

            OutputRange pageRange = new OutputRange(pageNumber, pageNumber);

            if (i == 0)
            {
                pdf1.MergeWithDocument(pdf, new MergeDocumentOptions()
                {
                    PagesRange = pageRange
                });
                pdf1.Save("page1.pdf");
            }

            if (i == 1)
            {
                pdf2.MergeWithDocument(pdf, new MergeDocumentOptions()
                {
                    PagesRange = pageRange
                });
                pdf2.Save("page2.pdf");
            }


            if (i == 2)
            {
                pdf3.MergeWithDocument(pdf, new MergeDocumentOptions()
                {
                    PagesRange = pageRange
                });
                pdf3.Save("page3.pdf");
            }

        }

        string path = @"SplitPDFByPage.zip";


        var docFiles = Directory.GetFiles(@"./", "page*.pdf");

        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }

        using var archive = ZipFile.Open(@"SplitPDFByPage.zip", ZipArchiveMode.Create, Encoding.UTF8);

        foreach (var docFile in docFiles)
        {
            var entry =
                archive.CreateEntryFromFile(
                    docFile,
                    Path.GetFileName(docFile),
                    CompressionLevel.Optimal
                );

            Console.WriteLine($"{entry.FullName} was compressed.");
        }

        archive.Dispose();

        var stream = new MemoryStream(System.IO.File.ReadAllBytes(path))
        {
            Position = 0
        };
        return new FileStreamResult(stream, "application/zip")
        {
            FileDownloadName = "SplitPDFByPage.zip"
        };
        

    }


    // PUT api/grapecitydocs/addwatermark/5
    [HttpPut("addwatermark/{id}")]
    public IActionResult Put(int id, [FromForm]WatermarkPdf watermarkPdf)
    {

        if (string.IsNullOrEmpty(watermarkPdf.SourcePdf) || string.IsNullOrEmpty(watermarkPdf.Watermark))
        {
            return new BadRequestObjectResult("Please pass a name on the query"
                    + " string or in the request body");
        }

        else
        {
            string? pdfDoc = watermarkPdf.SourcePdf;
            string? watermark = watermarkPdf.Watermark;

            // Create Pdf Document
            var pdf = new GcPdfDocument();
            var fs = new FileStream(Path.Combine(pdfDoc), FileMode.Open, FileAccess.Read);
            //Load the document
            pdf.Load(fs);



            foreach (var page in pdf.Pages)
            {
                //Create an instance of WatermarkAnnotation class and set its relevantproperties
                _ = new WatermarkAnnotation()
                {
                    Rect = new RectangleF(420, 23, 130, 50),
                    Image = Image.FromFile(watermark),
                    Page = page // Add watermark to page
                };
                // Save PDF Locally, Rootpath
                pdf.Save("Wetlands-Watermarked.pdf");
            }

            string path = @"Wetlands-Watermarked.pdf";
            var stream = new MemoryStream(System.IO.File.ReadAllBytes(path));
            return new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = "Wetlands-Watermarked.pdf"
            };

        }



    }

    // POST api/grapecitydocs/convert/format
    [HttpPost("convert/format")]
    public IActionResult Convert(int id, [FromForm]Convert convert)
    {
        if (string.IsNullOrEmpty(convert.Source) || string.IsNullOrEmpty(convert.Format))
        {
            return new BadRequestObjectResult("Please pass a name on the query"
                    + " string or in the request body");
        }

        else
        {
            string? source = convert.Source;
            string? format = convert.Format;


            // Render a PDF page to .svg 
            if (format == "pdf2svg")
            {
                // Create Pdf Document 
                var pdf = new GcPdfDocument();
                var fs = new FileStream(Path.Combine(source), FileMode.Open, FileAccess.Read, FileShare.Read);


                //Load the document 
                pdf.Load(fs);
                // Save in root path as invoiceBerlinPDF-convert.svg  
                pdf.Pages[0].SaveAsSvg("invoiceBerlinPDF-convert.svg", null,
                         new SaveAsImageOptions() { Zoom = 2f },
                         new XmlWriterSettings() { Indent = true });


                // Note that this path is abbreviated.  

                // Get fullpath from vsstudio by right-clicking your project  
                string path = @"invoiceBerlinPDF-convert.svg";


                // return as a download file in Swagger API 

                var stream = new MemoryStream(System.IO.File.ReadAllBytes(path));
                return new FileStreamResult(stream, "image/svg")
                {
                    FileDownloadName = "invoiceBerlinPDF-convert.svg"
                };
            } else
            {
                return NotFound();
            }

            
           
        }

    }

}