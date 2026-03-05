using MBSWeb.Data;
using MBSWeb.Migrations;
using MBSWeb.Models.Dto;
using MBSWeb.Models.Entities;
using MBSWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
//DownloadInvoiceByNumber
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Companding;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Buffers.Text;
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
using Microsoft.SqlServer.Server;
namespace MBSWeb.Services.Repositories
{
    public class InvoiceTransactionsRepository : IInvoiceTransactions
    {
        private readonly ApplicationDbContext _context;

        public InvoiceTransactionsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("getallinvoicesbycompany")]
        public async Task<MBSResponse> GetAllInvoicesByCompany( int? companyId = null, string? companyCode = null, string? searchTerm = null,  int pageNumber = 1,int pageSize = 10)
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

                    string inv = invoice.InvoiceNumber.Replace("INV", "").Trim();
                    inv= string.IsNullOrEmpty(inv) ? "0" : inv;
                     int a = int.TryParse(inv, out int result) ? result : 0;
                    
                    var items = await _context.ItemLines
                        .Where(i =>
                            i.CompanyId == companyid &&
                            i.DocEntry == result.ToString())
                        .OrderBy(i => i.LineNum)
                        .ToListAsync();

                    // 1. Create the PDF document
                    var document = new Document();
                    var section = document.AddSection();
                    section.PageSetup.PageFormat = PageFormat.A4;
                    section.PageSetup.TopMargin = Unit.FromCentimeter(0.5);    // 1 cm
                    section.PageSetup.BottomMargin = Unit.FromCentimeter(0.1); // 1 cm
                    section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);   // 1.5 cm
                    section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);  // 1.5 cm

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
                    table.Format.Font.Name = "Garamond";
                    table.Format.Font.Size = 7;   // <-- smaller invoice body text
                    table.TopPadding = 1.5;
                    table.BottomPadding = 1.5;
                    table.LeftPadding = 5;
                    table.RightPadding = 5;

                    // ---- Columns ----
                    table.AddColumn(Unit.FromCentimeter(8)); // 55% of usable width
                    table.AddColumn(Unit.FromCentimeter(3));  // remaining 3 columns equally
                    table.AddColumn(Unit.FromCentimeter(3));
                    table.AddColumn(Unit.FromCentimeter(3));
 

                    // ---- Row 1 (Header) ----
                    var row1 = table.AddRow();
                    row1.Height = Unit.FromCentimeter(0.9);
                    //row1.Format.Font.Bold = true;

                    row1.Cells[0].AddParagraph(invoice.CustomerName);
                    row1.Cells[0].Format.Font.Size = 10;
                    row1.Cells[0].Format.Font.Bold = true;
                    row1.Cells[0].Format.Font.Color = Colors.Black;
                    row1.Cells[0].Format.Font.Name = "Garamond";

                    row1.Cells[1].Format.Font.Size  = 8;
                    row1.Cells[1].Format.Font.Bold = false;
                    row1.Cells[0].Format.Font.Name = "Garamond";
                    row1.Cells[1].AddParagraph("Document Number").Format.Alignment = ParagraphAlignment.Left;
                    row1.Cells[2].AddParagraph("Document Date").Format.Alignment = ParagraphAlignment.Left; ;
                    row1.Cells[3].AddParagraph("Tin No").Format.Alignment = ParagraphAlignment.Left; ;

                    // ---- Row 2 ----
                    var row2 = table.AddRow();
                    row2.Height = Unit.FromCentimeter(0.9);
                    row2.Cells[0].AddParagraph($"ATTN: {customer.ContactPerson}");
                    row2.Cells[0].Format.Font.Size = 8;
                    row2.Cells[0].Format.Font.Bold = false;
                    row1.Cells[0].Format.Font.Name = "Garamond";
                    row2.Cells[0].Format.Font.Color = Colors.Black;

                    row2.Cells[1].Format.Font.Size = 8;
                    row2.Cells[1].Format.Font.Bold = true;
                    row1.Cells[0].Format.Font.Name = "Garamond";
                    row2.Cells[1].AddParagraph(invoice.InvoiceNumber).Format.Alignment = ParagraphAlignment.Left;

                    string dt = DateTime.Parse(invoice.InvoiceDate.ToString()).ToString("yyyy-MM-dd");
                    row2.Cells[2].Format.Font.Size = 8;
                    row1.Cells[0].Format.Font.Name = "Garamond";
                    row2.Cells[2].Format.Font.Bold = true;
                    row2.Cells[2].AddParagraph($"{dt}");
                    row2.Cells[3].AddParagraph(customer.TIN);
                    //"₦100,000"
                    // ---- Row 3 ----
                    var row3 = table.AddRow();
                    row3.Height = Unit.FromCentimeter(0.5);
                    row3.Cells[0].AddParagraph(customer.CustomerAddress);
                    row3.Cells[0].Format.Font.Size = 8;
                    row3.Cells[0].Format.Font.Bold = true;
                    row3.Cells[0].Format.Font.Color = Colors.DarkGray;
                    row1.Cells[0].Format.Font.Name = "Garamond";

                    row3.Cells[1].Format.Font.Size = 8;
                    row3.Cells[1].AddParagraph("Salesperson").Format.Alignment = ParagraphAlignment.Left; ;
                    row3.Cells[2].AddParagraph();
                    row3.Cells[3].AddParagraph("Mobile No").Format.Alignment=ParagraphAlignment.Left;
                    row1.Cells[0].Format.Font.Name = "Garamond";

                    // ---- Row 4 ----
                    var row4 = table.AddRow();
                    row4.Height = Unit.FromCentimeter(0.5);
                    row4.Cells[0].Format.Font.Bold = true;
                    row4.Cells[0].AddParagraph($"{customer.LgaCode },{customer.StateCode}, {customer.Country}"); // empty
                    row4.Cells[0].Format.Font.Size = 8;
                    row4.Cells[0].Format.Font.Bold = false;
                    row4.Cells[0].Format.Font.Color = Colors.DarkGray;
                    row1.Cells[0].Format.Font.Name = "Garamond";

                    row4.Cells[1].Format.Font.Size = 8;
                    row4.Cells[1].Format.Font.Bold = true;
                    row4.Cells[1].AddParagraph(customer.SalesPerson);
                    row4.Cells[2].AddParagraph();
                    row4.Cells[3].Format.Font.Bold = true;
                    row4.Cells[3].AddParagraph(customer.SalesPersonPhone);

                    var row5 = table.AddRow();
                    row5.Cells[0].MergeRight = 3;
                    // Merge the first cell across the remaining 3 columns
                    // Add text                    
                    string countryName = customer.Country.Trim().ToUpper() == "NG" ? "NIGERIA" : customer.Country.ToUpper();                     
                    row5.Cells[0].AddParagraph(countryName);
                    // Optional formatting
                    row5.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                  // blank row
                    //string countryName =  customer.Country.Trim().Length == "NG"? "Nigeria".ToUpper(): customer.Country.ToUpper();
                    var row6 = table.AddRow();
                    row6.Cells[0].MergeRight = 3;

                    var row7 = table.AddRow();
                    row7.Cells[0].MergeRight = 3;

                    var parag = row7.Cells[0].AddParagraph();

                    // Align everything to the right
                    parag.Format.Alignment = ParagraphAlignment.Right;

                    // Base font
                    parag.Format.Font.Size = 7;

                    // Normal text
                    parag.AddText("Currency ");

                    // Bold + bigger currency
                    var currencyText = parag.AddFormattedText(items[0].Currency.ToUpper());
                    currencyText.Bold = true;
                    currencyText.Size = 8;

                    // Products Table
                     
                    

                   
                    var rowpH = table.AddRow();
                    rowpH.Shading.Color = Colors.LightGray;
                    rowpH.Format.Font.Size = 8;
                    rowpH.Cells[0].AddParagraph("Item Description").Format.Alignment = ParagraphAlignment.Left;
                    rowpH.Cells[1].AddParagraph("Price").Format.Alignment = ParagraphAlignment.Right;
                    rowpH.Cells[2].AddParagraph("Quantity").Format.Alignment = ParagraphAlignment.Right;
                    rowpH.Cells[3].AddParagraph("Total").Format.Alignment = ParagraphAlignment.Right;

                    int toShade = 1;
                    foreach (var item in items)
                    {
                        var rowp = table.AddRow();
                        
                        rowp.Borders.Width = 0;
                        
                            rowp.Shading.Color = Colors.White;
                        
                        rowp.Format.Font.Size = 8;
                        rowp.Cells[0].AddParagraph(item.ItemDescription).Format.Alignment = ParagraphAlignment.Left;
                        rowp.Cells[1].AddParagraph(item.Price.ToString()).Format.Alignment = ParagraphAlignment.Right;
                        rowp.Cells[2].AddParagraph(item.Quantity.ToString()).Format.Alignment = ParagraphAlignment.Right;
                        rowp.Cells[3].AddParagraph(item.LineTotal.ToString()).Format.Alignment = ParagraphAlignment.Right;
                    }

                    var tableLine = section.AddTable();

                    // A4 width (21 cm) - margins (1.5 + 1.5)
                    tableLine.AddColumn(Unit.FromCentimeter(17));

                    var row = tableLine.AddRow();

                    row.Cells[0].Borders.Bottom.Width = 1.0;
                    row.Cells[0].Borders.Bottom.Color = Colors.SkyBlue;

                    row.Cells[0].Format.SpaceBefore = "0.2cm";
                    row.Cells[0].Format.SpaceAfter = "0.3cm";

                    // Add a nested table for totals

                    // Create the main total table
                    var tblTotal = section.AddTable();
                    tblTotal.Borders.Width = 0; // no visible borders
                    tblTotal.Format.Font.Name = "Garamond";
                    tblTotal.Format.Font.Size = 7;
                    tblTotal.TopPadding = 1.5;
                    tblTotal.BottomPadding = 1.5;
                    tblTotal.LeftPadding = 5;
                    tblTotal.RightPadding = 5;

                    // Columns: left 10 cm, right 7 cm
                    tblTotal.AddColumn(Unit.FromCentimeter(10));
                    tblTotal.AddColumn(Unit.FromCentimeter(7));

                    // Add a row
                    var rowTotal = tblTotal.AddRow();

                    // Left cell (will hold nested table)
                    rowTotal.Cells[0].Format.Font.Size = 8;
                    rowTotal.Cells[0].Format.Font.Bold = true;
                    rowTotal.Cells[0].Format.Alignment = ParagraphAlignment.Left;

                    // Right cell (will hold nested table)
                    rowTotal.Cells[1].Format.Font.Size = 8;
                    rowTotal.Cells[1].Format.Font.Bold = true;
                    rowTotal.Cells[1].Format.Alignment = ParagraphAlignment.Right;

                    // ----------------------------
                    // Create nested table for left cell
                    var tblLeft = rowTotal.Cells[0].Elements.AddTable();
                    tblLeft.Borders.Width = 0;  // no borders
                    tblLeft.AddColumn(Unit.FromCentimeter(2.5)); // first column of nested table
                    tblLeft.AddColumn(Unit.FromCentimeter(2.5)); // second column of nested table
                    tblLeft.AddColumn(Unit.FromCentimeter(2.5)); // Third column of nested table
                    tblLeft.AddColumn(Unit.FromCentimeter(2.5)); // Fourth column of nested table

                    // Add a row to left nested table
                    var leftRow = tblLeft.AddRow();
                    leftRow.Cells[0].MergeRight = 3; // merge across the entire left cell
                    leftRow.Cells[0].Format.Font.Name = "Garamond";
                    leftRow.Cells[0].AddParagraph("Tax Details");
                    leftRow.Cells[0].Format.Font.Size = 7;
                    leftRow.Cells[0].Format.Font.Bold = true;
                     

                    // Add another row if needed
                    var leftRow2 = tblLeft.AddRow();
                    leftRow2.Format.Font.Size = 6;
                    leftRow2.Height = Unit.FromCentimeter(0.5);
                    leftRow2.Shading.Color = Colors.LightGray;
                    leftRow2.Cells[0].AddParagraph($"Tax %").Format.Alignment=  ParagraphAlignment.Left;
                    leftRow2.Cells[1].AddParagraph("Net").Format.Alignment = ParagraphAlignment.Right;
                    leftRow2.Cells[2].AddParagraph("Tax").Format.Alignment = ParagraphAlignment.Right;
                    leftRow2.Cells[3].AddParagraph("Gross").Format.Alignment = ParagraphAlignment.Right;

                    var leftRow3 = tblLeft.AddRow();
                    leftRow3.Height = Unit.FromCentimeter(0.5);
                    leftRow3.Format.Font.Size = 6;
                    // Set bottom border for the last cell
                    leftRow3.Borders.Bottom.Width = 1.5;                 // thickness
                    leftRow3.Borders.Bottom.Color = Colors.SkyBlue;
                    leftRow3.Borders.Bottom.Style = BorderStyle.Single;
                    decimal NetAmount = invoice.TotalAmount - invoice.VatSum;
                    decimal TaxPercent = 100 * (invoice.VatSum) / NetAmount;

                    // Add data to cells
                    leftRow3.Cells[0].AddParagraph(TaxPercent.ToString()).Format.Alignment = ParagraphAlignment.Left;
                    leftRow3.Cells[1].AddParagraph(NetAmount.ToString()).Format.Alignment = ParagraphAlignment.Right;
                    leftRow3.Cells[2].AddParagraph(invoice.VatSum.ToString()).Format.Alignment = ParagraphAlignment.Right;
                    leftRow3.Cells[3].AddParagraph(invoice.TotalAmount.ToString()).Format.Alignment = ParagraphAlignment.Right;

                        // color
                      // solid line
                    // ----------------------------
                    // Create nested table for right cell
                    var tblRight = rowTotal.Cells[1].Elements.AddTable();
                    tblRight.Borders.Width = 0; // no borders
                    tblRight.AddColumn(Unit.FromCentimeter(3.5));
                    tblRight.AddColumn(Unit.FromCentimeter(3.5));

                    // Add a row to right nested table
                    var rightRow = tblRight.AddRow();
                    rightRow.Cells[0].AddParagraph("Tax:");
                    rightRow.Cells[1].AddParagraph("750");

                    // Add another row
                    var rightRow2 = tblRight.AddRow();
                    rightRow2.Cells[0].AddParagraph("Total:");
                    rightRow2.Cells[1].AddParagraph("10,750");






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
                        double y = 40;
                        double x = (page.Width.Point + 1.5 * ImageWidth + section.PageSetup.RightMargin + section.PageSetup.LeftMargin - img.Width) / 2; // + width;


                        // Draw the image on the page.
                        gfx.DrawImage(xImage, x-10, y, ImageWidth, ImageHeight);

                        ms.Dispose();

                    }

                   


                    // === 8. Save and return PDF ===
                    string fileName = $"invoice_{companyid.ToString()}{invoiceNumber}_{DateTime.Now:yyyyMMddhhmmssfff}.pdf";
                    string filePath = Path.Combine(Environment.CurrentDirectory, "INVDr", fileName);


 

                    pdf.Info.Author = companyid.ToString();
                    pdf.Info.Subject = "Cyberspace Limited";
                    pdf.Info.Keywords = "Invoice";
                    pdf.Info.CreationDate = DateTime.Now;

                    // Insert a QR Code

                    byte[] qrBytes = Convert.FromBase64String(invoice.QRCode);

                    using (var msQr = new MemoryStream(qrBytes))
                    {
                        var qrImage = XImage.FromStream(() => new MemoryStream(msQr.ToArray()));

                        double qrSize = 50; // square size

                        // center horizontally
                        double xQr = (page.Width.Point - qrSize) / 2;

                        // 1 inch from bottom
                        double yQr = page.Height.Point - qrSize - Unit.FromInch(1).Point;

                        gfx.DrawImage(qrImage, xQr, yQr, qrSize, qrSize);
                    }


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
