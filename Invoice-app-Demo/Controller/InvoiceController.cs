using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
// Add this using directive at the top of the file

namespace BuggyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public InvoiceController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetInvoice()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var invoice = new Invoice();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get invoice details
                    string invoiceQuery = "SELECT InvoiceID, CustomerName FROM Invoices WHERE InvoiceID = 1";
                    using (SqlCommand cmd = new SqlCommand(invoiceQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                invoice.InvoiceID = reader.GetInt32(0);
                                invoice.CustomerName = reader.GetString(1);
                            }
                        }
                    }

                    // Get invoice items
                    invoice.Items = new List<Item>();
                    string itemsQuery = "SELECT Name, Price FROM InvoiceItems WHERE InvoiceID = 1";
                    using (SqlCommand cmd = new SqlCommand(itemsQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                invoice.Items.Add(new Item
                                {
                                    Name = reader.GetString(0),
                                    Price = (double)reader.GetDecimal(1)
                                });
                            }
                        }
                    }
                }

                if (invoice.Items != null && invoice.Items.Count > 0)
                {
                    return Ok(invoice);
                }

                return NotFound("No invoice found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class Invoice
    {
        public int InvoiceID { get; set; }
        public string CustomerName { get; set; }
        public List<Item> Items { get; set; }
        public double Total => Items?.Sum(i => i.Price) ?? 0;
    }

    public class Item
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
