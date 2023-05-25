using System;
using System.Xml.Serialization;

/*File contains all the classes that will be used for generating CXML output*/


namespace CXmlInvoiceGenerator
{
    [XmlRoot(ElementName = "cXML", Namespace = "http://xml.cxml.org/schemas/cXML/1.2/InvoiceDetail.dtd")]
    public class Cxml
    {
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "payloadID")]
        public string PayloadID { get; set; }

        [XmlAttribute(AttributeName = "timestamp")]
        public string Timestamp { get; set; }

        public Header Header { get; set; }
        public Request Request { get; set; }
    }

    public class Header
    {
        public From From { get; set; }
        public To To { get; set; }
        public Sender Sender { get; set; }
    }

    public class From
    {
        public Credential Credential { get; set; }
    }

    public class To
    {
        public Credential Credential { get; set; }
    }

    public class Sender
    {
        public Credential Credential { get; set; }

        [XmlElement(ElementName = "UserAgent")]
        public string UserAgent { get; set; }
    }

    public class Credential
    {
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }

        public string Identity { get; set; }

        [XmlElement(ElementName = "SharedSecret")]
        public string SharedSecret { get; set; }
    }

    public class Request
    {
        [XmlAttribute(AttributeName = "deploymentMode")]
        public string DeploymentMode { get; set; }

        public InvoiceDetailRequest InvoiceDetailRequest { get; set; }
    }

    public class InvoiceDetailRequest
    {
        public InvoiceDetailRequestHeader InvoiceDetailRequestHeader { get; set; }
        public InvoiceDetailOrder InvoiceDetailOrder { get; set; }
        public InvoiceDetailSummary InvoiceDetailSummary { get; set; }
    }

    public class InvoiceDetailRequestHeader
    {
        [XmlAttribute(AttributeName = "invoiceID")]
        public string InvoiceID { get; set; }

        [XmlAttribute(AttributeName = "purpose")]
        public string Purpose { get; set; }

        [XmlAttribute(AttributeName = "operation")]
        public string Operation { get; set; }

        [XmlAttribute(AttributeName = "invoiceDate")]
        public string InvoiceDate { get; set; }

        public InvoiceDetailHeaderIndicator InvoiceDetailHeaderIndicator { get; set; }
        public InvoiceDetailLineIndicator InvoiceDetailLineIndicator { get; set; }
        public InvoicePartner[] InvoicePartner { get; set; }
        public PaymentTerm PaymentTerms { get; set; }
    }

    public class InvoiceDetailHeaderIndicator { }

    public class InvoiceDetailLineIndicator
    {
        [XmlAttribute(AttributeName = "isAccountingInLine")]
        public string IsAccountingInLine { get; set; }
    }

    public class InvoicePartner
    {
        public Contact Contact { get; set; }
    }

    public class Contact
    {
        [XmlAttribute(AttributeName = "role")]
        public string Role { get; set; }
        public string AddressID { get; set; }
        public string Name { get; set; }
        public PostalAddress PostalAddress { get; set; }
    }

    public class PostalAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public Country Country { get; set; }
    }

    public class Country
    {
        [XmlAttribute(AttributeName = "isoCountryCode")]
        public string IsoCountryCode { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class PaymentTerm
    {
        [XmlAttribute(AttributeName = "payInNumberofDays")]
        public int PayInNumberOfDays;

        [XmlElement(ElementName = "Discount")]
        public Discount Discount { get; set; }

        public int NetDueDays { get; set; }
    }

    public class Discount
    {
        public DiscountPercent DiscountPercent { get; set; }
        public int DiscountDueDays { get; set; }
    }

    public class DiscountPercent
    {
        [XmlAttribute(AttributeName = "percent")]
        public int Percent { get; set; }
    }

    public class InvoiceDetailOrder
    {
        public InvoiceDetailOrderInfo InvoiceDetailOrderInfo { get; set; }
        public InvoiceDetailItem[] InvoiceDetailItem { get; set; }
    }

    public class InvoiceDetailOrderInfo
    {
        public OrderReference OrderReference { get; set; }
    }

    public class OrderReference
    {
        public DocumentReference DocumentReference { get; set; }
    }

    public class DocumentReference
    {
        [XmlAttribute(AttributeName = "payloadID")]
        public string PayloadID { get; set; }
    }

    public class InvoiceDetailItem
    {
        [XmlAttribute(AttributeName = "invoiceLineNumber")]
        public decimal InvoiceLineNumber { get; set; }

        [XmlAttribute(AttributeName = "quantity")]
        public int Quantity { get; set; }

        public string UnitOfMeasure { get; set; }
        public UnitPrice UnitPrice { get; set; }
        public InvoiceDetailItemReference InvoiceDetailItemReference { get; set; }
        public SubtotalAmount SubtotalAmount { get; set; }
        public GrossAmount GrossAmount { get; set; }
        public NetAmount NetAmount { get; set; }
    }

    public class UnitPrice
    {
        public Money Money { get; set; }
    }

    public class InvoiceDetailItemReference
    {
        [XmlAttribute(AttributeName = "lineNumber")]
        public decimal LineNumber { get; set; }

        public ItemID ItemID { get; set; }

        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }

        public string ManufacturerPartID { get; set; }

        [XmlElement(ElementName = "ManufacturerName")]
        public string ManufacturerName { get; set; }
    }

    public class ItemID
    {
        public string SupplierPartID { get; set; }
    }

    public class SubtotalAmount
    {
        public Money Money { get; set; }
    }

    public class GrossAmount
    {
        public Money Money { get; set; }
    }

    public class NetAmount
    {
        public Money Money { get; set; }
    }

    public class InvoiceDetailSummary
    {
        public SubtotalAmount SubtotalAmount { get; set; }
        public Tax Tax { get; set; }
        public ShippingAmount ShippingAmount { get; set; }
        public GrossAmount GrossAmount { get; set; }
        public NetAmount NetAmount { get; set; }
        public DueAmount DueAmount { get; set; }
    }

    public class Tax
    {
        public Money Money { get; set; }

        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }

        public TaxDetail TaxDetail { get; set; }
    }

    public class TaxDetail
    {
        [XmlAttribute(AttributeName = "purpose")]
        public string Purpose { get; set; }

        [XmlAttribute(AttributeName = "category")]
        public string Category { get; set; }

        [XmlAttribute(AttributeName = "percentageRate")]
        public int PercentageRate { get; set; }

        public TaxableAmount TaxableAmount { get; set; }
        public TaxAmount TaxAmount { get; set; }

        [XmlElement(ElementName = "TaxLocation")]
        public string TaxLocation { get; set; }
    }

    public class TaxableAmount
    {
        public Money Money { get; set; }
    }

    public class TaxAmount
    {
        public Money Money { get; set; }
    }

    public class ShippingAmount
    {
        public Money Money { get; set; }
    }

    public class DueAmount
    {
        public Money Money { get; set; }
    }

    public class Money
    {
        [XmlAttribute(AttributeName = "currency")]
        public string Currency { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}