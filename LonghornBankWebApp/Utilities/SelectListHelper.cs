using Microsoft.AspNetCore.Mvc.Rendering;

namespace LonghornBankWebApp.Utilities

{
    public class SelectListHelper
    {

        public static IEnumerable<SelectListItem> GetStateList()
        {
            List<SelectListItem> StateList = new List<SelectListItem>();
            StateList.Add(new SelectListItem { Text = "Alabama", Value = "AL" });
            StateList.Add(new SelectListItem { Text = "Alaska", Value = "AK" });
            StateList.Add(new SelectListItem { Text = "Arizona", Value = "AZ" });
            StateList.Add(new SelectListItem { Text = "Arkansas", Value = "AR" });
            StateList.Add(new SelectListItem { Text = "California", Value = "CA" });
            StateList.Add(new SelectListItem { Text = "Colorado", Value = "CO" });
            StateList.Add(new SelectListItem { Text = "Connecticut", Value = "CT" });
            StateList.Add(new SelectListItem { Text = "Delaware", Value = "DE" });
            StateList.Add(new SelectListItem { Text = "Florida", Value = "FL" });
            StateList.Add(new SelectListItem { Text = "Georgia", Value = "GA" });
            StateList.Add(new SelectListItem { Text = "Hawaii", Value = "HI" });
            StateList.Add(new SelectListItem { Text = "Idaho", Value = "ID" });
            StateList.Add(new SelectListItem { Text = "Illinois", Value = "IL" });
            StateList.Add(new SelectListItem { Text = "Indiana", Value = "IN" });
            StateList.Add(new SelectListItem { Text = "Iowa", Value = "IA" });
            StateList.Add(new SelectListItem { Text = "Kansas", Value = "KS" });
            StateList.Add(new SelectListItem { Text = "Kentucky", Value = "KY" });
            StateList.Add(new SelectListItem { Text = "Louisiana", Value = "LA" });
            StateList.Add(new SelectListItem { Text = "Maine", Value = "ME" });
            StateList.Add(new SelectListItem { Text = "Maryland", Value = "MD" });
            StateList.Add(new SelectListItem { Text = "Massachusetts", Value = "MA" });
            StateList.Add(new SelectListItem { Text = "Michigan", Value = "MI" });
            StateList.Add(new SelectListItem { Text = "Minnesota", Value = "MN" });
            StateList.Add(new SelectListItem { Text = "Mississippi", Value = "MS" });
            StateList.Add(new SelectListItem { Text = "Missouri", Value = "MO" });
            StateList.Add(new SelectListItem { Text = "Montana", Value = "MT" });
            StateList.Add(new SelectListItem { Text = "Nebraska", Value = "NE" });
            StateList.Add(new SelectListItem { Text = "Nevada", Value = "NV" });
            StateList.Add(new SelectListItem { Text = "New Hampshire", Value = "NH" });
            StateList.Add(new SelectListItem { Text = "New Jersey", Value = "NJ" });
            StateList.Add(new SelectListItem { Text = "New Mexico", Value = "NM" });
            StateList.Add(new SelectListItem { Text = "New York", Value = "NY" });
            StateList.Add(new SelectListItem { Text = "North Carolina", Value = "NC" });
            StateList.Add(new SelectListItem { Text = "North Dakota", Value = "ND" });
            StateList.Add(new SelectListItem { Text = "Ohio", Value = "OH" });
            StateList.Add(new SelectListItem { Text = "Oklahoma", Value = "OK" });
            StateList.Add(new SelectListItem { Text = "Oregon", Value = "OR" });
            StateList.Add(new SelectListItem { Text = "Pennsylvania", Value = "PA" });
            StateList.Add(new SelectListItem { Text = "Rhode Island", Value = "RI" });
            StateList.Add(new SelectListItem { Text = "South Carolina", Value = "SC" });
            StateList.Add(new SelectListItem { Text = "South Dakota", Value = "SD" });
            StateList.Add(new SelectListItem { Text = "Tennessee", Value = "TN" });
            StateList.Add(new SelectListItem { Text = "Texas", Value = "TX" });
            StateList.Add(new SelectListItem { Text = "Utah", Value = "UT" });
            StateList.Add(new SelectListItem { Text = "Vermont", Value = "VT" });
            StateList.Add(new SelectListItem { Text = "Virginia", Value = "VA" });
            StateList.Add(new SelectListItem { Text = "Washington", Value = "WA" });
            StateList.Add(new SelectListItem { Text = "West Virginia", Value = "WV" });
            StateList.Add(new SelectListItem { Text = "Wisconsin", Value = "WI" });
            StateList.Add(new SelectListItem { Text = "Wyoming", Value = "WY" });
            return StateList;
        }




    }
}
