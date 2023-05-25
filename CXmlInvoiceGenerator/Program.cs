using DatabaseAccess;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

namespace CXmlInvoiceGenerator
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("New invoice generation run starting at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            GenerateCXMLForNewInvoices();
            Console.WriteLine("New invoice generation run finishing at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private static void GenerateCXMLForNewInvoices()
        {
            Invoices invoices = new Invoices();
            DataTable newInvoicesTable = invoices.GetNewInvoices();
            int i = 1;

            var cxml = new Cxml
            {
                Version = "1.0",
                PayloadID = "xxx.xxxx@example.coupahost.com",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

            };

           
            foreach (DataRow invoiceRow in newInvoicesTable.Rows)
            {
                int invoiceId = Convert.ToInt32(invoiceRow["Id"]);
                string? CurrCode = invoiceRow["CurrencyCode"].ToString();
                var InvoiceDetailItems = new List<InvoiceDetailItem>();
                DataTable itemsTable = invoices.GetItemsOnInvoice(invoiceId);
                DataRow deliveryAddressTable = invoices.GetDeliveryAddressForSalesOrder(Convert.ToInt32(invoiceRow["SalesOrderId"]));
                DataRow billingAddressTable = invoices.GetBillingAddressForInvoice(invoiceId);

                GenerateInvoiceDetailItems(CurrCode, InvoiceDetailItems, itemsTable);

                Console.WriteLine($"Generating Invoice Detail Items structures for invoice no {i} completed at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

                AddHeader(cxml);

                Console.WriteLine($"Generating Request structure for invoice no {i} started at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                //way too many params, but I decided to use bulilt in extract method function here for sake of everything being easier to read
                GenerateRequest(cxml, invoiceRow, invoiceId, CurrCode, InvoiceDetailItems, deliveryAddressTable, billingAddressTable);
                
                Console.WriteLine($"Generating Request structure for invoice no {i} completed at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

                SerializeCXML(i, cxml);
                i++;
            }


        }
        //It is entirely possible that I got some mappings wrong during creation of this structure, but that would be a general idea
        private static void GenerateRequest(Cxml cxml, DataRow invoiceRow, int invoiceId, string? CurrCode, List<InvoiceDetailItem> InvoiceDetailItems, DataRow deliveryAddressTable, DataRow billingAddressTable)
        {
            cxml.Request = new Request
            {
                DeploymentMode = "Test",
                InvoiceDetailRequest = new InvoiceDetailRequest
                {
                    InvoiceDetailRequestHeader = new InvoiceDetailRequestHeader
                    {
                        InvoiceID = invoiceId.ToString(),
                        Purpose = "standard",
                        Operation = "new",
                        InvoiceDate = DateTime.Now.ToString(),
                        InvoiceDetailHeaderIndicator = new InvoiceDetailHeaderIndicator(),
                        InvoiceDetailLineIndicator = new InvoiceDetailLineIndicator { IsAccountingInLine = "yes" },
                        InvoicePartner = new InvoicePartner[] {
                                new InvoicePartner
                                {
                                    Contact = new Contact
                                    {
                                        Role = "soldTo",
                                        Name = deliveryAddressTable["ContactName"].ToString() ?? "",
                                        PostalAddress = new PostalAddress
                                        {
                                            Street = deliveryAddressTable["AddrLine1"].ToString() ?? "",
                                            City = deliveryAddressTable["AddrLine2"].ToString() ?? "",
                                            State = deliveryAddressTable["AddrLine3"].ToString() ?? "",
                                            PostalCode = deliveryAddressTable["AddrPostCode"].ToString() ?? "",
                                            Country = new Country { IsoCountryCode = deliveryAddressTable["CountryCode"].ToString() ?? "", Value = deliveryAddressTable["Country"].ToString() ?? "" }
                                        }
                                    }
                                }, new InvoicePartner
                                {
                                    Contact = new Contact
                                    {
                                        Role = "billTo",
                                        Name = billingAddressTable["ContactName"].ToString() ?? "",
                                        PostalAddress = new PostalAddress
                                        {
                                            Street = billingAddressTable["AddrLine2"].ToString() ?? "",
                                            City = billingAddressTable["AddrLine3"].ToString() ?? "",
                                            State = billingAddressTable["AddrLine4"].ToString() ?? "",
                                            PostalCode = billingAddressTable["AddrPostCode"].ToString() ?? "",
                                            Country = new Country { IsoCountryCode = billingAddressTable["CountryCode"].ToString() ?? "", Value = billingAddressTable["Country"].ToString() ?? "" }
                                        }
                                    }
                                } },



                        PaymentTerms = new PaymentTerm
                        {
                            PayInNumberOfDays = Convert.ToInt32(invoiceRow["PaymentTermsDays"]), 

                        }
                    },
                    InvoiceDetailOrder = new InvoiceDetailOrder
                    {
                        InvoiceDetailItem = InvoiceDetailItems.ToArray(),
                        InvoiceDetailOrderInfo = new InvoiceDetailOrderInfo { OrderReference = new OrderReference { DocumentReference = new DocumentReference { PayloadID = "PayLoadId" } } }
                    },
                    InvoiceDetailSummary = new InvoiceDetailSummary
                    {
                        SubtotalAmount = new SubtotalAmount { Money = new Money { Currency = CurrCode, Value = "val" } },
                        Tax = new Tax
                        {

                            Money = new Money { Currency = "" },
                            Description = "Desc",
                            TaxDetail = new TaxDetail
                            {
                                Purpose = "Tax",
                                Category = "sales",
                                PercentageRate = Convert.ToInt32(invoiceRow["VATPercentage"]),

                                TaxableAmount = new TaxableAmount { Money = new Money { Currency = CurrCode, Value = invoiceRow["NetAmount"].ToString() } },
                                TaxAmount = new TaxAmount { Money = new Money { Currency = CurrCode, Value = invoiceRow["VATAmount"].ToString() } },
                                TaxLocation = "GB"
                            }

                        },
                        ShippingAmount = new ShippingAmount
                        {
                            Money = new Money { Currency = CurrCode, Value = "ShippingAmount" }
                        },

                        GrossAmount = new GrossAmount
                        {
                            Money = new Money { Currency = CurrCode, Value = invoiceRow["GrossAmount"].ToString() }
                        },

                        NetAmount = new NetAmount
                        {
                            Money = new Money { Currency = CurrCode, Value = invoiceRow["NetAmount"].ToString() }
                        },

                        DueAmount = new DueAmount
                        {
                            Money = new Money { Currency = CurrCode, Value = "DueAmount" }
                        },
                    },

                }

            };
        }

        private static void AddHeader(Cxml cxml)
        {
            cxml.Header = new Header
            {
                From = new From
                {
                    Credential = new Credential
                    {
                        Domain = "DUNS",
                        Identity = "xxxxxxxx"
                    }
                },
                To = new To
                {
                    Credential = new Credential
                    {
                        Domain = "NetworkID",
                        Identity = "yyyyyyyy"
                    }
                },
                Sender = new Sender
                {
                    Credential = new Credential
                    {
                        Domain = "DUNS",
                        Identity = "xxxxxxxxx",
                        SharedSecret = "xxxxxxxxx"
                    },
                    UserAgent = "Coupa Procurement 1.0"
                }
            };
        }

        private static void GenerateInvoiceDetailItems(string CurrCode, List<InvoiceDetailItem> InvoiceDetailItems, DataTable itemsTable)
        {
            int iteration = 0;
            foreach (DataRow itemRow in itemsTable.Rows)
            {
                iteration++;
                Console.WriteLine($"Generating Invoice detail Item structure no {iteration}  at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} ");

                InvoiceDetailItems.Add(new InvoiceDetailItem()
                {
                    InvoiceLineNumber = Convert.ToDecimal(itemRow["LineTotal"]),
                    Quantity = Convert.ToInt32(itemRow["Qty"]),
                    UnitOfMeasure = "EA",
                    UnitPrice = new UnitPrice { Money = new Money { Currency = CurrCode, Value = itemRow["UnitPrice"].ToString() } },
                    InvoiceDetailItemReference = new InvoiceDetailItemReference
                    {
                        LineNumber = Convert.ToDecimal(itemRow["LineTotal"]),
                        ItemID = new ItemID { SupplierPartID = itemRow["ID"].ToString() },
                        Description = itemRow["Description"].ToString(),
                        ManufacturerPartID = itemRow["PartNo"].ToString(),
                        ManufacturerName = itemRow["Manufacturer"].ToString()
                    }
                });
            };
        }

        //where i is invoice iterator, I have decided to provide each invoice in separate file
        private static void SerializeCXML(int i, Cxml cxml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Cxml));
            using (StringWriter writer = new StringWriter())
            {
                string filename = String.Concat("InvoiceRequest", i.ToString(), ".xml");
                Console.WriteLine($"XML serialization for invoice {i} starting at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                serializer.Serialize(writer, cxml);
                string xml = writer.ToString();
                // Save the XML to a file

                /*the assumption is that few things should be configurable however I have an issue with my IDE and despite
                 * added reference to System.Configuration.ConfigurationManager I somehow can't add app.config to my solution
                 * The idea would be to put there (among others) a path to output dir and then in code retrieve it by
                 * string path = ConfigurationManager.AppSettings["OutputPath"];
                 * But I don't have time to diagnose it further on
                 */

                File.WriteAllText(filename, xml);
                Console.WriteLine($"XML serialization for invoice {i} completed at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                Console.WriteLine($"Output for invoice {i} succesfully saved to a file: {filename}");
            }
        }


        // == Please complete this function ==

        // 1) Using the DatabaseAccess dll provided and referenced (in the refs folder), load each invoice from the database
        //
        // 2) Create a cXml invoice document using the information from each invoice

        // The following is a very helpful resource for cXml:

        // https://compass.coupa.com/en-us/products/product-documentation/supplier-resources/supplier-integration-resources/standard-invoice-examples/sample-cxml-invoice-with-payment-terms

        // Assume the invoice is raised on the same day you find it, so PaymentTerms is from Today

        // VAT mode is header (overall total) only, not at item level

        // 3) Save the created invoices into a specified output file with the .xml file extension

        // The "purpose" for each invoice is "standard"
        // The "operation" for each invoice is "new"
        // The output folder is entirely up to you, based on your file system
        // You can use "fake" credentials (Domain/Identity/SharedSecret etc. etc.) of your own choosing for the From/To/Sender section for this test
        //
        // It would likely be a good idea for all of these to be configurable in some way, in a .Net options/settings file or an external ini file

        // Ideally, you will write reasonable progress steps to the console window

        // You may add references to anything you want from the standard Nuget URL

        // You may modify the signature to this function if you want to pass values into it

        // You may move this code into another class (or indeed classes) of your choosing

    }
}