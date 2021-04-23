using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class EntryModel
    {
        public class Entry
        {
            public string ParcelID { get; set; }
            public string PropertyUrl { get; set; }
            public string Owner { get; set; }
            public string Address { get; set; }
            public string MailingAddress { get; set; }
            public string Neighborhood { get; set; }
            public string Subdivision { get; set; }
            public string CurrAppraisalValue { get; set; }
            public string PropertyClass { get; set; }
            public int BedRooms { get; set; }
            public int FullBaths { get; set; }
            public int HalfBaths { get; set; }
            public int FamilyRooms { get; set; }
            public string LivingArea { get; set; }
            public string GroundFlrArea { get; set; }
            public string BasementArea { get; set; }
            public string UpperFlrArea { get; set; }
            public string YearBuilt { get; set; }
            public string Style { get; set; }
            public string Foundation { get; set; }
            public string BasementType { get; set; }
            public string LastRemodel { get; set; }
            public string ConstructionQuality { get; set; }
            public string PhysicalCondition { get; set; }
            public string CDU { get; set; }
            public string PrevYearTax { get; set; }
            public string TotalCurrentBalance { get; set; }
            public string TaxRecordPDF { get; set; }
        }

        public class ComparableData
        {
            public string SubjectParcelID { get; set; }
            public string ParcelID { get; set; }
            public string PropertyUrl { get; set; }
            public string Address { get; set; }
            public string SaleDate { get; set; }
            public string ActualSalePrice { get; set; }
            public string AdjustedSalePrice { get; set; }


        }

        public class BuildingComponents
        {
            public string ParcelID { get; set; }
            public string Component { get; set; }
            public string Units { get; set; }
            public string YearAdded { get; set; }
            public string Percent { get; set; }
        }

        public class OtherImprovements
        {
            public string ParcelID { get; set; }
            public string Number { get; set; }
            public string Occupancy { get; set; }
            public string Quantity { get; set; }
            public string YearBuilt { get; set; }
            public string Stories { get; set; }
            public string Condition { get; set; }
            public string Function { get; set; }

        }
        
        public class PropertyList
        {
            public string Keyword { get; set; }
            public string PropertyID { get; set; }
        }

        public class Keywords
        {
            public string Keyword { get; set; }
        }

        public class AllInfoHTMLPage
        {
            public string PropertyInformation { get; set; }
            public string BuildingData { get; set; }
            public string TaxInformation { get; set; }
            public string TaxRecordPDF { get; set; }
            public string Comparables { get; set; }

        }

        public class AsyncProgress
        {
            public int ProgressValue { get; set; }
            public string ProgressText { get; set; }
            public string LogText { get; set; }

        }

        public class Address
        {
            public string ParcelID { get; set; }
            public string AddressValue { get; set; }
        }

        public class MailingAddress
        {
            public string ParcelID { get; set; }
            public string MailingAddressValue { get; set; }

        }

        public enum SearchBy
        {
            subdivision,
            address,
            parcel,
            owner
        }
    }
}
