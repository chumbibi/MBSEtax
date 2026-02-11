using MBSWeb.Data;
using MBSWeb.Migrations;
using MBSWeb.Models.Dto;
using MBSWeb.Models.Entities;
using MBSWeb.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
//DownloadInvoiceByNumber
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Utils;

using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Companding;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
//using TheArtOfDev.HtmlRenderer.PdfSharp;

using static MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes.ImageSource;
using static System.Net.Mime.MediaTypeNames;
using Color = MigraDocCore.DocumentObjectModel.Color;
using Document = MigraDocCore.DocumentObjectModel.Document;
using Image = SixLabors.ImageSharp.Image;
using System.Text;
using Microsoft.AspNetCore.Mvc;
namespace MBSWeb.Services.Repositories
{
    public class InvoiceTransactionsRepository : IInvoiceTransactions
    {
        private readonly ApplicationDbContext _context;

        public InvoiceTransactionsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MBSResponse> GetAllInvoicesByCompany(int companyId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                // Base query
                IQueryable<InvoiceTransactions> query = _context.InvoiceTransactions;

                // Filter by companyId or companyCode if provided
                if (companyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == companyId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(companyCode))
                {
                    companyCode = companyCode.Trim();
                    query = query.Where(i => i.CompanyId != null && i.CompanyId == companyId);
                }

                // Apply search term if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim();

                    query = query.Where(i =>
                        EF.Functions.Like(i.InvoiceNumber ?? "", $"%{searchTerm}%") ||
                        EF.Functions.Like(i.CustomerName ?? "", $"%{searchTerm}%") ||
                        EF.Functions.Like(i.CustomerCode ?? "", $"%{searchTerm}%") ||
                        EF.Functions.Like(i.IRN ?? "", $"%{searchTerm}%") ||
                        EF.Functions.Like(i.FirsInvoiceNumber ?? "", $"%{searchTerm}%")
                    );
                }

                // Total records before pagination
                var totalRecords = await query.CountAsync();

                // Paginate
                var invoices = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Response payload
                var responseData = new
                {
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Invoices = invoices
                };

                return Success("Invoices retrieved successfully", responseData);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoices: {ex.Message}");
            }
        }




        public async Task<MBSResponse> GetInvoiceByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0)
                    pageNumber = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                var query = _context.InvoiceTransactions
                    .Where(i =>
                        i.CompanyId == companyId &&
                        i.InvoiceNumber == invoiceNumber);

                var totalRecords = await query.CountAsync();

                if (totalRecords == 0)
                    return Fail("Invoice not found");

                var invoices = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseData = new
                {
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Invoices = invoices
                };

                return Success("Invoice(s) retrieved successfully", responseData);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoice: {ex.Message}");
            }
        }


        public async Task<MBSResponse> GetInvoiceItemsByInvoiceNumber(int companyId, string invoiceNumber, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Safety checks
                if (pageNumber <= 0)
                    pageNumber = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                var query = _context.ItemLines
                    .Where(i =>
                        i.CompanyId == companyId &&
                        i.DocEntry == invoiceNumber);

                var totalRecords = await query.CountAsync();

                if (totalRecords == 0)
                    return Fail("No invoice items found");

                var items = await query
                    .OrderBy(i => i.LineNum)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseData = new
                {
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Items = items
                };

                return Success("Invoice items retrieved successfully", responseData);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoice items: {ex.Message}");
            }
        }




        public async Task<MBSResponse> GetInvoicesByCustomerCode(int companyId, string customerCode, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Safety checks
                if (pageNumber <= 0)
                    pageNumber = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                var query = _context.InvoiceTransactions
                    .Where(i =>
                        i.CompanyId == companyId &&
                        i.CustomerCode == customerCode);

                var totalRecords = await query.CountAsync();

                if (totalRecords == 0)
                    return Fail("No invoices found for this customer");

                var invoices = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseData = new
                {
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Invoices = invoices
                };

                return Success("Invoices retrieved successfully", responseData);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoices: {ex.Message}");
            }
        }



        public async Task<MBSResponse> GetInvoicesByDateRange(DateRangeDto model, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (model.StartDate == null || model.EndDate == null)
                    return Fail("Invalid date range");

                if (pageNumber <= 0)
                    pageNumber = 1;

                if (pageSize <= 0)
                    pageSize = 10;

                var query = _context.InvoiceTransactions.AsQueryable();

                query = query.Where(i =>
                    i.CompanyId == model.CompanyId &&
                    i.InvoiceDate >= model.StartDate &&
                    i.InvoiceDate <= model.EndDate);

                if (!string.IsNullOrWhiteSpace(model.CustomerCode))
                {
                    query = query.Where(i => i.CustomerCode == model.CustomerCode);
                }

                var totalRecords = await query.CountAsync();

                if (totalRecords == 0)
                    return Fail("No invoices found for the selected date range");

                var invoices = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var responseData = new
                {
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Invoices = invoices
                };

                return Success("Invoices retrieved successfully", responseData);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to retrieve invoices: {ex.Message}");
            }
        }


        public async Task<MBSResponse> UpdateInvoiceByIRNAsync(string irn, PaymentStatusDto model)
        {
            try
            {
                var invoice = await _context.InvoiceTransactions
                    .FirstOrDefaultAsync(i => i.IRN == irn);

                if (invoice == null)
                    return Fail("Invoice not found");
                int paymentStatus = 0;
                switch (model.PaymentStatus)
                {
                    case PaymentStatus.Pending:
                        paymentStatus = 0;
                        break;
                    case PaymentStatus.Paid:
                        paymentStatus = 1;
                        break;
                    case PaymentStatus.Rejected:
                        paymentStatus = 2;
                        break;
                    default:
                        return Fail("Invalid payment status");
                }

                invoice.InvoicePaymentStatus = paymentStatus;
                invoice.PaymentStatusTransmited = 1;

                await _context.SaveChangesAsync();

                return Success("Invoice payment status updated successfully", invoice);
            }
            catch (Exception ex)
            {
                return Fail($"Failed to update invoice: {ex.Message}");
            }
        }


        public async Task<MBSResponse> DownloadInvoiceByNumber(int companyid, string invoiceNumber)
        {
            try
            {
                var invoice = await _context.InvoiceTransactions
                    .FirstOrDefaultAsync(i =>
                        i.CompanyId == companyid && (i.FirsInvoiceNumber == invoiceNumber ||
                        i.InvoiceNumber == invoiceNumber));
                if (invoice != null)
                {

                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerCode == invoice.CustomerCode);



                    var items = await _context.ItemLines
                        .Where(i =>
                            i.CompanyId == companyid &&
                            i.DocEntry == invoiceNumber)
                        .OrderBy(i => i.LineNum)
                        .ToListAsync();

                    // 1. Create the PDF document
                    var document = new Document();
                    var section = document.AddSection();
                    section.PageSetup.PageFormat = PageFormat.A4;
                    section.PageSetup.TopMargin = Unit.FromCentimeter(0.5);    // 1 cm
                    section.PageSetup.BottomMargin = Unit.FromCentimeter(0.1); // 1 cm
                    section.PageSetup.LeftMargin = Unit.FromCentimeter(0.2);   // 1.5 cm
                    section.PageSetup.RightMargin = Unit.FromCentimeter(0.8);  // 1.5 cm

                    // 2. Invoice Header
                    var paragraph = section.AddParagraph("INVOICE");
                    paragraph.Format.Font.Size = 14;
                    paragraph.Format.Font.Name = "Candara";
                    paragraph.Format.Font.Color = Colors.Black;
                    paragraph.Format.Font.Bold = true;
                    paragraph.Format.SpaceBefore = "0.0cm";
                    paragraph.Format.SpaceAfter = "0.1cm";
                    paragraph.Format.Alignment = ParagraphAlignment.Right;


                    // 2. Invoice Header
                    paragraph = section.AddParagraph("Original");
                    paragraph.Format.Font.Size = 8;
                    paragraph.Format.Font.Name = "Candara";
                    paragraph.Format.Font.Color = Colors.Black;
                    paragraph.Format.Font.Bold = true;
                    paragraph.Format.SpaceBefore = "0.1cm";
                    paragraph.Format.SpaceAfter = "2.0cm";
                    paragraph.Format.Alignment = ParagraphAlignment.Center;


                    // Draw a table with five rows and four columns
                    // ======================
                    // BORDERLESS TABLE (5 x 4) — MANUAL
                    // ======================
                    var table = section.AddTable();
                    table.Borders.Width = 0; // no visible borders

                    // ---- Columns ----
                    table.AddColumn(Unit.FromCentimeter(11)); // 55% of usable width
                    table.AddColumn(Unit.FromCentimeter(3));  // remaining 3 columns equally
                    table.AddColumn(Unit.FromCentimeter(3));
                    table.AddColumn(Unit.FromCentimeter(3));

                    table.LeftPadding = 4;
                    table.RightPadding = 4;

                    // ---- Row 1 (Header) ----
                    var row1 = table.AddRow();
                    row1.Height = Unit.FromCentimeter(0.9);
                    //row1.Format.Font.Bold = true;

                    row1.Cells[0].AddParagraph(invoice.CustomerName);
                    row1.Cells[0].Format.Font.Size = 10;
                    row1.Cells[0].Format.Font.Bold = true;
                    row1.Cells[0].Format.Font.Color = Colors.Black;

                    row1.Cells[1].AddParagraph("Qty");
                    row1.Cells[2].AddParagraph("Unit Price");
                    row1.Cells[3].AddParagraph("Amount");

                    // ---- Row 2 ----
                    var row2 = table.AddRow();
                    row2.Height = Unit.FromCentimeter(0.9);
                    row2.Cells[0].AddParagraph($"ATTN: FIN/ACC");
                    row2.Cells[0].Format.Font.Size = 8;
                    row2.Cells[0].Format.Font.Bold = false;
                    row2.Cells[0].Format.Font.Color = Colors.Black;


                    row2.Cells[1].AddParagraph(invoice.InvoiceNumber).Format.Alignment = ParagraphAlignment.Right;

                    string dt = DateTime.Parse(invoice.InvoiceDate.ToString()).ToString("yyyy-MM-dd");
                    row2.Cells[2].AddParagraph($"{dt}");
                    row2.Cells[3].AddParagraph(customer.TIN);
                    //"₦100,000"
                    // ---- Row 3 ----
                    var row3 = table.AddRow();
                    row3.Height = Unit.FromCentimeter(0.9);
                    row3.Cells[0].AddParagraph(invoice.CustomerAddress);
                    row3.Cells[0].Format.Font.Size = 6;
                    row3.Cells[0].Format.Font.Bold = false;
                    row3.Cells[0].Format.Font.Color = Colors.DarkGray;

                    row3.Cells[1].AddParagraph("1");
                    row3.Cells[2].AddParagraph("₦150,000");
                    row3.Cells[3].AddParagraph("₦150,000");

                    // ---- Row 4 ----
                    var row4 = table.AddRow();
                    row4.Height = Unit.FromCentimeter(0.9);
                    row4.Cells[0].AddParagraph($"{customer.LgaCode },{customer.StateCode}"); // empty
                    row4.Cells[0].Format.Font.Size = 6;
                    row4.Cells[0].Format.Font.Bold = false;
                    row4.Cells[0].Format.Font.Color = Colors.DarkGray;

                    row4.Cells[1].AddParagraph("");
                    row4.Cells[2].AddParagraph("Subtotal:");
                    row4.Cells[3].AddParagraph("₦350,000");
                    row4.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                    row4.Cells[3].Format.Alignment = ParagraphAlignment.Right;

                    // ---- Row 5 ----
                    var row5 = table.AddRow();
                    row5.Height = Unit.FromCentimeter(0.9);
                    row5.Cells[0].AddParagraph(customer.Country.ToUpper()); // empty
                    row5.Cells[0].Format.Font.Size = 6;
                    row5.Cells[0].Format.Font.Bold = false;
                    row5.Cells[0].Format.Font.Color = Colors.DarkGray;
                     

                    row5.Cells[1].AddParagraph("");
                    row5.Cells[2].AddParagraph("Total:");
                    row5.Cells[3].AddParagraph("₦350,000");
                    row5.Format.Font.Bold = true;
                    row5.Cells[2].Format.Alignment = ParagraphAlignment.Right;
                    row5.Cells[3].Format.Alignment = ParagraphAlignment.Right;



                    // 3. Render document to PDF
                    var renderer = new PdfDocumentRenderer(unicode: true);
                    renderer.Document = document;
                    renderer.RenderDocument();
                    var pdf = renderer.PdfDocument;

                    // 4. Set up graphics for drawing
                    var page = pdf.Pages[0];
                    var gfx = XGraphics.FromPdfPage(page);

                    // Insert Logo

                    var imagePath = Path.Combine(Environment.CurrentDirectory, $"Assets\\c{companyid.ToString()}.png");

                    var directory = Path.GetDirectoryName(imagePath);

                    // Ensure directory exists
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // var signaturePath = Path.Combine(Environment.CurrentDirectory, "registrar.png");

                    using (var img = Image.Load(imagePath))
                    using (var ms = new MemoryStream())
                    {
                        // Save the image as PNG into a memory stream
                        img.Save(ms, new PngEncoder());
                        ms.Position = 0;

                        // PdfSharpCore expects a Func<Stream>, so wrap the stream
                        var xImage = XImage.FromStream(() => new MemoryStream(ms.ToArray()));

                        //220;

                        double ImageWidth = 120;
                        double ImageHeight = 50;
                        double y = 55;
                        double x = (page.Width.Point + 1.5 * ImageWidth + section.PageSetup.RightMargin + section.PageSetup.LeftMargin - img.Width) / 2; // + width;


                        // Draw the image on the page.
                        gfx.DrawImage(xImage, x-10, y, ImageWidth, ImageHeight);

                        ms.Dispose();

                    }

                    //insert line

                    // ===== SETTINGS =====
                    //double marginLeft = 10;
                    //double marginRight = page.Width - 10;
                    //double yTop = 40;
                    //double gap = 5;
                    //// ===== LINE SPACING =====


                    //string title = "INVOICE";

                    //// Font
                    //var font = new XFont("Arial", 16, XFontStyle.Bold);

                    //// Measure text
                    //var textSize = gfx.MeasureString(title, font);

                    //// ===== 3/4 PAGE POSITION =====
                    //double textCenterX = page.Width * 0.75;

                    //double textStart = textCenterX - textSize.Width / 2;
                    //double textEnd = textCenterX + textSize.Width / 2;

                    //// ===== BLACK PENS =====
                    //var thickPen = new XPen(XColors.Black, 4);
                    //var thinPen = new XPen(XColors.Black, 1);

                    //// ===== DRAW UPPER (THICK) LINE =====
                    //gfx.DrawLine(thickPen, marginLeft, yTop, textStart - 10, yTop);
                    //gfx.DrawLine(thickPen, textEnd + 10, yTop, marginRight, yTop);

                    //// ===== DRAW LOWER (THIN) LINE =====
                    //double yBottom = yTop + gap + (thickPen.Width / 2); ;
                    //gfx.DrawLine(thinPen, marginLeft, yBottom, textStart - 10, yBottom);
                    //gfx.DrawLine(thinPen, textEnd + 10, yBottom, marginRight, yBottom);

                    //// ===== DRAW TEXT =====
                    //gfx.DrawString(title, font, XBrushes.Black,
                    //               new XPoint(textCenterX, yTop + 4),
                    //               XStringFormats.TopCenter);

                    // Insert Logo

                    //var linePath = Path.Combine(Environment.CurrentDirectory, $"Assets\\{"cyberspaceLine"}.png");


                    //using (var imgline = Image.Load(linePath))
                    //using (var linems = new MemoryStream())
                    //{
                    //    // Save the image as PNG into a memory stream
                    //    imgline.Save(linems, new PngEncoder());
                    //    linems.Position = 0;

                    //    // PdfSharpCore expects a Func<Stream>, so wrap the stream
                    //    var lineImage = XImage.FromStream(() => new MemoryStream(linems.ToArray()));

                    //    //220;

                    //    double lineWidth = 90;
                    //    double lineHeight = 10;
                    //    double y = 40;
                    //    double x = (page.Width.Point + lineWidth + section.PageSetup.RightMargin + section.PageSetup.LeftMargin - imgline.Width) / 2; // + width;


                    //    // Draw the image on the page.
                    //    gfx.DrawImage(lineImage, x, y, lineWidth, lineHeight);

                    //    linems.Dispose();

                    //}

                    // 

                    //double pageWidth = XUnit.FromMillimeter(210);  // A4 width
                    //double pageHeight = XUnit.FromMillimeter(297); // A4 height

                    //// Insert watermark image

                    //if (File.Exists(linePath))
                    //{
                    //    XImage watermark = XImage.FromFile(linePath);

                    //    //double centerX = (pageWidth - 240) / 2;
                    //     centerX = (pageWidth - 240) / 2;
                    //    double centerY = (pageHeight) / 4.5;

                    //    gfx.DrawImage(watermark, centerX, centerY, 240, 10);
                    //}

                    //// === Styles and dimensions ===
                    //double outerBorderThickness = 2;
                    //double innerBorderThickness = 4;
                    //double innerMargin = 10;
                    //double cornerCircleRadius = XUnit.FromMillimeter(10);  // ~6mm circle radius
                    //double cornerSquareSize = XUnit.FromMillimeter(12);    // 6mm square

                    //var outerPen = new XPen(XColor.FromArgb(255, 128, 0, 0), outerBorderThickness); // Maroon
                    //var innerPen = new XPen(XColor.FromArgb(255, 104, 44, 44), innerBorderThickness); // Dark red
                    //var circlePen = new XPen(XColors.Maroon, 2); // Thin black outline for circles
                    //var circleFill = XBrushes.White;              // White background
                    //var squareFill = XBrushes.White;

                    //// === 6. Draw inner rectangle ===

                    //double innerX = innerMargin;
                    //double innerY = innerMargin;
                    //double innerWidth = pageWidth - 2 * innerMargin;
                    //double innerHeight = pageHeight - 2 * innerMargin;

                    //gfx.DrawRectangle(innerPen, innerX, innerY, innerWidth, innerHeight);


                    // === 8. Save and return PDF ===
                    string fileName = $"invoice_{companyid.ToString()}{invoiceNumber}_{DateTime.Now:yyyy}.pdf";
                    string filePath = Path.Combine(Environment.CurrentDirectory, "INVDr", fileName);



                    //// === 8. Save and return PDF ===
                    //string fileName = $"invoice_{companyid.ToString()}{invoiceNumber}_{DateTime.Now:yyyyMMddHHmmssfff}.pdf";
                    // string filePath = Path.Combine(Environment.CurrentDirectory, "TranscriptDr", fileName);
                    //Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);



                    pdf.Info.Author = companyid.ToString();
                    pdf.Info.Subject = "Cyberspace Limited";
                    pdf.Info.Keywords = "Invoice";
                    pdf.Info.CreationDate = DateTime.Now;

                    // Insert a QR Code

                    pdf.Save(filePath);

                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    string base64 = Convert.ToBase64String(fileBytes);

                    //return new MBSResponse
                    //{
                    //    StatusCode = 200,
                    //    Message = base64,
                    //    Data = fileBytes
                    //};

                    return Success(base64, fileBytes);
                }
                else
                {
                    return Fail("Invoice not found");
                }

            }
            catch (Exception ex)
            {
                return Fail($"Failed to update invoice: {ex.Message}");
            }
        }

        private static MBSResponse Success(string message, object? data = null) =>
            new()
            {
                StatusCode = 200,
                Message = message,
                Data = data
            };

        private static MBSResponse Fail(string message) =>
            new()
            {
                StatusCode = 400,
                Message = message,
                Data = null
            };
    }
}
